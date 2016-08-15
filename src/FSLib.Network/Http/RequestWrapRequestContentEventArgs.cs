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
		/// 获得要发送的数据
		/// </summary>
		public object Data { get; private set; }

		/// <summary>
		/// 额外的请求信息
		/// </summary>
		public ExtraRequestInfo ExtraRequestInfo { get; private set; }

		/// <summary>
		/// 获得或设置对应的数据承载对象
		/// </summary>
		public HttpRequestContent RequestContent { get; set; }

		public ContentType? ContentType
		{
			get;
			set;
		}

		/// <summary>
		/// 获得或设置已处理的标记
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// 创建 <see cref="RequestWrapRequestContentEventArgs" />  的新实例(RequestWrapRequestContentEventArgs)
		/// </summary>
		/// <param name="httpClient"></param>
		/// <param name="data"></param>
		/// <param name="extraRequestInfo">额外的请求信息</param>
		public RequestWrapRequestContentEventArgs(HttpClient httpClient, object data, ExtraRequestInfo extraRequestInfo)
		{
			HttpClient = httpClient;
			Data = data;
			ExtraRequestInfo = extraRequestInfo;
		}

	}
}