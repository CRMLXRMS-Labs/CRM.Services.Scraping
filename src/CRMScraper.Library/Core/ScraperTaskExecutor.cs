using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CRMScraper.Library.Core
{
    public class ScraperTaskExecutor : IScraperTaskExecutor
    {
        private readonly IScraperClient _scraperClient;

        public ScraperTaskExecutor(IScraperClient scraperClient)
        {
            _scraperClient = scraperClient;
        }

        public async Task<List<ScrapedPageResult>> ExecuteScrapingTaskAsync(ScrapingTask task, CancellationToken cancellationToken)
        {
            var results = new List<ScrapedPageResult>();
            var pagesToScrape = new Queue<string>();
            pagesToScrape.Enqueue(task.TargetUrl);

            var startTime = DateTime.UtcNow;

            while (results.Count < task.MaxPages && pagesToScrape.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var url = pagesToScrape.Dequeue();

                try
                {
                    var pageResult = await _scraperClient.ScrapePageAsync(url);
                    results.Add(pageResult);

                    var newLinks = ExtractLinks(pageResult.HtmlContent);
                    foreach (var link in newLinks)
                    {
                        if (results.Count + pagesToScrape.Count < task.MaxPages)
                        {
                            pagesToScrape.Enqueue(link);
                        }
                    }

                    if (DateTime.UtcNow - startTime > task.TimeLimit)
                    {
                        Console.WriteLine("Task time limit exceeded.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to scrape {url}: {ex.Message}");
                }
            }

            return results;
        }

        private List<string> ExtractLinks(string htmlContent)
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
                    if (Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
                    {
                        links.Add(href);
                    }
                }
            }

            return links;
        }
    }
}
