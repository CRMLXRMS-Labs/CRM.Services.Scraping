using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Exceptions;
using CRMScraper.Library.Core.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CRMScraper.Tests
{
    public class ScraperHelperServiceTests
    {
        private readonly ScraperHelperService _scraperHelperService;

        public ScraperHelperServiceTests()
        {
            _scraperHelperService = new ScraperHelperService();
        }

        [Fact]
        public async Task ScrapeWithRetryAsync_SuccessOnFirstTry()
        {
            // Arrange
            var mockScrapeFunction = new Mock<Func<Task<ScrapedPageResult>>>();
            var expectedResult = new ScrapedPageResult { Url = "https://example.com" };

            mockScrapeFunction.Setup(f => f()).ReturnsAsync(expectedResult);

            // Act
            var result = await _scraperHelperService.ScrapeWithRetryAsync("https://example.com", mockScrapeFunction.Object);

            // Assert
            Assert.Equal("https://example.com", result.Url);
            mockScrapeFunction.Verify(f => f(), Times.Once);
        }

        [Fact]
        public async Task ScrapeWithRetryAsync_RetriesOnFailure()
        {
            // Arrange
            var mockScrapeFunction = new Mock<Func<Task<ScrapedPageResult>>>();
            var expectedResult = new ScrapedPageResult { Url = "https://example.com" };

            mockScrapeFunction.SetupSequence(f => f())
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _scraperHelperService.ScrapeWithRetryAsync("https://example.com", mockScrapeFunction.Object, maxRetries: 3);

            // Assert
            Assert.Equal("https://example.com", result.Url);
            mockScrapeFunction.Verify(f => f(), Times.Exactly(3)); 
        }

        [Fact]
        public async Task ScrapeWithRetryAsync_ThrowsAfterMaxRetries()
        {
            // Arrange
            var mockScraperClient = new Mock<IScraperClient>();
            var scraperHelperService = new ScraperHelperService();

            // Simulate retry failure scenario
            int retryCount = 0;
            Func<Task<ScrapedPageResult>> scrapeFunction = () =>
            {
                retryCount++;
                throw new HttpRequestException("Failed to scrape");
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<RetryLimitExceededException>(() =>
                scraperHelperService.ScrapeWithRetryAsync("https://example.com", scrapeFunction, 3));

            Assert.Equal(3, retryCount); // Ensure retries were attempted
        }



        [Fact]
        public void ExtractLinks_ReturnsValidAbsoluteLinks()
        {
            // Arrange
            string htmlContent = "<html><body><a href='/page1'></a><a href='https://example.com/page2'></a></body></html>";
            string baseUrl = "https://example.com";

            // Act
            var links = _scraperHelperService.ExtractLinks(htmlContent, baseUrl);

            // Assert
            Assert.Equal(2, links.Count);
            Assert.Contains("https://example.com/page1", links);
            Assert.Contains("https://example.com/page2", links);
        }

        [Fact]
        public void ExtractLinks_ReturnsEmptyList_WhenNoLinksFound()
        {
            // Arrange
            string htmlContent = "<html><body>No links here</body></html>";
            string baseUrl = "https://example.com";

            // Act
            var links = _scraperHelperService.ExtractLinks(htmlContent, baseUrl);

            // Assert
            Assert.Empty(links); 
        }
    }

}
