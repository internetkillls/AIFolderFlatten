using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Documents;

internal sealed record CitationEntry(
    string Key,
    string Type,
    string Title,
    string Author,
    string Year
);

internal static class CitationLiteParser
{
    private static readonly Regex BibEntryRegex = new(@"@(\w+)\s*\{\s*([^,\s]+)\s*,([\s\S]*?)(?=\n@|\Z)", RegexOptions.Compiled);
    private static readonly Regex BibFieldRegex = new(@"\b(title|author|year|journal|booktitle)\s*=\s*(?:\{([\s\S]*?)\}|""([\s\S]*?)""|([^,\n]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<CitationEntry> ParseBibTeX(string content)
    {
        var entries = new List<CitationEntry>();
        try
        {
            var matches = BibEntryRegex.Matches(content);
            foreach (Match match in matches)
            {
                var type = match.Groups[1].Value.Trim();
                var key = match.Groups[2].Value.Trim();
                var fieldsBody = match.Groups[3].Value;

                string title = "Unknown Title";
                string author = "Unknown Author";
                string year = "Unknown Year";

                var fieldMatches = BibFieldRegex.Matches(fieldsBody);
                foreach (Match fm in fieldMatches)
                {
                    var name = fm.Groups[1].Value.ToLowerInvariant();
                    var val = fm.Groups[2].Success ? fm.Groups[2].Value :
                              fm.Groups[3].Success ? fm.Groups[3].Value : fm.Groups[4].Value;
                    val = CleanBibValue(val);

                    if (name == "title") title = val;
                    else if (name == "author") author = val;
                    else if (name == "year") year = val;
                }

                entries.Add(new CitationEntry(key, type, title, author, year));
            }
        }
        catch { }
        return entries;
    }

    public static List<CitationEntry> ParseRIS(string content)
    {
        var entries = new List<CitationEntry>();
        try
        {
            var lines = content.Split('\n');
            string type = "";
            string key = "";
            string title = "";
            List<string> authors = new();
            string year = "";

            int entryIdx = 1;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("ER  -"))
                {
                    // End of Record, save entry
                    if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(type))
                    {
                        key = $"R{entryIdx++:0000}";
                        entries.Add(new CitationEntry(
                            Key: key,
                            Type: type,
                            Title: string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                            Author: authors.Count > 0 ? string.Join("; ", authors) : "Unknown Author",
                            Year: string.IsNullOrWhiteSpace(year) ? "Unknown Year" : year
                        ));
                    }
                    type = "";
                    title = "";
                    authors.Clear();
                    year = "";
                }
                else if (line.Contains("  - "))
                {
                    var idx = line.IndexOf("  - ");
                    var tag = line[..idx].Trim();
                    var val = line[(idx + 4)..].Trim();

                    if (tag == "TY") type = val;
                    else if (tag == "TI" || tag == "T1") title = val;
                    else if (tag == "AU" || tag == "A1") authors.Add(val);
                    else if (tag == "PY" || tag == "Y1" || tag == "DA")
                    {
                        // Extract year
                        year = val.Length >= 4 ? val[..4] : val;
                    }
                }
            }
        }
        catch { }
        return entries;
    }

    private static string CleanBibValue(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return "";
        val = val.Trim();
        // Remove enclosing brackets or braces if present
        val = Regex.Replace(val, @"^[\{\""\']|[\}\""\']$", "");
        // Clean double spaces
        val = Regex.Replace(val, @"\s+", " ");
        return val.Trim();
    }
}
