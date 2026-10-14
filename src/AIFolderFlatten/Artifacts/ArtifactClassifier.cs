using System;
using System.IO;

namespace AIFolderFlatten.Artifacts;

internal static class ArtifactClassifier
{
    public static string Classify(string extension)
    {
        var ext = extension.ToLowerInvariant().Trim();
        if (string.IsNullOrWhiteSpace(ext)) return "unknown";

        switch (ext)
        {
            case ".cs":
            case ".csproj":
            case ".sln":
            case ".py":
            case ".js":
            case ".jsx":
            case ".ts":
            case ".tsx":
            case ".java":
            case ".kt":
            case ".kts":
            case ".swift":
            case ".cpp":
            case ".c":
            case ".h":
            case ".go":
            case ".rs":
            case ".php":
            case ".rb":
            case ".ps1":
            case ".sh":
            case ".gitignore":
            case ".aiignore":
            case ".json":
            case ".jsonl":
            case ".yaml":
            case ".yml":
            case ".toml":
            case ".xml":
            case ".sql":
                return "code";

            case ".pdf":
            case ".epub":
            case ".mobi":
            case ".azw":
            case ".azw3":
            case ".docx":
            case ".odt":
            case ".html":
            case ".htm":
            case ".ipynb":
                return "document";

            case ".bib":
            case ".ris":
            case ".csl.json":
            case ".rdf":
                return "research";

            case ".tex":
            case ".nb":
            case ".wl":
            case ".wls":
            case ".xlsx":
            case ".xls":
            case ".parquet":
            case ".feather":
            case ".h5":
            case ".hdf5":
            case ".mat":
                return "modeling";

            case ".wav":
            case ".mp3":
            case ".flac":
            case ".mp4":
            case ".mov":
            case ".webm":
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".webp":
            case ".svg":
            case ".blend":
            case ".pd":
            case ".scd":
                return "creative";

            default:
                return "unknown";
        }
    }
}
