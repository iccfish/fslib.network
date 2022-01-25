using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Security;
#if NET4
using System.Threading.Tasks;
#endif
using System.Xml;

namespace FSLib.Network.Http
{
	using System.Diagnostics;
	using System.Net;

	/// <summary>
	/// 类型 HttpClient
	/// </summary>
	public partial class HttpClient
	{
		/// <summary>
		/// 获得或设置默认的HTTP监控
		/// </summary>
		public static HttpMonitor DefaultMonitor { get; set; }
		static HttpClient()
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
			{
				if (!(sender is HttpWebRequest))
					return sslPolicyErrors == SslPolicyErrors.None;

				var e = new CertificateValidationEventArgs((HttpWebRequest)sender, sslPolicyErrors, chain, certificate);
				OnServerCertificateValidation(sender, e);
				return e.Result;
			};
			CheckCertificateRevocationList = false;
			ServicePointManager.DefaultConnectionLimit = 1024;

#if NET35 || NET40
			// see also: https://stackoverflow.com/questions/33761919/tls-1-2-in-net-framework-4-0
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Ssl3
				| SecurityProtocolType.Tls
				| (SecurityProtocolType)3072 //TLS 1.2
				| (SecurityProtocolType)768; //TLS 1.1;
#endif
		}

		#region 全局设置

		/// <summary>
		/// [全局] 获得或设置是否检查证书吊销列表，等同于 <code>ServicePointManager.CheckCertificateRevocationList</code>
		/// </summary>
		public static bool CheckCertificateRevocationList
		{
			get { return ServicePointManager.CheckCertificateRevocationList; }
			set { ServicePointManager.CheckCertificateRevocationList = value; }
		}

		/// <summary>
		/// 获得或设置最大连接数，等同于 <code>ServicePointManager.DefaultConnectionLimit</code>
		/// </summary>
		public static int DefaultConnectionLimit
		{
			get => ServicePointManager.DefaultConnectionLimit;
			set => ServicePointManager.DefaultConnectionLimit = value;
		}

		#endregion

		#region 证书校验相关

		/// <summary>
		/// 全局：校验服务器证书
		/// </summary>
		public static event EventHandler<CertificateValidationEventArgs> ServerCertificateValidation;

#if !NET_GET_45
		static readonly Dictionary<HttpWebRequest, HttpContext> _contextMap = new Dictionary<HttpWebRequest, HttpContext>();

		internal void AddContextToCertificateValidationMap(HttpContext context)
		{
			if (context?.WebRequest == null)
				return;

			lock (_contextMap)
			{
				if (!_contextMap.ContainsKey(context.WebRequest))
				{
					_contextMap.Add(context.WebRequest, context);
				}
			}

			context.Disposed += (s, e) =>
			{
				var ctx = s as HttpContext;
				lock (_contextMap)
				{
					if (_contextMap.ContainsKey(ctx.WebRequest))
						_contextMap.Remove(ctx.WebRequest);
				}
			};
		}

#endif

		/// <summary>
		/// 引发 <see cref="ServerCertificateValidation"/> 事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void OnServerCertificateValidation(object sender, CertificateValidationEventArgs e)
		{
#if !NET_GET_45
			lock (_contextMap)
			{
				if (_contextMap.ContainsKey(e.Request))
				{
					var ctx = _contextMap[e.Request];
					e.HttpContext = ctx;
					e.Client = ctx.Client;

					ctx.OnServerCertificateValidation(e);
				}
			}
#endif
			ServerCertificateValidation?.Invoke(sender, e);
		}

		#endregion

		/// <summary>
		/// 获得或设置参数
		/// </summary>
		public HttpSetting Setting { get; private set; }

		/// <summary>
		/// 获得当前客户端的监控类
		/// </summary>
		public HttpMonitor Monitor { get; internal set; } = DefaultMonitor;

		/// <summary>
		/// 获得或创建HTTP请求句柄
		/// </summary>
		public IHttpHandler HttpHandler { get; set; }

