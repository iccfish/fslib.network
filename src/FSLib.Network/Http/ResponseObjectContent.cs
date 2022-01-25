using System;

namespace FSLib.Network.Http
{
	using System.Text.RegularExpressions;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// 包装了对象的响应结果
	/// </summary>
	public class ResponseObjectContent : ResponseBinaryContent
	{
		private JsonDeserializationSetting DeserializationSetting { get => Client.Setting.JsonDeserializationSetting; }

		/// <summary>
		/// 创建 <see cref="ResponseObjectContent"/>  的新实例(HttpObjectResponseContent)
		/// </summary>
		internal ResponseObjectContent(HttpContext context, HttpClient client)
			: base(context, client)
		{
			if (!HttpSetting.DisableComptatibleCheck)
				HttpSetting.CheckObjectTypeSupport(context.Request.ExceptType);
		}


		/// <summary>
		/// 获得或设置最终结果
		/// </summary>
		public object Object { get; private set; }

		/// <summary>
		/// 获得或设置最终内容类型
		/// </summary>
		public ResponseContentType ContentType { get; private set; }

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
				Object = null;
				return;
			}

			var requestType = Context.Request.ExceptType;
			var isJobject = requestType == typeof(JObject) || requestType == typeof(object);

			try
			{
				if (Result[index] == '<')
				{
					//XML反序列化
					Object = Context.Request.ExceptType.XmlDeserialize(Utils.RemoveXmlDeclaration(Utils.NormalizeString(StringResult)));
					ContentType = ResponseContentType.Xml;
				}
				else if (Result[index] == '{' || Result[index] == '[')
				{
					ContentType = ResponseContentType.Json;
					if (isJobject)
					{
						Object = JObject.Parse(StringResult);
					}
					else
					{
						//JSON反序列化
						Object = Utils.JsonDeserialize(StringResult, Context.Request.ExceptObject, Context.Request.ExceptType, DeserializationSetting);
					}
				}
				else
				{
					//根据目标结果的类型判断
					var jsonp = string.Empty;

					if (!string.IsNullOrEmpty(jsonp = GetJsonPContent(StringResult)))
					{
						//尝试找到JSONP
						ContentType = ResponseContentType.JsonP;

						Object = isJobject ? JObject.Parse(jsonp) : Utils.JsonDeserialize(jsonp, Context.Request.ExceptObject, Context.Request.ExceptType, DeserializationSetting);
					}
					else
					{
						throw new NotSupportedException("Response message not supported.");
					}

				}
			}
			catch (Exception ex)
			{
				if (ex is InvalidOperationException)
				{
					ex = new ObjectSerializationNotSupportException(requestType, ex);
				}

				Exception = ex;
				Object = default;
				if (AsyncData == null)
					throw;

				AsyncData.Exception = ex;
			}
			OnPostContentProcessed();
		}


		/// <summary>
		/// 当返回是JsonP的时候，对应的回调函数名
		/// </summary>
		public string JsonpCallbackName { get; private set; }


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
