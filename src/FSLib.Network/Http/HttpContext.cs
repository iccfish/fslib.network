// Copyright (c) 2011 rubicon IT GmbH
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.FishExtension;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.IO.Compression;
using System.Net.Security;

namespace FSLib.Network.Http
{
	using System.Diagnostics;
	using Newtonsoft.Json;

	using System.FishLib;
#if NET_GT_4
	using System.Threading.Tasks;
#endif

	/// <summary>
	/// 封装了一个请求的上下文环境
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay}")]
	public class HttpContext : IDisposable
	{
		bool _captureContext;

		Dictionary<string, object> _contextData;
		AsyncOperation _operation;

		HttpPerformance _performance;

		int _requsetResubmit;
		protected int ReadyStateValue;

		/// <summary>
		/// 创建 <see cref="HttpContext" />  的新实例(HttpContext)
		/// </summary>
		internal HttpContext(HttpClient client, HttpRequestMessage request)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client), "client is null.");
			if (request == null)
				throw new ArgumentNullException(nameof(request), "request is null.");

			Client = client;
			Request = request;

			client.OnHttpContextCreated(new WebEventArgs(this));
		}

		/// <summary>
		/// 请求已收到，请求判断响应类型
		/// </summary>
		public event EventHandler DetectResponseContentType;

		/// <summary>
		/// 性能计数器对象已经新建
		/// </summary>
		public event EventHandler PerformanceObjectCreated;

		/// <summary>
		/// 请求被取消
		/// </summary>
		public event EventHandler RequestCancelled;

		/// <summary>
		/// 当请求已经被创建时触发
		/// </summary>
		public event EventHandler RequestCreated;

		void SendEntryPoint()
		{
			if (CheckCancellation() || CheckException())
				return;

			if (!ChangeReadyState(HttpContextState.NotSended, HttpContextState.Init))
				return;

			Performance = new HttpPerformance(this);
			ConnectionInfo = new ConnectionInfo();
			if (AutoStartSpeedMonitor)
				Performance.EnableSpeedMonitor();

			var we = new WebEventArgs(this);
			Client.OnBeforeRequest(we);
			if (we.Cancelled)
			{
				Cancelled = true;
				CheckCancellation();
				return;
			}

			Request.Normalize(Client, this);
			WebRequest = Client.HttpHandler.GetRequest(Request.Uri, Request.Method, this);
			Client.CopyDefaultSettings(this);
			Request.InitializeWebRequest(this);
			Request.ExceptType?.OnRequestInit();
			Client.HttpHandler.PrepareContext(this);

#if NET_GET_45
			WebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
			 {
				 var e = new CertificateValidationEventArgs(this, sslPolicyErrors, chain, certificate);
				 OnServerCertificateValidation(e);
				 return e.Result;
			 };
#endif

			OnRequestCreated();
			Performance.InitialzieCompleteTime = DateTime.Now;
			if (CheckCancellation() || CheckException())
				return;

			//强行重载部分参数
			WebRequest.AutomaticDecompression = DecompressionMethods.None;
			OnRequestSending();
			ChangeReadyState(HttpContextState.Init, HttpContextState.SendingRequestHeader);

			//如果有数据，则请求并写入
			if (Request.RequestData != null && Request.AllowRequestBody)
			{
				Request.RequestData.Prepare(WebRequest);
				Client.HttpHandler.AfterRequestDataPrepared(this);
				WriteRequestData();
			}
			else
			{
				Performance.GetRequestStreamTime = DateTime.Now;
				//直接获得响应
				FlushRequestData();
			}
		}

		/// <summary>
		/// 校验服务器证书
		/// </summary>
		public event EventHandler<CertificateValidationEventArgs> ServerCertificateValidation;

		/// <summary>
		/// 引发 <see cref="ServerCertificateValidation"/> 事件
		/// </summary>
		/// <param name="e"></param>
		internal void OnServerCertificateValidation(CertificateValidationEventArgs e)
		{
			Client.HttpHandler.OnServerCertificateValidation(this, e);
			ServerCertificateValidation?.Invoke(this, e);
		}


		string DebuggerDisplay
		{
			get { return $"{Request.Method} \"{Request.Uri}\" Code={Status} Success:{IsSuccess} Elapsed:{Performance?.ElapsedTime}"; }
		}

		/// <summary>
		/// 以原子操作变更当前的状态。如果当前状态不符，则返回false
		/// </summary>
		/// <param name="originalState"></param>
		/// <param name="currentState"></param>
		/// <returns></returns>
		protected virtual bool ChangeReadyState(HttpContextState originalState, HttpContextState currentState)
		{
			var v1 = (int)originalState;
			var v2 = (int)currentState;
			if (Interlocked.CompareExchange(ref ReadyStateValue, v2, v1) == v1)
			{
				OnStateChanged();
				return true;
			}

			return false;
		}

		/// <summary>
		/// 检测是否取消。如果有异常，则执行结束操作。
		/// </summary>
		protected bool CheckCancellation()
		{
#if NET_GT_4
			if (Cancelled || (CancellationToken != null && CancellationToken.Value.IsCancellationRequested))
			{
				Abort();
				SetException(new OperationCanceledException());
				Cancelled = true;
				return true;
			}
#else
			if (Cancelled)
			{
				Abort();
				SetException(new OperationCanceledException());
				return true;
			}
#endif
			return false;
		}

		/// <summary>
		/// 检测是否已经抛出异常。如果有异常，则执行结束操作。
		/// </summary>
		/// <returns></returns>
		protected bool CheckException()
		{
			if (Exception == null)
				return false;

			CompleteRequest();
			return true;
		}

		/// <summary>
		/// 获得猜测的最理想的返回结果类型
		/// </summary>
		/// <returns></returns>
		protected virtual HttpResponseContent GetPreferedResponseContent()
		{
			var contentType = WebResponse.ContentType;
			var status = (int)WebResponse.StatusCode;

			var index = contentType.IndexOf(";");
			if (index != -1) contentType = contentType.Substring(0, index); //分解带字符串的

			if (contentType == "text/xml") return new ResponseXmlContent(this, Client, null); //XML
			if (contentType.StartsWith("image")) return new ResponseImageContent(this, Client); //图片

			if ((contentType == "text/json" || contentType == "application/json") && status >= 400 && status <= 499 && (Request?.ExtraRequestInfo?.ErrorResponseObject ?? Client.Setting.ErrorResponseObject) != null)
			{
				return Request.ExtraRequestInfo.ErrorResponseObject.ToResponseObjectContent(this, Client);
			}

			if (contentType.StartsWith("text") || contentType == "application/x-javascript") return new ResponseStringContent(this, Client);

			//30x-50x 请求均默认当作文本类型
			if (status >= 300)
			{
				return new ResponseStringContent(this, Client);
			}

			return new ResponseBinaryContent(this, Client);
		}

		protected virtual void InternalOnRequestFailed()
		{
			IsFinished = true;

			if (_operation == null)
			{
				if (Cancelled)
					OnRequestCancelled();
				OnRequestFailed();
				OnRequestEnd();
			}
			else
			{
				_operation.PostOperationCompleted(_ =>
				{
					if (Cancelled)
						OnRequestCancelled();

					OnRequestFailed();
					OnRequestEnd();
				}, null);
				_operation = null;
			}

			if (Cancelled)
			{
				Client.OnRequestCancelled(new WebEventArgs(this));
			}
			Client.OnRequestFailed(new WebEventArgs(this));
		}

		/// <summary>
		/// 请求结束并且成功，开始收尾操作。
		/// </summary>
		protected virtual void InternalOnRequestFinished()
		{
			IsFinished = true;
			if (_operation != null)
			{
				_operation.PostOperationCompleted(_ =>
				{
					OnRequestFinished();
					OnRequestEnd();
				}, null);
				_operation = null;
			}
			else
			{
				OnRequestFinished();
				OnRequestEnd();
			}
			Client.OnRequestSuccess(new WebEventArgs(this));
		}

		/// <summary>
		/// 请求结束
		/// </summary>
		public event EventHandler<WebEventArgs> RequestEnd;

		/// <summary>
		/// 引发 <see cref="DetectResponseContentType"/> 事件
		/// </summary>
		protected virtual void OnDetectResponseContentType()
		{
			DetectResponseContentType?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="PerformanceObjectCreated"/> 事件
		/// </summary>
		protected virtual void OnPerformanceObjectCreated()
		{
			PerformanceObjectCreated?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestCancelled" /> 事件
		/// </summary>
		protected virtual void OnRequestCancelled()
		{
			var handler = RequestCancelled;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestCreated" /> 事件
		/// </summary>
		protected virtual void OnRequestCreated()
		{
			var handler = RequestCreated;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestDataSended" /> 事件
		/// </summary>
		protected virtual void OnRequestDataSended()
		{
			var handler = RequestDataSended;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestDataSending" /> 事件
		/// </summary>
		protected virtual void OnRequestDataSending()
		{
			var handler = RequestDataSending;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestDataSendProgressChanged" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnRequestDataSendProgressChanged([NotNull] DataProgressEventArgs ea)
		{
			var handler = RequestDataSendProgressChanged;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 引发 <see cref="RequestFailed" /> 事件
		/// </summary>
		protected virtual void OnRequestFailed()
		{
			var handler = RequestFailed;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestFinished" /> 事件
		/// </summary>
		protected virtual void OnRequestFinished()
		{
			var handler = RequestFinished;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestRedirect" /> 事件
		/// </summary>
		protected virtual void OnRequestRedirect()
		{
			var handler = RequestRedirect;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestResubmit"/> 事件
		/// </summary>
		protected virtual void OnRequestResubmit()
		{
			RequestResubmit?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestSended" /> 事件
		/// </summary>
		protected virtual void OnRequestSended()
		{
			var handler = RequestSended;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}
		/// <summary>
		/// 触发 <see cref="RequestSending"/> 事件
		/// </summary>
		protected virtual void OnRequestSending()
		{
			EventHandler handler = RequestSending;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestStreamFetched" /> 事件
		/// </summary>
		protected virtual void OnRequestStreamFetched()
		{
			var handler = RequestStreamFetched;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="RequestValidateResponse" /> 事件
		/// </summary>
		protected virtual void OnRequestValidateResponse()
		{
			if (Request.ExtraRequestInfo?.Disable302Redirection == true && Response.Redirection != null)
			{
				throw new HttpClientException("httpcontext_ex_redirectiondetected");
			}

			RequestValidateResponse?.Invoke(this, EventArgs.Empty);
		}
		/// <summary>
		/// 引发 <see cref="ResponseDataReceiveCompleted"/>
		/// </summary>
		protected virtual void OnResponseDataReceiveCompleted()
		{
			ResponseDataReceiveCompleted?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="ResponseHeaderReceived" /> 事件
		/// </summary>
		protected virtual void OnResponseHeaderReceived()
		{
			var handler = ResponseHeaderReceived;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 引发 <see cref="ResponseReadProgressChanged" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnResponseReadProgressChanged(DataProgressEventArgs ea)
		{
			var handler = ResponseReadProgressChanged;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 引发 <see cref="ResponseStreamFetched" /> 事件
		/// </summary>
		protected virtual void OnResponseStreamFetched()
		{
			var handler = ResponseStreamFetched;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// 手动何止错误，用于拦截响应.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="completeRequest">是否直接结束请求</param>
		protected virtual void SetException(Exception ex, bool completeRequest = true)
		{
			if (ex == null)
				throw new ArgumentNullException("ex", "ex is null.");

			Exception = ex;
#if NET_GT_4
			Cancelled = ex is TaskCanceledException || ex is OperationCanceledException;
#else
			Cancelled = ex is OperationCanceledException;
#endif

			if (ex is WebException)
			{
				ExceptionStatus = (ex as WebException).Status;
			}

			if (completeRequest)
				CompleteRequest();
		}

		/// <summary>
		/// 设置当前HTTP请求状态，如果发生变更则引发 <see cref="StateChanged"/> 并返回true
		/// </summary>
		/// <param name="state"></param>
		protected virtual bool SetReadyState(HttpContextState state)
		{
			var v = (int)state;
			if (Interlocked.Exchange(ref ReadyStateValue, v) != v)
			{
				OnStateChanged();

				return true;
			}

			return false;
		}

		/// <summary>
		/// 附加监听器
		/// </summary>
		/// <param name="monitor"></param>
		internal void AttachMonitor(HttpMonitor monitor)
		{
			Monitor = monitor;
			MonitorItem = monitor.Register(this);
		}

		/// <summary>
		/// 中断请求
		/// </summary>
		public void Abort()
		{
			if (WebRequest == null || IsFinished)
				return;

			Cancelled = true;
			try
			{
				WebRequest.Abort();
				if (WebResponse != null)
					WebResponse.Close();
			}
			catch (Exception)
			{

			}
		}

		/// <summary>
		/// 发送请求并做响应
		/// </summary>
		public HttpContext Send()
		{
			if (!SendDelay.HasValue)
			{
				if (!Request.Async)
				{
					SendEntryPoint();
				}
				else
				{
					//capture context
					if (_operation == null)
					{
						if (Request.Async && CaptureContext)
							_operation = AsyncOperationManager.CreateOperation(null);
						else
							_operation = null;
					}

					ThreadPool.QueueUserWorkItem(_ =>
					{
						SendEntryPoint();
					}, null);
				}
			}
			else
			{
				if (!Request.Async)
				{
					Thread.Sleep(SendDelay.Value);
					SendEntryPoint();
				}
				else
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						Thread.Sleep(SendDelay.Value);
						SendEntryPoint();
					}, null);
				}
			}


			return this;
		}

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <returns></returns>
		public DeferredSource<HttpContext> SendAsPromise(bool captureContext = true)
		{
			if (ReadyStateValue != 0)
				throw new HttpClientException("httpcontext_ex_multiplecall", "SendAsPromise()");

			var def = new Deferred<HttpContext>(captureContext);
			RequestFinished += (s, e) => def.Resolve(this);
			RequestFailed += (s, e) => def.Reject(Exception);
			Request.Async = true;
			CaptureContext = captureContext;
			ThreadPool.QueueUserWorkItem(_ => Send(), this);

			return def.Promise();
		}

		/// <summary>
		/// 设置响应发送类型
		/// </summary>
		/// <param name="contentType"></param>
		public void SetRequestContentType(string contentType)
		{
			RequestContent?.SetContentType(contentType);
		}

		/// <summary>
		/// 设置响应发送类型
		/// </summary>
		/// <param name="contentType"></param>
		public void SetRequestContentType(ContentType contentType)
		{
			if (RequestContent != null)
			{
				RequestContent.ContentType = contentType;
			}
		}

		/// <summary>
		/// 获得或设置是否默认启动速度计数器
		/// <para>仅在请求未发送的时候有效</para>
		/// </summary>
		public bool AutoStartSpeedMonitor { get; set; }

		/// <summary>
		/// 获得操作是否已经被取消
		/// </summary>
		public bool Cancelled { get; private set; }


		/// <summary>
		/// 获得或设置是否捕捉线程上下文（异步模式）
		/// </summary>
		public bool CaptureContext
		{
			get { return _captureContext; }
			set
			{
				if (IsSended)
					throw new InvalidOperationException();

				_captureContext = value;
			}
		}

		/// <summary>
		/// 获得当前的客户端
		/// </summary>
		public HttpClient Client { get; private set; }

		/// <summary>
		/// 当前的连接信息
		/// </summary>
		public ConnectionInfo ConnectionInfo { get; private set; }

		/// <summary>
		/// 获得或设置用于保存上下文环境的数据
		/// </summary>
		public Dictionary<string, object> ContextData
		{
			get { return _contextData ?? (_contextData = new Dictionary<string, object>()); }
			set { _contextData = value; }
		}

		/// <summary>
		/// 与请求相关的终端信息
		/// </summary>
		public IEndPointInfo EndPointInfo { get; } = new EndPointInfo();

		/// <summary>
		/// 获得或设置关联的异常
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		/// 获得请求的错误状态
		/// </summary>
		public WebExceptionStatus? ExceptionStatus { get; protected set; }

		/// <summary>
		/// 获得当前的请求是否已经完成
		/// </summary>
		public bool IsFinished { get; private set; }

		/// <summary>
		/// 获得当前的请求是否重定向
		/// </summary>
		public bool IsRedirection => Redirection != null;

		/// <summary>
		/// 获得当前的请求是否已经发送
		/// </summary>
		public bool IsSended
		{
			get { return ReadyStateValue > 0; }
		}

		/// <summary>
		/// 获得是否成功
		/// </summary>
		public bool IsSuccess
		{
			get
			{
				return ResponseContent != null && Exception == null && ((int)Response.Status < 400 || Response.Status == HttpStatusCode.RequestedRangeNotSatisfiable);
			}
		}

		/// <summary>
		/// 获得或设置JSON反序列化设置
		/// </summary>
		public JsonDeserializationSetting JsonDeserializationSetting { get; set; }


		/// <summary>
		/// 获得或设置JSON序列化设置
		/// </summary>
		public JsonSerializationSetting JsonSerializationSetting { get; set; }
		/// <summary>
		/// 获得附加的监听器
		/// </summary>
		public HttpMonitor Monitor { get; private set; }

		/// <summary>
		/// 获得附加的监听源
		/// </summary>
		public HttpMonitorItem MonitorItem { get; private set; }

		/// <summary>
		/// 获得异步操作的引用
		/// </summary>
		public AsyncOperation Operation => _operation;

		/// <summary>
		/// 获得当前请求的性能对象
		/// </summary>
		public HttpPerformance Performance
		{
			get { return _performance; }
			private set
			{
				if (value == null || _performance == value)
					return;

				_performance = value;
				OnPerformanceObjectCreated();
			}
		}

		/// <summary>
		/// 获得就绪状态
		/// </summary>
		public HttpContextState ReadyState
		{
			get { return (HttpContextState)ReadyStateValue; }
		}

		/// <summary>
		/// 获得当前的重定向
		/// </summary>
		public HttpRedirection Redirection
		{
			get
			{
				if (!IsSended || !IsFinished)
				{
					return null;
				}
				if (Response == null)
				{
					return null;
				}

				return Response.Redirection;
			}
		}

		/// <summary>
		/// 获得请求信息
		/// </summary>
		public HttpRequestMessage Request { get; private set; }


		/// <summary>
		/// 获得请求内容
		/// </summary>
		public HttpRequestContent RequestContent
		{
			get { return Request.SelectValue(s => s.RequestData); }
		}

		/// <summary>
		/// 获得或设置当前请求是否需要重新发送。仅在请求结束的时候有效。
		/// 如果设置为true，则请求将会被重新发送。
		/// </summary>
		public bool RequsetResubmit
		{
			get { return _requsetResubmit > 0; }
			set { _requsetResubmit = value ? 1 : 0; }
		}

		/// <summary>
		/// 获得响应信息
		/// </summary>
		public HttpResponseMessage Response { get; private set; }

		/// <summary>
		/// 获得实际响应内容
		/// </summary>
		public HttpResponseContent ResponseContent
		{
			get { return Response.SelectValue(s => s.Content); }
		}

		/// <summary>
		/// 获得或设置发送之前等待的时间
		/// </summary>
		public TimeSpan? SendDelay { get; set; }

		/// <summary>
		/// 获得或设置状态对象
		/// </summary>
		public object State { get; set; }

		/// <summary>
		/// 获得当前响应的状态码
		/// </summary>
		public HttpStatusCode? Status => Response?.Status;

		/// <summary>
		/// 获得请求
		/// </summary>
		public HttpWebRequest WebRequest { get; private set; }

		/// <summary>
		/// 获得响应
		/// </summary>
		public HttpWebResponse WebResponse { get; private set; }

#if NET_GT_4
		protected CancellationToken? CancellationToken;
#endif


#if NET_GT_4

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <returns></returns>
		public Task<HttpResponseContent> SendAsync()
		{
			return SendAsync(new CancellationToken());
		}

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <returns></returns>
		public Task<HttpResponseContent> SendAsync(CancellationToken cancellationToken)
		{
			if (ReadyStateValue != 0)
				throw new HttpClientException("httpcontext_ex_multiplecall", "SendAsync()");

			var tlc = new TaskCompletionSource<HttpResponseContent>();
			RequestFinished += (s, e) => tlc.SetResult(ResponseContent);
			RequestFailed += (s, e) =>
			{
				if (HttpSetting.TreatWebErrorAsTaskFail)
				{
					if (Cancelled)
					{
						tlc.SetCanceled();
					}
					else
						tlc.SetException(Exception ?? TaskFailedException.Create(this));
				}
				else
				{
					tlc.SetResult(null);
				}
			};
			Request.Async = true;
			CaptureContext = false;

			cancellationToken.Register(() => CheckCancellation());
			CancellationToken = cancellationToken;
			ThreadPool.QueueUserWorkItem(_ => Send(), this);

			return tlc.Task;
		}
#endif


		#region 数据交互和异步处理

		/// <summary>
		/// 写入请求数据
		/// </summary>
		protected virtual void WriteRequestData()
		{
			if (CheckCancellation() || Exception != null)
				return;

			if (Request.Async)
			{
				var result = WebRequest.BeginGetRequestStream(_ => WriteRequestData(() =>
				  {
					  TransportContext context;
					  var stream = WebRequest.EndGetRequestStream(_, out context);
					  Request.TransportContext = context;

					  return stream;
				  }), this);

				if (Request.Timeout.HasValue)
					ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), this, Request.Timeout.Value, true);
			}
			else
			{
				WriteRequestData(() =>
				{
					TransportContext context;
					var stream = WebRequest.GetRequestStream(out context);
					Request.TransportContext = context;

					return stream;
				});
			}
		}

		/// <summary>
		/// 写入请求数据回调
		/// </summary>
		protected virtual void WriteRequestData(Func<Stream> getRequestStreamAction)
		{
			if (CheckCancellation() || CheckException())
				return;

			Stream stream;
			HttpStreamWrapper embedStream;
			try
			{
				var rawStream = getRequestStreamAction();
				ConnectionInfo.SetRequest(WebRequest, null);
				ConnectionInfo.SetStream(true, rawStream);
				stream = Client.HttpHandler.DecorateRequestStream(this, rawStream);
				embedStream = new HttpStreamWrapper(stream, stream.CanSeek ? stream.Length : WebRequest.ContentLength);
			}
			catch (Exception ex)
			{
				SetException(ex);
				return;
			}

			MonitorItem?.SetRequestStream(embedStream);

			ChangeReadyState(HttpContextState.SendingRequestHeader, HttpContextState.WriteRequestData);
			Performance.GetRequestStreamTime = DateTime.Now;
			embedStream.ProgressChanged += (s, e) =>
			{
				Performance.RequestLengthSended = e.BytesPassed;
				if (_operation != null)
					_operation.Post(_1 => OnRequestDataSendProgressChanged(e), null);
				else
					OnRequestDataSendProgressChanged(e);
			};
			if (_operation != null)
			{
				_operation.Post(s => OnRequestStreamFetched(), null);
				_operation.Post(s => OnRequestDataSending(), null);
			}
			else
			{
				OnRequestStreamFetched();
				OnRequestDataSended();
			}

			if (Request.Async)
			{
				var args = new AsyncStreamProcessData(embedStream, this, _ =>
				{
					try
					{
						stream.Close();
					}
					catch (Exception ex)
					{
						SetException(ex);
						return;
					}
					if (_.Exception != null)
					{
						SetException(_.Exception);
						return;
					}

					if (_operation == null)
						OnRequestDataSended();
					else
						_operation.Post(s => OnRequestDataSended(), null);
					FlushRequestData();
				});
				Request.RequestData.WriteToAsync(args);
			}
			else
			{
				try
				{
					Request.RequestData.WriteTo(embedStream);
					embedStream.Close();
				}
				catch (Exception ex)
				{
					SetException(ex);
					return;
				}

				if (_operation == null)
					OnRequestDataSended();
				else
					_operation.Post(s => OnRequestDataSended(), null);
				FlushRequestData();
			}
		}

		private static void TimeoutCallback(object state, bool timedOut)
		{
			if (timedOut)
			{
				var context = state as HttpContext;
				var request = context.WebRequest;

				request?.Abort();
			}
		}

		protected virtual void FlushRequestData()
		{
			OnRequestSended();
			Performance.CompleteRequestStreamTime = DateTime.Now;
			if (!ChangeReadyState(HttpContextState.WriteRequestData, HttpContextState.WaitingResponseHeader))
			{
				//如果上一步不是发送数据，则可能是因为没有要发送的数据直接过来的。
				ChangeReadyState(HttpContextState.SendingRequestHeader, HttpContextState.WriteRequestData);
				ChangeReadyState(HttpContextState.WriteRequestData, HttpContextState.WaitingResponseHeader);
			}

			if (CheckCancellation() || CheckException())
				return;

			if (Request.Async)
			{
				var result = WebRequest.BeginGetResponse(s => GetResponseCallback(() => { WebResponse = WebRequest.EndGetResponse(s) as HttpWebResponse; }), this);
				if (Request.Timeout.HasValue)
					ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), this, Request.Timeout.Value, true);
			}
			else
			{
				GetResponseCallback(() => { WebResponse = WebRequest.GetResponse() as HttpWebResponse; });
			}
		}


		protected virtual void GetResponseCallback([NotNull] Action getResponseAction)
		{
			try
			{
				getResponseAction();

				if (WebResponse == null)
					throw new Exception("Unable to get server response.");
			}
			catch (WebException webex) when (webex.Status == WebExceptionStatus.ProtocolError)
			{
				WebResponse = (HttpWebResponse)webex.Response;

				//有断点续传的时候，416错误不当作响应。
				if (WebResponse.StatusCode != HttpStatusCode.RequestedRangeNotSatisfiable)
				{
					//发生错误，清掉期待类型以便于猜测
					Request.ExceptType = null;
					Response = new HttpResponseMessage(WebResponse);
				}
			}
			catch (Exception ex)
			{
				WebRequest.Abort();
				SetException(ex);
				return;
			}
			ConnectionInfo.SetRequest(WebRequest, WebResponse);

			try
			{
				OnPreviewResponseHeader();
				OnValidateResponseHeader();
				Client.HttpHandler.ValidateResponse(this);
			}
			catch (Exception ex)
			{
				WebRequest.Abort();
				SetException(ex);
				return;
			}
			if (Response == null)
				Response = new HttpResponseMessage(WebResponse);

			if (_operation == null) OnResponseHeaderReceived();
			else
				_operation.Post(_ => OnResponseHeaderReceived(), null);

			Performance.GotResponseTime = DateTime.Now;

			if (CheckCancellation() || CheckException())
				return;

			//保持环境
			Client.Setting.LastUri = WebResponse.ResponseUri;

			var uri = WebRequest.RequestUri;
#if NET4
			if (!string.IsNullOrEmpty(WebRequest.Host))
			{
				uri = HttpUtility.ChangeHost(uri, WebRequest.Host);
			}
#endif
			if (WebResponse.ResponseUri != uri)
				Response.Redirection = new HttpRedirection(uri, WebResponse.ResponseUri);
			else if (WebResponse.IsRedirectHttpWebResponse())
			{
				var location = Response.Location;
				if (location.IsNullOrEmpty())
				{
					SetException(new ProtocolViolationException("是重定向的请求, 但是未提供新地址"));
					return;
				}
				Response.Redirection = new HttpRedirection(uri, new Uri(WebRequest.Address, location));
			}

			//保存Cookies状态
			if (Response.Cookies != null && Client.CookieContainer != null)
			{
				if (!Client.ProcessCookies(this) && Request.CookiesHandleMethod == CookiesHandleMethod.Auto)
				{
					if (Client.Setting.UseNonstandardCookieParser)
					{
						var header = Response.Headers.GetValues("Set-Cookie");
						if (header != null)
						{
							var headerCopy = new List<string>();
							foreach (var h in header)
							{
								if (headerCopy.Count == 0)
								{
									headerCopy.Add(h);
									continue;
								}
								if (!Regex.IsMatch(h, @"^[a-z\d-_]+=", RegexOptions.IgnoreCase))
								{
									headerCopy[headerCopy.Count - 1] += ", " + h;
								}
								else
								{
									headerCopy.Add(h);
								}
							}

							foreach (var h in headerCopy)
							{
								//干掉逗号
								var commaIndex = h.IndexOf(";");
								var fh = h.Substring(0, commaIndex).Replace(",", "%2C") + h.Substring(commaIndex);

								Client.CookieContainer.SetCookies(Response.ResponseUri, fh);
							}
						}
					}
					//else
					//{
					//	Client.CookieContainer.Add(Response.ResponseUri, Response.Cookies);
					//}
				}
			}

			//处理响应
			OnDetectResponseContentType();
			if (Request.ExceptType == null)
			{
				//没有设置优选，先确定内容
				Response.Content = GetPreferedResponseContent();
			}
			else
				Response.Content = Request.ExceptType;
			Response.Content.Initialize();
			OnResponseContentObjectIntialized();

			//获得响应流
			Stream responseStream;
			try
			{
				var responseStreamRaw = WebResponse.GetResponseStream();
				ConnectionInfo.SetStream(false, responseStreamRaw);
				responseStream = Client.HttpHandler.DecorateRawResponseStream(this, responseStreamRaw);
			}
			catch (Exception ex)
			{
				WebResponse.Close();
				SetException(ex);
				return;
			}
			if (CheckCancellation() || CheckException())
				return;

			//检测不支持断点续传的情况
			if (Request.Range != null && WebResponse.StatusCode != HttpStatusCode.PartialContent)
			{
				Request.Range = null;
			}

			//创建wrapper
			Performance.ResponseLength = responseStream.CanSeek ? responseStream.Length : Response.ContentLength;
			responseStream = new HttpStreamWrapper(responseStream, Performance.ResponseLength);

			MonitorItem?.SetRawResponseStream((HttpStreamWrapper)responseStream);
			(responseStream as HttpStreamWrapper).ProgressChanged += (s, e) =>
			{
				Performance.ResponseLengthProcessed = e.BytesPassed;
				if (_operation == null)
					OnResponseReadProgressChanged(e);
				else
					_operation.Post(_ => OnResponseReadProgressChanged(e), null);
			};
			if (_operation == null)
				OnResponseStreamFetched();
			else
				_operation.Post(_ => OnResponseStreamFetched(), null);
			if (MonitorItem != null)
			{
				MonitorItem.SetResponseStream((HttpStreamWrapper)responseStream);
			}

			//解压缩
			try
			{
				if (Request.AutoDecompressGzip)
				{
					if (Response.ContentEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) != -1)
					{
						Response.DecompressionMethod = DecompressionMethods.GZip;
						responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
					}
					else if (Response.ContentEncoding.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) != -1)
					{
						Response.DecompressionMethod = DecompressionMethods.Deflate;
						responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
					}
				}

				responseStream = Client.HttpHandler.DecorateResponseStream(this, responseStream);
			}
			catch (Exception ex)
			{
				SetException(ex, true);
				return;
			}
			ChangeReadyState(HttpContextState.WaitingResponseHeader, HttpContextState.ReadingResponse);
			Performance.GotResponseStreamTime = DateTime.Now;

			//同步或异步处理响应？
			if (Request.Async)
			{
				var args = new AsyncStreamProcessData(responseStream, this, _ =>
				{
					try
					{
						responseStream.Close();
					}
					catch (Exception ex)
					{
						SetException(ex);
						return;
					}
					finally
					{
						WebResponse.Close();
					}
					if (_.Exception != null)
					{
						SetException(_.Exception);
						return;
					}

					ResponseStreamProcessComplete();
				});
				ResponseContent.InternalProcessResponseAsync(args);
			}
			else
			{
				//读取响应
				try
				{
					ResponseContent.InternalProcessResponse(responseStream);
				}
				catch (Exception ex)
				{
					SetException(ex);
					return;
				}
				finally
				{
					responseStream.Close();
					WebResponse.Close();
				}

				ResponseStreamProcessComplete();
			}
		}

		protected virtual void ResponseStreamProcessComplete()
		{
			if (CheckCancellation() || CheckException())
				return;

			Performance.ReadResponseFinished = DateTime.Now;
			ChangeReadyState(HttpContextState.ReadingResponse, HttpContextState.ValidatingResponse);
			OnResponseDataReceiveCompleted();

			try
			{
				GlobalEvents.OnRequestValidateResponse(this);
				OnRequestValidateResponse();
			}
			catch (Exception ex)
			{
				SetException(ex);
				return;
			}
			finally
			{
				WebResponse.Close();
			}

			//跳转?
			if (Response.Redirection != null)
			{
				if (_operation == null)
					OnRequestRedirect();
				else
					_operation.Post(_ => OnRequestRedirect(), null);
			}
			Performance.FinishResponseTime = DateTime.Now;
			ChangeReadyState(HttpContextState.ValidatingResponse, HttpContextState.EndProcessResponse);

			CompleteRequest();
		}

		/// <summary>
		/// 完成请求处理
		/// </summary>
		protected virtual void CompleteRequest()
		{

			if (!SetReadyState(HttpContextState.Complete))
				return;

			//完成
			Performance.EndTime = DateTime.Now;

			//重新发送
			if (Interlocked.Exchange(ref _requsetResubmit, 0) == 1)
			{
				OnRequestResubmit();
				Request.ExceptType?.Reset();
				WebResponse = null;
				Response = null;
				SetReadyState(HttpContextState.NotSended);
				Send();

				return;
			}

			if (!IsSuccess)
			{
				InternalOnRequestFailed();
			}
			else
			{
				InternalOnRequestFinished();
			}

			var we = new WebEventArgs(this);
			Client.OnAfterRequest(we);
		}

		#endregion

		#region 事件

		/// <summary>
		/// 当前请求被重新发送
		/// </summary>
		public event EventHandler RequestResubmit;


		/// <summary>
		/// 状态发生变化
		/// </summary>
		public event EventHandler StateChanged;

		/// <summary>
		/// 引发 <see cref="StateChanged" /> 事件
		/// </summary>
		protected virtual void OnStateChanged()
		{
			if (_operation == null)
			{
				var handler = StateChanged;
				if (handler != null)
					handler(this, EventArgs.Empty);
			}
			else
			{
				_operation.Post(_ =>
				{
					var handler = StateChanged;
					if (handler != null)
						handler(this, EventArgs.Empty);
				}, null);
			}
		}


		/// <summary>
		/// 正在准备发送
		/// </summary>
		public event EventHandler RequestSending;

		/// <summary>
		/// 请求已经发送，正在等待写入请求数据或等待响应流
		/// </summary>
		public event EventHandler RequestSended;

		/// <summary>
		/// 已经获得请求流
		/// </summary>
		public event EventHandler RequestStreamFetched;

		/// <summary>
		/// 正在发送请求数据
		/// </summary>
		public event EventHandler RequestDataSending;

		/// <summary>
		/// 请求数据发送进度变化
		/// </summary>
		public event EventHandler<DataProgressEventArgs> RequestDataSendProgressChanged;

		/// <summary>
		/// 请求数据已经发送
		/// </summary>

		public event EventHandler RequestDataSended;

		/// <summary>
		/// 已经收到响应
		/// </summary>
		public event EventHandler ResponseHeaderReceived;

		/// <summary>
		/// 已经获得响应流
		/// </summary>
		public event EventHandler ResponseStreamFetched;

		/// <summary>
		/// 响应数据读取已经完成
		/// </summary>
		public event EventHandler ResponseDataReceiveCompleted;

		/// <summary>
		/// 响应读取进度变更
		/// </summary>
		public event EventHandler<DataProgressEventArgs> ResponseReadProgressChanged;

		/// <summary>
		/// 请求已经完成
		/// </summary>

		public event EventHandler RequestFinished;

		/// <summary>
		/// 请求失败
		/// </summary>
		public event EventHandler RequestFailed;

		/// <summary>
		/// 检测到重定向
		/// </summary>
		public event EventHandler RequestRedirect;

		/// <summary>
		/// 请求验证内容
		/// </summary>
		public event EventHandler RequestValidateResponse;

		/// <summary>
		/// 已经收到HTTP响应头，准备处理请求
		/// </summary>
		public event EventHandler PreviewResponseHeader;

		/// <summary>
		/// 引发 <see cref="PreviewResponseHeader"/> 事件
		/// </summary>

		protected virtual void OnPreviewResponseHeader()
		{
			PreviewResponseHeader?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 校验响应头是否正确，如果引发异常，则会导致请求失败。
		/// </summary>
		public event EventHandler ValidateResponseHeader;

		/// <summary>
		/// 引发 <see cref="ValidateResponseHeader"/> 事件
		/// </summary>

		protected virtual void OnValidateResponseHeader()
		{
			ValidateResponseHeader?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// 响应处理请求对象已经创建
		/// </summary>
		public event EventHandler ResponseContentObjectIntialized;

		/// <summary>
		/// 引发 <see cref="ResponseContentObjectIntialized"/> 事件
		/// </summary>

		protected virtual void OnResponseContentObjectIntialized()
		{
			ResponseContentObjectIntialized?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region 辅助函数

		/// <summary>
		/// Json序列化
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string JsonSerialize(object obj)
		{
			if (obj == null)
				return string.Empty;

			if (JsonSerializationSetting == null)
			{
				return JsonConvert.SerializeObject(obj);
			}
			if (JsonSerializationSetting.JsonConverts == null)
			{
				return JsonConvert.SerializeObject(obj, JsonSerializationSetting.Formatting, JsonSerializationSetting.Setting);
			}
			return JsonConvert.SerializeObject(obj, JsonSerializationSetting.Formatting, JsonSerializationSetting.JsonConverts);
		}

		/// <summary>
		/// 反序列化目标对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <param name="originalObj"></param>
		/// <returns></returns>
		public virtual T JsonDeserialize<T>(string result, object originalObj)
		{
			if (originalObj != null && !(originalObj is T))
			{
				originalObj = null;
			}

			if (originalObj == null || JsonDeserializationSetting == null || !JsonDeserializationSetting.KeepOriginalObject)
			{
				if (JsonDeserializationSetting == null)
					return JsonConvert.DeserializeObject<T>(result);
				if (JsonDeserializationSetting.JsonConverts == null)
					return JsonConvert.DeserializeObject<T>(result, JsonDeserializationSetting.Setting);
				return JsonConvert.DeserializeObject<T>(result, JsonDeserializationSetting.JsonConverts);
			}
			else
			{
				if (JsonDeserializationSetting == null)
				{
					JsonConvert.PopulateObject(result, originalObj);
				}
				else
					JsonConvert.PopulateObject(result, originalObj, JsonDeserializationSetting.Setting);

				return (T)originalObj;
			}
		}

		#endregion

		#region Dispose方法实现

		bool _disposed;

		public event EventHandler Disposed;

		/// <summary>
		/// 释放资源
		/// </summary>
		protected virtual void OnDisposed()
		{
			Disposed?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// 销毁对象
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;

			if (disposing)
			{
				WebResponse?.Close();
				WebRequest?.Abort();
				Request?.Dispose();
				Response?.Dispose();
			}
			//释放非托管资源
			OnDisposed();

			//挂起终结器
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 检查是否已经被销毁。如果被销毁，则抛出异常
		/// </summary>
		/// <exception cref="ObjectDisposedException">对象已被销毁</exception>
		protected void CheckDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		~HttpContext()
		{
			Dispose(false);
		}

		#endregion

		/// <summary>
		/// 引发 <see cref="RequestEnd"/> 事件
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRequestEnd()
		{
			RequestEnd?.Invoke(this, new WebEventArgs(this));
		}
	}

	/// <summary>
	/// 强类型的结果
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HttpContext<T> : HttpContext where T : class
	{
		/// <summary>
		/// 创建 <see cref="HttpContext" />  的新实例(HttpContext)
		/// </summary>
		internal HttpContext(HttpClient client, HttpRequestMessage request)
			: base(client, request)
		{
		}

		/// <summary>
		/// 发送请求
		/// </summary>
		/// <returns></returns>
		public new HttpContext<T> Send()
		{
			base.Send();
			return this;
		}

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <returns></returns>
		public new DeferredSource<HttpContext<T>> SendAsPromise(bool captureContext = true)
		{
			if (ReadyStateValue != 0)
				return null;

			var def = new Deferred<HttpContext<T>>(captureContext);
			CaptureContext = captureContext;
			RequestFinished += (s, e) => def.Resolve(this);
			RequestFailed += (s, e) => def.Reject(Exception, this);
			Request.Async = true;
			Send();

			return def.Promise();
		}

		/// <summary>
		/// 获得响应的结果
		/// </summary>
		public T Result
		{
			get
			{
				if (ResponseContent == null)
				{
					throw new InvalidOperationException(SR.HttpContext_Result_RequestNotComplete);
				}

				var t = typeof(T);
				object ret;
				if (t == typeof(string))
				{
					CheckResponseType<ResponseBinaryContent>();
					ret = ((ResponseBinaryContent)ResponseContent).StringResult;
				}
				else if (t == typeof(byte[]) || t == typeof(object))
				{
					CheckResponseType<ResponseBinaryContent>();
					ret = ((ResponseBinaryContent)ResponseContent).Result;
				}
				else if (t == typeof(Image))
				{
					CheckResponseType<ResponseImageContent>();
					ret = ((ResponseImageContent)ResponseContent).Image;
				}
				else if (t == typeof(XmlDocument))
				{
					CheckResponseType<ResponseXmlContent>();
					ret = ((ResponseXmlContent)ResponseContent).XmlDocument;
				}
				else if (t == typeof(Stream))
				{
					throw new InvalidOperationException("不可通过此方式操作流结果");
				}
				else if (t == typeof(ResponseFileContent))
				{
					ret = ResponseContent;
				}
				else
				{
					CheckResponseType<ResponseObjectContent<T>>();
					ret = ((ResponseObjectContent<T>)ResponseContent).Object;
				}

				return (T)(object)ret;
			}
		}

		void CheckResponseType<TResult>() where TResult : HttpResponseContent
		{
			if (!(ResponseContent is TResult))
				throw new InvalidOperationException(SR.HttpContext_CheckResponseType_ResponseTypeMismatch);
		}

#if NET_GT_4

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <returns></returns>
		public new Task<T> SendAsync()
		{
			return SendAsync(new CancellationToken());
		}

		/// <summary>
		/// 以任务模式发送请求
		/// </summary>
		/// <param name="cancellationToken">cancellationToken</param>
		/// <returns></returns>
		public new Task<T> SendAsync(CancellationToken cancellationToken)
		{
			if (ReadyStateValue != 0)
				return null;

			var tlc = new TaskCompletionSource<T>();
			RequestFinished += (s, e) => tlc.SetResult(Result);
			RequestFailed += (s, e) =>
			{
				if (HttpSetting.TreatWebErrorAsTaskFail)
				{
					if (Cancelled)
					{
						tlc.SetCanceled();
					}
					else
						tlc.SetException(Exception ?? TaskFailedException.Create(this));
				}
				else
				{
					tlc.SetResult(default(T));
				}
			};
			Request.Async = true;
			CaptureContext = false;

			cancellationToken.Register(() => CheckCancellation());
			CancellationToken = cancellationToken;
			ThreadPool.QueueUserWorkItem(_ => Send(), this);

			return tlc.Task;
		}

#endif

	}
}
