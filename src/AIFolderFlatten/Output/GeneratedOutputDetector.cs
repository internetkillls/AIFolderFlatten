using System;
using System.IO;

namespace AIFolderFlatten.Output;

internal static class GeneratedOutputDetector
{
    public static bool IsGeneratedOutput(string fullPath, bool isDirectory)
    {
        var name = Path.GetFileName(fullPath);

        // Pattern: *_AI_SEED_OUT/
        // Pattern: *_AI_SEED_*/
        if (isDirectory)
        {
            if (name.Contains("_AI_SEED_OUT", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Contains("_AI_SEED_", StringComparison.OrdinalIgnoreCase) && !name.Contains("MASTER", StringComparison.OrdinalIgnoreCase)) return true;
            
            // Marker file check
            if (File.Exists(Path.Combine(fullPath, ".ai-seed-output.json"))) return true;
            
            return false;
        }

        // Files
        if (name.Equals(".ai-seed-output.json", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Contains("_AI_SEED_MASTER_", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_overview.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_debug.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_architecture.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_obsidian.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_research.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.EndsWith("_prompt-pack.md", StringComparison.OrdinalIgnoreCase)) return true;

        // If inside a generated output folder, we might not need to check files if the folder was excluded.
        // But in case the folder exclusion was bypassed or missed:
        if (fullPath.Contains("_AI_SEED_OUT", StringComparison.OrdinalIgnoreCase) || 
            (fullPath.Contains("_AI_SEED_", StringComparison.OrdinalIgnoreCase) && fullPath.Contains("_pack", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }
}
