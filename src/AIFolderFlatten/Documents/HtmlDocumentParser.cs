using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Documents;

internal static class HtmlDocumentParser
{
    private static readonly Regex TitleRegex = new(@"<title\b[^>]*>(.*?)</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex MetaDescRegex = new(@"<meta\b[^>]*?\bname=[""']description[""'][^>]*?\bcontent=[""']([^""']*)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HeadingRegex = new(@"<(h[1-6])\b[^>]*>(.*?)</\1>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex LinkRegex = new(@"<a\b[^>]*?\bhref=[""']([^""']*)[""'][^>]*>(.*?)</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex ScriptRegex = new(@"<script\b[^>]*>([\s\S]*?)</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StyleRegex = new(@"<style\b[^>]*>([\s\S]*?)</style>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    public sealed record HtmlReport(
        string Title,
        string Description,
        IReadOnlyList<string> Headings,
        IReadOnlyList<string> Links,
        string TextPreview
    );

    public static HtmlReport Parse(string content)
    {
        var titleMatch = TitleRegex.Match(content);
        var title = titleMatch.Success ? titleMatch.Groups[1].Value.Trim() : "Untitled HTML Document";

        var descMatch = MetaDescRegex.Match(content);
        var desc = descMatch.Success ? descMatch.Groups[1].Value.Trim() : "No meta description available.";

        var headings = new List<string>();
        var headingMatches = HeadingRegex.Matches(content);
        foreach (Match match in headingMatches)
        {
            var cleanText = TagRegex.Replace(match.Groups[2].Value, "").Trim();
            headings.Add($"{match.Groups[1].Value.ToUpper()}: {cleanText}");
        }

        var links = new List<string>();
        var linkMatches = LinkRegex.Matches(content);
        foreach (Match match in linkMatches)
        {
            var href = match.Groups[1].Value.Trim();
            var text = TagRegex.Replace(match.Groups[2].Value, "").Trim();
            links.Add($"[{text}]({href})");
        }

        // Clean text preview: strip scripts, styles, then tags
        var cleaned = ScriptRegex.Replace(content, "");
        cleaned = StyleRegex.Replace(cleaned, "");
        cleaned = TagRegex.Replace(cleaned, " ");
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        var preview = cleaned.Length > 2000 ? cleaned[..2000] + "..." : cleaned;

        return new HtmlReport(title, desc, headings, links, preview);
    }
}
