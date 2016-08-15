using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Net;

	/// <summary>
	/// 配置请求的辅助类
	/// </summary>
	public class HttpConfigHelper
	{
		/// <summary>
		/// 设置是否使用不安全的标头解析
		/// </summary>
		/// <param name="enabled"></param>
		public static void SetUseUnsafeHeaderParsing(bool enabled)
		{
			var type = typeof(HttpWebRequest).Assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
			var prop = type.GetProperty("Section", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var obj = prop.GetValue(null, null);
			var propflag = type.GetField("useUnsafeHeaderParsing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			propflag.SetValue(obj, enabled);
		}
	}
}
