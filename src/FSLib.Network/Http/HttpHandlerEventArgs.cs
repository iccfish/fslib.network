namespace FSLib.Network.Http
{
	using System;
	using System.Net;

	/// <summary>
	/// 包含了HttpHandler的事件数据
	/// </summary>
	public class HttpHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// 获得或设置创建相关的HttpWebRequest
		/// </summary>
		public HttpWebRequest HttpWebRequest { get; set; }

		/// <summary>
		/// 获得或设置创建的HttpContext
		/// </summary>
		public HttpContext HttpContext { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpHandlerEventArgs" />  的新实例(HttpHandlerEventArgs)
		/// </summary>
		public HttpHandlerEventArgs(HttpWebRequest httpWebRequest, HttpContext httpContext)
		{
			HttpContext = httpContext;
			HttpWebRequest = httpWebRequest;
			
		}
	}
}