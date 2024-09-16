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

You can integrate the `CRMScraper.Library` into your project by including the package. Here's an example of using the `ScraperClient`:

```csharp
using CRMScraper.Library;
using CRMScraper.Library.Core;
using System.Net.Http;

var httpClient = new HttpClient();
var scraperClient = new ScraperClient(httpClient, new PageElementsExtractor());

var result = await scraperClient.ScrapePageAsync("https://example.com");
Console.WriteLine(result.HtmlContent);
```

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue. For larger changes, feel free to fork the repository and submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
```

### Key Sections Covered:
1. **Project Overview**: A description of the CRM Scraper and its main features.
2. **Project Structure**: Provides a high-level structure of the project.
3. **Getting Started**: Instructions for cloning, building, and running the project.
4. **Running Tests**: Commands for running tests and generating coverage reports.
5. **CI/CD**: A brief overview of the GitHub Actions pipeline.
6. **Creating a NuGet Package**: Instructions for generating a NuGet package.
7. **Usage Example**: Sample code showing how to use the library.
8. **Contributing**: Encourages open-source contributions.
9. **License**: Licensing information (MIT assumed, but this can be customized).