		/// <summary>
		/// 获得或设置使用的CookiesContainer
		/// </summary>
		public CookieContainer CookieContainer { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpClient" />  的新实例(HttpClient)
		/// </summary>
		public HttpClient(HttpSetting setting = null, IHttpHandler handler = null, CookieContainer cookieContainer = null)
		{
			Setting = setting ?? new HttpSetting();
			CookieContainer = cookieContainer ?? new CookieContainer(4096, 100, 4096);
			HttpHandler = handler ?? new HttpHandler();
		}
		/// <summary>
		/// 向当前客户端中导入Cookies
		/// </summary>
		/// <param name="cookies"></param>
		/// <param name="uri"></param>
		/// <param name="expiresTime">过期时间</param>
		public void ImportCookies(string cookies, Uri uri, DateTime? expiresTime = null)
		{
			CookieContainer?.ImportCookies(cookies, uri, expiresTime);
		}

		/// <summary>
		/// 重置客户端状态，清理诸如Cookies等存储
		/// </summary>
		public void Clear()
		{
			if (CookieContainer != null)
				CookieContainer = new CookieContainer(4096, 100, 4096);
			Setting.LastUri = null;
		}

		/// <summary>
		/// 自定义处理Cookies。仅在启用非标准cookeis解析的时候起效
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual bool ProcessCookies(HttpContext context)
		{
			return HttpHandler.ProcessCookies(context, this);
		}

		/// <summary>
		/// 将要进行地址解析
		/// </summary>
		public event EventHandler<UriResolveEventArgs> ResolveToUri;

		/// <summary>
		/// 将URL字符串分析成URI
		/// </summary>
		/// <param name="url"></param>
		/// <param name="header">当前解析使用的标头。如果为 <see langword="null" />，则为主要地址</param>
		/// <param name="contextData">当前的上下文数据</param>
		/// <returns></returns>
		protected virtual Uri ResolveUri(HttpRequestHeader? header, string url, Dictionary<string, object> contextData)
		{
			var uri = HttpHandler.ResolveUri(header, url, contextData);
			if (uri == null)
			{
				Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri);
			}
			var handler = ResolveToUri;
			if (handler != null)
			{
				var ea = new UriResolveEventArgs(url, header, contextData) { Uri = uri };
				handler(this, ea);
				uri = ea.Uri;
			}

			return uri;
		}

		#region 核心逻辑

		/// <summary>
		/// 创建请求
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public HttpContext Create(HttpRequestMessage message)
		{
			if (message.Encoding == null)
				message.Encoding = Setting.StringEncoding;
			var context = HttpHandler.GetContext(this, message);

			return context;
		}
		

		/// <summary>
		/// 创建网络请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="method">方法</param>
		/// <param name="data">写入的数据</param>
		/// <param name="refer">引用页</param>
		/// <param name="saveToFile">保存文件地址</param>
		/// <param name="streamInvoker">流读取对象，仅当返回结果为流时可用</param>
		/// <param name="async"></param>
		/// <param name="result"></param>
		/// <param name="isXhr"></param>
		/// <param name="contextData">关联的上下文数据</param>
		/// <param name="headers">额外的要发出去的标头</param>
		/// <param name="contentType">设置当发送对象类型时，设置发送类型</param>
		/// <param name="allowAutoRedirect">设置当服务器发送3xx代码时是否自动跟踪跳转</param>
		/// <param name="targetStream">要写入的目标流</param>
		/// <param name="extra">额外的请求数据</param>
		/// <typeparam name="TResult">结果类型</typeparam>
		/// <returns></returns>
		[Obsolete("This method was obsoleted. Please change sencod parameter from type Uri to string.")]
		public HttpContext<TResult> Create<TResult>(
			HttpMethod method,
			Uri uri,
			Uri refer,
			object data = null,
			TResult result = default(TResult),
			string saveToFile = null,
			EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null,
			bool async = false,
			bool? isXhr = null,
			Dictionary<string, object> contextData = null,
			WebHeaderCollection headers = null,
			ContentType? contentType = null,
			bool? allowAutoRedirect = null,
			Stream targetStream = null
			) where TResult : class => Create<TResult>(method, uri, refer?.OriginalString, data, result, saveToFile, streamInvoker, async, isXhr, contextData, headers, contentType, allowAutoRedirect, targetStream);

		/// <summary>
		/// 创建网络请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="method">方法</param>
		/// <param name="data">写入的数据</param>
		/// <param name="refer">引用页</param>
		/// <param name="saveToFile">保存文件地址</param>
		/// <param name="streamInvoker">流读取对象，仅当返回结果为流时可用</param>
		/// <param name="async"></param>
		/// <param name="result"></param>
		/// <param name="isXhr"></param>
		/// <param name="contextData">关联的上下文数据</param>
		/// <param name="headers">额外的要发出去的标头</param>
		/// <param name="contentType">设置当发送对象类型时，设置发送类型</param>
		/// <param name="allowAutoRedirect">设置当服务器发送3xx代码时是否自动跟踪跳转</param>
		/// <param name="targetStream">要写入的目标流</param>
		/// <typeparam name="TResult">结果类型</typeparam>
		/// <returns></returns>
		public HttpContext<TResult> Create<TResult>(
			HttpMethod method,
			Uri uri,
			string refer = null,
			object data = null,
			TResult result = default(TResult),
			string saveToFile = null,
			EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null,
			bool async = false,
			bool? isXhr = null,
			Dictionary<string, object> contextData = null,
			WebHeaderCollection headers = null,
			ContentType? contentType = null,
			bool? allowAutoRedirect = null,
			Stream targetStream = null
			) where TResult : class
			=> Create<TResult>(method.ToString().ToUpper(), uri, refer, data, result, saveToFile, streamInvoker, async, isXhr, contextData, headers, contentType, allowAutoRedirect, targetStream);

