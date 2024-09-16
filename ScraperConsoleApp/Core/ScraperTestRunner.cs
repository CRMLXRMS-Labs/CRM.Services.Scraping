using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScraperConsoleApp.Core
{
    public class ScraperTestRunner
    {
        private readonly ProgressBar _progressBar;

        public ScraperTestRunner()
        {
            _progressBar = new ProgressBar();
        }

        public async Task RunAllTestsAsync()
        {
            ScraperTestCase scraperTest = new ScraperTestCase();

            var testCases = new[]
            {
                new { Url = "https://www.forbes.com/", FileName = "forbes_result.txt" },
                new { Url = "https://www.pturol.org.pl/", FileName = "pturol_result.txt" },
                new { Url = "https://www.onet.pl/", FileName = "onet_result.txt" },
                new { Url = "https://www.pw.edu.pl/", FileName = "pw_result.txt" }
            };

            int totalTests = testCases.Length;
            int completedTests = 0;

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\nRunning test for: {testCase.Url}");
                await scraperTest.RunTestAsync(testCase.Url, testCase.FileName);
                completedTests++;

                _progressBar.DisplayProgress(completedTests, totalTests);
            }

            Console.WriteLine(); 
        }
    }

}