namespace CRMScraper.Library.Core.Entities
{
    public class ScrapedPageResult
    {
        public string Url { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public List<string> JavaScriptData { get; set; } = new List<string>();
        public List<string> ApiRequests { get; set; } = new List<string>();
    }
}
