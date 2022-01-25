using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using FSLib.Extension;

namespace FSLib.Network.Http
{
	using System.IO;
	using System.Text.RegularExpressions;

	/// <summary>
	/// HTTP请求信息
	/// </summary>
	public class HttpRequestMessage : IDisposable
	{
		WebHeaderCollection _headers;

		private bool _proxySet;

		/// <summary>
		///
		/// </summary>
		public HttpRequestMessage()
			: this(null, "Get")
		{
		}

		/// <summary>
		/// 创建 <see cref="HttpRequestMessage" />  的新实例(HttpRequestMessage)
		/// </summary>
		public HttpRequestMessage(Uri uri, string method)
		{
			Uri = uri;
			Method = method;
			AllowAutoRedirect = true;
			AutoDecompressGzip = true;
			Headers = new WebHeaderCollection();
		}


		/// <summary>
		/// 请求初始化 <see cref="HttpWebRequest"/> 的结束操作
		/// </summary>
		public event EventHandler<PostInitRequestEventArgs> PostInitRequest;

		/// <summary>
		/// 引发 <see cref="PostInitRequest" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnPostInitRequest(PostInitRequestEventArgs ea)
		{
			var handler = PostInitRequest;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 获得上下文环境
		/// </summary>
		protected internal HttpContext Context { get; set; }

		/// <summary>
		/// 获得或设置请求载荷的格式化形式
		/// </summary>
		public ContentType? ContentType { get; set; }

		/// <summary>
		/// 初始化请求
		/// </summary>
		/// <param name="context"></param>
		internal void InitializeWebRequest([NotNull] HttpContext context)
		{
			var request = context.WebRequest;
			if (!Accept.IsNullOrEmpty())
				request.Accept = Accept;
			if (!UserAgent.IsNullOrEmpty())
				request.UserAgent = UserAgent;
			if (!AcceptEncoding.IsNullOrEmpty())
				request.Headers.Add(HttpRequestHeader.AcceptEncoding, AcceptEncoding);
			request.ServicePoint.Expect100Continue = false;
			request.KeepAlive = KeepAlive;
			request.ServicePoint.UseNagleAlgorithm = UseNagleAlgorithm;
			request.AllowWriteStreamBuffering = AllowWriteStreamBuffering;
			Authorization?.SetRequest(context.WebRequest, context);

			if (Range != null)
			{
				var range = Range.Value;
#if NET_GT_4
				if (range.Value.HasValue)
					request.AddRange(range.Key, range.Value.Value);
				else request.AddRange(range.Key);
#else
				if (range.Value.HasValue)
					request.AddRange(range.Key, range.Value.Value);
				else request.AddRange(range.Key);
#endif
			}

			if (AppendAjaxHeader)
				request.Headers.Add("X-Requested-With", "XMLHttpRequest");

			request.KeepAlive = KeepAlive;

			if (Timeout.HasValue)
				request.Timeout = Timeout.Value;
			if (ReadWriteTimeout.HasValue)
				request.ReadWriteTimeout = ReadWriteTimeout.Value;
			if (!TransferEncoding.IsNullOrEmpty())
			{
				request.TransferEncoding = TransferEncoding;
				request.SendChunked = true;
			}

#if NET_GT_4 || NET5_0_OR_GREATER
			if (!string.IsNullOrEmpty(Host))
			{
				request.Host = Host;
			}
#endif
			request.AllowAutoRedirect = AllowAutoRedirect;
			request.AutomaticDecompression = DecompressionMethods.None;
#pragma warning disable 618
			if (ReferUri != null)
				request.Referer = ReferUri.OriginalString;
#pragma warning restore 618
			if (!Referer.IsNullOrEmpty())
				request.Referer = Referer;

			if (HttpRequestCacheLevel != null)
				request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Value);
			else if (HttpCacheAgeControl != null)
				request.CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.Value, MaxAge ?? TimeSpan.Zero, AgeOrFreshOrStale ?? TimeSpan.Zero, SyncTime ?? DateTime.Now);
			if (IfModifiedSince != null)
				request.IfModifiedSince = IfModifiedSince.Value;

			//copy headers
			if (Headers != null)
			{
				foreach (var header in Headers.AllKeys)
					request.Headers[header] = Headers[header];
			}

			if (_proxySet)
				request.Proxy = _webProxy;
			request.PreAuthenticate = PreAuthenticate;

			//初始化兼容参数
			if (request.Proxy != null)
			{
				if (Context.Client.Setting.ForceStreamBufferWithProxy)
				{
					//如果有代理，则默认允许流缓冲。如果不允许，则很可能会导致代理身份验证失败。
					request.AllowWriteStreamBuffering = true;
				}
				if (request.Proxy is WebProxy)
				{
					var proxy = request.Proxy as WebProxy;
					if (!proxy.UseDefaultCredentials && proxy.Credentials is NetworkCredential && request.Headers["Proxy-Authorization"].IsNullOrEmpty())
					{
						var cred = proxy.Credentials as NetworkCredential;
						request.Headers.Set(HttpRequestHeader.ProxyAuthorization, "Basic " + Convert.ToBase64String(Encoding.GetBytes(cred.UserName + ":" + cred.Password)));
					}
				}
			}

