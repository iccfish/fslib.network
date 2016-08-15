using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;
	using System.Threading;

	public class ResponseCopyStreamContent : HttpResponseContent
	{
		/// <summary>
		/// 获得关联的流
		/// </summary>
		public Stream Stream { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpResponseContent" />  的新实例(HttpResponseContent)
		/// </summary>
		public ResponseCopyStreamContent(HttpContext context, HttpClient client, Stream stream)
			: base(context, client)
		{
			if (stream != null && !stream.CanWrite)
			{
				throw new ArgumentException("targetStream can not write.");
			}

			Stream = stream;
		}

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="stream"></param>
		protected override void ProcessResponse(Stream stream)
		{
#if NET_GT_4
			stream.CopyTo(Stream);
#else
			var buffer = new byte[4 * 0x400];
			var count = 0;
			while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				Stream.Write(buffer, 0, count);
			}
#endif
		}

		/// <summary>
		/// 异步处理响应
		/// </summary>
		protected override void ProcessResponseAsync()
		{
			var buffer = new byte[AsyncData.HttpContext.Client.Setting.ReadBufferSize];
			AsyncData.Stream.BeginRead(buffer, 0, buffer.Length, NetworkReadCallback, buffer);
		}

		void NetworkReadCallback(IAsyncResult ar)
		{
			int count;
			try
			{
				count = AsyncData.Stream.EndRead(ar);
			}
			catch (Exception exception)
			{
				AsyncData.Exception = exception;
				CompleteCallback();

				return;
			}

			if (count == 0)
			{
				CompleteCallback();
			}
			else
			{
				Stream.Write(ar.AsyncState as byte[], 0, count);

				var buffer = new byte[AsyncData.HttpContext.Client.Setting.ReadBufferSize];
				AsyncData.Stream.BeginRead(buffer, 0, buffer.Length, NetworkReadCallback, buffer);
			}
		}
	}
}
