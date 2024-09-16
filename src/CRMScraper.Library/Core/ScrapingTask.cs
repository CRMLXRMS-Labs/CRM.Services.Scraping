namespace CRMScraper.Library.Core
{
    public class ScrapingTask
    {
        public string TargetUrl { get; set; } = string.Empty; 
        public int MaxPages { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int MaxConcurrentPages { get; set; } = 5; 
        public bool UseDynamicScraping { get; set; } = false; 
    }
}
