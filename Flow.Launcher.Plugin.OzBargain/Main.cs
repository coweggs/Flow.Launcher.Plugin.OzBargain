using System;
using System.Xml.Linq;
using System.Collections.Generic;
using Flow.Launcher.Plugin.OzBargain.Services;

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
            List<Result> results = new List<Result>();;
            
            if (query.SearchTerms.Length > 0)
            {
                if (query.SearchTerms[0].ToLower() == "search")
                {
                    
                }
                else // Assume first search term is a ozb url.
                {
                    results.AddRange(FetchFeed(query));
                }
            }
            else
            {
                results.AddRange(BuildBaseResults());
            }

            return results;
        }

        private List<Result> FetchFeed(Query query)
        {
            List<Result> results = new List<Result>();
            
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
                results.AddRange(RSSHelper.ParseRSS(doc, _context));;
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

            return results;
        }

        private List<Result> BuildBaseResults()
        {
            return new List<Result>
            {
                new()
                {
                    Title = "New Deals", SubTitle = "Ctr Click to Open URL", IcoPath = "icon.png",
                    AutoCompleteText = "oz https://www.ozbargain.com.au/deals",
                    Action = actionContext =>
                    {
                        bool ctrPressed = actionContext.SpecialKeyState.CtrlPressed;
                        if (ctrPressed)
                            _context.API.OpenUrl("https://www.ozbargain.com.au/deals");
                        else
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/deals");
                        return false;
                    }
                },
                new()
                {
                    Title = "Freebies", SubTitle = "Ctr Click to Open URL", IcoPath = "icon.png",
                    AutoCompleteText = "oz https://www.ozbargain.com.au/freebies",
                    Action = actionContext =>
                    {
                        bool ctrPressed = actionContext.SpecialKeyState.CtrlPressed;
                        if (ctrPressed)
                            _context.API.OpenUrl("https://www.ozbargain.com.au/freebies");
                        else
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/freebies");
                        return false;
                    }
                },
                new()
                {
                    Title = "Popular Deals", SubTitle = "Ctr Click to Open URL", IcoPath = "icon.png",
                    AutoCompleteText = "oz https://www.ozbargain.com.au/deals/popular",
                    Action = actionContext =>
                    {
                        bool ctrPressed = actionContext.SpecialKeyState.CtrlPressed;
                        if (ctrPressed)
                            _context.API.OpenUrl("https://www.ozbargain.com.au/deals/popular");
                        else
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au/deals/popular");
                        return false;
                    }
                },
                new()
                {
                    Title = "Refresh", SubTitle = "Too many could lead to rate limit!", IcoPath = "icon.png",
                    Action = _ =>
                    {
                        CachedFetches = new Dictionary<string, XDocument>();
                        _context.API.ShowMsg("Refreshed Plugin Cache!");
                        return false;
                    }
                }
            };
        }
    }
}
