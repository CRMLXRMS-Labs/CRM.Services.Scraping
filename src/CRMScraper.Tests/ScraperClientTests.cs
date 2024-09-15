using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CRMScraper.Tests
{
    public class ScraperClientTests
    {
        [Fact]
        public async Task ScrapePageAsync_ReturnsValidScrapedPageResult()
        {
            // Arrange
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body><a href='https://example.com/page2'></a><script>console.log('test')</script></body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://example.com");

            // Assert
            Assert.Equal("https://example.com", result.Url);
            Assert.Contains("<html><body><a href='https://example.com/page2'>", result.HtmlContent);
            Assert.Single(result.JavaScriptData);
            Assert.Single(result.ApiRequests);
        }

        [Fact]
        public async Task ScrapePageAsync_ReturnsEmptyWhenNoJavaScriptOrApiRequests()
        {
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("<html><body>No JavaScript or API requests here</body></html>"));
            var scraperClient = new ScraperClient(mockHttpClient);

            var result = await scraperClient.ScrapePageAsync("https://example.com");

            Assert.Equal("https://example.com", result.Url);
            Assert.Empty(result.JavaScriptData);  
            Assert.Empty(result.ApiRequests);     
        }

        [Fact]
        public async Task ScrapePageAsync_ThrowsHttpRequestExceptionOnFailure()
        {
            var mockHttpClient = new HttpClient(new FakeHttpMessageHandler("", HttpStatusCode.BadRequest));
            var scraperClient = new ScraperClient(mockHttpClient);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await scraperClient.ScrapePageAsync("https://example.com"));
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };
            return Task.FromResult(response);
        }
    }
}
