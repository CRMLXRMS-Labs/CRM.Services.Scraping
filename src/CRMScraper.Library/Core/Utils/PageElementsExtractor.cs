using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace CRMScraper.Library.Core.Utils
{
    public class PageElementsExtractor : IPageElementsExtractor
    {
        public List<string> ExtractJavaScript(HtmlDocument document)
        {
            var scripts = document.DocumentNode.SelectNodes("//script");
            return scripts?.Select(script => HttpUtility.HtmlDecode(script.InnerText)).ToList() ?? new List<string>();
        }

        public List<string> ExtractApiRequests(HtmlDocument document, string baseUrl)
        {
            var apiRequests = new List<string>();

            var links = document.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);

                    // Only process valid, non-empty links
                    if (!string.IsNullOrWhiteSpace(href) && Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
                    {
                        // Convert relative URL to absolute
                        if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                        {
                            href = new Uri(new Uri(baseUrl), href).ToString();
                        }

                        // Only add valid absolute URLs
                        if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                        {
                            apiRequests.Add(href);
                        }
                    }
                }
            }

            var forms = document.DocumentNode.SelectNodes("//form[@action]");
            if (forms != null)
            {
                foreach (var form in forms)
                {
                    var action = form.GetAttributeValue("action", string.Empty);

                    if (!string.IsNullOrWhiteSpace(action) && Uri.IsWellFormedUriString(action, UriKind.RelativeOrAbsolute))
                    {
                        if (Uri.IsWellFormedUriString(action, UriKind.Relative))
                        {
                            action = new Uri(new Uri(baseUrl), action).ToString();
                        }

                        if (Uri.IsWellFormedUriString(action, UriKind.Absolute))
                        {
                            apiRequests.Add(action);
                        }
                    }
                }
            }

            return apiRequests;
        }
    }
}
