using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
                catch (HttpRequestException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine($"Giving up after {maxRetries} attempts to scrape {url}: {ex.Message}");
                        throw;
                    }
                    var waitTime = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                    Console.WriteLine($"Retrying {url} in {waitTime.TotalSeconds} seconds due to error: {ex.Message}");
                    await Task.Delay(waitTime);
                }
            }
            throw new HttpRequestException($"Failed to scrape {url} after {maxRetries} retries.");
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
