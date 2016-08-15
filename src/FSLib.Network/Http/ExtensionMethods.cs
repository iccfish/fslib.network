﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace FSLib.Network.Http
{
	using System.Net;

#if NET_GT_4
	using RANGETYPE = Nullable<KeyValuePair<long, long?>>;
#else
	using RANGETYPE = Nullable<KeyValuePair<int, int?>>;
#endif

	/// <summary>
	/// 扩展方法
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public static class ExtensionMethods
	{
		#region HttpClient 扩展

		/// <summary>
		/// 确认当前的对象是否是成功的。此验证将会要求请求已发送且返回成功（状态码小于400），且返回的Result不为null。
		/// </summary>
		/// <typeparam name="T">HttpContext的返回值类型</typeparam>
		/// <param name="ctx">当前的<see cref="HttpContext{T}"/></param>
		/// <returns>如果判断成功，则返回true。否则返回false。</returns>
		/// <exception cref="ArgumentNullException">The value of 'ctx' cannot be null. </exception>
		/// <exception cref="InvalidOperationException">如果请求尚未发送，则引发此异常. </exception>
		public static bool IsValid<T>(this HttpContext<T> ctx) where T : class
		{
			if (ctx == null)
				throw new ArgumentNullException(nameof(ctx));

			if (!ctx.IsSended)
				throw new InvalidOperationException();

			if (!ctx.IsSuccess || ctx.Result == null)
				return false;

			if (ctx.Result is ResponseFileContent && !(ctx.Result as ResponseFileContent).Success)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 设置请求的域
		/// </summary>
		/// <param name="context">当前HTTP会话</param>
		/// <param name="range">请求的区域</param>
		public static void RequestRange(this HttpContext context, RANGETYPE range)
		{
			context.Request.Range = range;
		}


		/// <summary>
		/// 设置发送延迟。设置此标记后，发送前将会等待指定的时间后再发送。
		/// </summary>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static HttpContext SendDelay(this HttpContext context, TimeSpan value)
		{
			context.SendDelay = value;
			return context;
		}

		/// <summary>
		/// 设置发送延迟。设置此标记后，发送前将会等待指定的时间后再发送。
		/// </summary>
		/// <param name="context"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static HttpContext<T> SendDelay<T>(this HttpContext<T> context, TimeSpan value) where T : class
		{
			context.SendDelay = value;
			return context;
		}

		/// <summary>
		/// 设置此请求接受JSON响应
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static HttpContext<T> SetAcceptJson<T>(this HttpContext<T> context) where T : class
		{
			context.Request.Accept = "application/json";
			return context;
		}

		/// <summary>
		/// 设置此请求接受XML响应
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static HttpContext<T> SetAcceptXml<T>(this HttpContext<T> context) where T : class
		{
			context.Request.Accept = "text/xml";
			return context;
		}

		/// <summary>
		/// 设置此请求接受的结果类型
		/// </summary>
		/// <param name="context"></param>
		/// <param name="accpet"></param>
		/// <returns></returns>
		public static HttpContext SetAccept(this HttpContext context, string accpet = "*/*")
		{
			context.Request.Accept = accpet;
			return context;
		}

		/// <summary>
		/// 添加额外的标头（此方法用于添加额外的标头，特殊的标头需要直接设置相关属性）
		/// </summary>
		/// <param name="context"></param>
		/// <param name="header">标头</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public static HttpContext<T> AddRequestHeader<T>(this HttpContext<T> context, HttpRequestHeader header, string value) where T : class
		{
			context.Request.Headers.Add(header, value);
			return context;
		}

		/// <summary>
		/// 添加额外的标头（此方法用于添加额外的标头，特殊的标头需要直接设置相关属性）
		/// </summary>
		/// <param name="context"></param>
		/// <param name="header">标头</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public static HttpContext AddRequestHeader(this HttpContext context, HttpRequestHeader header, string value)
		{
			context.Request.Headers.Add(header, value);
			return context;
		}

		/// <summary>
		/// 添加额外的标头（此方法用于添加额外的标头，特殊的标头需要直接设置相关属性）
		/// </summary>
		/// <param name="context"></param>
		/// <param name="header">标头</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public static HttpContext<T> AddRequestHeader<T>(this HttpContext<T> context, string header, string value) where T : class
		{
			context.Request.Headers.Add(header, value);
			return context;
		}

		/// <summary>
		/// 添加额外的标头（此方法用于添加额外的标头，特殊的标头需要直接设置相关属性）
		/// </summary>
		/// <param name="context"></param>
		/// <param name="header">标头</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public static HttpContext AddRequestHeader(this HttpContext context, string header, string value)
		{
			context.Request.Headers.Add(header, value);
			return context;
		}

		/// <summary>
		/// 设置请求引用页，并返回自己
		/// </summary>
		/// <param name="context">当前的 <typeparam name="T">类型</typeparam></param>
		/// <param name="refer">引用页</param>
		/// <returns></returns>
		public static T SetRefer<T>(this T context, string refer) where T : HttpContext
		{
			context.Request.Referer = refer;
			return context;
		}

		/// <summary>
		/// 设置是否允许自动重定向
		/// </summary>
		/// <typeparam name="T">当前的实际请求内容类型</typeparam>
		/// <param name="context">当前请求类型为 <typeparamref name="T" /> 的上下文</param>
		/// <param name="enabled">是否允许自动重定向，如果允许，则为 <see langword="true" />，否则为 <see langword="false" /> 。默认为<see langword="true" /> 。</param>
		public static T SetAllowAutoRedirect<T>(this T context, bool enabled = true) where T : HttpContext
		{
			context.Request.AllowAutoRedirect = enabled;
			return context;
		}

		/// <summary>
		/// 设置是允许自动重定向
		/// </summary>
		/// <typeparam name="T">当前的实际请求内容类型</typeparam>
		/// <param name="context">当前请求类型为 <typeparamref name="T" /> 的上下文</param>
		public static T EnableAutoRedirect<T>(this T context) where T : HttpContext
		{
			context.Request.AllowAutoRedirect = true;
			return context;
		}

		/// <summary>
		/// 设置是不允许自动重定向
		/// </summary>
		/// <typeparam name="T">当前的实际请求内容类型</typeparam>
		/// <param name="context">当前请求类型为 <typeparamref name="T" /> 的上下文</param>
		public static T DisableAutoRedirect<T>(this T context) where T : HttpContext
		{
			context.Request.AllowAutoRedirect = false;
			return context;
		}

		/// <summary>
		/// 设置请求来源Origin请求头
		/// </summary>
		/// <typeparam name="T">当前的实际请求内容类型</typeparam>
		/// <param name="context">当前请求类型为 <typeparamref name="T" /> 的上下文</param>
		/// <param name="origin">当前请求来源</param>
		/// <returns></returns>
		public static T FromOrigin<T>(this T context, string origin) where T : HttpContext
		{
			context.Request.Origin = origin;

			return context;
		}

		/// <summary>
		/// 设置当前的请求头（XmlHttpRequest）
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">上下文</param>
		/// <param name="enabled">如果为 <see langword="true" />，则会设置此标头；如果为 <see langword="false" />，则会清除此标头</param>
		/// <returns></returns>
		public static T ViaXmlHttpRequest<T>(this T context, bool enabled = true) where T : HttpContext
		{
			context.Request.AppendAjaxHeader = enabled;
			return context;
		}

		/// <summary>
		/// 设置请求超时时间
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">上下文</param>
		/// <param name="timeout">超时时间</param>
		/// <returns></returns>
		public static T Timeout<T>(this T context, int timeout) where T : HttpContext
		{
			context.Request.Timeout = timeout;
			return context;
		}

		/// <summary>
		/// 设置读写超时
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">上下文</param>
		/// <param name="timeout">超时时间</param>
		/// <returns></returns>
		public static T ReadWriteTimeout<T>(this T context, int timeout) where T : HttpContext
		{
			context.Request.ReadWriteTimeout = timeout;
			return context;
		}

		/// <summary>
		/// 设置当前请求的Cookies处理模式
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">上下文</param>
		/// <param name="method">处理模式</param>
		/// <returns></returns>
		public static T CookiesHandle<T>(this T context, CookiesHandleMethod method) where T : HttpContext
		{
			context.Request.CookiesHandleMethod = method;
			return context;
		}

		/// <summary>
		/// 开启当前上下文的性能速度计数
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">上下文</param>
		/// <returns></returns>
		public static T WithSpeedMeter<T>(this T context) where T : HttpContext
		{
			context.AutoStartSpeedMonitor = true;
			return context;
		}

		/// <summary>
		/// 设置本地的缓存信息以便于提交给服务器确认是否有更新
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="context">当前上下文</param>
		/// <param name="etag">ETAG</param>
		/// <param name="modifiedSince">最后修改时间</param>
		/// <returns></returns>
		public static T WithLocalCache<T>(this T context, string etag = null, DateTime? modifiedSince = null) where T : HttpContext
		{
			if (etag.IsNullOrEmpty() && modifiedSince == null)
				throw new InvalidOperationException("At least one parameter must be set.");

			context.Request.IfModifiedSince = modifiedSince;
			if (!etag.IsNullOrEmpty())
				context.Request.Headers.Add(HttpRequestHeader.IfNoneMatch, etag);

			return context;
		}

		#endregion
	}
}