			//如果设置了证书，则添加
			if (X509Certificates?.Length > 0)
			{
				request.ClientCertificates.AddRange(X509Certificates);
			}

			OnPostInitRequest(new PostInitRequestEventArgs(request, context, context.Client));
		}


		/// <summary>
		/// 格式化信息
		/// </summary>
		internal void Normalize(HttpClient client, HttpContext context)
		{
			Client = client;
			Context = context;

			if (RequestData != null)
			{
				if (context.Request.ContentTypeOverride.IsNullOrEmpty() == false)
				{
					RequestData?.SetContentType(context.Request.ContentTypeOverride);
				}

				RequestData.BindContext(client, context, this);
				RequestData.PrepareData();

				//构建URL
				if (client.Setting.EnableUrlTokenIdentifier && RequestData is RequestFormDataContent)
				{
					var formData = (RequestFormDataContent)RequestData;
					var uri = Uri.OriginalString;

					if (Regex.IsMatch(uri, @"{[a-z\d-_$\.\[\]]+}", RegexOptions.IgnoreCase))
					{
						var fields = formData.ProcessedData;
						uri = Regex.Replace(uri, @"{([a-z\d-_$\.\[\]]+)}", _ =>
						{
							string value;
							var key = _.GetGroupValue(1);
							if (fields.TryRemove(key, out value))
							{
								formData.StringField.Remove(key);
								return value;
							}

							return _.Value;
						}, RegexOptions.IgnoreCase
							);

						Uri = new Uri(uri);
					}
				}

				if (!AllowRequestBody)
				{
					//有查询数据，序列化到查询中。
					var queryExtra = RequestData.SerializeAsQueryString();
					if (!queryExtra.IsNullOrEmpty())
					{
						var query = Uri.Query;
						if (!query.IsNullOrEmpty())
							query += "&" + queryExtra;
						else
							query = "?" + queryExtra;
						Uri = new Uri(Uri, query);
					}
				}
			}
		}

		/// <summary>
		/// 获得或设置接受类型
		/// </summary>
		public string Accept { get; set; }


		/// <summary>
		/// 获得或设置当前HTTP协议的接受编码类型
		/// </summary>
		public string AcceptEncoding { get; set; }

		/// <summary>
		/// 设置ageOrFreshOrSteal....很麻烦的东西，参阅手册
		/// </summary>
		public TimeSpan? AgeOrFreshOrStale { get; set; }

		/// <summary>
		/// 获得或设置是否允许自动重定向请求(HTTP 302-Found)
		/// </summary>
		public bool AllowAutoRedirect { get; set; }

		/// <summary>
		/// 获得当前请求是否可以附加主体数据
		/// </summary>
		public bool AllowRequestBody => _allowBodyMethods.Contains(Method);

		/// <summary>
		/// 是否缓冲写数据
		/// </summary>
		public bool AllowWriteStreamBuffering { get; set; }

		/// <summary>
		/// 获得或设置是否在请求中添加Ajax的标记
		/// </summary>
		public bool AppendAjaxHeader { get; set; }

		/// <summary>
		/// 获得或设置是否是异步请求
		/// </summary>
		public bool Async { get; set; }

		/// <summary>
		/// 获得或设置授权
		/// </summary>
		public IAuthorization Authorization { get; set; }

		/// <summary>
		/// 获得或设置是否自动解压缩GZIP的响应数据-仅HTTP请求有效
		/// </summary>
		public bool AutoDecompressGzip { get; set; }

		/// <summary>
		/// 获得客户端
		/// </summary>
		public HttpClient Client { get; private set; }

		/// <summary>
		/// 获得或设置cookeis处理逻辑
		/// </summary>
		public CookiesHandleMethod CookiesHandleMethod { get; set; }

		/// <summary>
		/// 获得或设置是否随请求发送DNT标头
		/// </summary>
		public bool Dnt
		{
			get { return Headers["DNT"] == "1"; }
			set
			{
				if (value)
				{
					Headers.Add("DNT", "1");
				}
				else
				{
					Headers.Remove("DNT");
				}
			}
		}

		/// <summary>
		/// 获得或设置编码
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// 获得或设置期望的结果类型。如果没有设置，将会根据响应类型返回默认的类型
		/// </summary>
		public Type ExceptType { get; set; }

		/// <summary>
		/// 获得或设置期望的数据结果
		/// </summary>
		public object ExceptObject { get; set; }

		/// <summary>
		/// 获得或设置是否强制对使用代理的情况进行流缓冲。如果禁止，在某些特定情况下可能会失败并引发异常。
		/// </summary>
		public bool ForceStreamBufferWithProxy { get; set; }

		/// <summary>
		/// 对应HTTP请求的请求标头
		/// </summary>
		public WebHeaderCollection Headers
		{
			get { return _headers ?? (_headers = new WebHeaderCollection()); }
			set { _headers = value; }
		}

