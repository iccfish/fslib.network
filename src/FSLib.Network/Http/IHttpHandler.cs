using System;
using System.Collections.Generic;
using System.Net;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// HTTP处理时使用的相关类
	/// </summary>
	public interface IHttpHandler
	{
		/// <summary>
		/// 验证响应。在这里抛出的异常将会导致请求被设置为失败。
		/// </summary>
		/// <param name="context"></param>
		void ValidateResponse(HttpContext context);

		/// <summary>
		/// 验证服务器端证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnServerCertificateValidation(object sender, CertificateValidationEventArgs e);

		/// <summary>
		/// 请求装饰响应流（已处理比如解压后的流）
		/// </summary>
		/// <param name="context">关联的上下文</param>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		System.IO.Stream DecorateResponseStream(HttpContext context, System.IO.Stream orignalStream);

		/// <summary>
		/// 请求装饰响应流（原始流）
		/// </summary>
		/// <param name="context">关联的上下文</param>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		System.IO.Stream DecorateRawResponseStream(HttpContext context, System.IO.Stream orignalStream);


		/// <summary>
		/// 要写入的数据准备完毕
		/// </summary>
		/// <param name="context">关联的上下文</param>
		void AfterRequestDataPrepared(HttpContext context);


		/// <summary>
		/// 请求装饰写入的流
		/// </summary>
		/// <param name="context">关联的上下文</param>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		System.IO.Stream DecorateRequestStream(HttpContext context, System.IO.Stream orignalStream);

		/// <summary>
		/// 自定义处理Cookies
		/// </summary>
		/// <param name="context"></param>
		/// <param name="client"></param>
		/// <returns></returns>
		bool ProcessCookies(HttpContext context,
							HttpClient client);

		/// <summary>
		/// 获得或设置BaseURI
		/// </summary>
		Uri BaseUri { get; set; }

		/// <summary>
		/// 已创建上下文
		/// </summary>
		event EventHandler<HttpHandlerEventArgs> HttpContextCreated;

		/// <summary>
		/// 已创建请求
		/// </summary>
		event EventHandler<HttpHandlerEventArgs> RequestCreated;

		/// <summary>
		/// 获得用于发送请求的Request对象
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="method"></param>
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
		/// <param name="requestContent">当前的请求数据</param>
		/// <param name="data">要发送的数据</param>
		/// <param name="contentTypeType">承载的方式</param>
		/// <returns></returns>
		HttpRequestContent WrapRequestContent(HttpClient client, HttpRequestContent requestContent, object data, ContentType? contentTypeType);

		/// <summary>
		/// 获得比较最适合的用于处理响应的类型
		/// </summary>
		/// <typeparam name="T">当前希望获得的结果</typeparam>
		/// <param name="client">当前的HTTP客户端</param>
		/// <param name="ctx">当前的上下文环境</param>
		/// <param name="responseContent">当前用来处理结果的对象</param>
		/// <param name="streamInvoker">如果希望能按流处理，那么用来处理响应的事件委托</param>
		/// <param name="result">当前希望获得的结果实例</param>
		/// <param name="targetStream">要将相应内容写入的流</param>
		/// <param name="saveToFilePath">要将当前请求写入的文件路径</param>
		/// <returns></returns>
		HttpResponseContent GetPreferedResponseType<T>(HttpClient client, HttpContext ctx, HttpResponseContent responseContent, EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null, T result = default(T), Stream targetStream = null, string saveToFilePath = null);
	}
}