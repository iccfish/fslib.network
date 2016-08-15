using System.Linq;

namespace FSLib.Network.Http
{
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;

	/// <summary>
	/// 表示一个表单中的图像域
	/// </summary>
	public class RequestImageField : HttpVirtualBytePostFile
	{
		/// <summary>
		/// 图像对象
		/// </summary>
		private Image Image { get; set; }

		/// <summary>
		/// 图像格式
		/// </summary>
		private ImageFormat ImageFormat { get; set; }

		/// <summary>
		/// 保存的质量（仅JPEG）
		/// </summary>
		public int Quality { get; set; }

		/// <summary>
		/// 表示一个上传的域
		/// </summary>
		/// <param name="image"></param>
		/// <param name="format"></param>
		/// <param name="quality"></param>
		/// <param name="fieldName"></param>
		/// <param name="filePath"></param>
		public RequestImageField(Image image, ImageFormat format = null, int quality = 90, string fieldName = null, string filePath = null)
			: base(fieldName, filePath, null)
		{
			if (format == null)
			{
				format = ImageFormat.Jpeg;
			}
			Image = image;
			ImageFormat = format;
			Quality = quality;

		}

		/// <summary>
		/// 计算长度(含开始信息)
		/// </summary>
		/// <returns></returns>
		public override long ComputeLength()
		{
			var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().FirstOrDefault(s => s.FormatID == ImageFormat.Guid);
			var encodeParamters = new EncoderParameters(1);
			encodeParamters.Param[0] = new EncoderParameter(Encoder.Quality, Quality);

			using (var ms = new MemoryStream())
			{
				Image.Save(ms, encoder, encodeParamters);
				Data = ms.ToArray();
			}
			ContentType = encoder.MimeType;

			return base.ComputeLength();
		}
	}
}
