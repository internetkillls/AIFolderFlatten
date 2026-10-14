using System;
using System.IO;

namespace AIFolderFlatten.Output;

internal static class OutputNamingPolicy
{
    public static (string MasterFilePath, string? PackDirectoryPath) DeterminePaths(
        string sourceRoot, 
        string sourceName, 
        string? customOutputPath, 
        OutputLayout layout, 
        string masterFormat, 
        string? gitState = null,
        bool isMultiSelect = false)
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd_HHmm");
        
        string baseName;
        if (isMultiSelect)
        {
            baseName = $"SELECTION_AI_SEED_MASTER_{timestamp}";
        }
        else if (!string.IsNullOrEmpty(gitState) && gitState != "working_folder")
        {
            baseName = $"{sourceName}_{gitState}_AI_SEED_MASTER_{timestamp}";
        }
        else
        {
            baseName = $"{sourceName}_AI_SEED_MASTER_{timestamp}";
        }

        string ext = GetExtensionForFormat(masterFormat);
        string masterFileName = $"{baseName}{ext}";

        string outputRoot = DetermineOutputRoot(sourceRoot, sourceName, customOutputPath, layout);

        if (layout == OutputLayout.Single)
        {
            return (Path.Combine(outputRoot, masterFileName), null);
        }

        string packDirName = $"{sourceName}_AI_SEED_OUT";
        if (isMultiSelect) packDirName = $"SELECTION_AI_SEED_OUT";

        string packDirectoryPath = Path.Combine(outputRoot, packDirName, "runs", timestamp);

        if (layout == OutputLayout.Pack || layout == OutputLayout.Both)
        {
            // In a pack, the master flatten is at the root of the run folder as MASTER_FLATTEN.<ext>
            string packMasterFilePath = Path.Combine(packDirectoryPath, $"MASTER_FLATTEN{ext}");
            
            if (layout == OutputLayout.Both)
            {
                // Both means a standalone master file PLUS a pack
                return (Path.Combine(outputRoot, masterFileName), packDirectoryPath);
            }
            
            return (packMasterFilePath, packDirectoryPath);
        }

        if (layout == OutputLayout.Zip)
        {
            // The master file goes inside the zip, but temporarily we write it. 
            // We'll return a temporary pack dir and a zip path.
            string zipPath = Path.Combine(outputRoot, $"{baseName}.zip");
            return (zipPath, packDirectoryPath);
        }

        return (Path.Combine(outputRoot, masterFileName), null);
    }

    private static string DetermineOutputRoot(string sourceRoot, string sourceName, string? customOutputPath, OutputLayout layout)
    {
        if (!string.IsNullOrWhiteSpace(customOutputPath) && customOutputPath != "auto")
        {
            return customOutputPath;
        }

        // Default behavior: next to source root (parent directory)
        var parentDir = Directory.GetParent(sourceRoot);
        if (parentDir != null && parentDir.Exists)
        {
            return parentDir.FullName;
        }

        // Fallback to source root if parent doesn't exist (e.g., root of drive)
        return sourceRoot;
    }

    public static string GetExtensionForFormat(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "md" => ".md",
            "txt" => ".txt",
            "xml" => ".xml",
            "json" => ".json",
            "jsonl" => ".jsonl",
            "yaml" => ".yaml",
            "html" => ".html",
            "pack" => ".md", // Default pack master to md
            "zip" => ".md",
            _ => ".md"
        };
    }
}
