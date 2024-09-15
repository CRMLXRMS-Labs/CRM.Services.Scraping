namespace CRMScraper.Library.Core
{
    public class ScrapedPageResult
    {
        public string Url { get; set; }
        public string HtmlContent { get; set; }
        public List<string> JavaScriptData { get; set; }
        public List<string> ApiRequests { get; set; }
    }
}
