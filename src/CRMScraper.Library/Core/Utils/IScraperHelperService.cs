using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CRMScraper.Library.Core.Utils
{
    public interface IScraperHelperService
    {
        Task<ScrapedPageResult> ScrapeWithRetryAsync(string url, Func<Task<ScrapedPageResult>> scrapeFunction, int maxRetries = 3);
        List<string> ExtractLinks(string htmlContent, string baseUrl);
    }
}