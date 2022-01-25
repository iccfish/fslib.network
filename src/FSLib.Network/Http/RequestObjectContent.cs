using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FSLib.Network.Http
{
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Reflection;
using Newtonsoft.Json;

/// <summary>
/// 表示一个对象
/// </summary>
	public class RequestObjectContent<T> : RequestFormDataContent
	{
		private IFormDataKeyGenerator _keyGenerator;

		/// <summary>
		/// 创建 <see>
		///     <cref>RequestObjectContent</cref>
		/// </see>
		///     的新实例(RequestObjectContent)
		/// </summary>
		public RequestObjectContent(T objectVar, ContentType contentType = ContentType.FormUrlEncoded) : base(contentType)
		{
			Object = objectVar;
		}

		/// <summary>
		/// 绑定对象数据
		/// </summary>
		protected virtual void BindObject(object obj, string prefix = "", int level = 0)
		{
			if (obj is IFormData)
			{
				var formdata = obj as IFormData;

				var fields = formdata.GetAllFields();
				fields.ForEach(s => StringField.Add(KeyGenerator.Generate(prefix, s.Key), s.Value));
				var files = formdata.GetAllFiles();
				files.ForEach(s =>
				{
					s.FieldName = KeyGenerator.Generate(prefix, s.FieldName);
					PostedFile.Add(s);
				});

				formdata.BindData(Client, Context, Message, this, prefix, level);
			}
			else if (obj is System.Collections.Specialized.NameValueCollection)
			{
				var nvc = (System.Collections.Specialized.NameValueCollection)obj;
				nvc.AllKeys.ForEach(s => StringField.Add(KeyGenerator.Generate(prefix, s), nvc[s]));
			}
			else if (obj is IDictionary<string, string>)
			{
				var nvc = (IDictionary<string, string>)obj;
				nvc.ForEach(s => StringField.Add(KeyGenerator.Generate(prefix, s.Key), s.Value));
			}
			else if (obj is IDictionary<string, object>)
			{
				var nvc = (IDictionary<string, object>)obj;
				nvc.ForEach(s => BindField(KeyGenerator.Generate(prefix, s.Key), s.Value, level < KeyGenerator.MaximumDeepth, level));
			}
			else if (obj is string[][])
			{
				var zigzagArray = (string[][])obj;
				foreach (var item in zigzagArray)
				{
					StringField.Add(KeyGenerator.Generate(prefix, item[0]), item[1] ?? "");
				}
			}
			else if (obj is string[])
			{
				var array = (string[])obj;
				for (int i = 0; i < array.Length; i++)
				{
					StringField.Add(KeyGenerator.Generate(prefix, i), array[i] ?? "");
				}
			}
			else if (obj is string[,])
			{
				var multiArray = (string[,])obj;
				for (int i = 0; i <= multiArray.GetUpperBound(0); i++)
				{
					StringField.Add(KeyGenerator.Generate(prefix, multiArray[i, 0]), multiArray[i, 1]);
				}
			}
			else
			{
				var props = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>();
				foreach (var pd in props)
				{
					if (pd.Attributes.OfType<IgnoreFieldAttribute>().Any()) continue;

					var fn = pd.Attributes.OfType<FormNameAttribute>().FirstOrDefault();
					var fp = pd.Attributes.OfType<AttachedFileAttribute>().FirstOrDefault();

					var key = KeyGenerator.Generate(prefix, (fn == null ? pd.Name : fn.Name));
					if (fp != null)
					{
						if (pd.PropertyType == typeof(string))
						{
							var path = (pd.GetValue(obj) ?? "").ToString();
							if (path.IsNullOrEmpty()) continue;

							PostedFile.Add(new HttpPostFile(key, path));
						}
						else
							throw new InvalidOperationException("附加上传文件的属性属性只能是字符串");
						continue;
					}

					var v = pd.GetValue(obj);

					//序列化
					var satt = pd.Attributes.OfType<ObjectSerializeAttribute>().FirstOrDefault();
					if (satt != null)
					{
						switch (satt.SerializeType)
						{
							case ObjectSerializationType.Xml:
								StringField.Add(key, v.XmlSerializeToString());
								break;
							case ObjectSerializationType.Json:
								StringField.Add(key, Utils.JsonSerialize(v, Context.JsonSerializationSetting));
								break;
						}
						continue;
					}

					BindField(key,v, level < KeyGenerator.MaximumDeepth, level);
				}
			}
		}

		void BindField(string key, object value, bool enableRecursive, int level)
		{
			if (value == null)
			{
				StringField.Add(key, "");
				return;
			}

			if (value is Image)
			{
				var img = value as Image;
				PostedFile.Add(new RequestImageField(img, ImageFormat.Jpeg, 90, key, @"c:\ifish.jpg"));
				return;
			}

			var type = value.GetType();

			if (type == typeof(string) || type.IsValueType)
			{
				StringField.Add(key, value.ToString());
			}
			else if (type == typeof(byte[]))
			{
				//bytes[] 直接当作文件
				PostedFile.Add(new HttpVirtualBytePostFile(key, "", value as byte[]));
			}
			else if (type.IsSubclassOf(typeof(Stream)))
			{
				var stream = value as Stream;
				if (stream != null)
				{
					PostedFile.Add(new HttpVirtualStreamPostFile(key, "", stream));
				}
			}
			else if (typeof(HttpPostFile).IsAssignableFrom(type))
			{
				var file = (HttpPostFile)value;
				if (string.IsNullOrEmpty(file.FieldName))
					file.FieldName = key;
				PostedFile.Add(file);
			}
			else
			{
				if (enableRecursive)
					BindObject(value, key, level + 1);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Object = default;
		}

		/// <summary>
		/// 准备数据
		/// </summary>
		public override void PrepareData()
		{
			if (!DataBinded)
			{
				DataBinded = true;
				BindObject(Object);
			}

			base.PrepareData();
		}

		/// <summary>
		/// 获得或创建当前默认使用的命名生成类
		/// </summary>
		public IFormDataKeyGenerator KeyGenerator
		{
			get { return _keyGenerator ?? (_keyGenerator = new DefaultFormDataKeyGenerator()); }
			set { _keyGenerator = value; }
		}

		/// <summary>
		/// 获得要上传的对象
		/// </summary>
		public T Object { get; private set; }

		protected bool DataBinded { get; set; }
	}
}
