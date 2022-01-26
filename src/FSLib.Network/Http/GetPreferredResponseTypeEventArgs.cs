namespace FSLib.Network.Http
{
	using System;
	using System.IO;

	/// <summary>
	/// 包含在获得对应的数据处理类事件中将会用到的参数
	/// </summary>
	public class GetPreferredResponseTypeEventArgs : EventArgs
	{
		public HttpRequestMessage Request { get; }

		/// <summary>
		/// 获得当前的客户端
		/// </summary>
		public HttpClient HttpClient { get; private set; }

		/// <summary>
		/// 获得当前的上下文环境
		/// </summary>
		public HttpContext HttpContext { get; private set; }

		/// <summary>
		/// 获得目标类型
		/// </summary>
		public Type ResultType => Request?.ExceptType;

		/// <summary>
		/// 获得已有目标数据
		/// </summary>
		public object TargetObject => Request?.ExceptObject;

		/// <summary>
		/// 获得要将当前请求写入的文件路径
		/// </summary>
		public string SaveToFilePath => Request?.SaveToFile;

		/// <summary>
		/// 获得处理当前数据片段的事件处理句柄
		/// </summary>
		public EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> StreamInvoker { get; private set; }

		/// <summary>
		/// 获得要写入的目标流
		/// </summary>
		public Stream TargetStream => Request?.CopyToStream;

		/// <summary>
		/// 获得或设置当前的数据处理类
		/// </summary>
		public HttpResponseContent ResponseContent { get; set; }

		/// <summary>
		/// 获得或设置是否已经结束处理
		/// </summary>
		public bool Handled { get; set; }


		/// <summary>
		/// 创建 <see cref="GetPreferredResponseTypeEventArgs" />  的新实例(GetPreferredResponseTypeEventArgs)
		/// </summary>
		/// <param name="httpClient">当前的客户端</param>
		/// <param name="httpContext">当前的上下文环境</param>
		internal GetPreferredResponseTypeEventArgs(HttpClient httpClient, HttpContext httpContext, HttpRequestMessage request)
		{
			Request = request;
			HttpClient = httpClient;
			HttpContext = httpContext;
			Handled = false;
		}

	}

}