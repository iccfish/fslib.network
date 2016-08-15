namespace FSLib.Network.Http
{
	using System;
	using System.Net;

	/// <summary>
	/// 包含事件 <see cref="HttpSetting.PostInitRequest"/> 的数据
	/// </summary>
	public class PostInitRequestEventArgs : EventArgs
	{
		/// <summary>
		/// 获得相关联的 <see cref="HttpWebRequest"/>
		/// </summary>
		public HttpWebRequest Request { get; private set; }

		/// <summary>
		/// 获得相关联的 <see cref="HttpContext"/>
		/// </summary>
		public HttpContext HttpContext { get; private set; }

		/// <summary>
		/// 获得相关联的 <see cref="HttpClient"/>
		/// </summary>
		public HttpClient HttpClient { get; private set; }

		/// <summary>
		/// 创建 <see cref="PostInitRequestEventArgs" />  的新实例(PostInitRequestEventArgs)
		/// </summary>
		/// <param name="request"></param>
		/// <param name="httpContext"></param>
		/// <param name="httpClient"></param>
		public PostInitRequestEventArgs(HttpWebRequest request, HttpContext httpContext, HttpClient httpClient)
		{
			Request = request;
			HttpContext = httpContext;
			HttpClient = httpClient;
		}
	}
}