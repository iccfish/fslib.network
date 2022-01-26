namespace FSLib.Network.Http.ObjectWrapper
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Xml;

	public class DefaultContentPayloadBuilder : IContentPayloadBuilder
	{
		/// <inheritdoc />
		public void GetResponseContent(GetPreferredResponseTypeEventArgs ea)
		{
			GlobalEvents.OnBeforeRequestGetPreferedResponseType(this, ea);
			if (ea.Handled)
				return;

			var content = ea.ResponseContent;
			var req = ea.Request;
			var ctx = ea.HttpContext;
			var client = ea.HttpClient;
			var resultType = req.ExceptType;
			var contentType = ea.HttpContext.WebResponse.ContentType ?? "";
			var status = (int)ea.HttpContext.WebResponse.StatusCode;

			var index = contentType.IndexOf(";");
			if (index != -1) contentType = contentType.Substring(0, index); //分解带字符串的


			if (!req.SaveToFile.IsNullOrEmpty())
			{
				content = new ResponseFileContent(ctx, client, req.SaveToFile);
			}
			else
			{
				if (resultType == typeof(string) || contentType.StartsWith("text") || contentType == "application/x-javascript")
					content = new ResponseStringContent(ctx, client);
				else if (resultType == typeof(Image) || contentType.StartsWith("image"))
					content = new ResponseImageContent(ctx, client); //图片
				else if (resultType == typeof(XmlDocument) || contentType == "text/xml")
					content = new ResponseXmlContent(ctx, client, null); //XML
				else if (status >= 300) //30x-50x 请求均默认当作文本类型
					content = new ResponseStringContent(ctx, client);
				else if (resultType == typeof(byte[]))
					content = new ResponseBinaryContent(ctx, client);
				else if (typeof(Stream).IsAssignableFrom(resultType))
					content = new ResponseCopyStreamContent(ctx, client, req.CopyToStream ?? new MemoryStream());
				else if (typeof(HttpResponseContent).IsAssignableFrom(resultType))
					content = (HttpResponseContent)req.ExceptObject;
				else if (resultType == typeof(XmlDocument))
					content = new ResponseXmlContent(ctx, client, (XmlDocument)req.ExceptObject);
				else if (resultType == typeof(EventHandler<ResponseStreamContent.RequireProcessStreamEventArgs>))
				{
					var r = new ResponseStreamContent(ctx, client);
					if (req.streamDataHandler != null)
					{
						r.RequireProcessStream += req.streamDataHandler;
					}

					content = r;
				}
				else
					content = new ResponseObjectContent(ctx, client);

			}

			ea.ResponseContent = content;

			//Global events
			GlobalEvents.OnRequestGetPreferedResponseType(this, ea);

			//http handler
			ctx.Client.HttpHandler.GetPreferredResponseType(ea);
		}

		static Dictionary<Type, ContentType?> _contentTypeAttributeDefineCache = new();

		/// <summary>
		/// 判断指定的对象是否定义了JSON请求结果属性
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public virtual ContentType? GetPreferContentType(Type t)
		{
			return _contentTypeAttributeDefineCache.GetValue(t,
				_ =>
				{
					var att = _.GetCustomerAttributes<ContentTypeAttribute>().FirstOrDefault();

					return att.SelectValue(s => (ContentType?)s.ContentType);
				});
		}

		/// <summary>
		/// 对请求数据进行包装，转换为合适的请求类型，并返回对应的负载类型
		/// </summary>
		/// <returns>经过包装的 <see cref="HttpRequestContent"/> 对象</returns>
		public virtual void WrapRequestContent(RequestWrapRequestContentEventArgs e)
		{
			if (e.RequestMessage.RequestPayload == null)
				return;

			var message = e.RequestMessage;
			var data = message.RequestPayload;
			e.RequestMessage.ContentType ??= e.RequestMessage.ContentType ?? GetPreferContentType(data.GetType()) ?? e.HttpClient.Setting.DefaultRequestContentType;
			GlobalEvents.OnBeforeRequestWrapRequestContent(this, e);
			var content = e.RequestContent;
			if (e.Handled)
				return;

			var type = e.RequestMessage.ContentType.Value;

			if (content == null)
			{
				if (data is HttpRequestContent v)
					content = v;
				else if (data is string tstr)
					content = WrapRequestDataToStringContent(tstr, type);
				else if (data is Stream tStream)
					content = WrapRequestDataToStreamContent(tStream, type);
				else if (data is byte[] bytes)
					content = WrapRequestDataToByteBufferContent(bytes, type);
				else if (data is IDictionary<string, string> dic && type != ContentType.Json)
					content = WrapRequestDataToFormDataContent(dic, type);
				else if (data is XmlDocument || data is XmlNode || data is System.Xml.Linq.XDocument)
					content = WrapRequestDataToXmlContent(data, type);
				else if (type == ContentType.Json)
					content = WrapRequestDataToJsonContent(data);
				else if (type == ContentType.Xml)
					content = WrapRequestDataToXmlContent(data, type);
				else
					content = WrapRequestDataToObjectContent(data, type);
			}

			e.RequestContent = content;

			//全局事件
			GlobalEvents.OnRequestWrapRequestContent(this, e);
			e.HttpClient.HttpHandler.WrapRequestContent(e);
		}

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToObjectContent<T>(T data, ContentType contentType) => new RequestObjectContent<T>(data, contentType);

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToFormDataContent(IDictionary<string, string> data, ContentType contentType) => new RequestFormDataContent(data, contentType);

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToStringContent(string data, ContentType contentType) => new RequestStringContent(data, contentType);

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToStreamContent(Stream data, ContentType contentType) => new RequestCopyStreamContent(data, contentType);

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToByteBufferContent(byte[] data, ContentType contentType) => new RequestByteBufferContent(data, contentType: contentType);

		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="contentType"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToXmlContent<T>(T data, ContentType contentType) => new RequestXmlContent(data, contentType);


		/// <summary>
		/// 将数据包装为指定的请求对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public virtual HttpRequestContent WrapRequestDataToJsonContent<T>(T data) => new RequestJsonContent(data);
	}
}
