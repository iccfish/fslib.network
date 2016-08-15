using System;
using System.IO;
using System.Net;
using System.Threading;

namespace FSLib.Network.Http
{
	using System.Diagnostics;

	/// <summary>
	/// 异步处理的参数
	/// </summary>
	public class AsyncStreamProcessData
	{
		/// <summary>
		/// 获得当前的Stream
		/// </summary>
		public Stream Stream { get; private set; }

		public HttpContext HttpContext { get; private set; }

		/// <summary>
		/// 用于继续处理的回调
		/// </summary>
		public Action<AsyncStreamProcessData> WriteCallback { get; private set; }

		/// <summary>
		/// 获得或设置在处理中遇到的异常数据
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// 获得当前的WebRequest
		/// </summary>
		public HttpWebRequest HttpWebRequest
		{
			get { return HttpContext == null ? null : HttpContext.WebRequest; }
		}

		/// <summary>
		/// 创建 <see cref="AsyncStreamProcessData" />  的新实例(AsyncStreamCopyData)
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="httpContext"></param>
		/// <param name="writeCallback"></param>
		public AsyncStreamProcessData(Stream stream, HttpContext httpContext, Action<AsyncStreamProcessData> writeCallback)
		{
			Stream = stream;
			HttpContext = httpContext;
			WriteCallback = writeCallback;
		}

		int _notified;

		/// <summary>
		/// 通知控制器已经完成
		/// </summary>
		public void NotifyAsyncComplete()
		{
			if (Interlocked.Exchange(ref _notified, 1) != 0)
				return;

			if (WriteCallback != null)
				WriteCallback(this);
		}

		/// <summary>
		/// 异步写入流
		/// </summary>
		internal IAsyncResult AsyncStreamWrite(byte[] buffer, bool needContinue, Action<IAsyncResult> resumeCallback)
		{
			return AsyncStreamWrite(buffer, 0, buffer.Length, needContinue, resumeCallback);
		}

		/// <summary>
		/// 异步写入流
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="needContinue"></param>
		/// <param name="resumeCallback"></param>
		/// <returns></returns>
		internal IAsyncResult AsyncStreamWrite(byte[] buffer, int offset, int count, bool needContinue, Action<IAsyncResult> resumeCallback)
		{
			return Stream.BeginWrite(buffer, offset, count, _ =>
			{
				try
				{
					Stream.EndWrite(_);
				}
				catch (Exception ex)
				{
					Exception = ex;
				}

				if (Exception != null || !needContinue)
				{
					NotifyAsyncComplete();
				}
				else if (resumeCallback != null)
				{
					resumeCallback(_);
				}
			}, this);
		}
	}
}