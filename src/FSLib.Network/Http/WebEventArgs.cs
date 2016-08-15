namespace FSLib.Network.Http
{
	using System.Linq;

	/// <summary>
	/// 包含了HTTP请求事件中存在的数据
	/// </summary>
	public class WebEventArgs : System.EventArgs
	{
		/// <summary>
		/// 事件关联的客户端
		/// </summary>
		public HttpClient Client => Context.Client;

		/// <summary>
		/// 获得上下文
		/// </summary>
		public HttpContext Context { get; private set; }

		/// <summary>
		/// 获得当前的请求
		/// </summary>
		public HttpRequestMessage Request => Context.Request;

		/// <summary>
		/// 获得当前的响应
		/// </summary>
		public HttpResponseMessage Response => Context.Response;

		/// <summary>
		/// 获得或设置是否取消操作
		/// </summary>
		public bool Cancelled { get; set; }

		/// <summary>
		/// 创建 <see cref="WebEventArgs" />  的新实例(WebEventArgs)
		/// </summary>
		public WebEventArgs(HttpContext context)
		{
			Context = context;
		}
	}
}
