using HtmlAgilityPack;

namespace CRMScraper.Library.Core.Utils
{
    public interface IPageElementsExtractor
    {
        List<string> ExtractJavaScript(HtmlDocument document);
        List<string> ExtractApiRequests(HtmlDocument document, string baseUrl);
    }
}
