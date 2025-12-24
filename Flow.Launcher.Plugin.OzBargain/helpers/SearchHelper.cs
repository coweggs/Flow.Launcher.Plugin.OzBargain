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
        HtmlNode SearchResults = doc.DocumentNode.SelectSingleNode("//body/main/div/div/dl");
        foreach(HtmlNode node in SearchResults.SelectNodes("dt"))
        {
            string title = node.Element("a").GetDirectInnerText();
			title = WebUtility.HtmlDecode(title);
            string expiry = "Expiry Date Unknown";
            if (node.Element("span") != null)
                expiry = node.Element("span").InnerText.ToLower() == "expired" ? "EXPIRED" : "Not Expired";
            string icoPath = node.Element("div").Element("a").Element("img").GetAttributeValue("src", "");
            string link = "https://www.ozbargain.com" + node.Element("div").Element("a").GetAttributeValue("href", "");
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

    public static HtmlDocument FetchDoc(string query)
    {
        web = web ?? new HtmlWeb();
        string encoded = Uri.EscapeDataString(query);
        return web.Load("https://www.ozbargain.com.au/search/node/" + encoded);
    }
}
