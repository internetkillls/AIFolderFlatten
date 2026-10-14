using System;
using System.Collections.Generic;
using System.Linq;

namespace AIFolderFlatten;

internal static class BudgetSelector
{
    public static IReadOnlyList<(string RelativePath, long Bytes, DateTime ModifiedUtc, string Content, FileAnalysis Analysis, IReadOnlyDictionary<string, int> Redactions)> SelectByBudget(
        IReadOnlyList<(string RelativePath, long Bytes, DateTime ModifiedUtc, string Content, FileAnalysis Analysis, IReadOnlyDictionary<string, int> Redactions)> raw,
        List<SkippedFile> skipped,
        AppConfig config)
    {
        if (config.MaxTotalChars == 0) return raw.ToList();

        var selected = new List<(string RelativePath, long Bytes, DateTime ModifiedUtc, string Content, FileAnalysis Analysis, IReadOnlyDictionary<string, int> Redactions)>();
        long total = 0;
        foreach (var item in raw.OrderByDescending(x => x.Analysis.PriorityScore).ThenBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var cost = item.Content.Length;
            if (total + cost > config.MaxTotalChars)
            {
                skipped.Add(new SkippedFile(item.RelativePath, $"context budget overflow: {total + cost} chars > {config.MaxTotalChars} chars"));
                continue;
            }
            selected.Add(item);
            total += cost;
        }

        return selected;
    }
}
