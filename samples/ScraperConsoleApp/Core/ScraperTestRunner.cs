using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScraperConsoleApp.Core
{
    public class ScraperTestRunner
    {
        private readonly ProgressBar _progressBar;
        private List<(string Url, long ElapsedTime, long MemoryUsed, long DiskUsage)> _testResults;

        public ScraperTestRunner()
        {
            _progressBar = new ProgressBar();
            _testResults = new List<(string Url, long ElapsedTime, long MemoryUsed, long DiskUsage)>();
        }

        public async Task RunAllTestsAsync()
        {
            ScraperTestCase scraperTest = new ScraperTestCase();

            var testCases = new[]
            {
                new { Url = "https://www.forbes.com/", FileName = "forbes_result.txt" },
                new { Url = "https://www.pturol.org.pl/", FileName = "pturol_result.txt" },
                new { Url = "https://www.onet.pl/", FileName = "onet_result.txt" },
                new { Url = "https://www.pw.edu.pl/", FileName = "pw_result.txt" },

                new { Url = "https://mudblazor.com/", FileName = "mudblazor_result.txt" },
                new { Url = "https://blazor-university.com/", FileName = "blazor_university_result.txt" },

                new { Url = "https://www.bbc.com/", FileName = "bbc_result.txt" },
                new { Url = "https://www.cnn.com/", FileName = "cnn_result.txt" },
                new { Url = "https://www.nytimes.com/", FileName = "nytimes_result.txt" },
                new { Url = "https://www.github.com/", FileName = "github_result.txt" },
                new { Url = "https://stackoverflow.com/", FileName = "stackoverflow_result.txt" },

                new { Url = "https://developer.linkedin.com/", FileName = "linkedin_result.txt" },
                new { Url = "https://twitter.com/", FileName = "twitter_result.txt" }
            };

            int totalTests = testCases.Length;
            int completedTests = 0;

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\nRunning test for: {testCase.Url}");
                
                var (elapsedTime, memoryUsed, diskUsage) = await scraperTest.RunTestAsync(testCase.Url, testCase.FileName);

                _testResults.Add((testCase.Url, elapsedTime, memoryUsed, diskUsage));
                
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
