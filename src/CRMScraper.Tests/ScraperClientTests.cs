using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using CRMScraper.Tests.FakeHttpMessageHandlers;

namespace CRMScraper.Tests
{
    public class ScraperClientTests
    {
        [Fact]
        public async Task ScrapePageAsync_ReturnsValidScrapedPageResult_SimpleMock()
        {
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><a href='https://example.com/page2'></a><script>console.log('test')</script></body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Contains("<html", result.HtmlContent); 
            Assert.Single(result.JavaScriptData); 
            Assert.Single(result.ApiRequests); 
        }

        [Fact]
        public async Task ScrapePageAsync_ReturnsMultipleLinks_SimpleMock()
        {
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><a href='https://example.com/page1'></a><a href='https://example.com/page2'></a></body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            Assert.Equal("https://example.com", result.Url);
            Assert.Equal(2, result.ApiRequests.Count); 
        }

        [Fact]
        public async Task ScrapePageAsync_HandlesNoLinksOrScripts()
        {
            // Arrange
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body>No links or scripts here</body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

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
            // Arrange
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("", System.Net.HttpStatusCode.NotFound));
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await scraperClient.ScrapePageAsync("https://example.com/invalid-page"));
        }

        [Fact]
        public async Task ScrapeDynamicPageAsync_ReturnsValidScrapedPageResult()
        {
            // Arrange
            var mockHttpClient = new HttpClient();
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act
            var result = await scraperClient.ScrapeDynamicPageAsync("https://www.example.com");

            // Assert
            Assert.Equal("https://www.example.com", result.Url);
            Assert.NotEmpty(result.ApiRequests);
        }

        [Fact]
        public async Task ScrapePageAsync_HandlesFormsAndExtractsApiRequests()
        {
            // Arrange: Mock a page with forms
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><form action='/submit'></form><a href='https://example.com/page'></a></body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Equal(2, result.ApiRequests.Count); 
            Assert.Contains("https://example.com/submit", result.ApiRequests); 
        }
    }
}
