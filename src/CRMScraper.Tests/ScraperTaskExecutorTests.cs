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
        public async Task ExecuteScrapingTaskAsync_LimitsResultsByMaxPages()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.SetupSequence(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com",
                    HtmlContent = "<html><body><a href='https://example.com/page2'>Link</a></body></html>"
                })
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://example.com/page2",
                    HtmlContent = "<html><body>No more links</body></html>"
                });

            var scraperTask = new ScrapingTask("https://example.com", 2, TimeSpan.FromMinutes(1));
            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
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
                    HtmlContent = "<html><body>No more links</body></html>"
                });

            var scraperTask = new ScrapingTask("https://example.com", 10, TimeSpan.FromMilliseconds(100));
            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.True(result.Count < 10); // Should stop before reaching 10 pages
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_HandlesScrapingFailures()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Failed to fetch page"));

            var scraperTask = new ScrapingTask("https://example.com", 1, TimeSpan.FromMinutes(1));
            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Empty(result); // No pages should be returned due to the exception
        }
    }
}
