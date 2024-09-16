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
        [Fact(Skip = "Skipping this test for now. Need to investigate and resolve issues.")]
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
                TimeLimit = System.TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Count);
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
