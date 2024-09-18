using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Utils;

namespace ScraperConsoleApp.Core
{
    public class ScraperTestCase
    {
        private readonly ScraperClient _scraperClient;
        private readonly PerformanceMonitor _performanceMonitor;

        public ScraperTestCase()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            IPageElementsExtractor pageElementsExtractor = new PageElementsExtractor();
            _scraperClient = new ScraperClient(httpClient, pageElementsExtractor);
            _performanceMonitor = new PerformanceMonitor();
        }

        public async Task<(long elapsedTime, long memoryUsed, long diskUsage)> RunTestAsync(string url, string fileName)
        {
            try
            {
                _performanceMonitor.StartMonitoring();

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid directory path."));

                ScrapedPageResult result = await _scraperClient.ScrapePageAsync(url);

                string output = $"Scraped URL: {result.Url}\n" +
                                $"HTML Content Preview: {result.HtmlContent.Substring(0, Math.Min(result.HtmlContent.Length, 10_000_000))}...\n" +
                                $"JavaScript Data Found: {string.Join(", ", result.JavaScriptData)}\n" +
                                $"API Requests/Links Found: {string.Join(", ", result.ApiRequests)}\n";

                await File.WriteAllTextAsync(filePath, output);

                Console.WriteLine(output);
                Console.WriteLine($"Results written to {filePath}\n");

                _performanceMonitor.StopMonitoring();
                _performanceMonitor.DisplayResults(url, filePath);

                // Return performance data including disk usage
                return (_performanceMonitor.GetElapsedTime(), _performanceMonitor.GetMemoryUsed(), _performanceMonitor.GetDiskUsage(filePath));
            }
            catch (HttpRequestException ex)
            {
                _performanceMonitor.StopMonitoring();
                string errorOutput = $"Error while scraping {url}: {ex.Message}";
                Console.WriteLine(errorOutput);
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test", fileName);
                await File.WriteAllTextAsync(filePath, errorOutput);

                return (0, 0, 0); // Return zero if there was an error
            }
        }
    }
}
