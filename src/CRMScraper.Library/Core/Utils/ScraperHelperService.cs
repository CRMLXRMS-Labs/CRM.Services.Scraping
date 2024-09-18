using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CRMScraper.Library.Core.Exceptions;
using CRMScraper.Library.Core.Entities;
using HtmlAgilityPack;

namespace CRMScraper.Library.Core.Utils
{
    public class ScraperHelperService : IScraperHelperService
    {
        // Retry mechanism with exponential backoff
        public async Task<ScrapedPageResult> ScrapeWithRetryAsync(string url, Func<Task<ScrapedPageResult>> scrapeFunction, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    return await scrapeFunction();
                }
                catch (HttpRequestException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new RetryLimitExceededException(url, retryCount);
                    }
                    var waitTime = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    await Task.Delay(waitTime);
                }
            }
            throw new RetryLimitExceededException(url, retryCount);
        }

        // Extract links from HTML content and ensure they are absolute URLs
        public List<string> ExtractLinks(string htmlContent, string baseUrl)
        {
            var links = new List<string>();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            var anchorTags = htmlDocument.DocumentNode.SelectNodes("//a[@href]");
            if (anchorTags != null)
            {
                foreach (var tag in anchorTags)
                {
                    var href = tag.GetAttributeValue("href", string.Empty);
                    if (!Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        // Convert relative URL to absolute
                        href = new Uri(new Uri(baseUrl), href).ToString();
                    }
                    if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        links.Add(href);
                    }
                }
            }

            return links;
        }
    }
}
