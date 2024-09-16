using Moq;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

            // Mocking the scraping of two pages
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

            // Creating a task that allows scraping a maximum of 2 pages
            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 2,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act: Execute the scraping task
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert: Since MaxPages is 2, the result should contain 2 pages
            Assert.Equal(2, result.Count); // Update this to expect 2 pages
            Assert.Equal("https://example.com", result[0].Url);
            Assert.Equal("https://example.com/page2", result[1].Url);
        }


        [Fact]
        public async Task ExecuteScrapingTaskAsync_StopsWhenTimeLimitExceeded_SimpleMock()
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
                TimeLimit = System.TimeSpan.FromMilliseconds(100) 
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.True(result.Count < 10);
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_HandlesScrapingFailures_SimpleMock()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Failed to fetch page"));

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 1,
                TimeLimit = System.TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
