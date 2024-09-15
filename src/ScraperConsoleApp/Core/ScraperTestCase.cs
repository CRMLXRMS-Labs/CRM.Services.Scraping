using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMScraper.Library;
using CRMScraper.Library.Core;

namespace ScraperConsoleApp.Core
{
    public class ScraperTestCase
    {
         private readonly ScraperClient _scraperClient;

        public ScraperTestCase()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            _scraperClient = new ScraperClient(httpClient);
        }

        public async Task RunTestAsync(string url, string fileName)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                ScrapedPageResult result = await _scraperClient.ScrapePageAsync(url);

                string output = $"Scraped URL: {result.Url}\n" +
                                $"HTML Content Preview: {result.HtmlContent.Substring(0, Math.Min(result.HtmlContent.Length, 500))}...\n" +
                                $"JavaScript Data Found: {string.Join(", ", result.JavaScriptData)}\n" +
                                $"API Requests/Links Found: {string.Join(", ", result.ApiRequests)}\n";

                await File.WriteAllTextAsync(filePath, output);

                Console.WriteLine(output);
                Console.WriteLine($"Results written to {filePath}\n");
            }
            catch (HttpRequestException ex)
            {
                string errorOutput = $"Error while scraping {url}: {ex.Message}";
                Console.WriteLine(errorOutput);
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test", fileName);
                await File.WriteAllTextAsync(filePath, errorOutput);
            }
        }
    }
}