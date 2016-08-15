namespace FSLib.Network.Http
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;

	/// <summary>
	/// Request和Response内容承载的包装行为默认实现
	/// </summary>
	public class ContentPayloadBuilder : IContentPayloadBuilder
	{
		class BuilderContextItem
		{
			public Func<object, RequestWrapRequestContentEventArgs, HttpRequestContent> CachedWrapMethod { get; set; }
			public Func<object, GetPreferedResponseTypeEventArgs, HttpResponseContent> CachedGetResponseContentMethod { get; set; }

			public object BuilderObject { get; set; }

		}

		static Dictionary<Type, BuilderContextItem> _contextCache = new Dictionary<Type, BuilderContextItem>();

		/// <summary>
		/// 包装内容
		/// </summary>
		/// <param name="ea"></param>
		/// <returns></returns>
		public HttpRequestContent WrapRequestContent(object data, RequestWrapRequestContentEventArgs ea)
		{
			if (data == null)
				return null;

			var contextItem = GetBuilderContextItem(data.GetType());

			return contextItem?.CachedWrapMethod?.Invoke(data, ea);
		}

		/// <summary>
		/// 包装内容
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public HttpResponseContent GetResponseContent<T>(T data, GetPreferedResponseTypeEventArgs<T> ea)
		{
			if (data == null)
				return null;

			var contextItem = GetBuilderContextItem(data.GetType());

			return contextItem?.CachedGetResponseContentMethod?.Invoke(data, ea);
		}


		static BuilderContextItem GetBuilderContextItem(Type type)
		{
			BuilderContextItem item;
			if (_contextCache.TryGetValue(type, out item))
				return item;
			lock (_contextCache)
			{
				if (_contextCache.TryGetValue(type, out item))
					return item;

				var attributes = type.GetCustomAttributes(true);

				foreach (var attribute in attributes)
				{
					if (attribute is RequestContentBuilderAttribute)
					{
						BuildRequestContentMethod((attribute as RequestContentBuilderAttribute).Type, item ?? (item = new BuilderContextItem()));
					}
					else if (attribute is ResponseContentBuilderAttribute)
					{
						BuildResponseContentMethod((attribute as ResponseContentBuilderAttribute).Type, item ?? (item = new BuilderContextItem()));
					}
				}

				_contextCache.Add(type, item);
			}

			return item;
		}

		static void BuildRequestContentMethod(Type t, BuilderContextItem item)
		{
			//get interface type
			var iface = t.GetInterface(typeof(IRequestContentBuilder<>).FullName);
			if (iface == null)
				return;
			var dataType = iface.GetGenericArguments()[0];

			if (item.BuilderObject == null)
				item.BuilderObject = Activator.CreateInstance(t);

			var p1 = Expression.Parameter(typeof(object), "p1");
			var p2 = Expression.Parameter(typeof(RequestWrapRequestContentEventArgs), "p2");
			var callMethod = Expression.Call(
											 Expression.Constant(item.BuilderObject),
											t.GetMethod("BuildRequestContent"),
											Expression.Convert(p1, dataType),
											p2
				);

			item.CachedWrapMethod = Expression.Lambda<Func<object, RequestWrapRequestContentEventArgs, HttpRequestContent>>(callMethod, p1, p2).Compile();
		}

		static void BuildResponseContentMethod(Type t, BuilderContextItem item)
		{
			//get interface type
			var iface = t.GetInterface(typeof(IResponseContentBuilder<>).FullName);
			if (iface == null)
				return;

			var dataType = iface.GetGenericArguments()[0];

			if (item.BuilderObject == null)
				item.BuilderObject = Activator.CreateInstance(t);

			var p1 = Expression.Parameter(typeof(object), "p1");
			var p2 = Expression.Parameter(typeof(GetPreferedResponseTypeEventArgs), "p2");
			var callMethod = Expression.Call(
											 Expression.Constant(item.BuilderObject),
											t.GetMethod("BuildResponseContentWrap"),
											Expression.Convert(p1, dataType),
											Expression.Convert(p2, typeof (GetPreferedResponseTypeEventArgs<>).MakeGenericType(dataType))
				);

			item.CachedGetResponseContentMethod = Expression.Lambda<Func<object, GetPreferedResponseTypeEventArgs, HttpResponseContent>>(callMethod, p1, p2).Compile();
		}
	}
}