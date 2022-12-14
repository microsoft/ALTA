namespace Microsoft.ALTA.SampleTests
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    [TestClass]
    public class SampleTest
    {
        private static HttpClient? client;
        private static IHttpClientFactory? httpClientFactory;
        private static ServiceProvider? serviceProvider;
        private static ILogger? logger;

        [ClassInitialize]
        public static void TestClassInitialize(TestContext context)
        {
            serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            client = httpClientFactory.CreateClient();
            logger = new LoggerFactory().CreateLogger("test");
        }

        [TestMethod]
        public async Task GetHomePage()
        {
            HttpResponseMessage? result = null;
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