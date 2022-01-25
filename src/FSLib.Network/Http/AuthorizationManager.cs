namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.Net;

	/// <summary>
	/// 身份验证管理类
	/// </summary>
	public interface IAuthorizationManager
	{
		/// <summary>
		/// 将信息写入HttpWebRequest中
		/// </summary>
		/// <param name="httpContext">当前的上下文</param>
		IAuthorization GetAuthorization(HttpContext httpContext);
	}

	/// <summary>
	/// 多个域名管理的身份验证类
	/// </summary>
	public class AuthorizationManager : IAuthorizationManager
	{
		/// <summary>
		/// 获得身份认证集合
		/// </summary>
		public Dictionary<string, IAuthorization> Authorizations { get; private set; }

		/// <summary>
		/// 创建 <see cref="AuthorizationManager" />  的新实例(BasicAuthorizationManager)
		/// </summary>
		public AuthorizationManager()
		{
			Authorizations = new Dictionary<string, IAuthorization>();
		}


		/// <summary>
		/// 向授权管理中添加一个基本授权
		/// </summary>
		/// <param name="host">域名</param>
		/// <param name="username">用户名</param>
		/// <param name="password">密码</param>
		public void AddBasicAuthorization(string host, string username, string password)
		{
			Authorizations[host] = new BasicAuthorization(username, password);
		}

		/// <summary>
		/// 将信息写入HttpWebRequest中
		/// </summary>
		/// <param name="httpContext">当前的上下文</param>
		public IAuthorization GetAuthorization(HttpContext httpContext)
		{
#if NET_GT_4 || NET5_0_OR_GREATER
			return Authorizations.GetValue(httpContext.Request.Host ?? httpContext.Request.Uri.Host ?? "");
#else
			return Authorizations.GetValue(httpContext.Request.Uri.Host);
#endif
		}
	}
}