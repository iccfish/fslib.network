namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;

	/// <summary>
	/// HTTP处理时使用的相关类
	/// </summary>
	public interface IHttpHandler
	{
		/// <summary>
		/// 获得或设置BaseURI
		/// </summary>
		Uri BaseUri { get; set; }

		/// <summary>
		/// 获得用于发送请求的Request对象
		/// </summary>
		/// <param name="uri">当前请求的目标地址</param>
		/// <param name="method">当前请求的HTTP方法</param>
		/// <param name="context">当前的上下文 <see cref="HttpContext" /></param>
		/// <returns></returns>
		HttpWebRequest GetRequest(Uri uri, string method, HttpContext context);

		/// <summary>
		/// 初始化上下文。此操作在上下文本身初始化完成之后、请求发出之前调用
		/// </summary>
		/// <param name="context"></param>
		void PrepareContext(HttpContext context);

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		HttpContext GetContext(HttpClient client, HttpRequestMessage request);

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		HttpContext<T> GetContext<T>(HttpClient client, HttpRequestMessage request) where T : class;

		/// <summary>
		/// 解析URL字符串为URI
		/// </summary>
		/// <param name="header">解析后的地址使用的位置</param>
		/// <param name="url">字符串地址</param>
		/// <param name="data">获得或设置相关联的数据</param>
		/// <returns></returns>
		Uri ResolveUri(HttpRequestHeader? header, string url, Dictionary<string, object> data);

		/// <summary>
		/// 请求转换请求承载的内容为最终可以发送的数据，并确定其承载方式
		/// </summary>
		/// <param name="client">当前的客户端</param>
		/// <param name="requestContent">请求的内容</param>
		/// <param name="data">要发送的数据</param>
		/// <param name="contentTypeType">承载的方式</param>
		/// <returns></returns>
		HttpRequestContent WrapRequestContent(HttpClient client, HttpRequestContent requestContent, object data, ContentType? contentTypeType);

		/// <summary>
		/// 获得比较最适合的用于处理响应的类型
		/// </summary>
		void GetPreferredResponseType(GetPreferredResponseTypeEventArgs ea);

		/// <summary>
		/// 自定义处理Cookies
		/// </summary>
		/// <param name="context">当前的HTTP请求上下文</param>
		/// <param name="client">当前的客户端</param>
		/// <returns>如果返回 <see langword="false" />，那么客户端将会进行Cookies的默认处理。如果返回 <see langword="true" />，则表示Cookies已由第三方代码处理，类库本身不再处理</returns>
		bool ProcessCookies(HttpContext context, HttpClient client);

		/// <summary>
		/// 已创建请求
		/// </summary>
		event EventHandler<HttpHandlerEventArgs> WebRequestCreated;

		/// <summary>
		/// 已创建上下文
		/// </summary>
		event EventHandler<WebEventArgs> HttpContextCreated;

		void AfterRequestDataPrepared(HttpContext context);

		/// <summary>
		/// 请求装饰写入的流
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		Stream DecorateRequestStream(HttpContext context, Stream orignalStream);

		/// <summary>
		/// 请求装饰响应流（原始流）
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		Stream DecorateRawResponseStream(HttpContext context, Stream orignalStream);

		/// <summary>
		/// 验证服务器端证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnServerCertificateValidation(object sender, CertificateValidationEventArgs e);

		/// <summary>
		/// 请求装饰响应流（已处理比如解压后的流）
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		Stream DecorateResponseStream(HttpContext context, Stream orignalStream);

		/// <summary>
		/// 请求验证当前会话
		/// </summary>
		event EventHandler<WebEventArgs> ValidateResponseHeader;

		/// <summary>
		/// 验证响应头。在这里抛出的异常将会导致请求被设置为失败。
		/// </summary>
		/// <param name="e"></param>
		void OnValidateResponseHeader(WebEventArgs e);

		event EventHandler<WebEventArgs> RequestEnd;

		void OnRequestEnd(WebEventArgs e);

		/// <summary>
		/// 引发 <see cref="HttpHandler.ResponseContentObjectInitialized"/> 事件
		/// </summary>
		void OnResponseContentObjectInitialized(WebEventArgs e);

		/// <summary>
		/// 引发 <see cref="HttpHandler.HttpContextCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		void OnHttpContextCreated(WebEventArgs ea);

		/// <summary>
		/// 引发 <see cref="HttpHandler.WebRequestCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		void OnWebRequestCreated(HttpHandlerEventArgs ea);

		/// <summary>
		/// 响应处理请求对象已经创建
		/// </summary>
		event EventHandler<WebEventArgs> ResponseContentObjectInitialized;

		/// <summary>
		/// 已经收到HTTP响应头，准备处理请求
		/// </summary>
		event EventHandler<WebEventArgs> PreviewResponseHeader;

		void OnPreviewResponseHeader(WebEventArgs e);

		/// <summary>
		/// 当前请求被重新发送
		/// </summary>
		event EventHandler<WebEventArgs> RequestResubmit;

		/// <summary>
		/// 状态发生变化
		/// </summary>
		event EventHandler<WebEventArgs> StateChanged;

		/// <summary>
		/// 引发 <see cref="HttpHandler.StateChanged" /> 事件
		/// </summary>
		void OnStateChanged(WebEventArgs e);

		/// <summary>
		/// 正在准备发送
		/// </summary>
		event EventHandler<WebEventArgs> RequestSending;

		/// <summary>
		/// 请求已经发送，正在等待写入请求数据或等待响应流
		/// </summary>
		event EventHandler<WebEventArgs> RequestSent;

		/// <summary>
		/// 已经获得请求流
		/// </summary>
		event EventHandler<WebEventArgs> RequestStreamFetched;

		/// <summary>
		/// 正在发送请求数据
		/// </summary>
		event EventHandler<WebEventArgs> RequestDataSending;

		/// <summary>
		/// 请求数据发送进度变化
		/// </summary>
		event EventHandler<DataProgressEventArgs> RequestDataSendProgressChanged;

		/// <summary>
		/// 请求数据已经发送
		/// </summary>
		event EventHandler<WebEventArgs> RequestDataSent;

		/// <summary>
		/// 已经收到响应
		/// </summary>
		event EventHandler<WebEventArgs> ResponseHeaderReceived;

		/// <summary>
		/// 已经获得响应流
		/// </summary>
		event EventHandler<WebEventArgs> ResponseStreamFetched;

		/// <summary>
		/// 响应数据读取已经完成
		/// </summary>
		event EventHandler<WebEventArgs> ResponseDataReceiveCompleted;

		/// <summary>
		/// 响应读取进度变更
		/// </summary>
		event EventHandler<DataProgressEventArgs> ResponseReadProgressChanged;

		/// <summary>
		/// 请求已经完成
		/// </summary>
		event EventHandler<WebEventArgs> RequestFinished;

		/// <summary>
		/// 请求失败
		/// </summary>
		event EventHandler<WebEventArgs> RequestFailed;

		/// <summary>
		/// 检测到重定向
		/// </summary>
		event EventHandler<WebEventArgs> RequestRedirect;

		/// <summary>
		/// 请求验证内容
		/// </summary>
		event EventHandler<WebEventArgs> RequestValidateResponse;

		void OnRequestResubmit(WebEventArgs e);

		void OnRequestSending(WebEventArgs e);

		void OnRequestFailed(WebEventArgs e);

		void OnRequestRedirect(WebEventArgs e);

		void OnRequestValidateResponse(WebEventArgs e);

		void OnRequestFinished(WebEventArgs e);

		void OnResponseReadProgressChanged(HttpContext sender, DataProgressEventArgs e);

		void OnRequestDataSent(WebEventArgs e);

		void OnResponseDataReceiveCompleted(WebEventArgs e);

		void OnResponseHeaderReceived(WebEventArgs e);

		void OnResponseStreamFetched(WebEventArgs e);

		void OnRequestDataSendProgressChanged(HttpContext sender, DataProgressEventArgs e);

		void OnRequestDataSending(WebEventArgs e);

		void OnRequestStreamFetched(WebEventArgs e);

		void OnRequestSent(WebEventArgs e);

		/// <summary>
		/// 请求已收到，请求判断响应类型
		/// </summary>
		event EventHandler<WebEventArgs> DetectResponseContentType;

		/// <summary>
		/// 性能计数器对象已经新建
		/// </summary>
		event EventHandler<WebEventArgs> PerformanceObjectCreated;

		/// <summary>
		/// 请求被取消
		/// </summary>
		event EventHandler<WebEventArgs> RequestCancelled;

		/// <summary>
		/// 当请求已经被创建时触发
		/// </summary>
		event EventHandler<WebEventArgs> RequestCreated;

		void OnDetectResponseContentType(WebEventArgs e);

		void OnPerformanceObjectCreated(WebEventArgs e);

		void OnRequestCancelled(WebEventArgs e);

		void OnRequestCreated(WebEventArgs e);

		/// <summary>
		/// 即将开始发送请求
		/// </summary>
		event EventHandler<WebEventArgs> BeforeRequest;

		void OnBeforeRequest(WebEventArgs e);

		void OnRequestDataPrepared(WebEventArgs e);

		/// <summary>
		/// 请求数据已经准备完毕
		/// </summary>
		event EventHandler<WebEventArgs> RequestDataPrepared;

	}
}
