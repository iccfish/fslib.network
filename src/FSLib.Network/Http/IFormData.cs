using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 表示对象是表单对象
	/// </summary>
	public interface IFormData
	{
		/// <summary>
		/// 获得所有的域
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, string>> GetAllFields();

		/// <summary>
		/// 获得所有要上传的文件
		/// </summary>
		/// <returns></returns>
		IEnumerable<HttpPostFile> GetAllFiles();

		/// <summary>
		/// 绑定内容到请求中
		/// </summary>
		/// <param name="client">当前的HTTP客户端</param>
		/// <param name="context">当前的上下文</param>
		/// <param name="requestMessage">当前的请求信息</param>
		/// <param name="content">当前的请求内容对象</param>
		/// <param name="prefix">前缀：当前绑定之前的父对象路径</param>
		/// <param name="level">当前绑定的级别</param>
		void BindData(HttpClient client, HttpContext context, HttpRequestMessage requestMessage, HttpRequestContent content, string prefix, int level);
	}
}
