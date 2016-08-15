using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 类型 ResponseObjectWrapper
	/// </summary>
	public abstract class ResponseObjectWrapper
	{

		internal abstract HttpResponseContent ToResponseObjectContent(HttpContext context, HttpClient client);

		/// <summary>
		/// 将指定的对象 <typeparamref name="T"/> 封装为封装对象 <see cref="ResponseObjectWrapper{T}"/>
		/// </summary>
		/// <param name="obj">要封装的对象</param>
		/// <typeparam name="T">要封装的对象类型</typeparam>
		/// <returns>封装的结果</returns>
		public static ResponseObjectWrapper<T> Wrap<T>(T obj = default(T)) where T : class
		{
			return obj;
		}
	}

	/// <summary>
	/// 类 ResponseObjectWrapper.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ResponseObjectWrapper<T> : ResponseObjectWrapper where T : class
	{
		private ResponseObjectWrapper()
		{
		}

		internal override HttpResponseContent ToResponseObjectContent(HttpContext context, HttpClient client)
		{
			return new ResponseObjectContent<T>(context, client);
		}

		/// <summary>
		/// 获得或设置内部对象
		/// </summary>
		public T ObjectInternal { get; private set; }

		/// <summary>
		/// Performs an implicit conversion from <see cref="ResponseObjectWrapper{T}"/> to <see cref="T"/>.
		/// </summary>
		/// <param name="wrapper">The wrapper.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator T(ResponseObjectWrapper<T> wrapper) => wrapper.ObjectInternal;

		/// <summary>
		/// Performs an implicit conversion from <see cref="T"/> to <see cref="ResponseObjectWrapper{T}"/>.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ResponseObjectWrapper<T>(T obj) => new ResponseObjectWrapper<T>() { ObjectInternal = obj };
	}
}
