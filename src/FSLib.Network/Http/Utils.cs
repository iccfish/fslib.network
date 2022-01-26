using System;

namespace FSLib.Network.Http
{
	using System.Text.RegularExpressions;

	using Newtonsoft.Json;

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

		/// <summary>
		/// Json序列化
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string JsonSerialize(object obj, JsonSerializationSetting setting)
		{
			if (obj == null)
				return string.Empty;

			if (setting == null)
			{
				return JsonConvert.SerializeObject(obj);
			}

			if (setting.JsonConverts == null)
			{
				return JsonConvert.SerializeObject(obj, setting.Formatting, setting.Setting);
			}

			return JsonConvert.SerializeObject(obj, setting.Formatting, setting.JsonConverts);
		}

		/// <summary>
		/// 反序列化目标对象
		/// </summary>
		/// <param name="result"></param>
		/// <param name="originalObj"></param>
		/// <returns></returns>
		public static object JsonDeserialize(string result, object originalObj, Type type, JsonDeserializationSetting setting)
		{

			if (setting == null)
				return JsonConvert.DeserializeObject(result, type);
			if (setting.JsonConverts == null)
				return JsonConvert.DeserializeObject(result, type, setting.Setting);
			return JsonConvert.DeserializeObject(result, type, setting.JsonConverts);
		}
	}
}
