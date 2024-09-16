using CRMScraper.Library;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Utils;
using CRMScraper.Tests.FakeHttpMessageHandlers;
using HtmlAgilityPack;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CRMScraper.Tests
{
    public class ScraperClientTests
    {
        [Fact]
        public async Task ScrapePageAsync_ReturnsValidScrapedPageResult_SimpleMock()
        {
            // Arrange: Mock the IPageElementsExtractor to simulate JavaScript and API extraction
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><a href='https://example.com/page2'></a><script>console.log('test')</script></body></html>"));
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>();
            
            // Simulate extracted JavaScript and API requests
            mockPageElementsExtractor.Setup(x => x.ExtractJavaScript(It.IsAny<HtmlDocument>()))
                .Returns(new List<string> { "console.log('test')" });
            mockPageElementsExtractor.Setup(x => x.ExtractApiRequests(It.IsAny<HtmlDocument>(), It.IsAny<string>()))
                .Returns(new List<string> { "https://example.com/page2" });

            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Single(result.JavaScriptData);
            Assert.Single(result.ApiRequests);
        }

        [Fact]
        public async Task ScrapePageAsync_ReturnsMultipleLinks_SimpleMock()
        {
            // Arrange: Mock the IPageElementsExtractor for multiple links
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><a href='https://example.com/page1'></a><a href='https://example.com/page2'></a></body></html>"));
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>();

            mockPageElementsExtractor.Setup(x => x.ExtractApiRequests(It.IsAny<HtmlDocument>(), It.IsAny<string>()))
                .Returns(new List<string> { "https://example.com/page1", "https://example.com/page2" });

            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Equal(2, result.ApiRequests.Count);
        }

        [Fact]
        public async Task ScrapePageAsync_HandlesNoLinksOrScripts()
        {
            // Arrange: Mock a page with no links or scripts
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body>No links or scripts here</body></html>"));
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>();

            // Simulate no JavaScript and no API requests
            mockPageElementsExtractor.Setup(x => x.ExtractJavaScript(It.IsAny<HtmlDocument>()))
                .Returns(new List<string>());
            mockPageElementsExtractor.Setup(x => x.ExtractApiRequests(It.IsAny<HtmlDocument>(), It.IsAny<string>()))
                .Returns(new List<string>());

            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Empty(result.ApiRequests);
            Assert.Empty(result.JavaScriptData);
        }

        [Fact]
        public async Task ScrapePageAsync_HandlesHttpRequestException_OnInvalidUrl()
        {
            // Arrange: Simulate an invalid request
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("", System.Net.HttpStatusCode.NotFound));
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>(); // Mock the extractor

            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await scraperClient.ScrapePageAsync("https://example.com/invalid-page"));
        }

        [Fact]
        public async Task ScrapeDynamicPageAsync_ReturnsValidScrapedPageResult()
        {
            // Arrange: Simulate dynamic page scraping with Playwright
            var mockHttpClient = new HttpClient();
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>(); // Playwright doesn't use this
            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act
            var result = await scraperClient.ScrapeDynamicPageAsync("https://www.example.com");

            // Assert
            Assert.Equal("https://www.example.com", result.Url);
            Assert.NotEmpty(result.ApiRequests);
        }

        [Fact]
        public async Task ScrapePageAsync_HandlesFormsAndExtractsApiRequests()
        {
            // Arrange: Mock a page with forms and links
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><form action='/submit'></form><a href='https://example.com/page'></a></body></html>"));
            var mockPageElementsExtractor = new Mock<IPageElementsExtractor>();

            // Simulate extracted form action and API requests
            mockPageElementsExtractor.Setup(x => x.ExtractApiRequests(It.IsAny<HtmlDocument>(), It.IsAny<string>()))
                .Returns(new List<string> { "https://example.com/submit", "https://example.com/page" });

            var scraperClient = new ScraperClient(mockHttpClient, mockPageElementsExtractor.Object);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Equal(2, result.ApiRequests.Count);
            Assert.Contains("https://example.com/submit", result.ApiRequests);
        }
    }
}
