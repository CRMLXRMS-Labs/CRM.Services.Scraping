using System.Collections.Concurrent;
using HtmlAgilityPack;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Exceptions;
using CRMScraper.Library.Core.Utils;
using System.Net.Http;

namespace CRMScraper.Library.Core
{
    public class ScraperTaskExecutor : IScraperTaskExecutor
    {
        private readonly IScraperClient _scraperClient;
        private readonly IScraperHelperService _scraperHelperService;

        public ScraperTaskExecutor(IScraperClient scraperClient, IScraperHelperService scraperHelperService)
        {
            _scraperClient = scraperClient ?? throw new ArgumentNullException(nameof(scraperClient));
            _scraperHelperService = scraperHelperService ?? throw new ArgumentNullException(nameof(scraperHelperService));
        }

        public async Task<List<ScrapedPageResult>> ExecuteScrapingTaskAsync(ScrapingTask task, CancellationToken cancellationToken)
        {
            var results = new ConcurrentBag<ScrapedPageResult>();
            var pagesToScrape = new ConcurrentQueue<string>();
            var visitedUrls = new ConcurrentDictionary<string, bool>();

            if (string.IsNullOrWhiteSpace(task.TargetUrl))
            {
                throw new InvalidUrlException(task.TargetUrl);
            }

            pagesToScrape.Enqueue(task.TargetUrl);
            visitedUrls[task.TargetUrl] = true;

            var startTime = DateTime.UtcNow;
            var tasks = new List<Task>();

            using (var semaphore = new SemaphoreSlim(task.MaxConcurrentPages))
            {
                while (!pagesToScrape.IsEmpty && results.Count < task.MaxPages && !cancellationToken.IsCancellationRequested)
                {
                    if (pagesToScrape.TryDequeue(out var url))
                    {
                        if (string.IsNullOrWhiteSpace(url))
                        {
                            throw new InvalidUrlException(url);
                        }

                        await semaphore.WaitAsync(cancellationToken);

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                ScrapedPageResult pageResult = null;
                                if (task.UseDynamicScraping)
                                {
                                    pageResult = await _scraperClient.ScrapeDynamicPageAsync(url);
                                }
                                else
                                {
                                    pageResult = await _scraperClient.ScrapePageAsync(url);
                                }

                                if (pageResult == null || string.IsNullOrWhiteSpace(pageResult.HtmlContent))
                                {
                                    return;
                                }

                                results.Add(pageResult);

                                var newLinks = _scraperHelperService.ExtractLinks(pageResult.HtmlContent, url) ?? new List<string>();
                                foreach (var link in newLinks)
                                {
                                    if (results.Count >= task.MaxPages) break;

                                    if (visitedUrls.TryAdd(link, true))
                                    {
                                        pagesToScrape.Enqueue(link);
                                    }
                                }

                                if (DateTime.UtcNow - startTime > task.TimeLimit)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                            }
                            catch (RetryLimitExceededException)
                            {
                                throw; 
                            }
                            catch (Exception ex)
                            {
                                throw new ScrapingFailedException(url, ex);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, cancellationToken));
                    }

                    if (DateTime.UtcNow - startTime > task.TimeLimit)
                    {
                        break;
                    }
                }

                await Task.WhenAll(tasks);
            }

            return results.ToList();
        }
    }

}

