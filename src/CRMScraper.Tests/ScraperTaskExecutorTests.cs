using Moq;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System;

namespace CRMScraper.Tests
{
    public class ScraperTaskExecutorTests
    {
        [Fact]
        public async Task ExecuteScrapingTaskAsync_LimitsResultsByMaxPages_Forbes()
        {
            // Arrange: Scraping Forbes homepage and limited to 2 pages
            var mockScraperClient = new Mock<IScraperClient>();

            mockScraperClient.SetupSequence(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://www.forbes.com",
                    HtmlContent = "<html><body><a href='https://www.forbes.com/page2'>Link</a></body></html>", 
                    JavaScriptData = new List<string> { "console.log('test');" },
                    ApiRequests = new List<string> { "https://www.forbes.com/page2" }
                })
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://www.forbes.com/page2",
                    HtmlContent = "<html><body>No more links</body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string>()
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://www.forbes.com",
                MaxPages = 2,  // Limit to two pages
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count); // Ensure that two pages were scraped
            Assert.Equal("https://www.forbes.com", result[0].Url); // First page
            Assert.Equal("https://www.forbes.com/page2", result[1].Url); // Second page
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_StopsWhenTimeLimitExceeded_Forbes()
        {
            // Arrange: Limit the time available for scraping
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ReturnsAsync(new ScrapedPageResult
                {
                    Url = "https://www.forbes.com",
                    HtmlContent = "<html><body>No more links</body></html>",
                    JavaScriptData = new List<string>(),
                    ApiRequests = new List<string>()
                });

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://www.forbes.com",
                MaxPages = 10,
                TimeLimit = TimeSpan.FromMilliseconds(200) // Short time limit to trigger timeout
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.True(result.Count < 10); // Ensure it stops before scraping 10 pages
        }

        [Fact]
        public async Task ExecuteScrapingTaskAsync_HandlesScrapingFailures_Forbes()
        {
            // Arrange: Simulate scraping failure
            var mockScraperClient = new Mock<IScraperClient>();
            mockScraperClient.Setup(s => s.ScrapePageAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Failed to fetch page"));

            var scraperTask = new ScrapingTask
            {
                TargetUrl = "https://www.forbes.com",
                MaxPages = 1,
                TimeLimit = TimeSpan.FromMinutes(1)
            };

            var scraperExecutor = new ScraperTaskExecutor(mockScraperClient.Object);

            // Act
            var result = await scraperExecutor.ExecuteScrapingTaskAsync(scraperTask, CancellationToken.None);

            // Assert
            Assert.Empty(result); // No pages should be scraped due to exception
        }
    }
}
