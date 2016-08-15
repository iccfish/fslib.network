namespace FSLib.Network.Http
{
	using System;
	using System.IO;

	/// <summary>
	/// 包含在获得对应的数据处理类事件中将会用到的参数
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GetPreferedResponseTypeEventArgs<T> : GetPreferedResponseTypeEventArgs
	{
		/// <summary>
		/// 创建 <see cref="GetPreferedResponseTypeEventArgs" />  的新实例(GetPreferedResponseTypeEventArgs)
		/// </summary>
		/// <param name="httpClient">当前的客户端</param>
		/// <param name="httpContext">当前的上下文环境</param>
		/// <param name="targetObject">已有目标数据</param>
		/// <param name="saveToFilePath">要将当前请求写入的文件路径</param>
		/// <param name="streamInvoker">处理当前数据片段的事件处理句柄</param>
		/// <param name="targetStream">要写入的目标流</param>
		internal GetPreferedResponseTypeEventArgs(HttpClient httpClient, HttpContext httpContext, T targetObject, string saveToFilePath, EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker, Stream targetStream, ExtraRequestInfo extraRequestInfo)
			: base(httpClient, httpContext, typeof(T), targetObject, saveToFilePath, streamInvoker, targetStream, extraRequestInfo)
		{
		}

		/// <summary>
		/// 获得已有目标数据
		/// </summary>
		public new T TargetObject { get { return (T)base.TargetObject; } }
	}

	/// <summary>
	/// 包含在获得对应的数据处理类事件中将会用到的参数
	/// </summary>
	public class GetPreferedResponseTypeEventArgs : EventArgs
	{
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
		public Type ResultType { get; private set; }

		/// <summary>
		/// 获得已有目标数据
		/// </summary>
		public object TargetObject { get; private set; }

		/// <summary>
		/// 获得要将当前请求写入的文件路径
		/// </summary>
		public string SaveToFilePath { get; private set; }

		/// <summary>
		/// 获得处理当前数据片段的事件处理句柄
		/// </summary>
		public EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> StreamInvoker { get; private set; }

		/// <summary>
		/// 获得要写入的目标流
		/// </summary>
		public Stream TargetStream { get; private set; }

		/// <summary>
		/// 获得或设置当前的数据处理类
		/// </summary>
		public HttpResponseContent ResponseContent { get; set; }

		/// <summary>
		/// 获得或设置是否已经结束处理
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		/// 额外的请求数据信息
		/// </summary>
		public ExtraRequestInfo ExtraRequestInfo { get; private set; }

		/// <summary>
		/// 创建 <see cref="GetPreferedResponseTypeEventArgs" />  的新实例(GetPreferedResponseTypeEventArgs)
		/// </summary>
		/// <param name="httpClient">当前的客户端</param>
		/// <param name="httpContext">当前的上下文环境</param>
		/// <param name="resultType">目标类型</param>
		/// <param name="targetObject">已有目标数据</param>
		/// <param name="saveToFilePath">要将当前请求写入的文件路径</param>
		/// <param name="streamInvoker">处理当前数据片段的事件处理句柄</param>
		/// <param name="targetStream">要写入的目标流</param>
		internal GetPreferedResponseTypeEventArgs(HttpClient httpClient, HttpContext httpContext, Type resultType, object targetObject, string saveToFilePath, EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs> streamInvoker, Stream targetStream, ExtraRequestInfo extraRequestInfo)
		{
			HttpClient = httpClient;
			HttpContext = httpContext;
			ResultType = resultType;
			TargetObject = targetObject;
			SaveToFilePath = saveToFilePath;
			StreamInvoker = streamInvoker;
			TargetStream = targetStream;
			ExtraRequestInfo = extraRequestInfo;
			Handled = false;
		}

	}

}