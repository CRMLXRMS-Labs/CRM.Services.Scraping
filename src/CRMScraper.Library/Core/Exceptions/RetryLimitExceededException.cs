using System;

namespace CRMScraper.Library.Core.Exceptions
{
    public class RetryLimitExceededException : ScraperException
    {
        public string Url { get; }
        public int RetryCount { get; }

        public RetryLimitExceededException(string url, int retryCount)
            : base($"Retry limit exceeded for URL: {url}. Retries: {retryCount}")
        {
            Url = url;
            RetryCount = retryCount;
        }
    }
}
