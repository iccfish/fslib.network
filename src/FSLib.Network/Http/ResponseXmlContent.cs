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

		string NormalizeString(string str)
		{
			return Regex.Replace(str, "[\u0000-\u0020]", _ =>
			{
				var c = (int)_.Value[0];
				return ((c >= 0 && c <= 8) || c == 11 || c == 12 || c >= 14 && c < 32) ? "&#x" + c.ToString("x") + ";" : _.Value;
			});
		}


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
				XmlDocument.LoadXml(NormalizeString(StringResult));
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
