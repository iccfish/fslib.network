using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSLib.Network.Http
{
	using System.Drawing;

	/// <summary>
	/// 
	/// </summary>
	public class ResponseImageContent : ResponseBinaryContent, IDisposable
	{

		/// <summary>
		/// 创建 <see cref="ResponseImageContent"/>  的新实例(HttpImageResponse)
		/// </summary>
		public ResponseImageContent(HttpContext context, HttpClient client)
			: base(context, client)
		{

		}

		/// <summary>
		/// 获得创建的图像
		/// </summary>
		public Image Image { get; private set; }

		#region Overrides of HttpResponseContent

		/// <summary>
		/// 请求处理最后的内容
		/// </summary>
		protected override void ProcessFinalResponse()
		{
			OnPreContentProcessed();

			base.ProcessFinalResponse();

			//保存缓存
			Context.ContextData["imagedata"] = ResultStream;

			try
			{
				Image = Image.FromStream(ResultStream);
			}
			catch (Exception ex)
			{
				Image = null;
				Context.ContextData["imageException"] = ex;
				if (AsyncData != null)
					AsyncData.Exception = ex;
				else
					throw;
			}

			OnPostContentProcessed();
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			Image = null;
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			Image = null;
		}

		/// <summary>
		/// 返回表示当前 <see cref="T:System.Object"/> 的 <see cref="T:System.String"/>。
		/// </summary>
		/// <returns>
		/// <see cref="T:System.String"/>，表示当前的 <see cref="T:System.Object"/>。
		/// </returns>
		public override string ToString()
		{
			if (Image == null)
			{
				return Exception?.Message;
			}

			return $"size={Image.Width}x{Image.Height} format={Image.PixelFormat} datasize={ResultStream.Length}";
		}
	}
}