		/// <summary>
		/// 创建网络请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="method">方法</param>
		/// <param name="data">写入的数据</param>
		/// <param name="refer">引用页</param>
		/// <param name="saveToFile">保存文件地址</param>
		/// <param name="streamInvoker">流读取对象，仅当返回结果为流时可用</param>
		/// <param name="async"></param>
		/// <param name="result"></param>
		/// <param name="isXhr"></param>
		/// <param name="contextData">关联的上下文数据</param>
		/// <param name="headers">额外的要发出去的标头</param>
		/// <param name="contentType">设置当发送对象类型时，设置发送类型</param>
		/// <param name="allowAutoRedirect">设置当服务器发送3xx代码时是否自动跟踪跳转</param>
		/// <param name="targetStream">要写入的目标流</param>
		/// <typeparam name="TResult">结果类型</typeparam>
		/// <returns></returns>
		public HttpContext<TResult> Create<TResult>(
			string method,
			Uri uri,
			string refer = null,
			object data = null,
			TResult result = default(TResult),
			string saveToFile = null,
			EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null,
			bool async = false,
			bool? isXhr = null,
			Dictionary<string, object> contextData = null,
			WebHeaderCollection headers = null,
			ContentType? contentType = null,
			bool? allowAutoRedirect = null,
			Stream targetStream = null
			) where TResult : class
		{
			if (string.IsNullOrEmpty(method))
				throw new ArgumentException($"{nameof(method)} is null or empty.", nameof(method));
			if (uri == null)
				throw new ArgumentNullException(nameof(uri), $"{nameof(uri)} is null.");

			if (typeof(TResult) == typeof(object))
			{
				throw new InvalidOperationException("context type cannot be object.");
			}

			contextData ??= new Dictionary<string, object>();
			method = method.ToUpper();

			var resultType = typeof(TResult);
			if (streamInvoker != null && typeof(Stream) == resultType)
				throw new InvalidOperationException("非流结果时不可设置流操作");

			var referUri = Setting.ResolveReferUri ? ResolveUri(HttpRequestHeader.Referer, refer, contextData) : null;
			var request = new HttpRequestMessage(uri, method)
			{
				Referer = referUri?.OriginalString ?? refer,
				//自动设置格式
				ExceptType = typeof(TResult),
				ExceptObject = result,
			};
			if (data != null)
			{
				request.RequestPayload = data;
			}
			else if (request.AllowRequestBody)
			{
				request.RequestData = new byte[0];
			}
			request.Async = async;

			var ctx = HttpHandler.GetContext<TResult>(this, request);
			Setting.InitializeHttpContext(ctx);

			if (isXhr != null)
				request.AppendAjaxHeader = isXhr.Value;
			ctx.ContextData = contextData;
			if (headers?.Count > 0)
			{
				if (request.Headers == null)
					request.Headers = headers;
				else
					headers.CopyTo(request.Headers);
			}
			request.AllowAutoRedirect = allowAutoRedirect ?? Setting.AllowAutoDirect;

			return ctx;
		}

		/// <summary>
		/// 创建网络请求
		/// </summary>
		/// <param name="url">请求地址</param>
		/// <param name="method">方法</param>
		/// <param name="data">写入的数据</param>
		/// <param name="refer">引用页</param>
		/// <param name="saveToFile">保存文件地址</param>
		/// <param name="streamInvoker">流读取对象，仅当返回结果为流时可用</param>
		/// <param name="async">是否是异步发送</param>
		/// <param name="result">预先设置结果类型</param>
		/// <param name="isXhr">是否增加AJAX请求头标记</param>
		/// <param name="contextData">关联的上下文数据</param>
		/// <param name="headers">要跟随请求一起发送的HTTP标头</param>
		/// <param name="contentType">设置当发送对象类型时，设置发送类型。不设置或传递null值将会自动判断</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <param name="targetStream">要写入的目标流</param>
		/// <typeparam name="TResult">结果类型</typeparam>
		/// <returns></returns>
		public HttpContext<TResult> Create<TResult>(
			HttpMethod method,
			string url,
			string refer = null,
			object data = null,
			TResult result = default(TResult),
			string saveToFile = null,
			EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null,
			bool async = false,
			bool? isXhr = null,
			Dictionary<string, object> contextData = null,
			WebHeaderCollection headers = null,
			ContentType? contentType = null,
			bool? allowAutoRedirect = null,
			Stream targetStream = null
			) where TResult : class
		{
			contextData = contextData ?? new Dictionary<string, object>();
			var uri = ResolveUri(null, url, contextData);
			if (uri == null)
				return null;

			return Create(method, uri, refer, data, result, saveToFile, streamInvoker, async, isXhr, contextData, headers, contentType, allowAutoRedirect, targetStream);
		}

