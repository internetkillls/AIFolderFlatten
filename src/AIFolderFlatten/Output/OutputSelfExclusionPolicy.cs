using System;
using System.IO;

namespace AIFolderFlatten.Output;

internal static class OutputSelfExclusionPolicy
{
    public static void ValidateAndExclude(string inputDir, string outputDir, AppConfig config, out string? warning)
    {
        warning = null;
        
        var inFull = Path.GetFullPath(inputDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var outFull = Path.GetFullPath(outputDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (outFull.StartsWith(inFull, StringComparison.OrdinalIgnoreCase))
        {
            if (!config.AllowOutputInsideInput)
            {
                throw new AIFolderFlattenException($"Output directory '{outFull}' is inside the input directory '{inFull}'. This is unsafe. Use --allow-output-inside-input true to force.", ExitCodes.ArgumentError);
            }
            
            warning = $"WARNING: Output directory was forced inside the input directory. The output path '{outFull}' has been dynamically excluded from this scan to prevent recursive pollution.";
            
            // Add to runtime exclusion
            var relativeOut = Path.GetRelativePath(inFull, outFull).Replace('\\', '/');
            config.ExcludeDirs.Add(relativeOut);
        }
    }
}
