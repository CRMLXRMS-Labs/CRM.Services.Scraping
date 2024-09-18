using ScraperConsoleApp.Core;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Utils;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide an argument: 1 for ScraperClient tests, 2 for ScraperTaskExecutor tests.");
            return;
        }

        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        IPageElementsExtractor pageElementsExtractor = new PageElementsExtractor();
        ScraperClient scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        IScraperHelperService scraperHelperService = new ScraperHelperService();
        ScraperTaskExecutor scraperTaskExecutor = new ScraperTaskExecutor(scraperClient, scraperHelperService);

        if (args[0] == "1")
        {
            Console.WriteLine("Running ScraperClient tests...");
            ScraperTestRunner scraperClientTestRunner = new ScraperTestRunner();
            await scraperClientTestRunner.RunAllTestsAsync();
            Console.WriteLine("ScraperClient tests completed.");
        }
        else if (args[0] == "2")
        {
            Console.WriteLine("Running ScraperTaskExecutor tests...");
            ScraperTaskExecutorTestRunner testRunner = new ScraperTaskExecutorTestRunner(scraperTaskExecutor);
            await testRunner.RunAllTestsAsync();
            Console.WriteLine("ScraperTaskExecutor tests completed.");
        }
        else
        {
            Console.WriteLine("Invalid argument. Please provide 1 for ScraperClient tests or 2 for ScraperTaskExecutor tests.");
        }
    }
}
