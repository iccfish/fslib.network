namespace FSLib.Network.Http
{
	using System;

	/// <summary>
	/// 包含了请求将发送数据包装为请求承载数据的事件参数
	/// </summary>
	public class RequestWrapRequestContentEventArgs : EventArgs
	{
		/// <summary>
		/// 获得当前的客户端
		/// </summary>
		public HttpClient HttpClient { get; private set; }

		/// <summary>
		/// 获得请求数据
		/// </summary>
		public HttpRequestMessage RequestMessage { get; private set; }

		/// <summary>
		/// 获得或设置对应的数据承载对象
		/// </summary>
		public HttpRequestContent RequestContent { get; set; }

		/// <summary>
		/// 获得或设置已处理的标记
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// 创建 <see cref="RequestWrapRequestContentEventArgs" />  的新实例(RequestWrapRequestContentEventArgs)
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="request"></param>
		public RequestWrapRequestContentEventArgs(HttpClient httpClient, HttpRequestMessage request)
		{
			HttpClient = httpClient;
			RequestMessage = request;
		}

	}
}