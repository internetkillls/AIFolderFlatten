using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Documents;

internal static class JupyterNotebookParser
{
    internal sealed record NotebookReport(
        int MarkdownCells,
        int CodeCells,
        IReadOnlyList<string> Headings,
        IReadOnlyList<string> Imports,
        IReadOnlyList<string> EquationCandidates);

    public static NotebookReport Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var markdownCells = 0;
        var codeCells = 0;
        var headings = new List<string>();
        var imports = new List<string>();
        var equations = new List<string>();

        if (!document.RootElement.TryGetProperty("cells", out var cells) ||
            cells.ValueKind != JsonValueKind.Array)
        {
            return new NotebookReport(0, 0, headings, imports, equations);
        }

        foreach (var cell in cells.EnumerateArray())
        {
            var cellType = cell.TryGetProperty("cell_type", out var typeProp)
                ? typeProp.GetString()
                : null;
            var source = ReadSource(cell);

            if (string.Equals(cellType, "markdown", StringComparison.OrdinalIgnoreCase))
            {
                markdownCells++;
                headings.AddRange(
                    source.Split('\n')
                        .Select(line => line.Trim())
                        .Where(line => Regex.IsMatch(line, @"^#{1,6}\s+\S"))
                        .Select(line => line.TrimStart('#', ' ')));

                equations.AddRange(
                    Regex.Matches(source, @"\$\$(?<equation>.+?)\$\$", RegexOptions.Singleline)
                        .Select(match => match.Groups["equation"].Value.Trim())
                        .Where(value => value.Length > 0));
            }
            else if (string.Equals(cellType, "code", StringComparison.OrdinalIgnoreCase))
            {
                codeCells++;
                imports.AddRange(
                    source.Split('\n')
                        .Select(line => line.Trim())
                        .Where(line => Regex.IsMatch(line, @"^(import\s+\S+|from\s+\S+\s+import\s+)")));
            }
        }

        return new NotebookReport(
            markdownCells,
            codeCells,
            headings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            imports.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            equations.Distinct(StringComparer.Ordinal).ToList());
    }

    private static string ReadSource(JsonElement cell)
    {
        if (!cell.TryGetProperty("source", out var source))
        {
            return string.Empty;
        }

        return source.ValueKind switch
        {
            JsonValueKind.Array => string.Concat(
                source.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString())),
            JsonValueKind.String => source.GetString() ?? string.Empty,
            _ => string.Empty
        };
    }
}
