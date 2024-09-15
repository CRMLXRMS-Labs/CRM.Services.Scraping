namespace CRMScraper.Library.Core
{
    public interface IScraperClient
    {
        Task<ScrapedPageResult> ScrapePageAsync(string url);
    }
}
