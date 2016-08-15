using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Text.RegularExpressions;

	/// <summary>
	/// 表示身份认证响应
	/// </summary>
	public class WwwAuthenticate
	{
		/// <summary>
		/// 获得或设置标记
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// 获得或设置提示
		/// </summary>
		public string Realm { get; set; }

		internal WwwAuthenticate(string tag)
		{
			var m = Regex.Match(tag, @"^([^\s]+)\s*realm=['""]?(.*?)['""]?$");
			if (!m.Success)
				throw new ArgumentException();

			Type = m.Groups[1].Value;
			Realm = m.Groups[2].Value;
		}
	}
}
