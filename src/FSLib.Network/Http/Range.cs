using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表示接收到的区域标记
	/// </summary>
	public class Range
	{


		/// <summary>
		/// 表示开始位置
		/// </summary>
		public long From { get; private set; }

		/// <summary>
		/// 获得结束位置
		/// </summary>
		public long To { get; private set; }

		/// <summary>
		/// 获得总长度
		/// </summary>
		public long Total { get; private set; }

		/// <summary>
		/// 获得接受单位
		/// </summary>
		public string AcceptRange { get; private set; }

		/// <summary>
		/// 创建 <see cref="Range" />  的新实例(Range)
		/// </summary>
		public Range(string acceptRange, long from, long to, long total)
		{
			AcceptRange = acceptRange;
			From = from;
			To = to;
			Total = total;
		}

		/// <summary>
		/// 创建 <see cref="Range" />  的新实例(Range)
		/// </summary>
		public Range(string arg)
		{
			var m = ParseReg.Match(arg);
			if (!m.Success) 
				throw new ArgumentException("无法分析指定的 Content-Range 标记 - " + arg);

			AcceptRange = m.Groups[1].Value;
			From = m.Groups[1].Value.ToInt64();
			To = m.Groups[1].Value.ToInt64();
			Total = m.Groups[1].Value.ToInt64();
		}

		private static readonly Regex ParseReg = new Regex(@"^([^\s]+)\s*(\d+)-(\d+)/(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	}
}
