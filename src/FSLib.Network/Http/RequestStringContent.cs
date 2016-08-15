using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FSLib.Network.Http
{
	using System.IO;

	/// <summary>
	/// 表示提交一个字符串的请求内容
	/// </summary>
	public class RequestStringContent : HttpRequestContent
	{
		string _content;

		#region Overrides of HttpRequestContent

		/// <summary>
		/// 将当前的内容序列化到查询中
		/// </summary>
		public override string SerializeAsQueryString()
		{
			CheckDisposed();
			return System.Web.HttpUtility.UrlEncode(Content, Message.Encoding ?? Encoding.UTF8);
		}

		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteTo(Stream stream)
		{
			var buffer = (Message.Encoding ?? Encoding.UTF8).GetBytes(Content ?? "");
			stream.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength()
		{
			return (Message.Encoding ?? Encoding.UTF8).GetByteCount(Content);
		}

		#endregion

		/// <summary>
		/// 获得或设置要写入的内容
		/// </summary>
		public string Content
		{
			get
			{
				CheckDisposed();
				return _content;
			}
			set { _content = value; }
		}

		/// <summary>
		/// 创建 <see cref="RequestStringContent" />  的新实例(StringContent)
		/// </summary>
		public RequestStringContent(string content, ContentType contentType = ContentType.FormUrlEncoded) : base(contentType)
		{
			Content = content;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static implicit operator string(RequestStringContent content)
		{
			return content.Content;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static implicit operator RequestStringContent(string content)
		{
			return new RequestStringContent(content);
		}

		/// <summary>
		/// 异步将数据写入当前的请求流中
		/// </summary>
		/// <param name="asyncData"></param>
		public override void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			base.WriteToAsync(asyncData);

			var buffer = (Message.Encoding ?? Encoding.UTF8).GetBytes(Content ?? "");
			asyncData.AsyncStreamWrite(buffer, 0, buffer.Length, false, null);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Content = null;
		}
	}
}
