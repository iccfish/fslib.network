namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 记录的内容
	/// </summary>
	public class HttpMonitorItem
	{
		/// <summary>
		/// 对应的HttpContext实例
		/// </summary>
		public HttpContext HttpContext { get; private set; }

		/// <summary>
		/// 对应的监听器
		/// </summary>
		public HttpMonitor HttpMonitor { get; private set; }

		/// <summary>
		/// 创建 <see cref="HttpMonitorItem" />  的新实例(HttpMonitorItem)
		/// </summary>
		/// <param name="httpContext"></param>
		public HttpMonitorItem(HttpContext httpContext, HttpMonitor httpMonitor)
		{
			HttpContext = httpContext;
			HttpMonitor = httpMonitor;
		}

		/// <summary>
		/// 获得监控的请求状态
		/// </summary>
		public HttpContextState ContextState { get { return HttpContext.ReadyState; } }

		/// <summary>
		/// 获得请求的写入流
		/// </summary>
		public StreamWithEventsWrapper RequestStream { get; private set; }

		/// <summary>
		/// 设置写入流
		/// </summary>
		/// <param name="stream"></param>
		internal void SetRequestStream(HttpStreamWrapper stream)
		{
			if (!HttpMonitor.RecordRequestContent || (HttpMonitor.MaxRecordContentSize > 0 && HttpMonitor.MaxRecordContentSize < stream.Length && stream.Length > 0))
				return;

			stream.EnableMirror();
			RequestStream = stream.MirrorStream;
		}

		/// <summary>
		/// 原始的响应流
		/// </summary>
		public StreamWithEventsWrapper ResponseRawStream { get; private set; }

		/// <summary>
		/// 响应流
		/// </summary>
		public StreamWithEventsWrapper ResponseStream { get; private set; }

		/// <summary>
		/// 设置原始响应流
		/// </summary>
		/// <param name="stream"></param>
		internal void SetRawResponseStream(HttpStreamWrapper stream)
		{
			if (!HttpMonitor.RecordResponseContent || (HttpMonitor.MaxRecordContentSize > 0 && HttpMonitor.MaxRecordContentSize < stream.Length && stream.Length > 0))
				return;

			stream.EnableMirror();
			ResponseRawStream = stream.MirrorStream;
		}

		/// <summary>
		/// 设置响应流
		/// </summary>
		/// <param name="stream"></param>
		internal void SetResponseStream(HttpStreamWrapper stream)
		{
			if (!HttpMonitor.RecordResponseContent || (HttpMonitor.MaxRecordContentSize > 0 && HttpMonitor.MaxRecordContentSize < stream.Length && stream.Length > 0))
				return;

			stream.EnableMirror();
			ResponseStream = stream.MirrorStream;
		}
	}
}