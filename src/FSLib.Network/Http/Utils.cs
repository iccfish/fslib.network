using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Text.RegularExpressions;

	class Utils
	{
		internal static string NormalizeString(string str)
		{
			return Regex.Replace(str,
				"[\u0000-\u0020]",
				_ =>
				{
					var c = (int)_.Value[0];
					return ((c >= 0 && c <= 8) || c == 11 || c == 12 || c >= 14 && c < 32) ? "&#x" + c.ToString("x") + ";" : _.Value;
				});
		}

		internal static string RemoveXmlDeclaration(string xml)
		{
			if (Regex.IsMatch(xml, @"^\s*<\?"))
			{
				xml = Regex.Replace(xml, @"^\s*<\?.*?\?>[\r\n]*", "");
			}

			return xml;
		}

	}
}
