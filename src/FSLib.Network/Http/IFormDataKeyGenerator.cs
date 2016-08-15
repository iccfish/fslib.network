using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.ComponentModel;

	/// <summary>
	/// 表单数据表单名生成工具
	/// </summary>
	public interface IFormDataKeyGenerator
	{
		/// <summary>
		/// 最大默认深度
		/// </summary>
		int MaximumDeepth { get; set; }

		/// <summary>
		/// 生成键名
		/// </summary>
		/// <param name="parentKey">上级KEY</param>
		/// <param name="currentKey">当前名</param>
		/// <returns>生成的键名</returns>
		string Generate(string parentKey, string currentKey);

		/// <summary>
		/// 生成键名
		/// </summary>
		/// <param name="parentKey">上级KEY</param>
		/// <param name="index">针对数组元素，当前的索引</param>
		/// <returns>生成的键名</returns>
		string Generate(string parentKey, int index);
	}

	/// <summary>
	/// 表单数据表单名生成工具 <see cref="IFormDataKeyGenerator"/> 的默认实现
	/// </summary>
	class DefaultFormDataKeyGenerator : IFormDataKeyGenerator
	{
		/// <summary>
		/// 最大递归深度，默认为2
		/// </summary>
		[DefaultValue(2)]
		public int MaximumDeepth { get; set; } = 2;

		/// <summary>
		/// 生成键名
		/// </summary>
		/// <param name="parentKey">上级KEY</param>
		/// <param name="currentKey">当前名</param>
		/// <returns>生成的键名</returns>
		public string Generate(string parentKey, string currentKey)
		{
			if(parentKey.IsNullOrEmpty())
				return currentKey;
			
			return parentKey + "_" + currentKey;
		}

		/// <summary>
		/// 生成键名
		/// </summary>
		/// <param name="parentKey">上级KEY</param>
		/// <param name="index">针对数组元素，当前的索引</param>
		/// <returns>生成的键名</returns>
		public string Generate(string parentKey, int index)
		{
			parentKey = parentKey ?? "";
			return parentKey + "[" + index + "]";
		}
	}
}
