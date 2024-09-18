using System;

namespace CRMScraper.Library.Core.Exceptions
{
    public class ScraperException : Exception
    {
        // Constructor with just a message
        public ScraperException(string message) : base(message) { }

        // Constructor with both a message and an inner exception
        public ScraperException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
