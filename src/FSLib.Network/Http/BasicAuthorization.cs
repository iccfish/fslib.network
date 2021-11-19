using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 适用于普通认证的认证结果
	/// </summary>
	public class BasicAuthorization : Authorization
	{
		/// <summary>
		/// 获得或设置用户名
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 获得或设置密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 获得或设置编码格式
		/// </summary>
		public Encoding TextEncoding { get; set; }

		/// <summary>
		/// 创建一个新的对象
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		public BasicAuthorization(string userName, string password)
		{
			Password = password;
			UserName = userName;
			TextEncoding = System.Text.Encoding.Default;
		}
		/// <summary>
		/// 将信息写入HttpWebRequest中
		/// </summary>
		/// <param name="request">请求</param>
		/// <param name="httpContext">当前的上下文</param>
		public override void SetRequest(HttpWebRequest request,
			HttpContext httpContext)
		{
			if (UserName.IsNullOrEmpty())
				return;

			request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(TextEncoding.GetBytes(UserName + ":" + Password)));
		}

	}
}
