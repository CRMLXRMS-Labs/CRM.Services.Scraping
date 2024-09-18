using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMScraper.Library;
using CRMScraper.Library.Core;
using CRMScraper.Library.Core.Entities;
using CRMScraper.Library.Core.Utils;

namespace ScraperConsoleApp.Core
{
    public class ScraperTaskExecutorTestRunner
    {
        private readonly ProgressBar _progressBar;
        private List<(string Url, long ElapsedTime, long MemoryUsed, long DiskUsage)> _testResults;
        private readonly ScraperTaskExecutor _scraperTaskExecutor;

        public ScraperTaskExecutorTestRunner(ScraperTaskExecutor scraperTaskExecutor)
        {
            _progressBar = new ProgressBar();
            _testResults = new List<(string Url, long ElapsedTime, long MemoryUsed, long DiskUsage)>();
            _scraperTaskExecutor = scraperTaskExecutor;
        }

        public async Task RunAllTestsAsync()
        {
            ScraperTaskExecutorTestCase scraperTestCase = new ScraperTaskExecutorTestCase(_scraperTaskExecutor);

            var testCases = new[]
            {
                new { TargetUrl = "https://www.forbes.com/", MaxPages = 1, FileName = "forbes_result.txt" },
                new { TargetUrl = "https://www.pturol.org.pl/", MaxPages = 1, FileName = "pturol_result.txt" },
                new { TargetUrl = "https://www.onet.pl/", MaxPages = 1, FileName = "onet_result.txt" },
                new { TargetUrl = "https://www.pw.edu.pl/", MaxPages = 1, FileName = "pw_result.txt" },
                new { TargetUrl = "https://mudblazor.com/", MaxPages = 1, FileName = "mudblazor_result.txt" },
                new { TargetUrl = "https://www.bbc.com/", MaxPages = 1, FileName = "bbc_result.txt" },
                new { TargetUrl = "https://www.github.com/", MaxPages = 1, FileName = "github_result.txt" }
            };

            int totalTests = testCases.Length;
            int completedTests = 0;

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\nRunning test for: {testCase.TargetUrl}");

                var scrapingTask = new ScrapingTask
                {
                    TargetUrl = testCase.TargetUrl,
                    MaxPages = testCase.MaxPages,
                    TimeLimit = TimeSpan.FromMinutes(1)
                };

                // Run the test using the ScraperTaskExecutor
                var (elapsedTime, memoryUsed, diskUsage) = await scraperTestCase.RunTestAsync(scrapingTask, testCase.FileName);

                _testResults.Add((testCase.TargetUrl, elapsedTime, memoryUsed, diskUsage));
                completedTests++;
                _progressBar.DisplayProgress(completedTests, totalTests);
            }

            DisplaySummaryTable();
        }

        private void DisplaySummaryTable()
        {
            Console.WriteLine("\nSummary of Performance Results:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("| Test URL                        | Time (ms) | Memory (bytes) | Disk Usage (bytes) |");
            Console.WriteLine("----------------------------------------------------");

            foreach (var result in _testResults)
            {
                Console.WriteLine($"| {result.Url,-30} | {result.ElapsedTime,9} | {result.MemoryUsed,14} | {result.DiskUsage,16} |");
            }

            Console.WriteLine("----------------------------------------------------");
        }
    }
}
