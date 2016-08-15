using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Scripts
{
	using System.Text.RegularExpressions;

	/// <summary>
	/// 包含了一些常见Packer的反Packer算法
	/// </summary>
	public class PackerDecode
	{
		/// <summary>
		/// 解密格式如 eval(.....) 格式的JS代码
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public static string Unpack(string code)
		{
			var match = Regex.Match(code, @"eval\(.*?}\s*\(\s*'(.*?)'\s*,\s*(\d+)\s*,\s*(\d+)s*,\s*'(.*?)'\.split", RegexOptions.Singleline);
			return !match.Success ? null : Unpack(match.GetGroupValue(1), match.GetGroupValue(2).ToInt32(), match.GetGroupValue(3).ToInt32(), match.GetGroupValue(4));
		}

		/// <summary>
		/// 使用指定的数据对代码进行解密。四个参数分别为packer后的开始四个参数
		/// </summary>
		/// <param name="p"></param>
		/// <param name="a"></param>
		/// <param name="c"></param>
		/// <param name="k"></param>
		/// <returns></returns>
		public static string Unpack(string p, int a, int c, string k)
		{
			var d = new Dictionary<string, string>();

			var segs = k.Split('|');

			Func<int, string> df = null;
			Func<int, int, string> toString = (num, radix) =>
			{
				var charBuffer = new List<char>();
				while (num > 0)
				{
					var tn = num % radix;
					charBuffer.Add((char)(tn < 10 ? '0' + tn : 'a' + tn - 10));
					num = num / radix;
				}
				if (charBuffer.Count == 0)
					charBuffer.Add('0');
				charBuffer.Reverse();
				return new string(charBuffer.ToArray());
			};

			df = _ => (_ < a ? "" : df(_ / a)) + ((_ = _ % a) > 32 ? ((char)(_ + 32)).ToString() : toString(_, 33));
			while (c-- > 0)
			{
				var key = df(c);
				var value = c > 0 && c < segs.Length ? segs[c] : df(c);

				d[key] = value;
			}
			return Regex.Replace(p, @"\b\w+\b", _ => d.ContainsKey(_.Value) ? d[_.Value] : "?");
		}
	}
}
