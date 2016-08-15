using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 证书校验事件参数
	/// </summary>
	public class CertificateValidationEventArgs : EventArgs
	{
		/// <summary>
		/// 相关联的请求
		/// </summary>
		public HttpWebRequest Request { get; internal set; }

		/// <summary>
		/// 相关联的客户端
		/// </summary>
		public HttpClient Client { get; internal set; }

		/// <summary>
		/// 相关联的上下文会话
		/// </summary>
		public HttpContext HttpContext { get; internal set; }

		/// <summary>
		/// 验证中发生的错误
		/// </summary>
		public SslPolicyErrors SslPolicyErrors { get; internal set; }

		/// <summary>
		/// 验证的证书链
		/// </summary>
		public X509Chain X509Chain { get; internal set; }

		/// <summary>
		/// 证书
		/// </summary>
		public X509Certificate Certificate { get; internal set; }

		/// <summary>
		/// 获得或设置验证结果
		/// </summary>
		public bool Result { get; set; }

		/// <summary>
		/// 新建新的 <see cref="CertificateValidationEventArgs"/> 对象
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="sslPolicyErrors"></param>
		/// <param name="x509Chain"></param>
		/// <param name="certificate"></param>
		internal CertificateValidationEventArgs(HttpContext httpContext, SslPolicyErrors sslPolicyErrors, X509Chain x509Chain, X509Certificate certificate)
		{
			HttpContext = httpContext;
			SslPolicyErrors = sslPolicyErrors;
			X509Chain = x509Chain;
			Certificate = certificate;
			Request = httpContext.WebRequest;
			Client = httpContext.Client;

			Result = SslPolicyErrors == SslPolicyErrors.None;
		}

		/// <summary>
		/// 新建新的 <see cref="CertificateValidationEventArgs"/> 对象
		/// </summary>
		internal CertificateValidationEventArgs(HttpWebRequest request, SslPolicyErrors sslPolicyErrors, X509Chain x509Chain, X509Certificate certificate)
		{
			Request = request;
			SslPolicyErrors = sslPolicyErrors;
			X509Chain = x509Chain;
			Certificate = certificate;

			Result = SslPolicyErrors == SslPolicyErrors.None;
		}
	}
}
