using Moq;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Exceptions;
using CRMScraper.Library.Core.Utils;
using System.Net.Http;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace CRMScraper.Tests
{
    public class ScraperTaskExecutorTests
    {
        [Fact]
        public async Task ExecuteScrapingTaskAsync_ThrowsInvalidUrlException_WhenUrlIsEmpty()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();
            var scraperTask = new ScrapingTask
            {
                TargetUrl = "", // Invalid URL
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidUrlException>(() => scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_DoesNotThrowRetryLimitExceededException_WhenRetriesAreSuccessful()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();

            // Simulate retry: fail twice, then succeed on the third attempt
            mockHelperService.SetupSequence(s => s.ScrapeWithRetryAsync(It.IsAny<string>(), It.IsAny<Func<Task<ScrapedPageResult>>>(), It.IsAny<int>()))
                .ThrowsAsync(new HttpRequestException("Simulated network failure"))  // First attempt fails
                .ThrowsAsync(new HttpRequestException("Simulated network failure"))  // Second attempt fails
                .ReturnsAsync(new ScrapedPageResult  // Third attempt succeeds
                {
                    Url = "https://example.com",
                    HtmlContent = "<html><body>Successful page scrape</body></html>"
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Empty(result);  // Ensure one page was successfully scraped
            mockHelperService.Verify(s => s.ScrapeWithRetryAsync(It.IsAny<string>(), It.IsAny<Func<Task<ScrapedPageResult>>>(), It.IsAny<int>()), Times.Exactly(0));
        }




        [Fact]
        public async Task ExecuteScrapingTaskAsync_LimitsResultsByMaxPages_SimpleMock()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();

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

            mockHelperService.Setup(s => s.ExtractLinks(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<string> { "https://example.com/page2" });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 2,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Count); // Two distinct pages should be scraped
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_StopsWhenTimeLimitExceeded()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();

            // Setup the ScrapeWithRetryAsync to return a page result
            mockHelperService.Setup(s => s.ScrapeWithRetryAsync(It.IsAny<string>(), It.IsAny<Func<Task<ScrapedPageResult>>>(), It.IsAny<int>()))
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
                TimeLimit = TimeSpan.FromMilliseconds(50) // Shorter time limit for testing
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.True(result.Count < 10, "Time limit should prevent scraping all pages.");
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_HandlesScrapingFailures()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();

            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Failed to fetch page"));

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://example.com",
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ScrapingFailedException>(() => scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None));
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_ScrapesDynamicPages()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var mockHelperService = new Mock<IScraperHelperService>();

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

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object, mockHelperService.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            mockScraperClient.Verify(s => s.ScrapeDynamicPageAsync(It.IsAny<string>()), Times.AtLeastOnce);
            Assert.Single(result);
        }
    }
}
