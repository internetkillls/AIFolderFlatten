using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Documents;

internal static class LatexLiteParser
{
    private static readonly Regex DocClassRegex = new(@"\\documentclass(?:\[.*?\])?\{(.*?)\}", RegexOptions.Compiled);
    private static readonly Regex PackageRegex = new(@"\\usepackage(?:\[.*?\])?\{(.*?)\}", RegexOptions.Compiled);
    private static readonly Regex SectionRegex = new(@"\\(section|subsection|subsubsection)(?:\[.*?\])?\{(.*?)\}", RegexOptions.Compiled);
    private static readonly Regex InlineMathRegex = new(@"\$(.*?)\$", RegexOptions.Compiled);
    private static readonly Regex BlockMathRegex = new(@"\$\$(.*?)\$\$|\\begin\{(equation|align|gather|eqnarray)\}([\s\S]*?)\\end\{\2\}", RegexOptions.Compiled);
    private static readonly Regex CiteRegex = new(@"\\cite\{(.*?)\}", RegexOptions.Compiled);
    private static readonly Regex LabelRegex = new(@"\\(label|ref)\{(.*?)\}", RegexOptions.Compiled);

    public sealed record LatexReport(
        string DocumentClass,
        IReadOnlyList<string> Packages,
        IReadOnlyList<string> Sections,
        IReadOnlyList<string> Equations,
        IReadOnlyList<string> Citations,
        IReadOnlyList<string> Labels
    );

    public static LatexReport Parse(string content)
    {
        var docClassMatch = DocClassRegex.Match(content);
        var docClass = docClassMatch.Success ? docClassMatch.Groups[1].Value : "article";

        var packages = new List<string>();
        foreach (Match match in PackageRegex.Matches(content))
        {
            var pList = match.Groups[1].Value.Split(',');
            foreach (var p in pList) packages.Add(p.Trim());
        }

        var sections = new List<string>();
        foreach (Match match in SectionRegex.Matches(content))
        {
            sections.Add($"{match.Groups[1].Value}: {match.Groups[2].Value.Trim()}");
        }

        var equations = new List<string>();
        foreach (Match match in BlockMathRegex.Matches(content))
        {
            var formula = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
            equations.Add(formula.Trim());
        }

        var citations = new List<string>();
        foreach (Match match in CiteRegex.Matches(content))
        {
            var keys = match.Groups[1].Value.Split(',');
            foreach (var key in keys) citations.Add(key.Trim());
        }

        var labels = new List<string>();
        foreach (Match match in LabelRegex.Matches(content))
        {
            labels.Add($"{match.Groups[1].Value}: {match.Groups[2].Value.Trim()}");
        }

        return new LatexReport(docClass, packages, sections, equations, citations, labels);
    }
}
