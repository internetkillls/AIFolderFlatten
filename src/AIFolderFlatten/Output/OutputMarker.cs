using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AIFolderFlatten.Output;

internal static class OutputMarker
{
    public static void WriteMarker(string outputRootDirectory, string sourceRootHash)
    {
        var markerPath = Path.Combine(outputRootDirectory, ".ai-seed-output.json");
        
        var markerObj = new
        {
            kind = "ai-seed-output",
            tool = "AIFolderFlatten",
            version = Program.AppVersion,
            sourceRootHash = sourceRootHash,
            createdAt = DateTimeOffset.Now.ToString("o"),
            runId = Guid.NewGuid().ToString("N"),
            doNotIngestByDefault = true
        };

        var json = JsonSerializer.Serialize(markerObj, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(markerPath, json, new UTF8Encoding(false));
    }
}
