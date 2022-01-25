namespace FSLib.Network.Http
{
	/// <summary>
	/// 请求构建器
	/// </summary>
	public class ContextBuilder
	{
		/// <summary>
		/// 请求方式
		/// </summary>
		public HttpMethod Method { get; set; }

		/// <summary>
		/// URL地址
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 获得或设置关联的 <see cref="HttpClient"/>
		/// </summary>
		public HttpClient Client { get; set; }

		/// <summary>
		/// 新建一个 <see cref="ContextBuilder"/> 对象
		/// </summary>
		/// <param name="method"></param>
		/// <param name="url"></param>
		public ContextBuilder(HttpMethod method, string url)
		{
			Method = method;
			Url = url;
		}

		/// <summary>
		/// 获得或设置请求数据
		/// </summary>
		public object RequestBody { get; set; }

		/// <summary>
		/// 发送请求数据
		/// </summary>
		/// <param name="data"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public ContextBuilder WithBody<T>(T data)
		{
			RequestBody = data;
			return this;
		}

		/// <summary>
		/// 获得或设置引用页面地址
		/// </summary>
		public string Refer { get; set; }

		/// <summary>
		/// 设置引用页地址
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public ContextBuilder WithReferrer(string url)
		{
			Refer = url;
			return this;
		}
	}

	/// <summary>
	/// 适用于构建器的扩展方法
	/// </summary>
	public static class ContextBuilderExtensions
	{
		/// <summary>
		/// GET一个URL地址
		/// </summary>
		/// <param name="client"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Get(this HttpClient client, string url) => new ContextBuilder(HttpMethod.Get, url);

		/// <summary>
		/// POST一个URL地址
		/// </summary>
		/// <param name="client"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Post(this HttpClient client, string url) => new ContextBuilder(HttpMethod.Post, url);

		/// <summary>
		/// PUT一个URL地址
		/// </summary>
		/// <param name="client"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Put(this HttpClient client, string url) => new ContextBuilder(HttpMethod.Put, url);

		/// <summary>
		/// HEAD一个URL地址
		/// </summary>
		/// <param name="client"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Head(this HttpClient client, string url) => new ContextBuilder(HttpMethod.Head, url);

		/// <summary>
		/// DELETE一个URL地址
		/// </summary>
		/// <param name="client"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Delete(this HttpClient client, string url) => new ContextBuilder(HttpMethod.Delete, url);

		/// <summary>
		/// 以指定的方式新建一个构建
		/// </summary>
		/// <param name="client"></param>
		/// <param name="method">请求方法</param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ContextBuilder Build(this HttpClient client, HttpMethod method, string url) => new ContextBuilder(method, url);
	}
}
