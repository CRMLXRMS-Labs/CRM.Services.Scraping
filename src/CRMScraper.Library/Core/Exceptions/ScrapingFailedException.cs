using System;

namespace CRMScraper.Library.Core.Exceptions
{
    public class ScrapingFailedException : ScraperException
    {
        public ScrapingFailedException(string url, Exception innerException)
            : base($"Failed to scrape URL: {url}", innerException)
        { }
    }
}
