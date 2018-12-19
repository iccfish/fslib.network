namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;

	/// <summary>
	/// 创建指定类型对象实例的工厂类
	/// </summary>
	public class HttpHandler : AbstractHttpHandler
	{
		/// <summary>
		/// 获得用于发送请求的Request对象
		/// </summary>
		/// <param name="uri">当前请求的目标地址</param>
		/// <param name="method">当前请求的HTTP方法</param>
		/// <param name="context">当前的上下文 <see cref="HttpContext" /></param>
		/// <returns></returns>
		public override HttpWebRequest GetRequest(Uri uri, string method, HttpContext context)
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
			OnWebRequestCreated(e);

			return e.HttpWebRequest;
		}


		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public override HttpContext GetContext(HttpClient client, HttpRequestMessage request)
		{
			//证书
			client.Setting.CertificateManager?.SetRequest(request);

			var ctx = new HttpContext(client, request);

			//附加监听器
			if (client.Monitor != null)
				ctx.AttachMonitor(client.Monitor);

			return ctx;
		}

		/// <summary>
		/// 创建上下文环境
		/// </summary>
		/// <param name="client"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public override HttpContext<T> GetContext<T>(HttpClient client, HttpRequestMessage request)
		{
			var ctx = new HttpContext<T>(client, request);

			//附加监听器
			if (client.Monitor != null)
				ctx.AttachMonitor(client.Monitor);

			return ctx;
		}

		/// <summary>
		/// 解析URL字符串为URI
		/// </summary>
		/// <param name="header">解析后的地址使用的位置</param>
		/// <param name="url">字符串地址</param>
		/// <param name="data">获得或设置相关联的数据</param>
		/// <returns></returns>
		public override Uri ResolveUri(HttpRequestHeader? header, string url, Dictionary<string, object> data)
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
		public override HttpRequestContent WrapRequestContent(HttpClient client, HttpRequestContent requestContent, object data, ContentType? contentTypeType)
		{
			return requestContent;
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

			IPEndPoint localEndPoint = null;
			var addFamily = remoteEndPoint.AddressFamily;

			if (context.Request.LocalIpEndPoint != null)
				localEndPoint = context.Request.LocalIpEndPoint;
			else if (addFamily == AddressFamily.InterNetwork && context.Client.Setting.LocalIpAddressIpV4 != null)
				localEndPoint = new IPEndPoint(context.Client.Setting.LocalIpAddressIpV4, 0);
			else if (addFamily == AddressFamily.InterNetworkV6 && context.Client.Setting.LocalIpAddressIpV6 != null)
				localEndPoint = new IPEndPoint(context.Client.Setting.LocalIpAddressIpV6, 0);

			//if (remoteEndPoint.AddressFamily != localEndPoint.AddressFamily)
			//	throw new IPAddressFamilyMismatchException(remoteEndPoint, localEndPoint);
			ef.LocalEndPoint = localEndPoint;

			return localEndPoint;
		}
	}
}
