using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using CRMScraper.Library.Core;
using System.Web;

namespace CRMScraper.Library
{
    public class ScraperClient : IScraperClient
    {
        private readonly HttpClient _httpClient;

        public ScraperClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ScrapedPageResult> ScrapePageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var javascriptData = ExtractJavaScript(htmlDocument);
            var apiRequests = ExtractApiRequests(htmlDocument);

            return new ScrapedPageResult
            {
                Url = url,
                HtmlContent = content,
                JavaScriptData = javascriptData,
                ApiRequests = apiRequests
            };
        }

        private List<string> ExtractJavaScript(HtmlDocument document)
        {
            var scripts = document.DocumentNode.SelectNodes("//script");
            return scripts?.Select(script => HttpUtility.HtmlDecode(script.InnerText)).ToList() ?? new List<string>();
        }

        private List<string> ExtractApiRequests(HtmlDocument document)
        {
            var apiRequests = new List<string>();

            var links = document.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    if (Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
                    {
                        apiRequests.Add(href);
                    }
                }
            }

            var forms = document.DocumentNode.SelectNodes("//form[@action]");
            if (forms != null)
            {
                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", string.Empty);
                    if (Uri.IsWellFormedUriString(action, UriKind.RelativeOrAbsolute))
                    {
                        apiRequests.Add(action);
                    }
                }
            }

            return apiRequests;
        }
    }
}
