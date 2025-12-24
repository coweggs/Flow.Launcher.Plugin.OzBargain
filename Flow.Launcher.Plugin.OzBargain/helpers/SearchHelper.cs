using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;
using System;

namespace Flow.Launcher.Plugin.OzBargain.Services;

public class SearchHelper
{
    private static HtmlWeb web;

    public static List<Result> ParseDoc(HtmlDocument doc, PluginInitContext _context)
    {
        List<Result> results = new List<Result>();
        HtmlNodeCollection haystack = doc.DocumentNode.SelectNodes("//body/main/div/div/dl");
        HtmlNode SearchResults = haystack[0]; // the needle
        foreach(HtmlNode n in haystack)
        {
            if (n.HasClass("search-results"))
            {
                SearchResults = n;
                break;
            }
        }

        foreach(HtmlNode node in SearchResults.SelectNodes("dt"))
        {
            string title = node.Element("a").InnerText;
			title = WebUtility.HtmlDecode(title);
            string expiry = "Expiry Date Unknown";
            if (node.Element("span") != null)
                expiry = node.Element("span").InnerText.ToLower() == "expired" ? "EXPIRED" : $"[{node.Element("span").InnerText}]";
            string icoPath = node.Element("div")?.Element("a")?.Element("img")?.GetAttributeValue("src", "") ?? "";
            string link = "https://www.ozbargain.com" + node.Element("div")?.Element("a")?.GetAttributeValue("href", "") ?? "";
            results.Add(new()
                {
                    Title = title,
                    SubTitle = expiry,
                    IcoPath = icoPath,
                    Action = _ =>
                    {
                        _context.API.OpenUrl(link);
                        return true;
                    }
                });
        }

        return results;
    }

    public static HtmlDocument FetchDoc(string query, bool showExpired)
    {
        web = web ?? new HtmlWeb();
        string encoded = Uri.EscapeDataString(query + (showExpired ? "" : " option:noexpired"));
        return web.Load("https://www.ozbargain.com.au/search/node/" + encoded);
    }
}
