using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Drawing;
using FSLib.Extension;
	using System.Security.Policy;
	using System.Threading;

	using Extension;

#if NET_GT_4 || NET5_0_OR_GREATER
	using System.Threading.Tasks;

#endif

	/// <summary>
	/// 简化用法
	/// </summary>
	partial class HttpClient
	{
		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<T> Get<T>(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null) where T : class
		{
			return Create<T>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
		}
		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<byte[]> GetData(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return Create<byte[]>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public byte[] GetDataResult(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResult<byte[]>(uri, queryParam, refer, allowAutoRedirect);
		}
		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<Image> GetImage(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return Create<Image>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Image GetImageResult(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResult<Image>(uri, queryParam, refer, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public T GetResult<T>(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null) where T : class
		{
			var ctx = Create<T>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
			ctx.Send();
			return ctx.IsValid() ? ctx.Result : default(T);
		}
		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<string> GetString(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return Create<string>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public string GetStringResult(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResult<string>(uri, queryParam, refer, allowAutoRedirect);
		}
		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<T> Post<T>(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null) where T : class
		{
			return Create<T>(HttpMethod.Post, uri, refer, data, allowAutoRedirect: allowAutoRedirect, contentType: usingJsonBody == true ? ContentType.Json : usingJsonBody == false ? (ContentType?)ContentType.FormUrlEncoded : null);
		}

		/// <summary>
		/// 以Post模式发送一个JSON内容的请求
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public HttpContext<TResult> PostJson<TRequest, TResult>(string url, TRequest data) where TResult : class
		{
			return Create<TResult>(HttpMethod.Post, url, data: data, contentType: ContentType.Json);
		}

		/// <summary>
		/// 以Post模式发送一个JSON内容的请求
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="url"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public HttpContext<TResult> PutJson<TRequest, TResult>(string url, TRequest data) where TResult : class
		{
			return Create<TResult>(HttpMethod.Put, url, data: data, contentType: ContentType.Json);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<byte[]> PostForData(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return Post<byte[]>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public byte[] PostForDataResult(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostForResult<byte[]>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}
		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<Image> PostForImage(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return Post<Image>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Image PostForImageResult(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostForResult<Image>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public T PostForResult<T>(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null) where T : class
		{
			var ctx = Create<T>(HttpMethod.Post, uri, refer, data, allowAutoRedirect: allowAutoRedirect, contentType: usingJsonBody == true ? ContentType.Json : usingJsonBody == false ? (ContentType?)ContentType.FormUrlEncoded : null);
			ctx.Send();
			return ctx.IsValid() ? ctx.Result : default(T);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public HttpContext<string> PostForString(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return Post<string>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public string PostForStringResult(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostForResult<string>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}
		/// <summary>
		/// 带重试的发送请求，直到判定成功或者超过次数。
		/// </summary>
		/// <typeparam name="T">内容类型</typeparam>
		/// <param name="contextGenerator">创建目标HttpContext的回调</param>
		/// <param name="maxRetryCount">最多重试次数。如果为null，则默认使用全局设置</param>
		/// <param name="contextChecker">判断是否正确的响应，默认为判断 IsValid 为true</param>
		/// <param name="retryIndicator">重试的时候将会通知</param>
		/// <param name="sleepTime">两次重试之间的休息时间（毫秒）</param>
		/// <returns></returns>
		public HttpContext<T> SendContextWithRetry<T>(
			[NotNull] Func<HttpContext<T>> contextGenerator,
			Func<HttpContext<T>, bool> contextChecker = null,
			Action<int, HttpContext<T>> retryIndicator = null,
			int? maxRetryCount = null,
			int? sleepTime = null) where T : class
		{
			if (contextGenerator == null)
				throw new ArgumentNullException(nameof(contextGenerator));
			if (contextChecker == null)
				contextChecker = _ => _.IsValid();

			HttpContext<T> context = null;
			maxRetryCount = maxRetryCount ?? Setting.DefaultRetryLimit;

			for (int i = 0; i < maxRetryCount.Value; i++)
			{
				context = contextGenerator();
				retryIndicator?.Invoke(i, context);
				context.Send();

				if (contextChecker(context))
					return context;

				if (i < maxRetryCount.Value)
					Thread.Sleep(sleepTime ?? Setting.DefaultRetrySleepTime);
			}

			return context;
		}

#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<T> GetResultAsync<T>(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null) where T : class
		{
			return GetResultAsync<T>(new CancellationToken(), uri, queryParam, refer, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<T> GetResultAsync<T>(CancellationToken cancellationToken, string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null) where T : class
		{
			var ctx = Create<T>(HttpMethod.Get, uri, refer, queryParam, allowAutoRedirect: allowAutoRedirect);
			return ctx.SendAsync(cancellationToken);
		}

#endif

#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<byte[]> GetDataResultAsync(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetDataResultAsync(new CancellationToken(), uri, queryParam, refer, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<byte[]> GetDataResultAsync(CancellationToken cancellationToken, string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResultAsync<byte[]>(cancellationToken, uri, queryParam, refer, allowAutoRedirect);
		}

#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<Image> GetImageResultAsync(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetImageResultAsync(new CancellationToken(), uri, queryParam, refer, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<Image> GetImageResultAsync(CancellationToken cancellationToken, string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResultAsync<Image>(cancellationToken, uri, queryParam, refer, allowAutoRedirect);
		}
#endif



#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<string> GetStringResultAsync(string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetStringResultAsync(new CancellationToken(), uri, queryParam, refer, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="queryParam">请求参数</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<string> GetStringResultAsync(CancellationToken cancellationToken, string uri, object queryParam = null, string refer = null, bool? allowAutoRedirect = null)
		{
			return GetResultAsync<string>(cancellationToken, uri, queryParam, refer, allowAutoRedirect);
		}
#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<T> PostResultAsync<T>(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null) where T : class
		{
			return PostResultAsync<T>(new CancellationToken(), uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <typeparamref name="T"/> 的请求
		/// </summary>
		/// <typeparam name="T">期望的结果类型</typeparam>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<T> PostResultAsync<T>(CancellationToken cancellationToken, string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null) where T : class
		{
			var ctx = Create<T>(HttpMethod.Post, uri, refer, data, allowAutoRedirect: allowAutoRedirect, contentType: usingJsonBody == true ? ContentType.Json : usingJsonBody == false ? (ContentType?)ContentType.FormUrlEncoded : null);
			return ctx.SendAsync(cancellationToken);
		}
#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<byte[]> PostDataResultAsync(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostResultAsync<byte[]>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Array"/> 的请求
		/// </summary>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<byte[]> PostDataResultAsync(CancellationToken cancellationToken, string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostResultAsync<byte[]>(cancellationToken, uri, data, refer, usingJsonBody, allowAutoRedirect);
		}
#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<Image> PostImageResultAsync(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostResultAsync<Image>(uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see cref="Image"/> 的请求
		/// </summary>
		/// <param name="cancellationToken">取消标记</param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<Image> PostImageResultAsync(CancellationToken cancellationToken, string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostResultAsync<Image>(cancellationToken, uri, data, refer, usingJsonBody, allowAutoRedirect);
		}
#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<string> PostStringResultAsync(string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostStringResultAsync(new CancellationToken(), uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

		/// <summary>
		/// 发起一个期望结果类型为 <see langword="string" /> 的请求
		/// </summary>
		/// <param name="cancellationToken">用来取消的 <see cref="CancellationToken"/></param>
		/// <param name="uri">请求地址</param>
		/// <param name="refer">引用页</param>
		/// <param name="data">请求参数</param>
		/// <param name="usingJsonBody">是否将数据序列化成JSON内容发送</param>
		/// <param name="allowAutoRedirect">是否允许自动重定向</param>
		/// <returns></returns>
		public Task<string> PostStringResultAsync(CancellationToken cancellationToken, string uri, object data = null, string refer = null, bool? usingJsonBody = null, bool? allowAutoRedirect = null)
		{
			return PostResultAsync<string>(cancellationToken, uri, data, refer, usingJsonBody, allowAutoRedirect);
		}

#endif


#if NET_GT_4 || NET5_0_OR_GREATER

		/// <summary>
		/// 带重试的发送请求，直到判定成功或者超过次数。
		/// </summary>
		/// <typeparam name="T">内容类型</typeparam>
		/// <param name="contextGenerator">创建目标HttpContext的回调</param>
		/// <param name="maxRetryCount">最多重试次数。如果为null，则默认使用全局设置</param>
		/// <param name="contextChecker">判断是否正确的响应，默认为判断 IsValid 为true</param>
		/// <param name="retryIndicator">重试的时候将会通知</param>
		/// <param name="sleepTime">休息时间</param>
		/// <returns></returns>
		public Task<HttpContext<T>> SendContextWithRetryAsync<T>(
			[NotNull] Func<HttpContext<T>> contextGenerator,
			Func<HttpContext<T>, bool> contextChecker = null,
			Action<int, HttpContext<T>> retryIndicator = null,
			int? maxRetryCount = null,
			int? sleepTime = null) where T : class
		{
			return SendContextWithRetryAsync<T>(contextGenerator, new CancellationToken(), contextChecker, retryIndicator, maxRetryCount, sleepTime);
		}


#if NET45

		/// <summary>
		/// 带重试的发送请求，直到判定成功或者超过次数。
		/// </summary>
		/// <typeparam name="T">内容类型</typeparam>
		/// <param name="contextGenerator">创建目标HttpContext的回调</param>
		/// <param name="token">取消标记</param>
		/// <param name="maxRetryCount">最多重试次数。如果为null，则默认使用全局设置</param>
		/// <param name="contextChecker">判断是否正确的响应，默认为判断 IsValid 为true</param>
		/// <param name="retryIndicator">重试的时候将会通知</param>
		/// <param name="sleepTime">休息时间</param>
		/// <returns></returns>
		public async Task<HttpContext<T>> SendContextWithRetryAsync<T>(
			[NotNull] Func<HttpContext<T>> contextGenerator,
			CancellationToken token,
			Func<HttpContext<T>, bool> contextChecker = null,
			Action<int, HttpContext<T>> retryIndicator = null,
			int? maxRetryCount = null,
			int? sleepTime = null
			) where T : class
		{
			if (contextGenerator == null)
				throw new ArgumentNullException(nameof(contextGenerator));

			if (contextChecker == null)
				contextChecker = _ => _.IsValid();

			HttpContext<T> context = null;
			maxRetryCount = maxRetryCount ?? Setting.DefaultRetryLimit;

			token.Register(() =>
							{
								context?.Abort();
							});

			for (int i = 0; i < maxRetryCount.Value; i++)
			{
				context = contextGenerator();
				retryIndicator?.Invoke(i, context);

				await context.SendAsync().ConfigureAwait(true);

				if (contextChecker(context))
					return context;

				if (i < maxRetryCount.Value)
					await Task.Delay(sleepTime ?? Setting.DefaultRetrySleepTime).ConfigureAwait(true);
			}

			return context;
		}
#else
		/// <summary>
		/// 带重试的发送请求，直到判定成功或者超过次数。
		/// </summary>
		/// <typeparam name="T">内容类型</typeparam>
		/// <param name="contextGenerator">创建目标HttpContext的回调</param>
		/// <param name="token">取消标记</param>
		/// <param name="maxRetryCount">最多重试次数。如果为null，则默认使用全局设置</param>
		/// <param name="contextChecker">判断是否正确的响应，默认为判断 IsValid 为true</param>
		/// <param name="retryIndicator">重试的时候将会通知</param>
		/// <param name="sleepTime">休息时间</param>
		/// <returns></returns>
		public Task<HttpContext<T>> SendContextWithRetryAsync<T>(
			[NotNull] Func<HttpContext<T>> contextGenerator, 
			CancellationToken token,
			Func<HttpContext<T>, bool> contextChecker = null, 
			Action<int, HttpContext<T>> retryIndicator = null, 
			int? maxRetryCount = null,
			int? sleepTime = null
			) where T : class
		{
			return Task<HttpContext<T>>.Factory.StartNew(() =>
			{
				if (contextGenerator == null)
					throw new ArgumentNullException(nameof(contextGenerator));

				if (contextChecker == null)
					contextChecker = _ => _.IsValid();

				HttpContext<T> context = null;
				maxRetryCount = maxRetryCount ?? Setting.DefaultRetryLimit;

				token.Register(() =>
				{
					context?.Abort();
				});

				for (int i = 0; i < maxRetryCount.Value; i++)
				{
					context = contextGenerator();
					retryIndicator?.Invoke(i, context);
					context.Send();

					if (contextChecker(context))
						return context;

					if (i < maxRetryCount.Value)
						Thread.Sleep(sleepTime ?? Setting.DefaultRetrySleepTime);
				}

				return context;
			});
		}

#endif
#endif

	}
}
