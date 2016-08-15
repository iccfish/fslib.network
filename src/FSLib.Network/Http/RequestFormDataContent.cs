using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表单数据
	/// </summary>
	public class RequestFormDataContent : HttpRequestContent
	{

		/// <summary>
		/// 创建 <see cref="RequestFormDataContent" />  的新实例(RequestFormDataContent)
		/// </summary>
		public RequestFormDataContent(ContentType contentType = ContentType.FormUrlEncoded) : base(contentType)
		{
			StringField = new Dictionary<string, string>();
			PostedFile = new List<HttpPostFile>();
		}

		/// <summary>
		/// 创建 <see cref="RequestFormDataContent" />  的新实例(RequestFormDataContent)
		/// </summary>
		public RequestFormDataContent(IDictionary<string, string> stringField, ContentType contentType = ContentType.FormUrlEncoded)
			: base(contentType)
		{
			StringField = stringField;
			PostedFile = new List<HttpPostFile>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			StringField = null;
			PostedFile = null;
			ProcessedData = null;
		}


		/// <summary>
		/// 预处理数据
		/// </summary>
		protected virtual void ProcessData()
		{
			if (PostedFile.Count != 0)
			{
				ContentType = ContentType.FormData;

				var index = 0;
				foreach (var postFile in PostedFile)
				{
					if (string.IsNullOrEmpty(postFile.FilePath))
						postFile.FilePath = "(fake path)";
					if (postFile.FieldName.IsNullOrEmpty())
						postFile.FieldName = $"file{++index}";
				}
			}

			if (ContentType == ContentType.FormUrlEncoded)
				ProcessedData = ProcessedData ?? StringField.Where(s => s.Value != null).ToDictionary(s => System.Web.HttpUtility.UrlEncode(s.Key, Message.Encoding), s => System.Web.HttpUtility.UrlEncode(s.Value, Message.Encoding));
			else
			{
				ProcessedData = ProcessedData ?? StringField.Where(s => s.Value != null).ToDictionary(s => s.Key, s => s.Value);
			}
		}

		/// <summary>
		/// 获得附加的文件列表
		/// </summary>
		public List<HttpPostFile> PostedFile { get; private set; }

		/// <summary>
		/// 已处理(转义)后的数据
		/// </summary>
		public Dictionary<string, string> ProcessedData { get; private set; }

		/// <summary>
		/// 获得或设置请求的分界
		/// </summary>
		public string RequestBoundary { get; set; }

		/// <summary>
		/// 获得文本域
		/// </summary>
		public IDictionary<string, string> StringField { get; private set; }

		#region Overrides of HttpRequestContent

		/// <summary>
		/// 将当前的内容序列化到查询中
		/// </summary>
		public override string SerializeAsQueryString()
		{
			if (PostedFile.Count > 0)
			{
				throw new InvalidOperationException("有文件附件时，不可以作为查询字符串，请使用POST查询。");
			}
			if (ProcessedData.Count == 0)
				return string.Empty;
			return ProcessedData.Select(s => s.Key + "=" + s.Value).Join("&");
		}

		/// <summary>
		/// 写入内容到流中
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteTo(Stream stream)
		{

			if (PostedFile.Count > 0 || ContentType == ContentType.FormData)
			{
				//写入普通区域
				foreach (var v in ProcessedData)
				{
					if (v.Value == null)
						continue;

					var str = "--" + RequestBoundary + "\r\nContent-Disposition: form-data; name=\"" + v.Key + "\"\r\n\r\n";
					stream.Write(Context.Request.Encoding.GetBytes(str));
					if (!v.Value.IsNullOrEmpty())
						stream.Write(Context.Request.Encoding.GetBytes(v.Value));
					stream.Write(Context.Request.Encoding.GetBytes("\r\n"));
				}

				//写入文件
				PostedFile.ForEach(s => s.WriteTo(stream));
				var endingstr = "--" + RequestBoundary + "--";
				stream.Write(Context.Request.Encoding.GetBytes(endingstr));
			}
			else
			{
				stream.Write((ContentType == ContentType.FormUrlEncoded ? System.Text.Encoding.ASCII : Message.Encoding).GetBytes(ProcessedData.Where(s => s.Value != null).Select(s => s.Key + "=" + s.Value).Join("&")));
			}
		}

		/// <summary>
		/// 异步将数据写入当前的请求流中
		/// </summary>
		/// <param name="asyncData"></param>
		public override void WriteToAsync(AsyncStreamProcessData asyncData)
		{
			base.WriteToAsync(asyncData);

			//异步写入的时候，如果没有文件，则一次性写入
			if (ContentType == ContentType.FormUrlEncoded)
			{
				asyncData.AsyncStreamWrite((ContentType == ContentType.FormUrlEncoded ? System.Text.Encoding.ASCII : Message.Encoding).GetBytes(ProcessedData.Where(s => s.Value != null).Select(s => s.Key + "=" + s.Value).Join("&")), false, null);
				return;
			}

			//否则先写入普通数据，再写入文件。
			byte[] textBuffer = null;
			using (var ms = new MemoryStream())
			{
				//写入普通区域
				foreach (var v in ProcessedData)
				{
					if (v.Value == null)
						continue;

					var str = "--" + RequestBoundary + "\r\nContent-Disposition: form-data; name=\"" + v.Key + "\"\r\n\r\n";
					ms.Write(Message.Encoding.GetBytes(str));
					if (!v.Value.IsNullOrEmpty())
						ms.Write(Message.Encoding.GetBytes(v.Value));
					ms.Write(Message.Encoding.GetBytes("\r\n"));
				}
				ms.Close();
				textBuffer = ms.ToArray();
			}

			asyncData.AsyncStreamWrite(textBuffer, true, _ =>
			{
				_currentAsyncFileIndex = 0;

				if (AsyncData.Exception != null)
					return;

				WriteFileAsync();
			});
		}

		int _currentAsyncFileIndex = 0;

		void WriteFileAsync()
		{
			if (_currentAsyncFileIndex >= PostedFile.Count)
			{
				FlushWriteFileAsync();
				return;
			}

			PostedFile[_currentAsyncFileIndex++].WriteToAsync(AsyncData, WriteFileAsync);
		}

		void FlushWriteFileAsync()
		{
			var endingstr = "--" + RequestBoundary + "--";
			AsyncData.AsyncStreamWrite(Message.Encoding.GetBytes(endingstr), false, null);
		}

		/// <summary>
		/// 计算长度
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength()
		{
			if (ContentType == ContentType.FormData)
			{
				if (string.IsNullOrEmpty(RequestBoundary))
				{
					RequestBoundary = "ifish_network_client_" + Guid.NewGuid().ToString().Replace("-", "");
				}

				var boundary = RequestBoundary;
				WebRequset.ContentType = "multipart/form-data; boundary=" + boundary;

				var size = ProcessedData.Where(s => s.Value != null).Select(
																		 s =>
																		(long)2 //--
																		+ boundary.Length //boundary
																		+ 2 //\r\n
																		+ 43 // content-disposition
																		+ s.Key.Length //key
																		+ Message.Encoding.GetByteCount(s.Value) //value
																		+ 2 //\r\n
					).Sum();

				//attach file
				size += PostedFile.Sum(s =>
				{
					s.AttachContext(Context);
					return s.ComputeLength();
				});
				size += 4 + boundary.Length; //结束标记
				return size;
			}
			else
			{
				return Math.Max(0, ProcessedData.Select(s => s.Key.Length + 1 + (s.Value == null ? 0 : (ContentType == ContentType.FormUrlEncoded ? s.Value.Length : Message.Encoding.GetByteCount(s.Value)))).Sum() + ProcessedData.Count - 1);
			}
		}

		/// <summary>
		/// 准备数据
		/// </summary>
		public override void PrepareData()
		{
			if (ProcessedData == null)
				ProcessData();

			base.PrepareData();
		}


		#endregion

	}
}
