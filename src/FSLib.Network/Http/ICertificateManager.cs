namespace FSLib.Network.Http
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public interface ICertificateManager
    {

        /// <summary>
        /// 设置请求的证书 
        /// </summary>
        /// <param name="message">请求</param>
        void SetRequest(HttpRequestMessage message);

        /// <summary>
        /// 添加证书到管理器中 
        /// </summary>
        /// <param name="host">对应的主机</param>
        /// <param name="certificates">要添加的证书</param>
        void AddCertificates(string host, X509Certificate[] certificates);
    }
}