using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 表示向当前HTTP请求的一个写入流内容
	/// </summary>
	public class RequestCopyStreamContent : HttpRequestContent
	{
		/// <summary>
		/// 获得或设置文件内容
		/// </summary>
		public virtual Stream Stream
		{
			get
			{
				CheckDisposed();
				return _stream;
			}
			set { _stream = value; }
		}

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		/// <exception cref="ArgumentNullException">stream</exception>
		/// <exception cref="InvalidOperationException">流不可读或不可搜索. </exception>
		public RequestCopyStreamContent(Stream stream, ContentType contentType = ContentType.FormUrlEncoded) : base(contentType)
		{
			if (stream == null)
				throw new ArgumentNullException("stream", "stream is null.");
			if (!stream.CanRead)
				throw new InvalidOperationException();

			Stream = stream;
		}
		#region Overrides of HttpRequestContent

		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteTo(Stream stream)
		{
			var buffer = new byte[0x400 * 4];
			var count = 0;
			while ((count = Stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				stream.Write(buffer, 0, count);
			}
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength()
		{
			if (!Stream.CanSeek)
			{
				return -1L;
			}
			return Stream.Length;
		}

		#endregion

		/// <summary>
		/// 允许隐式转换为流
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator System.IO.Stream(RequestCopyStreamContent obj)
		{
			obj.CheckDisposed();
			return obj.Stream;
		}

		/// <summary>
		/// 允许隐式转换为流
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static implicit operator RequestCopyStreamContent(System.IO.Stream obj)
		{
			return new RequestCopyStreamContent(obj);
		}

		/// <summary>
		/// 异步将数据写入当前的请求流中
		/// </summary>
		/// <param name="asyncData"></param>
		public override void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			base.WriteToAsync(asyncData);

			_buffer = new byte[Client.Setting.WriteBufferSize];
			BeginReadSourceStream();
		}

		byte[] _buffer;
		Stream _stream;

		void BeginReadSourceStream()
		{
			Stream.BeginRead(_buffer, 0, _buffer.Length, _ =>
			{
				try
				{
					var count = Stream.EndRead(_);
					if (count > 0)
						AsyncData.AsyncStreamWrite(_buffer, 0, count, true, __ => BeginReadSourceStream());
					else
					{
						AsyncData.NotifyAsyncComplete();
					}
				}
				catch (Exception ex)
				{
					AsyncData.Exception = ex;
					AsyncData.NotifyAsyncComplete();
				}
			}, this);
		}

		/// <summary>
		/// 准备数据
		/// </summary>
		public override void PrepareData()
		{
			if (_stream.CanSeek)
				_stream.Seek(0, SeekOrigin.Begin);

			base.PrepareData();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Stream = null;
		}
	}
}
