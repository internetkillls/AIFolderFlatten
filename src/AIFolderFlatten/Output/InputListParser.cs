using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIFolderFlatten.Output;

internal static class InputListParser
{
    public static List<string> Parse(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput)) return new List<string>();

        var separators = new[] { "|", ";;", ";" };
        string[] rawPaths;

        if (rawInput.Contains("|"))
        {
            rawPaths = rawInput.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }
        else if (rawInput.Contains(";;"))
        {
            rawPaths = rawInput.Split(new[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);
        }
        else if (rawInput.Contains(";"))
        {
            rawPaths = rawInput.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            rawPaths = new[] { rawInput };
        }

        var result = new List<string>();
        foreach (var rp in rawPaths)
        {
            var trimmed = rp.Trim(' ', '"', '\'', '\r', '\n');
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                result.Add(trimmed);
            }
        }
        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