		/// <summary>
		/// 复制默认设置到对应的HttpContext中，供初始化请求使用
		/// </summary>
		/// <param name="context"></param>
		public virtual void CopyDefaultSettings(HttpContext context)
		{
			context.Request.CookiesHandleMethod = Setting.CookiesHandleMethod;
			if (context.Request.CookiesHandleMethod != CookiesHandleMethod.Ignore && CookieContainer != null)
			{
				if (Setting.UseNonstandardCookieParser || context.Request.CookiesHandleMethod == CookiesHandleMethod.OnlySendWithoutReceive)
				{
					context.WebRequest.Headers.Add(HttpRequestHeader.Cookie, CookieContainer.GetCookieHeader(context.Request.Uri));
				}
				else
				{
					context.WebRequest.CookieContainer = CookieContainer;
				}
			}
			else
			{
				context.WebRequest.CookieContainer = null;
			}

			if (Setting.UseNonstandardCookieParser)
			{
				context.WebRequest.AllowAutoRedirect = false;
			}
		}


		/// <summary>
		/// 准备发送请求
		/// </summary>
		public event EventHandler<WebEventArgs> BeforeRequest;

		/// <summary>
		/// 引发 <see cref="BeforeRequest" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnBeforeRequest(WebEventArgs ea)
		{
			var handler = BeforeRequest;

			Debug.WriteLine("即将发起网络请求，源地址=" + ea.Request.Uri + "，提交数据=" + (ea.Request.RequestData == null ? "(null)" : ea.Request.RequestData.ToString()));

			handler?.Invoke(this, ea);
			GlobalEvents.OnBeforeRequest(this, ea);
		}



		/// <summary>
		/// WEB请求成功
		/// </summary>
		public event EventHandler<WebEventArgs> RequestSuccess;

		/// <summary>
		/// 引发 <see cref="RequestSuccess" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnRequestSuccess(WebEventArgs ea)
		{
			var handler = RequestSuccess;
			handler?.Invoke(this, ea);
			GlobalEvents.OnRequestSuccess(this, ea);
			Debug.WriteLine("网络请求成功，地址=" + ea.Request.Uri);
		}


		/// <summary>
		/// 请求发送失败
		/// </summary>
		public event EventHandler<WebEventArgs> RequestFailed;


		/// <summary>
		/// 引发 <see cref="RequestFailed" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnRequestFailed(WebEventArgs ea)
		{
			var handler = RequestFailed;
			handler?.Invoke(this, ea);
			Debug.WriteLine("网络请求失败，地址=" + ea.Request.Uri);
			GlobalEvents.OnRequestFailed(this, ea);
		}

		/// <summary>
		/// 请求发送取消
		/// </summary>
		public event EventHandler<WebEventArgs> RequestCancelled;


		/// <summary>
		/// 引发 <see cref="RequestFailed" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnRequestCancelled(WebEventArgs ea)
		{
			var handler = RequestCancelled;
			handler?.Invoke(this, ea);
			GlobalEvents.OnRequestCancelled(this, ea);
			Debug.WriteLine("网络请求已取消，地址=" + ea.Request.Uri);
		}


		/// <summary>
		/// 请求发送取消
		/// </summary>
		public event EventHandler<WebEventArgs> RequestEnd;

		/// <summary>
		/// 引发 <see cref="RequestFailed" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnRequestEnd(WebEventArgs ea)
		{
			var handler = RequestEnd;
			GlobalEvents.OnRequestEnd(this, ea);
			handler?.Invoke(this, ea);
			Debug.WriteLine("网络请求已完成，地址=" + ea.Request.Uri);
		}

		/// <summary>
		/// HTTP会话被创建
		/// </summary>
		public event EventHandler<WebEventArgs> HttpContextCreated;

		/// <summary>
		/// 引发 <see cref="HttpContextCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnHttpContextCreated(WebEventArgs ea)
		{
			var handler = HttpContextCreated;
			handler?.Invoke(this, ea);
			GlobalEvents.OnHttpContextCreated(this, ea);
		}

		///// <summary>
		///// 最多重试次数
		///// </summary>
		//public int MaxiumRetryCount { get; set; }

		#endregion
	}
}
