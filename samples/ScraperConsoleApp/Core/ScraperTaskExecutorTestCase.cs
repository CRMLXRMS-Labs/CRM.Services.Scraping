using System;
using System.IO;
using System.Threading.Tasks;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Utils;

namespace ScraperConsoleApp.Core
{
    public class ScraperTaskExecutorTestCase
    {
        private readonly ScraperTaskExecutor _scraperTaskExecutor;
        private readonly PerformanceMonitor _performanceMonitor;

        public ScraperTaskExecutorTestCase(ScraperTaskExecutor scraperTaskExecutor)
        {
            _scraperTaskExecutor = scraperTaskExecutor;
            _performanceMonitor = new PerformanceMonitor();
        }

        public async Task<(long elapsedTime, long memoryUsed, long diskUsage)> RunTestAsync(ScrapingTask scrapingTask, string fileName)
        {
            try
            {
                _performanceMonitor.StartMonitoring();

                // Create test output directory
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid directory path."));

                // Execute scraping task
                var results = await _scraperTaskExecutor.ExecuteScrapingTaskAsync(scrapingTask, new System.Threading.CancellationToken());

                string output = $"Scraped Task Target URL: {scrapingTask.TargetUrl}\n";
                foreach (var result in results)
                {
                    output += $"Scraped URL: {result.Url}\n" +
                              $"HTML Content Preview: {result.HtmlContent.Substring(0, Math.Min(result.HtmlContent.Length, 10_000_000))}...\n" +
                              $"JavaScript Data Found: {string.Join(", ", result.JavaScriptData)}\n" +
                              $"API Requests/Links Found: {string.Join(", ", result.ApiRequests)}\n\n";
                }

                // Save result to file
                await File.WriteAllTextAsync(filePath, output);
                Console.WriteLine(output);
                Console.WriteLine($"Results written to {filePath}\n");

                // Stop monitoring
                _performanceMonitor.StopMonitoring();
                _performanceMonitor.DisplayResults(scrapingTask.TargetUrl, filePath);

                // Return performance data
                return (_performanceMonitor.GetElapsedTime(), _performanceMonitor.GetMemoryUsed(), _performanceMonitor.GetDiskUsage(filePath));
            }
            catch (Exception ex)
            {
                _performanceMonitor.StopMonitoring();
                Console.WriteLine($"Error while executing scraping task: {ex.Message}");
                return (0, 0, 0); // Return zero in case of an error
            }
        }
    }
}
