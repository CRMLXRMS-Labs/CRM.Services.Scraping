
# CRM Scraper

CRM Scraper is a powerful library designed to scrape CRM (Customer Relationship Management) systems and extract valuable data. This project provides a comprehensive scraping solution, supporting both static and dynamic websites using HTML parsing and Playwright for dynamic content rendering.

## Features

- **HTML Parsing**: Scrape static websites using `HtmlAgilityPack` for extracting structured data.
- **Dynamic Content Scraping**: Utilizes Playwright to scrape websites with dynamic content (JavaScript-heavy websites).
- **Extensible API**: Built with flexibility in mind, allowing users to extend the scraper as per their use case.
- **Retry Mechanism**: Built-in retry mechanism with exponential backoff for failed requests.
- **Concurrent Scraping**: Supports concurrent scraping tasks to speed up large-scale data extraction.
- **Unit Tests**: Extensive test coverage using `xUnit` for core functionalities.

## Project Structure

```bash
.
├── ScraperConsoleApp           # Console application to manually test the library
├── src
│   ├── CRMScraper.Library      # Main library containing scraper logic
│   ├── CRMScraper.Tests        # Unit tests for the library
├── TestResults                 # Test result artifacts, including coverage reports
├── .github                     # GitHub Actions for CI/CD
├── scraping_service_library_net.sln # Solution file
```

### Library Components

- **ScraperClient**: Core scraping logic that handles page requests, both static and dynamic.
- **ScraperTaskExecutor**: Manages the execution of scraping tasks concurrently.
- **PageElementsExtractor**: Service that handles the extraction of JavaScript and API links from the page.
- **ScraperHelperService**: Provides helper methods such as retry logic for scraping.

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Playwright (for dynamic content scraping)

### Installing

1. **Clone the repository:**

   ```bash
   git clone https://github.com/yourusername/scraping_service_library_net.git
   cd scraping_service_library_net
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the project:**

   ```bash
   dotnet build --configuration Release
   ```

4. **Run the console application:**

   ```bash
   cd ScraperConsoleApp
   dotnet run
   ```

## Running Tests

The project uses `xUnit` for unit tests and `coverlet` for code coverage. To run the tests and generate a coverage report:

```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/ --logger "trx;LogFileName=TestResults.trx"
```

## CI/CD

This project uses GitHub Actions for continuous integration and deployment. The CI pipeline performs the following tasks:

- Build the project
- Run unit tests with code coverage
- Generate a NuGet package and upload it as an artifact

The `.github/workflows/dotnet-ci.yml` file defines the build and test steps.

## Creating a NuGet Package

To create a NuGet package, use the following command:

```bash
dotnet pack --configuration Release --output ./nupkgs
```

## Usage

This section shows how to use the `CRMScraper.Library` for both static and dynamic content scraping.

### 1. Scraping Static Pages

To scrape static web pages, you can use the `ScraperClient` class, which leverages `HtmlAgilityPack` to extract the HTML content, JavaScript, and API requests from the page.

#### Example: Scraping a Static Page

```csharp
using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        var pageElementsExtractor = new PageElementsExtractor();  // Implement this to extract JavaScript and API requests
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        // Scrape a static page
        var result = await scraperClient.ScrapePageAsync("https://example.com");

        // Output scraped results
        Console.WriteLine($"URL: {result.Url}");
        Console.WriteLine($"HTML Content: {result.HtmlContent}");
        Console.WriteLine($"JavaScript Data: {string.Join(", ", result.JavaScriptData)}");
        Console.WriteLine($"API Requests: {string.Join(", ", result.ApiRequests)}");
    }
}
```

### 2. Scraping Dynamic Pages (JavaScript-heavy)

For dynamic content scraping (JavaScript-heavy websites), `ScraperClient` utilizes Playwright to render the page fully before extracting content and links.

#### Example: Scraping a Dynamic Page

```csharp
using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        var pageElementsExtractor = new PageElementsExtractor();  // Implement this to extract JavaScript and API requests
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        // Scrape a dynamic page using Playwright
        var result = await scraperClient.ScrapeDynamicPageAsync("https://example.com");

        // Output the results
        Console.WriteLine($"URL: {result.Url}");
        Console.WriteLine($"HTML Content: {result.HtmlContent}");
        Console.WriteLine($"API Requests: {string.Join(", ", result.ApiRequests)}");
    }
}
```

### 3. Concurrent Scraping

If you want to scrape multiple pages concurrently and efficiently, use the `ScraperTaskExecutor` class. This class manages concurrent scraping tasks, respecting limits on the number of pages and time.

#### Example: Concurrent Scraping Task

```csharp
using CRMScraper.Library.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        var pageElementsExtractor = new PageElementsExtractor();
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);
        var scraperTaskExecutor = new ScraperTaskExecutor(scraperClient);

        // Define a scraping task with a maximum of 10 pages and a 1-minute time limit
        var scrapingTask = new ScrapingTask
        {
            TargetUrl = "https://example.com",
            MaxPages = 10,
            TimeLimit = TimeSpan.FromMinutes(1),
            MaxConcurrentPages = 3,  // Max 3 concurrent pages
            UseDynamicScraping = true
        };

        // Run the scraping task
        var cancellationTokenSource = new CancellationTokenSource();
        var results = await scraperTaskExecutor.ExecuteScrapingTaskAsync(scrapingTask, cancellationTokenSource.Token);

        // Output the results
        foreach (var result in results)
        {
            Console.WriteLine($"Scraped URL: {result.Url}");
            Console.WriteLine($"HTML Content: {result.HtmlContent}");
        }
    }
}
```

### Summary of Core Classes

- **ScraperClient**: Core logic for scraping static and dynamic pages.
- **ScraperTaskExecutor**: Manages concurrent scraping tasks and retries.
- **ScrapedPageResult**: Represents the result of a scraping operation.
- **ScrapingTask**: Represents a scraping task configuration, including limits on pages and time.

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue. For larger changes, feel free to fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

