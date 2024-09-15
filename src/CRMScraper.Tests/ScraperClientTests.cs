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
        public async Task ScrapePageAsync_ReturnsValidScrapedPageResult_Forbes()
        {
            // Arrange: Use a real client to scrape Forbes homepage
            var httpClient = new HttpClient();
            var scraperClient = new ScraperClient(httpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://www.forbes.com/");

            // Assert
            Assert.Equal("https://www.forbes.com/", result.Url);
            Assert.Contains("<html", result.HtmlContent); // Ensure HTML content is returned
            Assert.True(result.JavaScriptData.Count > 0); // Ensure JavaScript data is present
            Assert.True(result.ApiRequests.Count > 0); // Ensure API requests (or links) are found
        }

        [Fact]
        public async Task ScrapePageAsync_ReturnsMultipleLinks_Forbes()
        {
            // Arrange: Use the real Forbes homepage
            var httpClient = new HttpClient();
            var scraperClient = new ScraperClient(httpClient);

            // Act
            var result = await scraperClient.ScrapePageAsync("https://www.forbes.com/");

            // Assert
            Assert.Equal("https://www.forbes.com/", result.Url);
            Assert.True(result.ApiRequests.Count > 0, "Should find multiple links on the page");

            // Check if at least one internal Forbes link is found
            Assert.Contains(result.ApiRequests, link => link.StartsWith("https://www.forbes.com/"));
        }


       [Fact]
        public async Task ScrapePageAsync_HandlesSoft404_ForbesInvalidPage()
        {
            // Arrange: Use a real client and the URL for a non-existent page on Forbes
            var httpClient = new HttpClient();
            var scraperClient = new ScraperClient(httpClient);

            // Act: Scrape the invalid page
            var result = await scraperClient.ScrapePageAsync("https://www.forbes.com/invalid-page");

            // Assert: Ensure that the result contains the "not found" message from the page content
            Assert.Contains(" find the page ", result.HtmlContent);
        }

    }
}
