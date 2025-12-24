using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.OzBargain.Services;

/// <summary>
/// Helper class to parse RSS.
/// </summary>
public class RSSHelper
{
    /// <summary>
    /// Parse RSS XML into List of Results.
    /// </summary>
    public static List<Result> ParseRSS(XDocument doc, PluginInitContext _context)
    {
        List<Result> results = new List<Result>();
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