using System;
using System.Xml.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using Flow.Launcher.Plugin.OzBargain.Services;

namespace Flow.Launcher.Plugin.OzBargain
{
    public class OzBargain : IPlugin
    {
        private PluginInitContext _context;

        private static Dictionary<string, XDocument> CachedFetches = new Dictionary<string, XDocument>();
        private static string currentlySearching = "";

        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();;
            
            if (query.FirstSearch != "")
            {
                switch (query.FirstSearch.Trim().ToLower())
                {
                    case "search":
                    case "searchnotexpired":
                        results.AddRange(HandleSearchQuery(query));
                        break;
                    case "feed":
                        currentlySearching = "";
                        results.AddRange(BuildFeedResults());
                        break;
                    default:
                        currentlySearching = "";
                        if (query.Search.ToLower().Contains("ozbargain.com"))
                            results.AddRange(FetchFeed(query));
                        break;
                }
            }
            else
            {
                results.AddRange(BuildBaseResults());
            }

            return results;
        }

        private List<Result> HandleSearchQuery(Query query)
        {
            List<Result> results = new List<Result>();
            if (currentlySearching == query.Search.Substring(query.FirstSearch.Length).Trim())
            {
                results.AddRange(DoSearch(query));
            }
            else
            {
                currentlySearching = "";
                results.Add(new()
                {
                    Title = $"Search \"{query.Search.Substring(query.FirstSearch.Length).Trim()}\"", IcoPath = "icon.png",
                    Action = _ =>
                    {
                        _context.API.ReQuery(false);
                        currentlySearching = query.Search.Substring(query.FirstSearch.Length).Trim();
                        return false;
                    }
                });
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

        private List<Result> DoSearch(Query query)
        {
            List<Result> results = new List<Result>();

            // everything after keyword "search"
            bool showExpired = !query.FirstSearch.ToLower().Contains("notexpired");
            string searchQuery = query.Search.Substring(query.FirstSearch.Length).Trim();
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                HtmlDocument doc = SearchHelper.FetchDoc(searchQuery, showExpired);
                results.AddRange(SearchHelper.ParseDoc(doc, _context));
            }

            return results;
        }

        private List<Result> BuildBaseResults()
        {
            return new List<Result>
            {
                new()
                {
                    Title = "Search", SubTitle = "Ctr Click to Search Not Expired", IcoPath = "icon.png",
                    AutoCompleteText = "oz search ",
                    Action = actionContext =>
                    {
                        bool ctrPressed = actionContext.SpecialKeyState.CtrlPressed;
                        if (ctrPressed)
                            _context.API.ChangeQuery("oz searchnotexpired ");
                        else
                            _context.API.ChangeQuery("oz search ");
                        return false;
                    }
                },
                new()
                {
                    Title = "Feeds", IcoPath = "icon.png",
                    AutoCompleteText = "oz feed",
                    Action = _ =>
                    {
                        _context.API.ChangeQuery("oz feed");
                        return false;
                    }
                },
                new()
                {
                    Title = "Refresh Feed", SubTitle = "Too many could lead to rate limit!", IcoPath = "icon.png",
                    Action = _ =>
                    {
                        CachedFetches = new Dictionary<string, XDocument>();
                        _context.API.ShowMsg("Refreshed Plugin Cache!");
                        return false;
                    }
                }
            };
        }
    
        private List<Result> BuildFeedResults()
        {
            return new List<Result>()
            {
                new()
                {
                    Title = "Front Page", SubTitle = "Ctr Click to Open URL", IcoPath = "icon.png",
                    AutoCompleteText = "oz https://www.ozbargain.com.au",
                    Action = actionContext =>
                    {
                        bool ctrPressed = actionContext.SpecialKeyState.CtrlPressed;
                        if (ctrPressed)
                            _context.API.OpenUrl("https://www.ozbargain.com.au");
                        else
                            _context.API.ChangeQuery("oz https://www.ozbargain.com.au");
                        return false;
                    }
                },
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
                }
            };
        }
    }
}
