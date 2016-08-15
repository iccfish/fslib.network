using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 模拟的上传文件
	/// </summary>
	public class HttpVirtualStreamPostFile : HttpPostFile
	{
		/// <summary>
		/// 获得或设置文件内容
		/// </summary>
		public virtual Stream Stream { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpVirtualStreamPostFile(Stream stream)
			: this(null, stream)
		{
		}

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpVirtualStreamPostFile(string filePath, Stream stream)
			: this(null, filePath, stream)
		{
		}

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpVirtualStreamPostFile(string fieldName, string filePath, Stream stream)
			: base(fieldName, filePath)
		{
			if (stream == null)
				throw new ArgumentNullException("stream", "stream is null.");
			if (!stream.CanSeek || !stream.CanRead) throw new InvalidOperationException();

			Stream = stream;
		}

		/// <summary>
		/// 计算数据区长度
		/// </summary>
		/// <returns></returns>
		protected override long ComputeBodyLength()
		{
			return Stream.Length;
		}

		/// <summary>
		/// 写入数据区
		/// </summary>
		/// <param name="stream"></param>
		protected override void WriteBody(System.IO.Stream stream)
		{
			var length = Stream.Length;

			var ee = new DataProgressEventArgs(length, 0L);
			var buffer = new byte[4 * 400];
			int count;
			var op = Context.Operation;

			if (op == null)
				OnProgressChanged(ee);
			else
				op.Post(_ => OnProgressChanged(ee), null);

			while ((count = Stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				stream.Write(buffer, 0, count);
				ee = new DataProgressEventArgs(length, Stream.Position);
				if (op == null)
					OnProgressChanged(ee);
				else
					op.Post(_ => OnProgressChanged(ee), null);
			}
		}

		byte[] _buffer;

		protected override void WriteBodyAsync()
		{
			_buffer = new byte[0x400 * 4];
			var ee = new DataProgressEventArgs(Stream.Length, 0L);
			var op = Context.Operation;
			if (op == null)
				OnProgressChanged(ee);
			else op.Post(_ => OnProgressChanged(ee), null);
			ReadStreamAsync();
		}

		void ReadStreamAsync()
		{
			Stream.BeginRead(_buffer, 0, _buffer.Length, _ =>
			{
				try
				{
					var count = Stream.EndRead(_);
					var ee = new DataProgressEventArgs(Stream.Length, Stream.Position);
					var op = Context.Operation;
					if (op == null)
						OnProgressChanged(ee);
					else op.Post(__ => OnProgressChanged(ee), null);
					if (count == 0)
					{
						Stream.Close();
						WriteFooterAsync();
					}
					else
					{
						AsyncData.AsyncStreamWrite(_buffer, 0, count, true, __ => ReadStreamAsync());
					}
				}
				catch (Exception ex)
				{
					AsyncData.Exception = ex;
					AsyncData.NotifyAsyncComplete();
				}
			}, null);

		}

	}
}
