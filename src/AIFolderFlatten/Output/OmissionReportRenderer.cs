using System;
using System.Linq;
using System.Text;

namespace AIFolderFlatten.Output;

internal static class OmissionReportRenderer
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Omission Report");
        sb.AppendLine();

        var sortedSkipped = package.Skipped
            .GroupBy(
                s => s.RelativePath.Replace('\\', '/'),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new SkippedFile(
                group.Key,
                string.Join("; ", group
                    .Select(s => s.Reason)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(reason => reason, StringComparer.OrdinalIgnoreCase))))
            .OrderBy(s => s.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (sortedSkipped.Count == 0)
        {
            sb.AppendLine("> All discovered files were included.");
            sb.AppendLine();
            return sb.ToString();
        }

        sb.AppendLine("The following files were discovered but omitted from the context. Their reasons for omission are listed below:");
        sb.AppendLine();
        sb.AppendLine("| Relative Path | Reason |");
        sb.AppendLine("| :--- | :--- |");

        foreach (var s in sortedSkipped)
        {
            sb.AppendLine($"| `{s.RelativePath}` | {s.Reason} |");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
