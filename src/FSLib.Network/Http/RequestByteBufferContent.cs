using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 字节数组上传数据
	/// </summary>
	public class RequestByteBufferContent : HttpRequestContent
	{
		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteTo(System.IO.Stream stream)
		{
			stream.Write(Buffer, 0, Length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asyncData"></param>
		public override void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			base.WriteToAsync(asyncData);

			asyncData.AsyncStreamWrite(Buffer, 0, Buffer.Length, false, null);
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength()
		{
			return Length;
		}

		/// <summary>
		/// 获得或设置缓冲内容
		/// </summary>
		public byte[] Buffer { get; set; }

		/// <summary>
		/// 获得或设置索引
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// 获得或设置长度
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// 创建 <see cref="RequestByteBufferContent" />  的新实例(ByteBufferContent)
		/// </summary>
		public RequestByteBufferContent(byte[] buffer, int? offset = null, int? length = null, ContentType contentType = ContentType.FormUrlEncoded) : base(contentType)
		{
			if (buffer == null)
				throw new ArgumentException("buffer is null or empty.", "buffer");

			Buffer = buffer;
			Offset = offset ?? 0;
			Length = length ?? buffer.Length;
		}

		public RequestByteBufferContent()
		{

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static implicit operator byte[] (RequestByteBufferContent content)
		{
			return content.Buffer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static implicit operator RequestByteBufferContent(byte[] buffer)
		{
			return new RequestByteBufferContent(buffer);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Buffer = null;
		}
	}
}
