namespace FSLib.Network.Http
{
	using System;
	using System.Net;

	/// <summary>
	/// 辅助工具类
	/// </summary>
	public static class HttpProcessUtility
	{
		/// <summary>
		/// 向指定的CookieContainer中导入Cookies
		/// </summary>
		/// <param name="container"></param>
		/// <param name="cookies"></param>
		/// <param name="uri"></param>
		/// <param name="expiresTime">过期时间</param>
		public static void ImportCookies(this CookieContainer container, string cookies, Uri uri = null, DateTime? expiresTime = null)
		{
			container.Add(uri, ParseCookies(cookies, uri, expiresTime));
		}

		/// <summary>
		/// 将指定的字符串分析为CookieCollection
		/// </summary>
		/// <param name="text"></param>
		/// <param name="url"></param>
		/// <param name="expiresTime">过期时间</param>
		/// <returns></returns>
		public static CookieCollection ParseCookies(string text, Uri url, DateTime? expiresTime = null)
		{
			if (String.IsNullOrEmpty(text))
				throw new ArgumentException("text is null or empty.", nameof(text));
			if (url == null)
				throw new ArgumentNullException(nameof(url), "url is null.");

			var cookieSegments = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var cc             = new CookieCollection();

			foreach (var cookieSegment in cookieSegments)
			{
				var splitIndx = cookieSegment.IndexOf('=');
				if (splitIndx == -1)
					continue;

				var name  = cookieSegment.Substring(0, splitIndx).Trim();
				var value = cookieSegment.Substring(splitIndx + 1).Trim();

				var cok = new Cookie(name, value, url.AbsolutePath, url.Host);
				cc.Add(cok);
			}

			return cc;
		}
	}
}
