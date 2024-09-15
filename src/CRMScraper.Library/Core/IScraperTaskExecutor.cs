// IScraperTaskExecutor.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRMScraper.Library.Core
{
    public interface IScraperTaskExecutor
    {
        Task<List<ScrapedPageResult>> ExecuteScrapingTaskAsync(ScrapingTask task, CancellationToken cancellationToken);
    }
}
