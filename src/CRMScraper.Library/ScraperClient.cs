using HtmlAgilityPack;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CRMScraper.Library.Core;

namespace CRMScraper.Library
{
    public class ScraperClient : IScraperClient
    {
        private readonly HttpClient _httpClient;

        public ScraperClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ScrapedPageResult> ScrapePageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var javascriptData = ExtractJavaScript(htmlDocument);
            var apiRequests = ExtractApiRequests(htmlDocument, url);

            return new ScrapedPageResult
            {
                Url = url,
                HtmlContent = content,
                JavaScriptData = javascriptData,
                ApiRequests = apiRequests
            };
        }

        // Use Playwright for JavaScript-heavy or SPA sites
        public async Task<ScrapedPageResult> ScrapeDynamicPageAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            // Navigate to the page
            await page.GotoAsync(url);

            // Wait for the page to finish loading (you can add specific waits for dynamic content)
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Get fully-rendered HTML content
            var content = await page.ContentAsync();

            // Extract dynamic links
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

        private List<string> ExtractJavaScript(HtmlDocument document)
        {
            var scripts = document.DocumentNode.SelectNodes("//script");
            return scripts?.Select(script => HttpUtility.HtmlDecode(script.InnerText)).ToList() ?? new List<string>();
        }

        private List<string> ExtractApiRequests(HtmlDocument document, string baseUrl)
        {
            var apiRequests = new List<string>();

            var links = document.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                    {
                        href = new Uri(new Uri(baseUrl), href).ToString();
                    }
                    if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        apiRequests.Add(href);
                    }
                }
            }

            var forms = document.DocumentNode.SelectNodes("//form[@action]");
            if (forms != null)
            {
                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", string.Empty);
                    if (Uri.IsWellFormedUriString(action, UriKind.Relative))
                    {
                        action = new Uri(new Uri(baseUrl), action).ToString();
                    }
                    if (Uri.IsWellFormedUriString(action, UriKind.Absolute))
                    {
                        apiRequests.Add(action);
                    }
                }
            }

            return apiRequests;
        }
    }
}
