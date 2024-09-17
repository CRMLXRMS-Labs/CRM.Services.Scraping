using CRMScraper.Library.Core.Entities;

namespace CRMScraper.Library.Core
{
    public interface IScraperTaskExecutor
    {
        Task<List<ScrapedPageResult>> ExecuteScrapingTaskAsync(ScrapingTask task, CancellationToken cancellationToken);
    }
}
