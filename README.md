# CRM Scraper - .NET Web Scraper Library

[![codecov](https://codecov.io/gh/CRMLXRMS-Labs/scraping_service_library_net/graph/badge.svg?token=R261U013KP)](https://codecov.io/gh/CRMLXRMS-Labs/scraping_service_library_net)  
[![NuGet Version](https://img.shields.io/nuget/v/CRMScraper.Library)](https://www.nuget.org/packages/CRMScraper.Library)  
[![Build Status](https://img.shields.io/github/actions/workflow/status/CRMLXRMS-Labs/scraping_service_library_net/.github/workflows/dotnet-build-test-ci.yml?branch=main)](https://github.com/CRMLXRMS-Labs/scraping_service_library_net/actions)  
![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&logoColor=white)

CRM Scraper is a robust and high-performance **web scraper** library built on .NET 8.0. It provides flexible tools for **web scraping**, including **static HTML parsing** and **dynamic content scraping** using Playwright. With features like retry logic, concurrency, and extensive test coverage, CRM Scraper is your go-to solution for any **data extraction** needs.

## Key Features of CRM Scraper

- **Static HTML Parsing**: Extract data from static websites using `HtmlAgilityPack`, an efficient library for **scraping static content**.
- **Dynamic Content Scraping**: Use Playwright to scrape JavaScript-heavy websites, ensuring you capture the full page content.
- **Extensible API**: Easily extendable for custom scraping tasks, making it adaptable to a variety of **web scraping** requirements.
- **Retry Mechanism**: Built-in retry logic with exponential backoff, ensuring reliability when scraping unstable websites.
- **Concurrent Scraping**: Supports scraping multiple web pages simultaneously, enabling efficient and scalable **web scraping services**.
- **Unit Tested**: Extensive test coverage using `xUnit`, ensuring the reliability and robustness of the scraping library.

## Installation - Get CRM Scraper via NuGet

You can install the `CRMScraper.Library` package through NuGet for fast integration into your **scraping service** projects:

| Platform | Version |
| -------- | ------- |
| .NET 8.0 | [1.1.58](https://www.nuget.org/packages/CRMScraper.Library) |

### Install via .NET CLI:

```bash
dotnet add package CRMScraper.Library --version 1.1.58
```

### Install via NuGet Package Manager:

```bash
Install-Package CRMScraper.Library -Version 1.1.58
```

### Key Dependencies

- `HtmlAgilityPack (>= 1.11.65)` - For static HTML parsing.
- `Microsoft.Playwright (>= 1.47.0)` - For dynamic web scraping.

## Project Structure Overview

```bash
.
├── .github                     # CI/CD workflows via GitHub Actions
├── .gitignore                   # Git ignore rules
├── README.md                    # Documentation (you are here!)
├── samples                      # Sample applications for manual testing
├── scripts                      # Scripts for building and publishing NuGet packages
├── src
│   ├── CRMScraper.Library       # Main library containing the scraping logic
│   └── CRMScraper.Tests         # Unit tests for the library
└── scraping_service_library_net.sln # Solution file
```

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Playwright (for **dynamic web scraping**)

### Step-by-Step Setup

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

## Running Unit Tests for Web Scraper

CRM Scraper is rigorously tested using `xUnit` to ensure reliability. Run tests and generate code coverage reports:

```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/ --logger "trx;LogFileName=TestResults.trx"
```

## CI/CD Pipeline

CRM Scraper uses GitHub Actions for continuous integration and deployment. The pipeline automatically:
- Builds the project.
- Runs unit tests with code coverage.
- Generates a NuGet package and uploads it as an artifact.

Check out the CI/CD pipeline configuration in `.github/workflows/dotnet-ci.yml`.

## Building a NuGet Package

To generate a NuGet package for the **web scraper library**:

```bash
dotnet pack --configuration Release --output ./nupkgs
```

## How to Use CRM Scraper

This section provides usage examples for static and dynamic content scraping with CRM Scraper.

### 1. Scraping Static Pages

The `ScraperClient` class allows scraping of static web pages and extracting HTML content, JavaScript data, and API requests.

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
        var pageElementsExtractor = new PageElementsExtractor();
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

For scraping **dynamic web pages**, use Playwright to ensure the full page is rendered before scraping.

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
        var pageElementsExtractor = new PageElementsExtractor();
        var scraperClient = new ScraperClient(httpClient, pageElementsExtractor);

        var result = await scraperClient.ScrapeDynamicPageAsync("https://example.com");

        Console.WriteLine($"URL: {result.Url}");
        Console.WriteLine($"HTML Content: {result.HtmlContent}");
        Console.WriteLine($"API Requests: {string.Join(", ", result.ApiRequests)}");
    }
}
```

### 3. Concurrent Scraping Tasks

Use the `ScraperTaskExecutor` for **concurrent web scraping**, enabling you to scrape multiple pages at once.

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

### Core Components in CRM Scraper

- **ScraperClient**: Handles static and dynamic web scraping.
- **ScraperTaskExecutor**: Manages concurrent scraping tasks with retry logic.
- **ScrapedPageResult**: Represents the result of a scraping operation.
- **ScrapingTask**: Defines a web scraping task, including limits on pages and time.

## Contributing

We welcome contributions! Feel free to open an issue or submit a pull request if you find a bug or have feature suggestions.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

