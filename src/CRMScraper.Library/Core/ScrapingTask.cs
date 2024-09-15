namespace CRMScraper.Library.Core
{
    public class ScrapingTask
    {
        public string TargetUrl { get; }
        public int MaxPages { get; }
        public TimeSpan TimeLimit { get; }

        public ScrapingTask(string targetUrl, int maxPages, TimeSpan timeLimit)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                throw new ArgumentNullException(nameof(targetUrl));
            }
            TargetUrl = targetUrl;
            MaxPages = maxPages;
            TimeLimit = timeLimit;
        }
    }
}
