using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;
	using System.Net;
	using System.Net.Sockets;

	/// <summary>
	/// 创建指定类型对象实例的工厂类
	/// </summary>
	public class HttpHandler : IHttpHandler
	{

		/// <summary>
		/// 获得或设置BaseURI
		/// </summary>
		public Uri BaseUri { get; set; }

		/// <summary>
		/// 引发 <see cref="HttpContextCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnHttpContextCreated(HttpHandlerEventArgs ea)
		{
			var handler = HttpContextCreated;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 获得用于发送请求的Request对象
		/// </summary>
		/// <param name="uri">当前请求的目标地址</param>
		/// <param name="method">当前请求的HTTP方法</param>
		/// <param name="context">当前的上下文 <see cref="HttpContext" /></param>
		/// <returns></returns>
		public virtual HttpWebRequest GetRequest(Uri uri, string method, HttpContext context)
		{
			var req = (HttpWebRequest)WebRequest.Create(uri);
			req.Method = method;

			//客户端证书
			if (context.Client.Setting.X509Certificates?.Length > 0)
			{
				req.ClientCertificates.AddRange(context.Client.Setting.X509Certificates);
			}
			if (context.Request.X509Certificates?.Length > 0)
			{
				req.ClientCertificates.AddRange(context.Request.X509Certificates);
			}

			req.ServicePoint.BindIPEndPointDelegate = (servicePoint, rep, rc) => BindIPEndPoint(context, uri, method, servicePoint, rep, rc);

			var e = new HttpHandlerEventArgs(req, null);
			OnRequestCreated(e);

			return e.HttpWebRequest;
		}

		/// <summary>
		/// 确定要使用的本地IP端口
		/// </summary>
		/// <remarks>
		/// <para>绑定的IP端口的地址族需要正确。比如要连接的远程地址是IPV4的，则本地的端口也需要IPV4.</para>
		/// <para>如果远程端口是IPV6的，则本地端口也需要是IPV6. 否则绑定无法起效。</para>
		/// </remarks>
		/// <param name="method">当前请求的HTTP方法</param>
		/// <param name="servicePoint">
		/// 当前的 <see cref="System.Net.ServicePoint" />
		/// </param>
		/// <param name="remoteEndPoint">
		/// 当前要连接的远程 <see cref="System.Net.IPEndPoint" />
		/// </param>
		/// <param name="retryCount">重试次数</param>
		/// <param name="context">当前的上下文 <see cref="HttpContext" /></param>
		/// <param name="uri">当前请求的目标地址</param>
		/// <returns>
		/// 返回当前要使用的本地端口
		/// </returns>
		/// <exception cref="IPAddressFamilyMismatchException">本地的IP地址组和远程地址的地址族不匹配.</exception>
		protected virtual IPEndPoint BindIPEndPoint(HttpContext context, Uri uri, string method, ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
		{
			var ef = (EndPointInfo)context.EndPointInfo;
			ef.RemoteEndPoint = remoteEndPoint;
			ef.ServicePoint = servicePoint;

			IPEndPoint localEndPoint;
			if (context.Request.LocalIpEndPoint != null)
				localEndPoint = context.Request.LocalIpEndPoint;
			else if (remoteEndPoint.AddressFamily == AddressFamily.InterNetwork && context.Client.Setting.LocalIpAddressIpV4 != null)
				localEndPoint = new IPEndPoint(context.Client.Setting.LocalIpAddressIpV4, 0);
			else if (remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 && context.Client.Setting.LocalIpAddressIpV6 != null)
				localEndPoint = new IPEndPoint(context.Client.Setting.LocalIpAddressIpV6, 0);
			else
				localEndPoint = new IPEndPoint(remoteEndPoint.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);

			if (remoteEndPoint.AddressFamily != localEndPoint.AddressFamily)
				throw new IPAddressFamilyMismatchException(remoteEndPoint, localEndPoint);
			ef.LocalEndPoint = localEndPoint;

			return localEndPoint;
		}


		/// <summary>
		/// 初始化上下文。此操作在上下文本身初始化完成之后、请求发出之前调用
		/// </summary>
		/// <param name="context"></param>
		public void PrepareContext(HttpContext context)
		{
		}

		/// <summary>
		/// 引发 <see cref="RequestCreated" /> 事件
		/// </summary>
		/// <param name="ea">包含此事件的参数</param>
		protected virtual void OnRequestCreated(HttpHandlerEventArgs ea)
		{
			var handler = RequestCreated;
			if (handler != null)
				handler(this, ea);
		}

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public virtual HttpContext GetContext(HttpClient client, HttpRequestMessage request)
		{
			var ctx = new HttpContext(client, request);

			//附加监听器
			if (client.Monitor != null)
				ctx.AttachMonitor(client.Monitor);

			var e = new HttpHandlerEventArgs(null, ctx);

			OnHttpContextCreated(e);

			return e.HttpContext;
		}

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public virtual HttpContext<T> GetContext<T>(HttpClient client, HttpRequestMessage request) where T : class
		{
			var ctx = new HttpContext<T>(client, request);

			//附加监听器
			if (client.Monitor != null)
				ctx.AttachMonitor(client.Monitor);

			var e = new HttpHandlerEventArgs(null, ctx);

			OnHttpContextCreated(e);

			return e.HttpContext as HttpContext<T>;
		}

		/// <summary>
		/// 解析URL字符串为URI
		/// </summary>
		/// <param name="header">解析后的地址使用的位置</param>
		/// <param name="url">字符串地址</param>
		/// <param name="data">获得或设置相关联的数据</param>
		/// <returns></returns>
		public virtual Uri ResolveUri(HttpRequestHeader? header, string url, Dictionary<string, object> data)
		{
			if (url == null)
				return null;

			if (BaseUri != null && Uri.IsWellFormedUriString(url, UriKind.Relative))
			{
				return new Uri(BaseUri, url);
			}
			if (url.IsNullOrEmpty())
				return null;

			return new Uri(url);
		}

		/// <summary>
		/// 请求转换请求承载的内容为最终可以发送的数据，并确定其承载方式
		/// </summary>
		/// <param name="client">当前的客户端</param>
		/// <param name="requestContent">请求的内容</param>
		/// <param name="data">要发送的数据</param>
		/// <param name="contentTypeType">承载的方式</param>
		/// <returns></returns>
		public HttpRequestContent WrapRequestContent(HttpClient client, HttpRequestContent requestContent, object data, ContentType? contentTypeType)
		{
			return requestContent;
		}

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
		public HttpResponseContent GetPreferedResponseType<T>(HttpClient client, HttpContext ctx, HttpResponseContent responseContent, EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker = null, T result = default(T), Stream targetStream = null, string saveToFilePath = null)
		{
			return responseContent;
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
		public event EventHandler<HttpHandlerEventArgs> RequestCreated;

		/// <summary>
		/// 已创建上下文
		/// </summary>
		public event EventHandler<HttpHandlerEventArgs> HttpContextCreated;

		/// <summary>
		/// 请求装饰写入的流
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		public virtual Stream DecorateRequestStream(Stream orignalStream)
		{
			return orignalStream;
		}

		/// <summary>
		/// 请求装饰响应流（原始流）
		/// </summary>
		/// <param name="orignalStream"></param>
		/// <returns></returns>
		public virtual Stream DecorateRawResponseStream(Stream orignalStream)
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
		public virtual Stream DecorateResponseStream(Stream orignalStream)
		{
			return orignalStream;
		}

		/// <summary>
		/// 验证响应。在这里抛出的异常将会导致请求被设置为失败。
		/// </summary>
		/// <param name="context"></param>
		public virtual void ValidateResponse(HttpContext context)
		{

		}
	}
}
