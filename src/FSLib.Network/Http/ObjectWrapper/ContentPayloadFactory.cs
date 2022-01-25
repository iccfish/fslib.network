namespace FSLib.Network.Http.ObjectWrapper;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

/// <summary>
/// Request和Response内容承载的包装行为默认实现
/// </summary>
public class ContentPayloadFactory : IContentPayloadFactory
{
	class BuilderContextItem
	{
		public Func<RequestWrapRequestContentEventArgs, HttpRequestContent> CachedWrapMethod { get; set; }
		public Action<GetPreferredResponseTypeEventArgs> CachedGetResponseContentMethod { get; set; }

		public object BuilderObject { get; set; }

	}

	static Dictionary<Type, BuilderContextItem> _contextCache = new();
	private static Dictionary<Type, object> _builderObjects = new();

	static object GetBuilderObject(Type t)
	{
		object obj;

		if (_builderObjects.TryGetValue(t, out obj))
		{
			return obj;
		}

		lock (_builderObjects)
		{
			if (_builderObjects.TryGetValue(t, out obj))
			{
				return obj;
			}

			obj = Activator.CreateInstance(t);
			_builderObjects[t] = obj;
		}

		return obj;
	}

	/// <summary>
	/// 包装内容
	/// </summary>
	/// <param name="ea"></param>
	/// <returns></returns>
	public HttpRequestContent WrapRequestContent(RequestWrapRequestContentEventArgs ea)
	{
		if (ea.RequestContent == null)
			return null;

		var contextItem = GetBuilderContextItem(ea.RequestContent.GetType());

		return contextItem?.CachedWrapMethod?.Invoke(ea);
	}

	/// <summary>
	/// 包装内容
	/// </summary>
	/// <returns></returns>
	public void GetResponseContent(GetPreferredResponseTypeEventArgs ea)
	{
		if (ea.ResultType == null)
			return;

		var contextItem = GetBuilderContextItem(ea.ResultType);

		contextItem?.CachedGetResponseContentMethod?.Invoke(ea);
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
			item = new BuilderContextItem();

			foreach (var attribute in attributes)
			{
				if (attribute is RequestContentBuilderAttribute att1)
				{
					BuildRequestContentMethod(att1.Type, item);
				}
				else if (attribute is ResponseContentBuilderAttribute att2)
				{
					BuildResponseContentMethod(att2.Type, item);
				}
			}

			if (item.CachedWrapMethod == null)
				BuildRequestContentMethod(typeof(DefaultContentPayloadBuilder), item);
			if (item.CachedGetResponseContentMethod == null)
				BuildResponseContentMethod(typeof(DefaultContentPayloadBuilder), item);

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
			item.BuilderObject = GetBuilderObject(t);

		var p2 = Expression.Parameter(typeof(RequestWrapRequestContentEventArgs), "p1");
		var callMethod = Expression.Call(
			Expression.Constant(item.BuilderObject),
			t.GetMethod("BuildRequestContent"),
			p2
		);

		item.CachedWrapMethod = Expression.Lambda<Func<RequestWrapRequestContentEventArgs, HttpRequestContent>>(callMethod, p2).Compile();
	}

	static void BuildResponseContentMethod(Type t, BuilderContextItem item)
	{
		var iface = t.GetInterface(typeof(IResponseContentBuilder).FullName);
		if (iface == null)
			return;

		if (item.BuilderObject == null)
			item.BuilderObject = GetBuilderObject(t);

		var p2 = Expression.Parameter(typeof(GetPreferredResponseTypeEventArgs), "p2");
		var callMethod = Expression.Call(
			Expression.Constant(item.BuilderObject),
			t.GetMethod(nameof(IResponseContentBuilder.BuildResponseContentWrap)),
			p2
		);

		item.CachedGetResponseContentMethod = Expression.Lambda<Action<GetPreferredResponseTypeEventArgs>>(callMethod, p2).Compile();
	}
}
