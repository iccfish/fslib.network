using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http.Scripts
{
	using System.Diagnostics;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Web;
	using HttpContext = FSLib.Network.Http.HttpContext;

	/// <summary>
	/// 数字的网站宝防CC攻击脚本
	/// </summary>
	public class DigitalWzbAutoBypass
	{


		static void GlobalEvents_RequestValidateResponse(object sender, EventArgs e)
		{
			var ctx = sender as HttpContext;
			if (!(ctx?.ResponseContent is ResponseBinaryContent))
				return;

			var cookies = ctx.Response.Cookies?["wzwsconfirm"];
			if (cookies == null)
				return;
			var result = ((ResponseBinaryContent)ctx.ResponseContent)?.StringResult;
			if (result.IndexOf("<noscript>", StringComparison.Ordinal) <= 0)
				return;
			new CodeGenerator(ctx, result, null).Process();
		}

		/// <summary>
		/// 禁用
		/// </summary>
		public static void Disable()
		{
			GlobalEvents.RequestValidateResponse -= GlobalEvents_RequestValidateResponse;
		}

		/// <summary>
		/// 启用
		/// </summary>
		public static void Enable()
		{
			GlobalEvents.RequestValidateResponse -= GlobalEvents_RequestValidateResponse;
			GlobalEvents.RequestValidateResponse += GlobalEvents_RequestValidateResponse;
		}
		class CodeGenerator
		{
			readonly HttpContext _context;
			readonly string _response;
			string _templateCookies;
			string _templateConfirmCookies;
			string _code, _template, _dynamicurl, _challenge, _challengex, _confirmLabel;
			int _hashoptimes, _hashopplus;
			readonly Uri _reponseUri;

			public CodeGenerator(HttpContext ctx, string response, Uri responseUri)
			{
				_context = ctx;
				_response = response;
				_reponseUri = responseUri ?? ctx?.Response?.ResponseUri;
			}

			public void Process()
			{
				try
				{
					_code = PackerDecode.Unpack(_response);
				}
				catch (Exception ex)
				{
					Trace.TraceWarning("警告：解密JS时出现错误！源文件：\r\n\r\n" + _response + "\r\n\r\n" + "错误信息：" + ex.ToString());
					return;
				}

				//获得template id
				_template = Regex.Match(_code, @"\stemplate\s*=\s*['""]?([\da-z-_]+)['""]?", RegexOptions.Singleline).GetGroupValue(1);
				_dynamicurl = Regex.Match(_code, @"\sdynamicurl\s*=\s*['""]?([^'""]+)['""]?", RegexOptions.Singleline).GetGroupValue(1);
				_challenge = Regex.Match(_code, @"\swzwschallenge\s*=\s*['""]?([^'""]+)['""]?", RegexOptions.Singleline).GetGroupValue(1);
				_challengex = Regex.Match(_code, @"\swzwschallengex\s*=\s*['""]?([^'""]+)['""]?", RegexOptions.Singleline).GetGroupValue(1);
				_hashoptimes= Regex.Match(_code, @"hash\s*\*=\s*(\d+)", RegexOptions.Singleline).GetGroupValue(1).ToInt32();
				_hashopplus = Regex.Match(_code, @"hash\s*\+=\s*(\d+)", RegexOptions.Singleline).GetGroupValue(1).ToInt32();
				_confirmLabel = Regex.Match(_code, @"['""]([^'""]+)['""]\s*\+\s*hash", RegexOptions.Singleline).GetGroupValue(1);
				_templateCookies = "wzwstemplate";
				_templateConfirmCookies = "wzwschallenge";

				if (_templateCookies.IsNullOrEmpty() || _template.IsNullOrEmpty() || _challengex.IsNullOrEmpty() || _challenge.IsNullOrEmpty())
				{
					Trace.TraceWarning("警告：无法获得Cookies参数！源文件：\r\n\r\n" + _response + "\r\n\r\n" + "代码：" + _code);
					return;
				}

				var newCookies = CreateCookies();

				if (newCookies?.Count > 0 && _context != null)
				{
					_context.Client.CookieContainer.Add(_reponseUri, newCookies);

					//直接访问
					var nextRequest = _context.Client.Create<string>(HttpMethod.Get, new Uri(_reponseUri, _dynamicurl)).Send();
					if (nextRequest.IsRedirection)
					{
						_context.HasRequestResubmit = true;
					}
				}
			}

			const string Encoderchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

			static string EncodeStr(string str)
			{
				var result = "";
				var i = 0;
				var len = str.Length;
				byte c1, c2, c3;

				while (i < len)
				{
					c1 = (byte)((byte)str[i++] & 0xff);
					if (i == len)
					{
						result += Encoderchars[c1 >> 2];
						result += Encoderchars[(c1 & 0x3) << 4];
						result += "==";
						break;
					}
					c2 = (byte)str[i++];
					if (i == len)
					{
						result += Encoderchars[c1 >> 2];
						result += Encoderchars[((c1 & 0x3) << 4) | ((c2 & 0xf0) >> 4)];
						result += Encoderchars[(c2 & 0xf) << 2];
						result += "=";
						break;
					}
					c3 = (byte)str[i++];
					result += Encoderchars[c1 >> 2];
					result += Encoderchars[((c1 & 0x3) << 4) | ((c2 & 0xf0) >> 4)];
					result += Encoderchars[((c2 & 0xf) << 2) | ((c3 & 0xc0) >> 6)];
					result += Encoderchars[c3 & 0x3f];
				}
				return result;
			}

			string CreateConfirm()
			{
				var tmp = _challenge + _challengex;
				var hash = tmp.Aggregate(0, (current, ch) => current + ch);
				hash *= _hashoptimes;
				hash += _hashopplus;
				return _confirmLabel + hash;
			}

			CookieCollection CreateCookies()
			{
				var collection = new CookieCollection()
								{
									new Cookie(_templateCookies, EncodeStr(_template)),
									new Cookie(_templateConfirmCookies, EncodeStr(CreateConfirm()))
								};

				return collection;
			}
		}
	}
}