		/// <summary>
		/// 获得或设置缓存策略
		/// </summary>
		public HttpCacheAgeControl? HttpCacheAgeControl { get; set; }

		/// <summary>
		/// 获得或设置缓存标记
		/// </summary>
		public HttpRequestCacheLevel? HttpRequestCacheLevel { get; set; }

		/// <summary>
		/// 获得或设置当前请求的日期判断
		/// </summary>
		public DateTime? IfModifiedSince { get; set; }

		/// <summary>
		/// 获得或设置是否保持活动连接
		/// </summary>
		public bool KeepAlive { get; set; }
		/// <summary>
		/// 本地IP地址终端
		/// </summary>
		public IPEndPoint LocalIpEndPoint { get; set; }

		/// <summary>
		/// 获得或设置最大生命周期
		/// </summary>
		public TimeSpan? MaxAge { get; set; }

		/// <summary>
		/// 获得或设置操作的方法
		/// </summary>
		public string Method { get; set; }

		/// <summary>
		/// 获得或设置请求的Origin标头
		/// </summary>
		public string Origin
		{
			get { return Headers["Origin"]; }
			set
			{
				Headers["Origin"] = value;
			}
		}
		/// <summary>
		/// 获得或设置是否默认预先授权
		/// </summary>
		public bool PreAuthenticate { get; set; }

		/// <summary>
		/// 获得或设置超时时间
		/// </summary>
		public int? ReadWriteTimeout { get; set; }

		/// <summary>
		/// 获得或设置引用页地址
		/// </summary>
		public string Referer { get; set; }

		/// <summary>
		/// 获得或设置引用页地址
		/// </summary>
		[Obsolete("This property was obsoleted. Use 'Referer' instead.")]
		public Uri ReferUri { get; set; }

		/// <summary>
		/// 获得当前响应的请求载荷
		/// </summary>
		public object RequestPayload { get; set; }

		/// <summary>
		/// 获得或设置请求数据
		/// </summary>
		public HttpRequestContent RequestData { get; set; }

		/// <summary>
		/// 获得或设置默认的Socket接收缓存
		/// </summary>
		public int? SocketReceiveBufferSize { get; set; }

		/// <summary>
		/// 获得或设置失效时间
		/// </summary>
		public DateTime? SyncTime { get; set; }
		/// <summary>
		/// 获得或设置超时时间
		/// </summary>
		public int? Timeout { get; set; }

		/// <summary>
		/// TransferEncoding
		/// </summary>
		/// <value>The transfer encoding.</value>
		public string TransferEncoding
		{
			get;
			set;
		}

		/// <summary>
		/// 获得传输上下文
		/// </summary>
		public TransportContext TransportContext { get; internal set; }

		/// <summary>
		/// 获得或设置请求的地址
		/// </summary>
		public Uri Uri { get; set; }
		/// <summary>
		/// 是否使用NagleAlgorithm
		/// </summary>
		public bool UseNagleAlgorithm { get; set; }

		/// <summary>
		/// 获得或设置UserAgent
		/// </summary>
		public string UserAgent { get; set; }

		/// <summary>
		/// 获得或设置代理
		/// </summary>
		public IWebProxy WebProxy
		{
			get { return _webProxy; }
			set
			{
				_webProxy = value;
				_proxySet = true;
			}
		}

		/// <summary>
		/// 获得或设置客户端证书
		/// </summary>
		public X509Certificate[] X509Certificates { get; set; }

		static HashSet<string> _allowBodyMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
												{
													"Post",
													"Put",
													"Patch",
													"Delete",
													"PropFind",
													"Lock",
													"Link",
													"Unlink"
												};

#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 获得或设置主机头
		/// </summary>
		public string Host { get; set; }

#endif


		/// <summary>
		/// 获得或设置当前请求的范围
		/// </summary>
#if NET_GT_4
		public KeyValuePair<long, long?>? Range { get; set; }
#else
		public KeyValuePair<int, int?>? Range { get; set; }
#endif

		/// <summary>
		/// 是否禁止302跳转。如果设置为 <see langword="true" />，则当服务器返回302/301跳转时，视为错误
		/// </summary>
		public bool Disable302Redirection { get; set; }

		/// <summary>
		/// 内容类型(强制重写)
		/// </summary>
		public string ContentTypeOverride { get; set; }

		/// <summary>
		/// 获得或设置响应保存到文件的位置
		/// </summary>
		public string SaveToFile { get; set; }

		/// <summary>
		/// 获得或设置用于处理数据内容的委托
		/// </summary>
		public EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamDataHandler { get; set; }

		/// <summary>
		/// 获得或设置响应复制到的流
		/// </summary>
		public Stream CopyToStream { get; set; }

		#region Dispose方法实现

		bool _disposed;
		private IWebProxy _webProxy;

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			_disposed = true;

			if (disposing)
			{
				if (RequestData != null)
					RequestData.Dispose();

			}
			//TODO 释放非托管资源

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
				throw new ObjectDisposedException(this.GetType().Name);
		}


		#endregion

	}
}
