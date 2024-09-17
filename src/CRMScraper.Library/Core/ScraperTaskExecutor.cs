using System.Collections.Concurrent;
using HtmlAgilityPack;
using CRMScraper.Library.Core.Entities;

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
            var results = new ConcurrentBag<ScrapedPageResult>();
            var pagesToScrape = new ConcurrentQueue<string>();
            var visitedUrls = new ConcurrentDictionary<string, bool>();

            // Enqueue the initial target URL
            pagesToScrape.Enqueue(task.TargetUrl);
            visitedUrls[task.TargetUrl] = true;

            var startTime = DateTime.UtcNow;
            var tasks = new List<Task>();

            using (var semaphore = new SemaphoreSlim(task.MaxConcurrentPages))
            {
                // Continue scraping while pages are available, max pages haven't been reached, and cancellation is not requested
                while (!pagesToScrape.IsEmpty && results.Count < task.MaxPages && !cancellationToken.IsCancellationRequested)
                {
                    if (pagesToScrape.TryDequeue(out var url))
                    {
                        await semaphore.WaitAsync();

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // Scrape the current URL with retries if needed
                                var pageResult = await ScrapeWithRetryAsync(url, () =>
                                    task.UseDynamicScraping
                                        ? _scraperClient.ScrapeDynamicPageAsync(url)
                                        : _scraperClient.ScrapePageAsync(url));

                                results.Add(pageResult);
                                Console.WriteLine($"Scraped: {url}");

                                // Extract new links from the scraped page
                                var newLinks = ExtractLinks(pageResult.HtmlContent, url);

                                foreach (var link in newLinks)
                                {
                                    // Stop if max pages are already reached
                                    if (results.Count >= task.MaxPages) break;

                                    // Only enqueue new links that haven't been visited yet
                                    if (visitedUrls.TryAdd(link, true))
                                    {
                                        Console.WriteLine($"Enqueueing: {link}");
                                        pagesToScrape.Enqueue(link);
                                    }
                                }

                                // Check time limit and stop if exceeded
                                if (DateTime.UtcNow - startTime > task.TimeLimit)
                                {
                                    Console.WriteLine("Task time limit exceeded.");
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to scrape {url}: {ex.Message}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, cancellationToken));
                    }

                    // Stop if time limit is reached
                    if (DateTime.UtcNow - startTime > task.TimeLimit)
                    {
                        Console.WriteLine("Task time limit exceeded.");
                        break;
                    }
                }

                // Wait for all scraping tasks to complete
                await Task.WhenAll(tasks);
            }

            return results.ToList();
        }

        // Retry mechanism with exponential backoff
        private async Task<ScrapedPageResult> ScrapeWithRetryAsync(string url, Func<Task<ScrapedPageResult>> scrapeFunction, int maxRetries = 3)
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
        private List<string> ExtractLinks(string htmlContent, string baseUrl)
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
