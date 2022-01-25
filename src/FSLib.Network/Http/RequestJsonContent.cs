using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 以JSON模式发送的对象
	/// </summary>
	public class RequestJsonContent : HttpRequestContent
	{
		string _jsonContent;
		object _object;

		/// <summary>
		/// 新建一个JSON请求内容对象
		/// </summary>
		/// <param name="jsonContent"></param>
		public RequestJsonContent(string jsonContent) : base(ContentType.Json)
		{
			_jsonContent = jsonContent;
		}

		/// <summary>
		/// 新建一个JSON请求内容对象
		/// </summary>
		/// <param name="obj"></param>
		public RequestJsonContent(object obj) : base(ContentType.Json)
		{
			_object = obj;
		}

		/// <summary>
		/// 准备发出请求
		/// </summary>
		/// <param name="request"></param>
		public override void Prepare(HttpWebRequest request)
		{
			if (_object != null)
				_jsonContent = Utils.JsonSerialize(_object, Context.JsonSerializationSetting);

			ContentType = ContentType.Json;
			base.Prepare(request);
		}

		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteTo(Stream stream)
		{
			stream.Write(Encoding.UTF8.GetBytes(_jsonContent));
		}

		/// <summary>
		/// 异步将数据写入当前的请求流中
		/// </summary>
		/// <param name="asyncData"></param>
		public override void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			base.WriteToAsync(asyncData);

			var buffer = (Message.Encoding ?? Encoding.UTF8).GetBytes(_jsonContent ?? "");
			asyncData.AsyncStreamWrite(buffer, 0, buffer.Length, false, null);
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength() => Encoding.UTF8.GetByteCount(_jsonContent);

		/// <summary>
		/// 销毁当前对象
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_object = null;
			_jsonContent = null;
		}
	}
}
