using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSLib.Network.Tests.NetCore
{
	using System.Threading.Tasks;

	using Http;

	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async Task TestHttpGetStringAsync()
		{
			var client = new HttpClient();
			var context = client.GetString("https://www.fishlee.net/");

			var result = await context.SendAsync();
			Assert.IsNotNull(result);
			Assert.IsTrue(result?.Contains("��԰") == true, "result?.Contains('��԰')==true");
		}

		[TestMethod]
		public async Task TestPostJson()
		{
			var client = new HttpClient();
			var context = client.PostJson("https://www.fishlee.net/", new { }, "");

			var result = await context.SendAsync();

			Assert.IsTrue(context.RequestContent is RequestJsonContent, "Wrap type error.");
		}
	}
}