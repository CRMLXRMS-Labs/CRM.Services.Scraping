
# CRM Scraper

CRM Scraper is a comprehensive library designed to scrape CRM (Customer Relationship Management) systems and extract valuable data. It supports both static and dynamic websites using HTML parsing and Playwright for dynamic content rendering.

## Features

- **Static HTML Parsing**: Scrape static websites using `HtmlAgilityPack`.
- **Dynamic Content Scraping**: Use Playwright to scrape JavaScript-heavy websites.
- **Extensible API**: Flexible, allowing users to extend functionality.
- **Retry Mechanism**: Built-in retry with exponential backoff for failed requests.
- **Concurrent Scraping**: Scrape multiple pages simultaneously.
- **Unit Tested**: Tested with `xUnit` for reliability.

## Project Structure

```bash
.
├── .github                     # GitHub Actions for CI/CD workflows
├── .gitignore                   # Git ignore rules
├── README.md                    # Project documentation
├── samples                      # Sample applications for testing
│   └── ScraperConsoleApp        # Console application to manually test the library
├── scraping_service_library_net.sln # Solution file
├── scripts                      # Scripts for building and publishing
│   ├── build_and_test.sh        # Script for building and running tests
│   └── publish_nuget.sh         # Script for packing and publishing NuGet packages
├── src
│   ├── CRMScraper.Library       # Main library containing the scraping logic
│   │   ├── Core                 # Core components for scraping logic
│   │   ├── CRMScraper.Library.csproj # Library project file
│   └── CRMScraper.Tests         # Unit tests for the library
└── scraping_service_library_net.sln # Solution file

```

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Playwright for dynamic content scraping

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

Use `xUnit` for unit tests and `coverlet` for code coverage. To run tests and generate coverage reports:

```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/ --logger "trx;LogFileName=TestResults.trx"
```

## CI/CD Pipeline

This project uses GitHub Actions for continuous integration and deployment. The pipeline automatically:
- Builds the project
- Runs unit tests with code coverage
- Generates a NuGet package and uploads it as an artifact

See `.github/workflows/dotnet-ci.yml` for the pipeline configuration.

## Creating a NuGet Package

To create a NuGet package, run the following command:

```bash
dotnet pack --configuration Release --output ./nupkgs
```

## Usage

This section explains how to use the `CRMScraper.Library` for both static and dynamic content scraping.

### 1. Scraping Static Pages

Use the `ScraperClient` class to scrape static web pages and extract HTML content, JavaScript, and API requests.

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
        var pageElementsExtractor = new PageElementsExtractor();  // Implement to extract JavaScript and API requests
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        var result = await scraperClient.ScrapePageAsync("https://example.com");

        Console.WriteLine($"URL: {result.Url}");
        Console.WriteLine($"HTML Content: {result.HtmlContent}");
        Console.WriteLine($"JavaScript Data: {string.Join(", ", result.JavaScriptData)}");
        Console.WriteLine($"API Requests: {string.Join(", ", result.ApiRequests)}");
    }
}
```

### 2. Scraping Dynamic Pages

For JavaScript-heavy websites, `ScraperClient` uses Playwright to fully render the page before scraping.

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
        var pageElementsExtractor = new PageElementsExtractor();  // Implement to extract JavaScript and API requests
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        var result = await scraperClient.ScrapeDynamicPageAsync("https://example.com");

        Console.WriteLine($"URL: {result.Url}");
        Console.WriteLine($"HTML Content: {result.HtmlContent}");
        Console.WriteLine($"API Requests: {string.Join(", ", result.ApiRequests)}");
    }
}
```

### 3. Concurrent Scraping

For large-scale scraping, use `ScraperTaskExecutor` to scrape multiple pages concurrently.

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

        var scrapingTask = new ScrapingTask
        {
            TargetUrl = "https://example.com",
            MaxPages = 10,
            TimeLimit = TimeSpan.FromMinutes(1),
            MaxConcurrentPages = 3,
            UseDynamicScraping = true
        };

        var cancellationTokenSource = new CancellationTokenSource();
        var results = await scraperTaskExecutor.ExecuteScrapingTaskAsync(scrapingTask, cancellationTokenSource.Token);

        foreach (var result in results)
        {
            Console.WriteLine($"Scraped URL: {result.Url}");
            Console.WriteLine($"HTML Content: {result.HtmlContent}");
        }
    }
}
```

### Core Classes

- **ScraperClient**: Core logic for static and dynamic page scraping.
- **ScraperTaskExecutor**: Manages concurrent scraping tasks and retries.
- **ScrapedPageResult**: Represents the result of a scraping operation.
- **ScrapingTask**: Defines a scraping task with limits on pages and time.

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

