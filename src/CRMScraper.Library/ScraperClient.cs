using HtmlAgilityPack;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Utils;

namespace CRMScraper.Library
{
    public class ScraperClient : IScraperClient
    {
        private readonly HttpClient _httpClient;
        private readonly IPageElementsExtractor _pageElementsExtractor;

        public ScraperClient(HttpClient httpClient, IPageElementsExtractor pageElementsExtractor)
        {
            _httpClient = httpClient;
            _pageElementsExtractor = pageElementsExtractor;
        }

        public async Task<ScrapedPageResult> ScrapePageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var javascriptData = _pageElementsExtractor.ExtractJavaScript(htmlDocument);
            var apiRequests = _pageElementsExtractor.ExtractApiRequests(htmlDocument, url);

            return new ScrapedPageResult
            {
                Url = url,
                HtmlContent = content,
                JavaScriptData = javascriptData,
                ApiRequests = apiRequests
            };
        }

        public async Task<ScrapedPageResult> ScrapeDynamicPageAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync(url);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();
            var links = await page.EvaluateAsync<string[]>("Array.from(document.querySelectorAll('a')).map(a => a.href)");

            await browser.CloseAsync();

            return new ScrapedPageResult
            {
                Url = url,
                HtmlContent = content,
                ApiRequests = new List<string>(links),
                JavaScriptData = new List<string>() 
            };
        }
    }
}
