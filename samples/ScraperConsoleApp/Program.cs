using ScraperConsoleApp;
using ScraperConsoleApp.Core;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

Console.WriteLine("Starting Scraper Test Runner...");
ScraperTestRunner testRunner = new ScraperTestRunner();
await testRunner.RunAllTestsAsync();
Console.WriteLine("Scraping tests completed.");
