using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Diagnostics;
	using System.Net;
	using System.Text.RegularExpressions;

	/// <summary>
	///
	/// </summary>
	[DebuggerDisplay("byte[] length={ResultStream.Length}")]
	public class ResponseBinaryContent : HttpResponseContent, IDisposable
	{
		byte[] _result;
		MemoryStream _resultStream;

		private string _stringResult;

		/// <summary>
		/// 创建 <see cref="ResponseBinaryContent"/>  的新实例(HttpBinaryResponse)
		/// </summary>
		public ResponseBinaryContent(HttpContext context, HttpClient client)
			: base(context, client)
		{

		}

		Encoding GetEncoding(string encoding)
		{
			var utfTest = Regex.Match(encoding, "utf-?([78])", RegexOptions.IgnoreCase);
			if (utfTest.Success)
			{
				return utfTest.GetGroupValue(1) == "7" ? Encoding.UTF7 : Encoding.UTF8;
			}
			try
			{
				return System.Text.Encoding.GetEncoding(encoding);
			}
			catch (Exception)
			{
			}

			return null;
		}

		/// <summary>
		/// 判断是否有UTF8标头
		/// </summary>
		/// <returns></returns>
		protected bool CheckUtf8Bom()
		{
			return Result.Length > 3 && Result[0] == 0xEF && Result[1] == 0xBB && Result[2] == 0xBF;
		}


		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_resultStream != null)
				{
					_resultStream.Dispose();
					_resultStream = null;
				}
			}
			_result = null;
			_stringResult = null;

		}

		/// <summary>
		/// 获得结果的字符串表现形式
		/// </summary>
		/// <returns></returns>
		protected virtual string GetDataString()
		{
			if (ResponseTextEncoding == null)
			{
				Encoding defaultEncoding = null;
				var removeBom = false;

				if (CheckUtf8Bom())
				{
					defaultEncoding = Encoding.UTF8;
					removeBom = Client.Setting.RemoveStringBom;
				}
				else
				{
					var contentType = Context.Response.Headers[HttpResponseHeader.ContentType];
					var charset = contentType == null ? null : _charsetHeaderSearch.Match(contentType);

					if (charset != null && charset.Success)
					{
						defaultEncoding = GetEncoding(charset.Groups[1].Value);
					}
					else
					{
						var dataAsAscii = Encoding.ASCII.GetString(Result, 0, Math.Min(Result.Length, Context.Client.Setting.DecodeForSearchCharsetRange));
						//查找可能的编码
						charset = _charsetSearch.Match(dataAsAscii);
						if (charset.Success)
						{
							defaultEncoding = GetEncoding(charset.Groups[1].Value);
						}
					}
				}
				ResponseTextEncoding = defaultEncoding ?? Context.Request.Encoding ?? System.Text.Encoding.UTF8;
				return GetDataString(removeBom);
			}

			return GetDataString(Client.Setting.RemoveStringBom && CheckUtf8Bom());
		}

		/// <summary>
		/// 使用指定的 <see cref="ResponseTextEncoding"/> 来获得最终的结果字符串
		/// </summary>
		/// <param name="removeBom">是否已经检测到BOM头。如果为 <see langword="true" />，则表明已经检测到了BOM头，最好能移除BOM头</param>
		/// <returns>代表结果的 <see langword="string"/></returns>
		protected virtual string GetDataString(bool removeBom)
		{
			return ResponseTextEncoding.GetString(Result, removeBom ? 3 : 0, removeBom ? Result.Length - 3 : Result.Length);
		}

		/// <summary>
		/// 请求处理最后的内容
		/// </summary>
		protected virtual void ProcessFinalResponse()
		{
			ResultStream.Seek(0, SeekOrigin.Begin);
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public override void Reset()
		{
			base.Reset();

			_stringResult = null;
			_resultStream = null;
			_result = null;
		}

		/// <summary>
		/// 获得响应的二进制内容。不是所有响应结果类型都可用
		/// </summary>
		public override byte[] RawBinaryData
		{
			get { return Result; }
			set { Result = value; }
		}

		/// <summary>
		/// 获得原始响应流。不是所有响应结果类型都可用
		/// </summary>
		public override MemoryStream RawStream
		{
			get { return ResultStream; }
			set { ResultStream = value; }
		}

		/// <summary>
		/// 获得响应的文本内容。不是所有响应结果类型都可用
		/// </summary>
		public override string RawStringResult
		{
			get { return StringResult; }
			set { StringResult = value; }
		}

		/// <summary>
		/// 获得响应文本编码
		/// </summary>
		public virtual Encoding ResponseTextEncoding { get; protected set; }

		/// <summary>
		/// 获得结果
		/// </summary>
		public byte[] Result
		{
			get
			{
				CheckDisposed();
				return _result ?? (_result = ResultStream.ToArray());
			}
			set { _result = value; }
		}

		/// <summary>
		/// 获得响应的内存流
		/// </summary>
		public MemoryStream ResultStream
		{
			get
			{
				CheckDisposed();
				return _resultStream;
			}
			set { _resultStream = value; }
		}

		/// <summary>
		/// 获得字符串结果
		/// </summary>
		public string StringResult
		{
			get
			{
				CheckDisposed();
				return _stringResult ?? (_stringResult = GetDataString());
			}
			set { _stringResult = value; }
		}

		#region Overrides of HttpResponseContent

		byte[] _buffer;

		/// <summary>
		/// 处理响应
		/// </summary>
		/// <param name="stream"></param>
		protected override void ProcessResponse(Stream stream)
		{
			ResultStream = new System.IO.MemoryStream();
			var buffer = new byte[Client.Setting.ReadBufferSize];
			var count = 0;
			while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				ResultStream.Write(buffer, 0, count);
			}
			ResultStream.Flush();
			ProcessFinalResponse();
			ResultStream.Seek(0, SeekOrigin.Begin);
		}


		/// <summary>
		/// 异步处理响应
		/// </summary>
		protected override void ProcessResponseAsync()
		{
			ResultStream = new MemoryStream();

			_buffer = new byte[AsyncData.HttpContext.Client.Setting.ReadBufferSize];
			AsyncData.Stream.BeginRead(_buffer, 0, _buffer.Length, NetworkReadCallback, this);
		}

		void NetworkReadCallback(IAsyncResult ar)
		{
			var count = 0;
			try
			{
				count = AsyncData.Stream.EndRead(ar);
				if (count > 0) ResultStream.Write(_buffer, 0, count);
			}
			catch (Exception exception)
			{
				AsyncData.Exception = exception;
				CompleteCallback();

				return;
			}

			if (count == 0)
			{
				ProcessFinalResponse();
				ResultStream.Seek(0, SeekOrigin.Begin);
				CompleteCallback();
			}
			else
			{
				AsyncData.Stream.BeginRead(_buffer, 0, _buffer.Length, NetworkReadCallback, this);
			}
		}

		#endregion

		static Regex _charsetSearch = new Regex("<meta.*?charset=[\'\"]?([a-zA-Z0-9-]+)", RegexOptions.IgnoreCase);
		static Regex _charsetHeaderSearch = new Regex(";\\s*charset=[\'\"]?([a-zA-Z0-9-]+)", RegexOptions.IgnoreCase);
	}
}
