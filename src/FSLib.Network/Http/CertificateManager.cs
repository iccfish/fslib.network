namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.Security.Cryptography.X509Certificates;

	/// <summary>
	/// 默认的证书管理类 
	/// </summary>
	public class CertificateManager : ICertificateManager
	{
		Dictionary<string, X509Certificate[]> _certificates;
		private Dictionary<string, string> _mappedCache;
		readonly object _lockObject = new object();

		public CertificateManager()
		{
			_certificates = new Dictionary<string, X509Certificate[]>(StringComparer.OrdinalIgnoreCase);
			_mappedCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// 设置请求的证书 
		/// </summary>
		/// <param name="message">请求</param>
		public void SetRequest(HttpRequestMessage message)
		{
			if (_certificates.Count == 0)
				return;

#if NET_GT_4
			var host = message.Host.IsNullOrEmpty() ? message.Uri.Host : message.Host;
#else
            var host = message.Uri.Host;
#endif
			string result;
			lock (_mappedCache)
			{
				if (!_mappedCache.TryGetValue(host, out result))
				{
					result = LookupHostCertificates(host);
					_mappedCache.Add(host, result);
				}
			}

			if (!result.IsNullOrEmpty())
				message.X509Certificates = _certificates[result];
		}

		/// <summary>
		/// 查找设置的主机
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		string LookupHostCertificates(string host)
		{
			lock (_certificates)
			{
				var key = host;
				do
				{
					if (_certificates.ContainsKey(key))
						return key;

					var index = key.IndexOf('.');
					if (index == -1)
						break;

					key = key.Substring(index);
				} while (true);
			}

			return null;
		}

		/// <summary>
		/// 添加证书到管理器中 
		/// </summary>
		/// <param name="host">对应的主机</param>
		/// <param name="certificates">要添加的证书</param>
		public void AddCertificates(string host, X509Certificate[] certificates)
		{
			if (host.IsNullOrEmpty())
				throw new ArgumentException(nameof(host), "host can not be null or empty");
			if (certificates == null || certificates.Length == 0)
				throw new ArgumentException(nameof(certificates), "certificates can not be null or empty");

			host = host.Trim('*');
			lock (_certificates)
			{
				_certificates.AddOrUpdate(host, certificates);
			}
		}
	}
}