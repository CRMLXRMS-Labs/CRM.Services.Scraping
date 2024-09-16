using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using CRMScraper.Library.Core.Utils;
using Xunit;

namespace CRMScraper.Tests
{
    public class PageElementsExtractorTests
    {
        private readonly PageElementsExtractor _pageElementsExtractor;

        public PageElementsExtractorTests()
        {
            _pageElementsExtractor = new PageElementsExtractor();
        }

        [Fact]
        public void ExtractJavaScript_ReturnsJavaScriptContent()
        {
            // Arrange
            var htmlContent = "<html><head><script>console.log('test1');</script><script>console.log('test2');</script></head><body></body></html>";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Act
            var result = _pageElementsExtractor.ExtractJavaScript(htmlDocument);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("console.log('test1');", result);
            Assert.Contains("console.log('test2');", result);
        }

        [Fact]
        public void ExtractJavaScript_ReturnsEmptyList_WhenNoScriptTagsFound()
        {
            // Arrange
            var htmlContent = "<html><head></head><body></body></html>";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Act
            var result = _pageElementsExtractor.ExtractJavaScript(htmlDocument);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ExtractApiRequests_ReturnsValidLinksAndFormActions()
        {
            // Arrange
            var htmlContent = @"
                <html>
                    <body>
                        <a href='/page1'></a>
                        <a href='https://example.com/page2'></a>
                        <form action='/submit'></form>
                    </body>
                </html>";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            var baseUrl = "https://example.com";

            // Act
            var result = _pageElementsExtractor.ExtractApiRequests(htmlDocument, baseUrl);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains("https://example.com/page1", result);
            Assert.Contains("https://example.com/page2", result);
            Assert.Contains("https://example.com/submit", result);
        }

        [Fact]
        public void ExtractApiRequests_ReturnsEmptyList_WhenNoLinksOrFormsFound()
        {
            // Arrange
            var htmlContent = "<html><body>No links or forms here</body></html>";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            var baseUrl = "https://example.com";

            // Act
            var result = _pageElementsExtractor.ExtractApiRequests(htmlDocument, baseUrl);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ExtractApiRequests_ProcessesRelativeUrlsCorrectly()
        {
            // Arrange
            var htmlContent = @"
                <html>
                    <body>
                        <a href=''></a>
                        <a href='invalid-url'></a>
                    </body>
                </html>";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);
            var baseUrl = "https://example.com";

            // Act
            var result = _pageElementsExtractor.ExtractApiRequests(htmlDocument, baseUrl);

            // Assert
            Assert.Contains("https://example.com/invalid-url", result);
            Assert.Single(result);
        }

    }
}
