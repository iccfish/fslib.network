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
			Assert.IsTrue(result?.Contains("后花园") == true, "result?.Contains('后花园')==true");
		}
	}
}