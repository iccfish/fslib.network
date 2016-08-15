using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 跟踪辅助开关
	/// </summary>
	public static class TraceHelper
	{
		/// <summary>
		/// 动态启用网络调试
		/// </summary>
		public static void EnableNetworkTrace(params TraceListener[] listener)
		{
			var logging = typeof(HttpWebRequest).Assembly.GetType("System.Net.Logging");
			var enabled = logging.GetField("s_LoggingEnabled", BindingFlags.NonPublic | BindingFlags.Static);

			logging.GetProperty("On", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null, null);

			enabled.SetValue(null, true);

			new[]
				{
					"Web",
					"Http",
					"HttpListener",
					"RequestCache",
					"Sockets",
					"WebSockets"
				}.ForEach(s => EnableLog(logging, s, listener));
		}

		static void EnableLog(Type logType, string propName, params TraceListener[] listener)
		{
			var propInfo = logType.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Static);
			if (propInfo != null)
			{
				var source = (propInfo.GetValue(null, null) as TraceSource);
				if (source != null)
				{
					source.Switch.Level = SourceLevels.Verbose;
					if (listener != null)
						source.Listeners.AddRange(listener);
					source.Attributes.Add("tracemode", "includehex");
					source.Attributes.Add("maxdatasize", "1024");
				}
			}
		}
	}
}
