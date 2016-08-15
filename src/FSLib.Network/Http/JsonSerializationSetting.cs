using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using Newtonsoft.Json;

	/// <summary>
	/// JSON设置基类
	/// </summary>
	public class JsonSetting
	{
		/// <summary>
		/// 新建一个 <see cref="JsonSetting"/> 的实例
		/// </summary>
		public JsonSetting()
		{
			Setting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
		}

		/// <summary>
		/// 获得或设置用于序列化的设置
		/// </summary>
		public JsonSerializerSettings Setting { get; private set; }

		/// <summary>
		/// 获得或设置用于反序列化的Json反序列化类
		/// </summary>
		public JsonConverter[] JsonConverts { get; set; }
	}

	/// <summary>
	/// 序列化类时的设置
	/// </summary>
	public class JsonSerializationSetting : JsonSetting
	{
		/// <summary>
		/// 获得或设置用于序列化的格式设置
		/// </summary>
		public Formatting Formatting { get; set; }
	}

	/// <summary>
	/// 反序列化类时的设置
	/// </summary>
	public class JsonDeserializationSetting : JsonSetting
	{
		/// <summary>
		/// 获得或设置在创建结果的过程中，如果已经提供了原始对象，是否保持原有对象不变。
		/// <para>如果设置为false，则会创建新对象。如果设置为true，则会使用源对象并填充数据</para>
		/// <para>默认为ture，此设置与JSON.NET库自身的反序列化设置并没有关系。</para>
		/// </summary>
		public bool KeepOriginalObject { get; set; }

		/// <summary>
		/// 创建 <see cref="JsonDeserializationSetting" />  的新实例(JsonDeserializationSetting)
		/// </summary>
		public JsonDeserializationSetting()
		{
			KeepOriginalObject = true;
		}
	}
}
