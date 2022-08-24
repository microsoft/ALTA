using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ALTA.SampleTest
{
    [TestClass]
    public class SampleTest 
    { 
        private static HttpClient client;
        private static IHttpClientFactory _httpClientFactory;
        private static ServiceProvider serviceProvider;
        private static ILogger logger;

        [ClassInitialize]
        public static void TestClassInitialize(TestContext context)
        {
            serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            _httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            client = _httpClientFactory.CreateClient();
            logger = new LoggerFactory().CreateLogger("test");

        }

        [TestMethod]
        public async Task GetBingHomePage()
        {
            HttpResponseMessage result = null;
            try
            {
                result = await client.GetAsync("https://www.bing.com/");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "test failed");
                Assert.IsTrue(false);
            }
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.IsTrue(result.Content.Headers.Any());

        }
    }
}
