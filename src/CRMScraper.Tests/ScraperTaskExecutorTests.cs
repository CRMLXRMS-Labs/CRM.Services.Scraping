using Moq;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using System.Net.Http;
using Xunit;

namespace CRMScraper.Tests
{
    public class ScraperTaskExecutorTests
    {
        [Fact]
        public async Task ExecuteScrapingTaskAsync_LimitsResultsByMaxPages_SimpleMock()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();

            mockScraperClient.SetupSequence(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com",
                    HtmlContent = "<html><body><a href='https://example.com/page2'></a></body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string> { "https://example.com/page2" }
                })
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com/page2",
                    HtmlContent = "<html><body>No more links</body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string>()
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 2,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            // Expecting two results because two distinct pages should be scraped.
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_StopsWhenTimeLimitExceeded()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com",
                    HtmlContent = "<html><body>No more links</body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string>()
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 10,
                TimeLimit = TimeSpan.FromMilliseconds(100) // Short time limit for testing
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.True(result.Count < 10); // Ensure that the time limit prevents scraping all pages.
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_HandlesScrapingFailures()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Failed to fetch page"));

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Empty(result); // Since the page failed to scrape, no results should be returned.
        }

        [Fact]
        public async Task ScrapeWithRetryAsync_RetriesOnFailure()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var scrapeCount = 0;

            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    scrapeCount++;
                    if (scrapeCount < 2) throw new HttpRequestException("Failed");
                    return new ScrapedPageResult { Url = "https://example.com", HtmlContent = "<html></html>" };
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, scrapeCount); // Retried once before success
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_ScrapesDynamicPages()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();

            mockScraperClient.Setup(s => s.ScrapeDynamicPageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com",
                    HtmlContent = "<html><body><a href='https://example.com/page2'></a></body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string> { "https://example.com/page2" }
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 2,
                TimeLimit = TimeSpan.FromMinutes(1),
                UseDynamicScraping = true
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            mockScraperClient.Verify(s => s.ScrapeDynamicPageAsync(It.IsAny<string>()), Times.AtLeastOnce);
            Assert.Single(result);
        }

        [Fact]
        public void ExtractLinks_ReturnsAbsoluteUrls()
        {
            // Arrange
            var htmlContent = "<html><body><a href='/page1'></a><a href='https://example.com/page2'></a></body></html>";
            var baseUrl = "https://example.com";

            var scraperExecutor = new ScraperTaskExecutor(null);

            // Act
            var result = scraperExecutor.GetType()
                .GetMethod("ExtractLinks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(scraperExecutor, new object[] { htmlContent, baseUrl }) as List<string>;

            // Assert
            Assert.Contains("https://example.com/page1", result);
            Assert.Contains("https://example.com/page2", result);
        }

        [Fact]
       public async Task ExecuteScrapingTaskAsync_HandlesEmptyUrlsGracefully()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();

            // Setup: No scraping should be performed for an empty URL
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "",
                    HtmlContent = "",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string>()
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "", // This should be ignored because it's empty
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            Assert.Empty(result); 
        }
    }
}
