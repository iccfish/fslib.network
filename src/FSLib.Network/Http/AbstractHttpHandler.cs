namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;

	public abstract class AbstractHttpHandler : IHttpHandler
	{
		/// <summary>
		/// 获得或设置BaseURI
		/// </summary>
		public Uri BaseUri { get; set; }

		/// <summary>
		/// 获得用于发送请求的Request对象
		/// </summary>
		/// <param name="uri">当前请求的目标地址</param>
		/// <param name="method">当前请求的HTTP方法</param>
		/// <param name="context">当前的上下文 <see cref="HttpContext" /></param>
		/// <returns></returns>
		public abstract HttpWebRequest GetRequest(Uri uri, string method, HttpContext context);

		/// <summary>
		/// 初始化上下文。此操作在上下文本身初始化完成之后、请求发出之前调用
		/// </summary>
		/// <param name="context"></param>
		public virtual void PrepareContext(HttpContext context)
		{
		}

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public abstract HttpContext GetContext(HttpClient client, HttpRequestMessage request);

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public abstract HttpContext<T> GetContext<T>(HttpClient client, HttpRequestMessage request) where T : class;

		/// <summary>
		/// 解析URL字符串为URI
		/// </summary>
		/// <param name="header">解析后的地址使用的位置</param>
		/// <param name="url">字符串地址</param>
		/// <param name="data">获得或设置相关联的数据</param>
		/// <returns></returns>
		public abstract Uri ResolveUri(HttpRequestHeader? header, string url, Dictionary<string, object> data);

		/// <summary>
		/// 请求转换请求承载的内容为最终可以发送的数据，并确定其承载方式
		/// </summary>
		/// <returns></returns>
		public virtual void WrapRequestContent(RequestWrapRequestContentEventArgs e) { }

		/// <summary>
		/// 获得比较最适合的用于处理响应的类型
		/// </summary>
		/// <returns></returns>
		public void GetPreferredResponseType(GetPreferredResponseTypeEventArgs ea)
		{
		}


		/// <summary>
		/// 自定义处理Cookies
		/// </summary>
		/// <param name="context">当前的HTTP请求上下文</param>
		/// <param name="client">当前的客户端</param>
		/// <returns>如果返回 <see langword="false" />，那么客户端将会进行Cookies的默认处理。如果返回 <see langword="true" />，则表示Cookies已由第三方代码处理，类库本身不再处理</returns>
		public virtual bool ProcessCookies(HttpContext context, HttpClient client)
		{
			return false;
		}

		/// <summary>
		/// 已创建请求
		/// </summary>
		public virtual event EventHandler<HttpHandlerEventArgs> WebRequestCreated;

		/// <summary>
		/// 已创建上下文
		/// </summary>
		public virtual event EventHandler<WebEventArgs> HttpContextCreated;

		/// <summary>
		/// 请求数据已经准备完毕
		/// </summary>

		public virtual event EventHandler<WebEventArgs> RequestDataPrepared;

		public virtual void AfterRequestDataPrepared(HttpContext context)
		{
		}

		/// <summary>
		/// 请求装饰写入的流
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		public virtual Stream DecorateRequestStream(HttpContext context, Stream orignalStream)
		{
			return orignalStream;
		}

		/// <summary>
		/// 请求装饰响应流（原始流）
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		public virtual Stream DecorateRawResponseStream(HttpContext context, Stream orignalStream)
		{
			return orignalStream;
		}

		/// <summary>
		/// 验证服务器端证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnServerCertificateValidation(object sender, CertificateValidationEventArgs e)
		{
		}

		/// <summary>
		/// 请求装饰响应流（已处理比如解压后的流）
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		public virtual Stream DecorateResponseStream(HttpContext context, Stream orignalStream)
		{
			return orignalStream;
		}

		/// <summary>
		/// 请求验证当前会话
		/// </summary>
		public virtual event EventHandler<WebEventArgs> ValidateResponseHeader;

		/// <summary>
		/// 验证响应头。在这里抛出的异常将会导致请求被设置为失败。
		/// </summary>
		/// <param name="e"></param>
		public virtual void OnValidateResponseHeader(WebEventArgs e) { ValidateResponseHeader?.Invoke(e.Context, e); }

		public virtual event EventHandler<WebEventArgs> RequestEnd;

		public virtual void OnRequestEnd(WebEventArgs e) { RequestEnd?.Invoke(e.Context, e); }

		/// <summary>
		/// 引发 <see cref="HttpHandler.ResponseContentObjectInitialized"/> 事件
		/// </summary>
		public virtual void OnResponseContentObjectInitialized(WebEventArgs e) => ResponseContentObjectInitialized?.Invoke(e.Context, e);

		/// <summary>
		/// 引发 <see cref="HttpHandler.HttpContextCreated.HttpContextCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnHttpContextCreated(WebEventArgs ea)
		{
			var handler = HttpContextCreated;
			handler?.Invoke(ea.Context, ea);
		}

		/// <summary>
		/// 引发 <see cref="HttpHandler.WebRequestCreated.WebRequestCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		public virtual void OnWebRequestCreated(HttpHandlerEventArgs ea) => WebRequestCreated?.Invoke(ea.HttpContext, ea);

		/// <summary>
		/// 响应处理请求对象已经创建
		/// </summary>
		public virtual event EventHandler<WebEventArgs> ResponseContentObjectInitialized;

		/// <summary>
		/// 已经收到HTTP响应头，准备处理请求
		/// </summary>
		public virtual event EventHandler<WebEventArgs> PreviewResponseHeader;

		public virtual void OnPreviewResponseHeader(WebEventArgs e) { PreviewResponseHeader?.Invoke(e.Context, e); }

		/// <summary>
		/// 当前请求被重新发送
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestResubmit;

		/// <summary>
		/// 状态发生变化
		/// </summary>
		public virtual event EventHandler<WebEventArgs> StateChanged;

		/// <summary>
		/// 引发 <see cref="HttpHandler.StateChanged" /> 事件
		/// </summary>
		public virtual void OnStateChanged(WebEventArgs e) => StateChanged?.Invoke(this, e);

		/// <summary>
		/// 正在准备发送
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestSending;

		/// <summary>
		/// 请求已经发送，正在等待写入请求数据或等待响应流
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestSent;

		/// <summary>
		/// 已经获得请求流
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestStreamFetched;

		/// <summary>
		/// 正在发送请求数据
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestDataSending;

		/// <summary>
		/// 请求数据发送进度变化
		/// </summary>
		public virtual event EventHandler<DataProgressEventArgs> RequestDataSendProgressChanged;

		/// <summary>
		/// 请求数据已经发送
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestDataSent;

		/// <summary>
		/// 已经收到响应
		/// </summary>
		public virtual event EventHandler<WebEventArgs> ResponseHeaderReceived;

		/// <summary>
		/// 已经获得响应流
		/// </summary>
		public virtual event EventHandler<WebEventArgs> ResponseStreamFetched;

		/// <summary>
		/// 响应数据读取已经完成
		/// </summary>
		public virtual event EventHandler<WebEventArgs> ResponseDataReceiveCompleted;

		/// <summary>
		/// 响应读取进度变更
		/// </summary>
		public virtual event EventHandler<DataProgressEventArgs> ResponseReadProgressChanged;

		/// <summary>
		/// 请求已经完成
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestFinished;

		/// <summary>
		/// 请求失败
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestFailed;

		/// <summary>
		/// 检测到重定向
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestRedirect;

		/// <summary>
		/// 请求验证内容
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestValidateResponse;

		public virtual void OnRequestResubmit(WebEventArgs e) { RequestResubmit?.Invoke(e.Context, e); }

		public virtual void OnRequestSending(WebEventArgs e) { RequestSending?.Invoke(e.Context, e); }

		public virtual void OnRequestFailed(WebEventArgs e) { RequestFailed?.Invoke(e.Context, e); }

		public virtual void OnRequestRedirect(WebEventArgs e) { RequestRedirect?.Invoke(e.Context, e); }

		public virtual void OnRequestValidateResponse(WebEventArgs e) { RequestValidateResponse?.Invoke(e.Context, e); }

		public virtual void OnRequestFinished(WebEventArgs e) { RequestFinished?.Invoke(e.Context, e); }

		public virtual void OnResponseReadProgressChanged(HttpContext sender, DataProgressEventArgs e) { ResponseReadProgressChanged?.Invoke(sender, e); }

		public virtual void OnRequestDataSent(WebEventArgs e) { RequestDataSent?.Invoke(this, e); }

		public virtual void OnResponseDataReceiveCompleted(WebEventArgs e) { ResponseDataReceiveCompleted?.Invoke(e.Context, e); }

		public virtual void OnResponseHeaderReceived(WebEventArgs e) { ResponseHeaderReceived?.Invoke(e.Context, e); }

		public virtual void OnResponseStreamFetched(WebEventArgs e) { ResponseStreamFetched?.Invoke(e.Context, e); }

		public virtual void OnRequestDataSendProgressChanged(HttpContext sender, DataProgressEventArgs e) { RequestDataSendProgressChanged?.Invoke(sender, e); }

		public virtual void OnRequestDataSending(WebEventArgs e) { RequestDataSending?.Invoke(e.Context, e); }

		public virtual void OnRequestStreamFetched(WebEventArgs e) { RequestStreamFetched?.Invoke(e.Context, e); }

		public virtual void OnRequestSent(WebEventArgs e) { RequestSent?.Invoke(e.Context, e); }

		/// <summary>
		/// 请求已收到，请求判断响应类型
		/// </summary>
		public virtual event EventHandler<GetPreferredResponseTypeEventArgs> DetectResponseContentType;

		/// <summary>
		/// 性能计数器对象已经新建
		/// </summary>
		public virtual event EventHandler<WebEventArgs> PerformanceObjectCreated;

		/// <summary>
		/// 请求被取消
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestCancelled;

		/// <summary>
		/// 当请求已经被创建时触发
		/// </summary>
		public virtual event EventHandler<WebEventArgs> RequestCreated;

		/// <summary>
		/// 即将开始发送请求
		/// </summary>
		public virtual event EventHandler<WebEventArgs> BeforeRequest;

		public virtual void OnDetectResponseContentType(GetPreferredResponseTypeEventArgs e) => DetectResponseContentType?.Invoke(this, e);

		public virtual void OnPerformanceObjectCreated(WebEventArgs e) => PerformanceObjectCreated?.Invoke(this, e);

		public virtual void OnRequestCancelled(WebEventArgs e) => RequestCancelled?.Invoke(this, e);

		public virtual void OnRequestCreated(WebEventArgs e) => RequestCreated?.Invoke(this, e);

		public virtual void OnBeforeRequest(WebEventArgs e) => BeforeRequest?.Invoke(e.Context, e);

		public virtual void OnRequestDataPrepared(WebEventArgs e) => RequestDataPrepared?.Invoke(this, e);
	}
}