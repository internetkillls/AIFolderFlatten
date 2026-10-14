using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Research;

internal sealed record MethodSignal(
    string Signal,
    string Evidence,
    string Implicitness,
    string Risk
);

internal sealed record InferenceSignal(
    string Claim,
    string Type,
    string Evidence,
    string Strength,
    string Missing
);

internal sealed record FeasibilityDimension(
    string Dimension,
    double Score,
    string Reason,
    string Risk,
    string Mitigation
);

internal static class ResearchSignalAnalyzer
{
    private static readonly Regex MethodRegex = new(@"(?i)\b(purposive sampling|random sampling|survey|questionnaire|controlled trial|semi-structured interview|thematic analysis|regression analysis|grounded theory|case study)s?\b", RegexOptions.Compiled);
    private static readonly Regex ClaimRegex = new(@"(?i)\b(implies|conclude|demonstrate|show that|argue that|evidence suggests|find that)\b", RegexOptions.Compiled);

    public static List<MethodSignal> ExtractMethods(string fileId, string content)
    {
        var list = new List<MethodSignal>();
        var matches = MethodRegex.Matches(content);
        foreach (Match match in matches)
        {
            var word = match.Groups[1].Value.ToLowerInvariant();
            var lineNum = GetLineNumberAtIndex(content, match.Index);
            list.Add(new MethodSignal(
                Signal: word,
                Evidence: $"{fileId}:L{lineNum}",
                Implicitness: "explicit",
                Risk: $"Heuristic match for research method '{word}' requires validation."
            ));
        }
        return list;
    }

    public static List<InferenceSignal> ExtractInferences(string fileId, string content)
    {
        var list = new List<InferenceSignal>();
        var matches = ClaimRegex.Matches(content);
        foreach (Match match in matches)
        {
            var word = match.Value.ToLowerInvariant();
            var lineNum = GetLineNumberAtIndex(content, match.Index);
            list.Add(new InferenceSignal(
                Claim: $"Heuristic inference clue: '{word}'",
                Type: "abductive/inductive",
                Evidence: $"{fileId}:L{lineNum}",
                Strength: "medium",
                Missing: "Independent verification of claim is recommended."
            ));
        }
        return list;
    }

    public static List<FeasibilityDimension> ComputeFeasibility(double researchDensity, double codeDensity)
    {
        var list = new List<FeasibilityDimension>
        {
            new FeasibilityDimension(
                "Academic",
                Math.Min(0.5 + researchDensity * 0.1, 0.95),
                "Based on academic vocabulary density in seed files.",
                "Literature review might not be exhaustive.",
                "Generate state-of-the-art query pack."
            ),
            new FeasibilityDimension(
                "Effort",
                0.75,
                "Determined by folder size and project components.",
                "Manual extraction is time consuming.",
                "Use POST automated indexing."
            ),
            new FeasibilityDimension(
                "Technical",
                Math.Min(0.4 + codeDensity * 0.1, 0.95),
                "Based on code file presence.",
                "Depends on parser limitations.",
                "Validate using automated tests."
            ),
            new FeasibilityDimension(
                "Financial",
                0.90,
                "Leverages offline local compilation.",
                "Commercial API costs.",
                "Keep operations local-first."
            )
        };

        return list;
    }

    public static List<string> GenerateQueries(IReadOnlyList<KeyValuePair<string, int>> unigrams, IReadOnlyList<KeyValuePair<string, int>> bigrams)
    {
        var queries = new List<string>();
        var topUni = unigrams.Take(3).Select(kv => kv.Key).ToList();
        var topBi = bigrams.Take(3).Select(kv => kv.Key).ToList();

        if (topBi.Count > 0)
        {
            queries.Add($"Google Scholar query: \"{topBi[0]}\" AND (method OR hypothesis)");
            queries.Add($"OpenAlex query: concepts.id:\"{topBi[0]}\"");
        }
        if (topUni.Count > 0)
        {
            queries.Add($"Google Scholar query: \"{topUni[0]}\" state of the art");
        }
        if (topUni.Count > 1 && topBi.Count > 0)
        {
            queries.Add($"Crossref seed query: {topBi[0]} {topUni[1]}");
        }

        return queries;
    }

    private static int GetLineNumberAtIndex(string content, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n') line++;
        }
        return line;
    }
}
