using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FSLib.Network.Http
{
	using System.Xml;

	/// <summary>
	/// 
	/// </summary>
	public class ResponseXmlContent : ResponseStringContent
	{
		XmlDocument _xmlDocument;
		private bool _created = false;

		/// <summary>
		/// 创建 <see cref="ResponseXmlContent"/>  的新实例(HttpXmlResponse)
		/// </summary>
		public ResponseXmlContent(HttpContext context, HttpClient client, XmlDocument document = null)
			: base(context, client)
		{
			_xmlDocument = document;
		}

		/// <summary>
		/// 获得XML文档
		/// </summary>
		public XmlDocument XmlDocument
		{
			get
			{
				CheckDisposed();
				return _xmlDocument;
			}
			private set { _xmlDocument = value; }
		}

		#region Overrides of HttpResponseContent

		/// <summary>
		/// 请求处理最后的内容
		/// </summary>
		protected override void ProcessFinalResponse()
		{
			OnPreContentProcessed();
			base.ProcessFinalResponse();
			if (XmlDocument == null)
			{
				_created = true;
				XmlDocument = new XmlDocument();
			}
			try
			{
				XmlDocument.LoadXml(Utils.NormalizeString(StringResult));
			}
			catch (Exception ex)
			{
				Exception = ex;
				if (_created)
					XmlDocument = null;

				if (AsyncData == null)
					throw;

				AsyncData.Exception = ex;
			}
			OnPostContentProcessed();
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_created)
				_xmlDocument = null;
			_exception = null;
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			if (_created)
				XmlDocument = null;
			_exception = null;
		}

		/// <summary>
		/// 返回表示当前对象的字符串。
		/// </summary>
		/// <returns>
		/// 表示当前对象的字符串。
		/// </returns>
		public override string ToString()
		{
			return StringResult;
		}
	}
}
