namespace FSLib.Network.Http
{
	using System.Collections.Generic;

	/// <summary>
	/// 一个抽象的表单对象
	/// </summary>
	public abstract class AbstractFormData : IFormData
	{
		/// <summary>
		/// 获得所有的域
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<KeyValuePair<string, string>> GetAllFields()
		{
			yield break;
		}

		/// <summary>
		/// 获得所有要上传的文件
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<HttpPostFile> GetAllFiles()
		{
			yield break;
		}

		/// <summary>
		/// 绑定内容到请求中
		/// </summary>
		/// <param name="client">当前的HTTP客户端</param>
		/// <param name="context">当前的上下文</param>
		/// <param name="requestMessage">当前的请求信息</param>
		/// <param name="content">当前的请求内容对象</param>
		/// <param name="prefix">前缀：当前绑定之前的父对象路径</param>
		/// <param name="level">当前绑定的级别</param>
		public void BindData(HttpClient client, HttpContext context, HttpRequestMessage requestMessage, HttpRequestContent content, string prefix, int level)
		{
		}
	}
}