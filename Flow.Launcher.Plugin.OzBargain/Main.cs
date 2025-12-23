using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows;

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
                                    Process.Start(url);
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
                            System.Windows.Clipboard.SetData(DataFormats.Text, e.ToString());
                            return true;
                        }
                    }};
                }
            }
            else    
            {
                results = new List<Result>
                {
                    new() { Title = "New Deals", IcoPath = "icon.png",
                            SubTitle = "Press Ctr+Tab (or autocomplete hotkey)",
                            AutoCompleteText = "oz https://www.ozbargain.com.au/deals" },
                    new() { Title = "Freebies", IcoPath = "icon.png",
                            SubTitle = "Press Ctr+Tab (or autocomplete hotkey)",
                            AutoCompleteText = "oz https://www.ozbargain.com.au/freebies" },
                    new() { Title = "Popular Deals", IcoPath = "icon.png",
                            SubTitle = "Press Ctr+Tab (or autocomplete hotkey)",
                            AutoCompleteText = "oz https://www.ozbargain.com.au/deals/popular" },
                };
            }

            return results;
        }

        private static void ExtractVars(XElement item, out string title, out string expiry, out string icoPath)
        {
            title = item.Element("title").Value;

            expiry = "Unknown";
            string rawExpiry = item.Elements()
                                    .FirstOrDefault(i => i.Attribute("expiry") != null)
                                    ?.Attribute("expiry")
                                    ?.Value;
            if (!string.IsNullOrEmpty(rawExpiry) && DateTimeOffset.TryParse(rawExpiry, out var dto))
            {
                // parse to human readable format
                expiry = dto.LocalDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
            expiry = "Expires: " + expiry;
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