using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using AIFolderFlatten.Artifacts;
using AIFolderFlatten.Post;

namespace AIFolderFlatten.Output;

internal static class MasterFlattenWriter
{
    public static ExportResult Write(
        string inputDir, 
        ContextPackage package, 
        AppConfig config, 
        OrientationState? orientation, 
        List<ArtifactIR> artifacts,
        string domain,
        string lens)
    {
        // Compute paths
        var paths = OutputNamingPolicy.DeterminePaths(
            inputDir, 
            package.RootName, 
            config.OutputPath, 
            config.OutputLayout, 
            config.MasterFormat,
            package.Git?.Mode,
            config.IsMultiSelect
        );

        var outputPaths = new List<string>();
        long totalCharacters = 0;

        // Render master file content based on format
        string masterContent = string.Empty;
        var format = config.MasterFormat.ToLowerInvariant();
        
        if (format == "xml") masterContent = XmlRenderer.Render(package);
        else if (format == "json") masterContent = JsonRenderer.Render(package);
        else if (format == "jsonl") masterContent = JsonlRenderer.Render(package);
        else if (format == "yaml" || format == "yml") masterContent = YamlRenderer.Render(package);
        else if (format == "html" || format == "htm") masterContent = HtmlPreviewRenderer.Render(package);
        else masterContent = MasterFlattenRenderer.RenderMarkdown(package, config, orientation, domain);

        // Append prompt starter footer if requested
        var starterFooter = PromptStarterLibrary.Resolve(config.PromptStarter, inputDir);
        if (starterFooter != null)
            masterContent = masterContent.TrimEnd() + "\n\n" + starterFooter.TrimStart();

        // Write standalone master file if layout is Single or Both
        if (config.OutputLayout == OutputLayout.Single || config.OutputLayout == OutputLayout.Both)
        {
            EnsureParentDirectory(paths.MasterFilePath);
            File.WriteAllText(paths.MasterFilePath, masterContent, Utf8NoBom());
            outputPaths.Add(paths.MasterFilePath);
            totalCharacters += masterContent.Length;
        }

        // If Pack, Both, or Zip, write the pack folder (temporarily for zip)
        if (config.OutputLayout == OutputLayout.Pack || config.OutputLayout == OutputLayout.Both || config.OutputLayout == OutputLayout.Zip)
        {
            var packDir = paths.PackDirectoryPath!;
            Directory.CreateDirectory(packDir);
            
            // If Pack or Zip (not Both), write the master file inside the pack
            if (config.OutputLayout == OutputLayout.Pack || config.OutputLayout == OutputLayout.Zip)
            {
                var packMasterPath = config.OutputLayout == OutputLayout.Zip ? Path.Combine(packDir, Path.GetFileName(paths.MasterFilePath)) : paths.MasterFilePath;
                File.WriteAllText(packMasterPath, masterContent, Utf8NoBom());
                if (config.OutputLayout != OutputLayout.Zip) outputPaths.Add(packMasterPath);
                totalCharacters += masterContent.Length;
                
                // Write Sidecars
                ExportWriter.WritePackSidecars(packDir, inputDir, package, config, orientation, artifacts, domain, lens, packMasterPath);
            }
            else
            {
                // Layout Both
                ExportWriter.WritePackSidecars(packDir, inputDir, package, config, orientation, artifacts, domain, lens, paths.MasterFilePath);
            }
            
            // Write marker to pack dir
            OutputMarker.WriteMarker(packDir, ExportWriter.CalculateSHA256String(inputDir));

            if (config.OutputLayout != OutputLayout.Zip)
            {
                outputPaths.Add(packDir);
            }
        }

        // Zip logic
        if (config.OutputLayout == OutputLayout.Zip)
        {
            var packDir = paths.PackDirectoryPath!;
            var zipPath = paths.MasterFilePath; // Naming policy returns zip path here
            
            EnsureParentDirectory(zipPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            
            System.IO.Compression.ZipFile.CreateFromDirectory(packDir, zipPath, System.IO.Compression.CompressionLevel.Fastest, true);
            Directory.Delete(packDir, true);
            
            outputPaths.Add(zipPath);
        }

        return new ExportResult(outputPaths, package.Files.Count, package.Skipped.Count, (int)totalCharacters);
    }

    private static void EnsureParentDirectory(string path)
    {
        var parent = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(parent)) Directory.CreateDirectory(parent);
    }

    private static Encoding Utf8NoBom() => new UTF8Encoding(false);
}
