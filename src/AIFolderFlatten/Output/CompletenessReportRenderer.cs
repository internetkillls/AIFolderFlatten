using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AIFolderFlatten.Output;

internal static class CompletenessReportRenderer
{
    public static string Render(ContextPackage package)
    {
        var includedCount = package.Files.Count;
        var uniqueSkipped = package.Skipped
            .GroupBy(s => s.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        var excludedDirectoryCount = uniqueSkipped.Count(IsDirectoryEntry);
        var omittedFileCount = uniqueSkipped.Count - excludedDirectoryCount;
        var limitingOmissionCount = uniqueSkipped.Count(IsCoverageLimiting);
        var policyExclusionCount = uniqueSkipped.Count - limitingOmissionCount;
        var inventoryTotal = includedCount + omittedFileCount;

        var sb = new StringBuilder();
        sb.AppendLine("## Completeness Report");
        sb.AppendLine();

        sb.AppendLine($"*   **Files Included**: `{includedCount}`");
        sb.AppendLine($"*   **Unique Omitted File Entries**: `{omittedFileCount}`");
        sb.AppendLine($"*   **Excluded Directory Entries**: `{excludedDirectoryCount}`");
        sb.AppendLine($"*   **Policy/Format Exclusions**: `{policyExclusionCount}`");
        sb.AppendLine($"*   **Coverage-Limiting Omissions**: `{limitingOmissionCount}`");

        if (inventoryTotal > 0)
        {
            var ratio = (double)includedCount / inventoryTotal * 100;
            sb.AppendLine(
                $"*   **Raw Inventory Inclusion Ratio**: `{ratio.ToString("F1", CultureInfo.InvariantCulture)}%` (diagnostic only)");
        }

        sb.AppendLine();
        if (limitingOmissionCount == 0)
        {
            sb.AppendLine("> No files were omitted because of context budget, file-size limits, or read/access failures.");
        }
        else
        {
            sb.AppendLine($"> `{limitingOmissionCount}` omission(s) may reduce usable context because of limits or read/access failures.");
        }
        sb.AppendLine("> Do not infer repository patch-readiness from the raw inventory ratio alone. Review omission reasons: unsupported formats, ignored files, generated outputs, binaries, and excluded directories may be intentional.");
        sb.AppendLine();
        return sb.ToString();
    }

    private static bool IsDirectoryEntry(SkippedFile skipped)
    {
        return skipped.RelativePath.EndsWith("/", StringComparison.Ordinal)
            || skipped.Reason.Contains("directory", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCoverageLimiting(SkippedFile skipped)
    {
        return skipped.Reason.Contains("context budget overflow", StringComparison.OrdinalIgnoreCase)
            || skipped.Reason.Contains("too large:", StringComparison.OrdinalIgnoreCase)
            || skipped.Reason.Contains("access/read error", StringComparison.OrdinalIgnoreCase)
            || skipped.Reason.Contains("access denied", StringComparison.OrdinalIgnoreCase)
            || skipped.Reason.Contains("I/O error", StringComparison.OrdinalIgnoreCase);
    }
}
