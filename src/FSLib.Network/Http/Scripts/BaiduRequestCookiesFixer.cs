using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Scripts
{
	using System.Text.RegularExpressions;

	/// <summary>
	/// 自动修复百度主页的cookies
	/// </summary>
	public static class BaiduRequestCookiesFixer
	{
		/// <summary>
		/// 启用自动修复
		/// </summary>
		public static void Enable()
		{
			GlobalEvents.RequestEnd -= GlobalEvents_AfterRequest;
			GlobalEvents.RequestEnd += GlobalEvents_AfterRequest;
		}

		/// <summary>
		/// 禁用自动修复
		/// </summary>
		public static void Disable()
		{
			GlobalEvents.RequestEnd -= GlobalEvents_AfterRequest;
		}

		private static void GlobalEvents_AfterRequest(object sender, WebEventArgs e)
		{
			var context = e.Context;
			var response = e.Response;

			if (e.Context.Client.CookieContainer == null)
				return;

			if (response == null || string.Compare("www.baidu.com", response.ResponseUri.Host, StringComparison.OrdinalIgnoreCase) != 0)
				return;

			var cookies = context.Response.Headers.GetValues("Set-Cookie");
			if (cookies == null || !cookies.Any(s => Regex.IsMatch(s, @"(?<d1>\d{2}-[a-z]{3}-)(?<d2>\d{2})", RegexOptions.IgnoreCase)))
				return;

			var targetList = new List<string>();

			var current = "";
			foreach (var cookieSeg in cookies)
			{
				if (Regex.IsMatch(cookieSeg, "^[^=;]+=[^;]+"))
				{
					//start new
					if (!current.IsNullOrEmpty())
					{
						targetList.Add(current);
						current = "";
					}
				}
				current += (current.Length > 0 ? " " : "") + Regex.Replace(cookieSeg, @"(?<d1>\d{2}-[a-z]{3}-)(?<d2>\d{2})", _ =>
				{
					var year = _.GetGroupValue(2).ToInt32();
					var suffix = year < DateTime.Now.Year % 100 ? DateTime.Now.Year / 100 + 1 : DateTime.Now.Year / 100;

					return $"{_.GetGroupValue(1)}{suffix}{year}";
				}, RegexOptions.IgnoreCase);
			}
			if (!current.IsNullOrEmpty())
				targetList.Add(current);

			foreach (var cookieSeg in targetList)
			{
				context.Client.CookieContainer.SetCookies(context.Response.ResponseUri, cookieSeg);
			}
		}

	}
}
