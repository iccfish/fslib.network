using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace FSLib.Network.Http
{
	using System.FishExtension;
	using System.Net;
	using System.Security.Cryptography.X509Certificates;
#if !NET45
	using System.RunTime.CompilerServices;
#endif
	using System.Text.RegularExpressions;

	/// <summary>
	/// HTTP设置
	/// </summary>
	public class HttpSetting : INotifyPropertyChanged
	{

		private IContentPayloadBuilder _contentPalPayloadBuilder;

		bool _proxySet = false;

		/// <summary>
		/// 创建 <see cref="HttpSetting" />  的新实例(HttpSetting)
		/// </summary>
		public HttpSetting()
		{
			AcceptLanguage = System.Globalization.CultureInfo.CurrentCulture.Name;
			UserAgent = DefaultUserAgent;
			_proxy = WebRequest.DefaultWebProxy;
			_proxySet = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// 校验类型支持度
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Exception CheckObjectTypeSupport(Type type)
		{
			if (DisableComptatibleCheck)
				return null;

			return _checkCache.GetValue(type, _ =>
			{

				var fullName = _.FullName;
				if (fullName == "System.Windows.Forms.HtmlDocument")
					return new Exception(SR.Tip_HtmlDocumentNotSupport);
				if (fullName == "HtmlAgilityPack.HtmlDocument")
					return new Exception(SR.Tip_HtmlDocumentNotInited);

				return null;
			});
		}

		/// <summary>
		/// 显式禁用代理
		/// </summary>
		public void DisableProxy()
		{
			Proxy = null;
		}

		/// <summary>
		/// 初始化上下文
		/// </summary>
		/// <param name="context"></param>
		public virtual void InitializeHttpContext(HttpContext context)
		{
			var request = context.Request;

			request.Accept = Accept;
			if (Timeout.HasValue)
				request.Timeout = Timeout.Value;
			if (ReadWriteTimeout.HasValue)
				request.ReadWriteTimeout = ReadWriteTimeout.Value;
			request.UserAgent = UserAgent;
			request.Headers = new WebHeaderCollection();
			if (Headers != null && Headers.Count > 0)
			{
				foreach (string item in Headers.AllKeys)
				{
					if (Headers[item] == null || request.Headers[item] != null || WebHeaderCollection.IsRestricted(item))
						continue;

					request.Headers[item] = Headers[item];
				}
			}
			request.KeepAlive = KeepAlive;
			request.TransferEncoding = TransferEncoding;
			request.UseNagleAlgorithm = UseNagleAlgorithm;
			request.AllowWriteStreamBuffering = AllowWriteStreamBuffering;
			request.Authorization = AuthorizationManager?.GetAuthorization(context) ?? Authorization;
			if (KeepReferBetweenRequest && request.Referer.IsNullOrEmpty() && LastUri != null)
			{
				request.Referer = LastUri.OriginalString;
			}
			request.AppendAjaxHeader = AppendAjaxHeader;

			request.Encoding = StringEncoding;
			request.ForceStreamBufferWithProxy = ForceStreamBufferWithProxy;
			if (SocketReceiveBufferSize.HasValue)
				request.SocketReceiveBufferSize = SocketReceiveBufferSize;
			context.JsonDeserializationSetting = JsonDeserializationSetting;
			context.JsonSerializationSetting = JsonSerializationSetting;


			//如果没有设置过代理并且现在的代理是系统默认的，那么就处理默认代理
			if (_proxySet)
			{
				request.WebProxy = _proxy;
			}
			else if (_defaultProxySet)
			{
				request.WebProxy = _defaultProxy;
			}
			request.PreAuthenticate = PreAuthenticate;
		}

		/// <summary>
		/// 设置接受JSON类型的响应
		/// </summary>
		public void SetAcceptJson()
		{
			Accept = "application/json";
		}

		/// <summary>
		/// 设置接受XML类型的响应
		/// </summary>
		public void SetAcceptXml()
		{
			Accept = "text/xml";
		}

		/// <summary>
		/// 设置不保持连接
		/// </summary>
		public void SetCloseConnection()
		{
			Connection = "Close";
		}

		/// <summary>
		/// 获得或设置是否自动为提交数据的 ContentType 添加字符集说明
		/// </summary>
		/// <value>如果设置为 <see langword="true"/> ，那么如果提交的指定的 ContentType 没有字符集说明，会自动添加。</value>
		public bool AutoAppendCharsetInContentType { get; set; } = true;

		/// <summary>
		/// 内容数据包装工厂
		/// </summary>
		public IContentPayloadBuilder ContentPayloadBuilder
		{
			get { return _contentPalPayloadBuilder ?? (_contentPalPayloadBuilder = new ContentPayloadBuilder()); }
			set { _contentPalPayloadBuilder = value; }
		}

		/// <summary>
		/// 获得或设置cookies处理逻辑
		/// </summary>
		public CookiesHandleMethod CookiesHandleMethod { get; set; } = CookiesHandleMethod.Auto;

		/// <summary>
		/// 默认最多重试次数
		/// </summary>
		public int DefaultRetryLimit { get; set; } = 5;

		/// <summary>
		/// 默认重试的时候等待时间（默认为100）
		/// </summary>
		public int DefaultRetrySleepTime { get; set; } = 100;

		/// <summary>
		/// 获得或设置是否启用URL字段替换
		/// </summary>
		public bool EnableUrlTokenIdentitier { get; set; } = true;


		/// <summary>
		/// 获得或设置如果请求发生了HTTP协议级别的错误（返回码大于400小于等于599），那么返回什么样的内容 
		/// </summary>
		/// <value>The response object.</value>
		public ResponseObjectWrapper ErrorResponseObject { get; set; }

		/// <summary>
		/// 获得或设置JSON反序列化设置
		/// </summary>
		public JsonDeserializationSetting JsonDeserializationSetting { get; set; }

		/// <summary>
		/// 获得或设置JSON序列化设置
		/// </summary>
		public JsonSerializationSetting JsonSerializationSetting { get; set; }

		/// <summary>
		/// 获得或设置是否在不同的请求中保持引用
		/// </summary>
		public bool KeepReferBetweenRequest { get; set; } = true;


		/// <summary>
		/// 获得或设置当前 <see cref="HttpClient"/> 的使用代理
		/// </summary>
		public IWebProxy Proxy
		{
			get { return _proxy; }
			set
			{
				_proxy = value;
				_proxySet = true;
			}
		}

		/// <summary>
		/// 文件操作时获得或设置读取缓冲区大小
		/// </summary>
		public int ReadBufferSize { get; set; } = 0x400 * 4;

		/// <summary>
		/// 获得或设置默认的Socket接收缓存
		/// </summary>
		public int? SocketReceiveBufferSize { get; set; }

		/// <summary>
		/// 获得或设置是否重新分析设置Cookies标头（仅供当原Cookies带有逗号值等自带的分析器无法分析的情况使用）
		/// 当启用此属性时，AutoRedirect将会被强行禁用，以防止出错。
		/// </summary>
		public bool UseNonstandardCookieParser { get; set; } = false;

		/// <summary>
		/// 文件操作时获得或设置写入缓冲区大小
		/// </summary>
		public int WriteBufferSize { get; set; } = 0x400 * 4;

		/// <summary>
		/// 获得客户端证书集合
		/// </summary>
		public X509Certificate[] X509Certificates { get; set; }

		/// <summary>
		/// 获得或设置证书管理器
		/// </summary>
		public ICertificateManager CertificateManager
		{
			get { return _certificateManager ?? (_certificateManager = new CertificateManager()); }
			set { _certificateManager = value; }
		}


		static Dictionary<Type, Exception> _checkCache = new Dictionary<Type, Exception>();

		#region 静态变量

		/// <summary>
		/// 获得或设置是否添加类库作者的UserAgent标记
		/// </summary>
		public static bool AppendLibAuthorVendor { get; set; }

		/// <summary>
		/// 获得或设置是否禁用内置的提示
		/// </summary>
		public static bool DisableComptatibleCheck { get; set; } = false;

		/// <summary>
		/// 获得或设置是否默认预先授权
		/// </summary>
		public bool PreAuthenticate { get; set; } = true;

		static bool _defaultProxySet = false;

		/// <summary>
		/// 获得或设置默认使用的代理服务器。
		/// <para>此设置仅仅会影响HttpClient类型的全局代理服务器设置，不会影响到HttpWebRequest的代理服务器设置</para>
		/// </summary>
		public static IWebProxy DefaultProxy
		{
			get { return _defaultProxy; }
			set
			{
				_defaultProxy = value;
				_defaultProxySet = true;
			}
		}

		/// <summary>
		/// 获得或设置是否强制对使用代理的情况进行流缓冲。如果禁止，在某些特定情况下可能会失败并引发异常。
		/// </summary>
		public bool ForceStreamBufferWithProxy { get; set; }

		/// <summary>
		/// 默认的UserAgent
		/// </summary>
		public static string DefaultUserAgent = string.Format("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.39 Safari/537.36 iFish_Network_Client/{0};", System.Reflection.Assembly.GetExecutingAssembly().GetFileVersionInfo().FileVersion);

#if NET_GT_4
		/// <summary>
		/// 获得或设置是否将网络请求失败标记为任务失败。
		/// <para>此设置将会影响到<see cref="HttpContext.SendTask()"/>的执行行为。</para>
		/// <para>如果为true，那么不正确的响应或失败的响应会引发Task的失败，并抛出异常。</para>
		/// <para>如果设置为false，则任务始终正常完成。</para>
		/// </summary>
		public static bool TreatWebErrorAsTaskFail { get; set; }
#endif

		/// <summary>
		/// 获得或设置是否分析引用页地址
		/// </summary>
		public bool ResolveReferUri { get; set; } = true;

		/// <summary>
		/// 检查类库作者标记
		/// </summary>
		/// <param name="agent"></param>
		/// <returns></returns>
		static string CheckAuthorVendor(string agent)
		{
			if (string.IsNullOrEmpty(agent))
				return null;

			if (!AppendLibAuthorVendor) return agent;

			if (agent.IndexOf("iFish", StringComparison.OrdinalIgnoreCase) == -1)
			{
				var uaappend = "iFish Network Client/" + System.Reflection.Assembly.GetExecutingAssembly().GetFileVersionInfo().ToString();
				var m = Regex.Match(agent, @"^(.*?\(\s*(.*?)(\).*?))$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

				if (m.Success)
				{
					return m.GetGroupValue(1) + (m.GetGroupValue(2).Length > 0 ? ";" : "") + m.GetGroupValue(3);
				}
				else
				{
					return agent + "(" + uaappend + ")";
				}
			}

			return agent;
		}

		static IWebProxy _defaultProxy;

		string _userAgent;
		IWebProxy _proxy;
		int _speedMonitorInterval = 1000;
		private ICertificateManager _certificateManager;

		/// <summary>
		/// 获得或设置是否在请求中添加Ajax的标记
		/// </summary>
		public bool AppendAjaxHeader { get; set; }

		/// <summary>
		/// 获得或设置当前HTTP协议的接受编码类型
		/// </summary>
		public string AcceptEncoding { get; set; } = "gzip, deflate";

		/// <summary>
		/// 获得或设置最后响应的网址
		/// </summary>
		public Uri LastUri { get; set; }

		/// <summary>
		/// 获得或设置字符编码
		/// </summary>
		public Encoding StringEncoding { get; set; } = Encoding.UTF8;

		/// <summary>
		/// 获得或设置当前的授权
		/// </summary>
		public Authorization Authorization { get; set; }

		/// <summary>
		/// 获得或设置当前的授权管理
		/// </summary>
		public AuthorizationManager AuthorizationManager { get; set; }

		/// <summary>
		/// 获得或设置本地用于发送请求的IP地址
		/// </summary>
		public IPAddress LocalIpAddressIpV4 { get; set; }

		/// <summary>
		/// 获得或设置本地用于发送请求的IP地址
		/// </summary>
		public IPAddress LocalIpAddressIpV6 { get; set; }

		/// <summary>
		/// 自动移除响应头的BOM标记
		/// </summary>
		public bool RemoveStringBom { get; set; } = true;

		/// <summary>
		/// 获得或设置默认超时时间
		/// </summary>
		public int? Timeout { get; set; } = 30000;

		/// <summary>
		/// 获得或设置默认超时时间
		/// </summary>
		public int? ReadWriteTimeout { get; set; }

		/// <summary>
		/// 对应HTTP请求的请求标头
		/// </summary>
		public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();

		/// <summary>
		/// 获得或设置当前HTTP协议的保持连接设置
		/// </summary>
		public string Connection { get; set; } = "Close";

		/// <summary>
		/// 获得或设置当前HTTP协议的接受内容类型
		/// </summary>
		public string Accept { get; set; } = "*/*";

		/// <summary>
		/// 获得或设置当前HTTP协议的接受编码类型
		/// </summary>
		public string AcceptLanguage { get { return Headers[HttpRequestHeader.AcceptLanguage]; } set { Headers[HttpRequestHeader.AcceptLanguage] = value; } }

		/// <summary>
		/// 获得或设置当前HTTP协议的用户协议
		/// </summary>
		public string UserAgent
		{
			get { return _userAgent; }
			set { _userAgent = CheckAuthorVendor(value); }
		}

		/// <summary>
		/// 获得或设置当前正文的编码类型
		/// </summary>
		public string TransferEncoding { get; set; }

		/// <summary>
		/// 搜索Charset标记的默认最大区域（为了节约内存，默认1KB）
		/// </summary>
		public int DecodeForSearchCharsetRange { get; set; } = 0x400;

		/// <summary>
		/// 获得或设置是否保持活动
		/// </summary>
		public bool KeepAlive { get; set; } = false;

		/// <summary>
		/// 是否使用NagleAlgorithm
		/// </summary>
		public bool UseNagleAlgorithm { get; set; } = false;

		/// <summary>
		/// 是否缓冲写数据
		/// </summary>
		public bool AllowWriteStreamBuffering { get; set; } = true;

		/// <summary>
		/// 是否设置默认允许自动重定向
		/// </summary>
		public bool AllowAutoDirect { get; set; } = false;

		/// <summary>
		/// 用于计算下载速度时的定时器周期
		/// </summary>
		public int SpeedMonitorInterval
		{
			get { return _speedMonitorInterval; }
			set
			{
				if (value == _speedMonitorInterval)
					return;
				_speedMonitorInterval = value;
				OnPropertyChanged(nameof(SpeedMonitorInterval));
			}
		}

		/// <summary>
		/// 获得或设置默认的请求数据类型
		/// </summary>
		/// <value>The type of the content.</value>
		public ContentType DefaultRequestContentType { get; set; } = ContentType.FormUrlEncoded;

		static HttpSetting()
		{
#if NET_GT_4
			TreatWebErrorAsTaskFail = false;
#endif
			_defaultProxy = WebRequest.DefaultWebProxy;
			_defaultProxySet = false;
		}

		#endregion
	}
}
