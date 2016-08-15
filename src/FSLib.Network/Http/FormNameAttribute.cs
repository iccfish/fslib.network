using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	/// <summary>
	/// 设置一个表单的别名
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class FormNameAttribute : System.Attribute
	{
		/// <summary>
		/// 获得表单名
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 创建 <see cref="FormNameAttribute" />  的新实例(FormNameAttribute)
		/// </summary>
		public FormNameAttribute(string name)
		{
			Name = name;
		}
	}
}
