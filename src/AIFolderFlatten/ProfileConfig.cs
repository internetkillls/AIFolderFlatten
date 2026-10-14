using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AIFolderFlatten;

internal static class ProfileConfig
{
    public static Dictionary<string, string> LoadFromFile(string folderPath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return result;
        }

        var configPath = Path.Combine(folderPath, ".ai-flatten.json");
        if (!File.Exists(configPath))
        {
            return result;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Number => prop.Value.GetRawText(),
                        JsonValueKind.Null => null,
                        _ => prop.Value.GetRawText()
                    };

                    if (value != null)
                    {
                        result[prop.Name] = value;
                    }
                }
            }
        }
        catch
        {
            // Fail-silent, config loading error is skipped, or default is used.
        }

        return result;
    }
}
