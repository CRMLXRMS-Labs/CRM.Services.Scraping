using System;

namespace CRMScraper.Library.Core.Exceptions
{
    public class InvalidUrlException : ScraperException
    {
        public InvalidUrlException(string url) 
            : base($"Invalid or empty URL: {url}")
        { }
    }
}
