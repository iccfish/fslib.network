using System;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace FSLib.Network.Http
{
	/// <summary>
	/// XML格式的请求内容
	/// </summary>
	public class RequestXmlContent : RequestByteBufferContent
	{
		/// <summary>
		/// 获得或设置要发送的XML文本
		/// </summary>
		public string Xml { get; set; }

		/// <summary>
		/// 创建 <see cref="RequestXmlContent"/> 的新对象类型
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="contentType"></param>
		public RequestXmlContent(string xml, ContentType contentType = ContentType.Xml)
		{
			Xml = xml;
			ContentType = contentType;
		}
		/// <summary>
		/// 创建 <see cref="RequestXmlContent"/> 的新对象类型
		/// </summary>
		public RequestXmlContent(XmlDocument xml, ContentType contentType = ContentType.Xml)
		{
			Xml = xml.OuterXml;
			ContentType = contentType;
		}

		/// <summary>
		/// 创建 <see cref="RequestXmlContent"/> 的新对象类型
		/// </summary>
		public RequestXmlContent(object obj, ContentType contentType = ContentType.Xml)
		{
			Xml = ConvertToXmlString(obj);
			ContentType = contentType;
		}

		/// <summary>
		/// 获得请求内容
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected virtual string ConvertToXmlString(object obj)
		{
			if (obj == null)
				return string.Empty;

			var xml = (obj as XmlDocument)?.OuterXml ?? (obj as XmlNode)?.OuterXml;

			if (xml == null)
			{
				if (obj is XDocument)
				{
					var xd = obj as XDocument;
					var tw = new System.IO.StringWriter();
					xd.Save(tw);

					xml = tw.ToString();
				}
			}

			var document = obj as XmlDocument;
			if (document != null)
				return document.OuterXml;
			var node = obj as XmlNode;
			if (node != null)
				return node.OuterXml;

			return obj.XmlSerializeToString();
		}

		#region Overrides of HttpRequestContent

		/// <summary>
		/// 准备发出请求
		/// </summary>
		/// <param name="request"></param>
		public override void Prepare(HttpWebRequest request)
		{
			base.Prepare(request);
			Buffer = Context.Request.Encoding.GetBytes(Xml);
			request.ContentType = "text/" + ContentType.ToString().ToLower() + "; charset=" + this.Context.Request.Encoding.WebName;
		}

		#endregion

		/// <summary>
		/// 销毁当前对象
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Xml = null;
		}
	}
}