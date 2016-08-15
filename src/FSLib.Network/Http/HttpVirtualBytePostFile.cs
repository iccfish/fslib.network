using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表示一个内存数据的虚拟上传文件
	/// </summary>
	public class HttpVirtualBytePostFile : HttpPostFile
	{
		/// <summary>
		/// 获得或设置文件内容
		/// </summary>
		public virtual byte[] Data { get; set; }

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpVirtualBytePostFile(string filePath, byte[] data)
			: this(null, filePath, data)
		{
		}

		/// <summary>
		/// 创建 <see cref="HttpPostFile" />  的新实例(HttpPostFile)
		/// </summary>
		public HttpVirtualBytePostFile(string fieldName, string filePath, byte[] data)
			: base(fieldName, filePath)
		{
			Data = data;
		}

		/// <summary>
		/// 计算数据区长度
		/// </summary>
		/// <returns></returns>
		protected override long ComputeBodyLength()
		{
			return Data?.Length ?? 0;
		}

		/// <summary>
		/// 写入数据区
		/// </summary>
		/// <param name="stream"></param>
		protected override void WriteBody(System.IO.Stream stream)
		{
			if (Data != null)
				stream.Write(Data, 0, Data.Length);
		}

		protected override void WriteBodyAsync()
		{
			if (Data != null)
				AsyncData.AsyncStreamWrite(Data, true, _ => WriteFooterAsync());
			else WriteFooterAsync();
		}
	}
}
