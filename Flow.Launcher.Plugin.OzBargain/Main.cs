using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.OzBargain
{
    /// <summary>
    /// OzBargain Plugin.
    /// </summary>
    public class OzBargain : IPlugin
    {
        private PluginInitContext _context;

        private static Dictionary<string, XDocument> CachedFetches = new Dictionary<string, XDocument>();

        /// <summary>
        /// Initializes the plugin.
        /// </summary>
        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        /// <summary>
        /// On User Query Event.
        /// </summary>
        public List<Result> Query(Query query)
        {
            List<Result> results;
            if (query.SearchTerms.Length > 0)
            {
                results = new List<Result>();

                string RSS_url = query.SearchTerms[0] + "/feed";
                
                try
                {
                    // fetch data
                    XDocument doc;
                    if (CachedFetches.ContainsKey(RSS_url))
                    {
                        doc = CachedFetches[RSS_url];
                    }
                    else
                    {
                        doc = XDocument.Load(RSS_url);
                        CachedFetches[RSS_url] = doc;
                    }
                    // parse data
                    foreach (XElement item in doc.Descendants("item"))
                    {
                        ExtractVars(item, out string title, out string expiry, out string icoPath);
                        results.Add(
                            new()
                            {
                                Title = title,
                                SubTitle = expiry,
                                IcoPath = icoPath,
                                Action = _ =>
                                {
                                    string url = item.Element("link").Value;
                                    _context.API.OpenUrl(url);
                                    return true; // close flow after action
                                }
                            }
                        );
                    }
                }
                catch (Exception e)
                {
                    string title = "Failed to fetch feed.";
                    if (e.ToString().Contains("429")) title = "Rate limited, try again later.";
                    results = new List<Result> { new() {
                        Title = title,
                        Action = _ =>
                        {
                            _context.API.CopyToClipboard(e.ToString());
                            return true;
                        }
                    }};
                }
            }
            else    
            {
                results = new List<Result>
                {
                    new()
                    {
                        Title = "New Deals", IcoPath = "icon.png",
                        Action = _ =>
                        {
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/deals");
                            return false;
                        }
                    },
                    new()
                    {
                        Title = "Freebies", IcoPath = "icon.png",
                        Action = _ =>
                        {
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/freebies");
                            return false;
                        }
                    },
                    new()
                    {
                        Title = "Popular Deals", IcoPath = "icon.png",
                        Action = _ =>
                        {
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/deals/popular");
                            return false;
                        }
                    },
                    new()
                    {
                        Title = "Refresh", IcoPath = "icon.png",
                        Action = _ =>
                        {
                            CachedFetches = new Dictionary<string, XDocument>();
                            _context.API.ShowMsg("Refreshed Plugin Cache!");
                            return false;
                        }
                    }
                };
            }

            return results;
        }

        private static void ExtractVars(XElement item, out string title, out string expiry, out string icoPath)
        {
            title = item.Element("title").Value;

            expiry = "Expiry Date Unknown";
            string rawExpiry = item.Elements()
                                    .FirstOrDefault(i => i.Attribute("expiry") != null)
                                    ?.Attribute("expiry")
                                    ?.Value;
            if (!string.IsNullOrEmpty(rawExpiry) && DateTimeOffset.TryParse(rawExpiry, out var ExpiryDateTimeOffset))
            {
                // parse to human readable format
                TimeSpan timeUntil = ExpiryDateTimeOffset - DateTimeOffset.UtcNow;
                double totalHours = timeUntil.TotalHours;
                expiry = $"Expires in {Math.Floor(totalHours / 24.0)} days and {Math.Floor((totalHours % 24.0) * 10) / 10} hours";
            }
            if (item.Elements().FirstOrDefault(e => e.Name.LocalName == "title-msg")?.Value.ToLower() == "expired")
            {
                expiry = "EXPIRED";
            }

            icoPath = item.Elements().FirstOrDefault(
                        i => i.Attribute("image") != null
                    )?.Attribute("image").Value ?? "";
        }
    }
}