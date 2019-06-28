using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Text.RegularExpressions;

	using Extension;
	using Newtonsoft.Json.Linq;

	public abstract class ResponseObjectContentBase<T> : ResponseBinaryContent
	{
		T _o;

		/// <summary>
		/// 创建 <see cref="ResponseObjectContentBase{T}"/>  的新实例(HttpObjectResponseContent)
		/// </summary>
		protected ResponseObjectContentBase(HttpContext context, HttpClient client)
			: base(context, client)
		{
		}

		/// <summary>
		/// 获得反序列化的结果
		/// </summary>
		public T Object
		{
			get
			{
				CheckDisposed();
				return _o;
			}
			protected set { _o = value; }
		}

		T _objectInternal;

		internal T ObjectInternal { get { return _objectInternal; } set { _objectInternal = Object = value; } }

		/// <summary>
		/// 返回的数据类型
		/// </summary>
		public ResponseContentType ContentType { get; protected set; }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_o != null && _o is IDisposable)
				{
					(_o as IDisposable).Dispose();
				}
				_o = default(T);
			}
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			Object = _objectInternal;
		}
	}

	/// <summary>
	/// 反序列化结果
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ResponseObjectContent<T> : ResponseObjectContentBase<T>
	{
		/// <summary>
		/// 创建 <see cref="ResponseObjectContentBase{T}"/>  的新实例(HttpObjectResponseContent)
		/// </summary>
		public ResponseObjectContent(HttpContext context, HttpClient client)
			: base(context, client)
		{
			if (!HttpSetting.DisableComptatibleCheck)
				HttpSetting.CheckObjectTypeSupport(typeof(T));
		}

		#region Overrides of HttpResponseContent

		/// <summary>
		/// 请求处理最后的内容
		/// </summary>
		protected override void ProcessFinalResponse()
		{
			OnPreContentProcessed();
			base.ProcessFinalResponse();

			//查找开始标记
			var index = CheckUtf8Bom() ? 3 : 0;
			while ((index < Result.Length && (Result[index] == ' ' || Result[index] == '\t' || Result[index] == '\r' || Result[index] == '\n')))
			{
				index++;
			}
			if (index >= Result.Length)
			{
				Object = default(T);
				return;
			}


			var isJobject = typeof(T) == typeof(JObject) || typeof(T) == typeof(object);

			try
			{
				if (Result[index] == '<')
				{
					//XML反序列化
					Object = (T)typeof(T).XmlDeserialize(Utils.RemoveXmlDeclaration(Utils.NormalizeString(StringResult)));
					ContentType = ResponseContentType.Xml;
				}
				else if (Result[index] == '{' || Result[index] == '[')
				{
					ContentType = ResponseContentType.Json;
					if (isJobject)
					{
						Object = (T)(object)JObject.Parse(StringResult);
					}
					else
					{
						//JSON反序列化
						Object = Context.JsonDeserialize<T>(StringResult, Object);
					}
				}
				else
				{
					//根据目标结果的类型判断
					var jsonp = string.Empty;

					if (!IsBinaryContent() && !string.IsNullOrEmpty(jsonp = GetJsonPContent(StringResult)))
					{
						//尝试找到JSONP
						ContentType = ResponseContentType.JsonP;

						if (isJobject)
						{
							Object = (T)(object)JObject.Parse(jsonp);
						}
						else
						{
							//JSON反序列化
							Object = Context.JsonDeserialize<T>(jsonp, Object);
						}
					}
					else
					{
						//二进制序列化
						Object = (T)BinarySerializeHelper.DeserialzieFromBytes(Result);
						ContentType = ResponseContentType.Binary;
					}

				}
			}
			catch (Exception ex)
			{
				if (ex is InvalidOperationException)
				{
					ex = new ObjectSerializationNotSupportException(typeof(T), ex);
				}

				Exception = ex;
				Object = default(T);
				if (AsyncData == null)
					throw;

				AsyncData.Exception = ex;
			}
			OnPostContentProcessed();
		}

		#endregion

		/// <summary>
		/// 当返回是JsonP的时候，对应的回调函数名
		/// </summary>
		public string JsonpCallbackName { get; private set; }

		/// <summary>
		/// 判断当前的响应是否是二进制响应。如果不是，则会尝试进行文本的反序列化
		/// </summary>
		/// <returns></returns>
		protected virtual bool IsBinaryContent()
		{
			return Result.Take(Math.Min(10, Result.Length)).Any(s => s < 20);
		}

		/// <summary>
		/// 从一段文本中获得JSON内容
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		protected virtual string GetJsonPContent(string content)
		{
			var m = Regex.Match(content, @"^.*?\s*([a-zA-Z_\d]+)\s*\(", RegexOptions.Singleline | RegexOptions.IgnoreCase);

			if (!m.Success)
				return null;

			//找到结尾
			var index = content.Length - 1;
			while (index > m.Index && content[index] != '}')
				index--;

			if (index <= m.Index + m.Length)
				return null;

			JsonpCallbackName = m.GetGroupValue(1);
			return content.Substring(m.Index + m.Length, index - m.Index - m.Length + 1);
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			JsonpCallbackName = null;
		}
	}
}
