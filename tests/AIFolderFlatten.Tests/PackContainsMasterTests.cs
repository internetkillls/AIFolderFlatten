using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Verifies that pack output contains MASTER_FLATTEN.md as the first artifact (AC-MASTER-003).
/// </summary>
public class PackContainsMasterTests
{
    [Fact]
    public void PackOutput_ContainsMasterFlattenMd()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_pack_test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "hello.txt"), "Hello world");
            File.WriteAllText(Path.Combine(tmp, "readme.md"), "# Test");

            var config = AppConfig.FromArgs(new[]
            {
                "--input", tmp,
                "--format", "pack",
                "--output-layout", "pack",
                "--mode", "overview"
            });

            var package = BuildMinimalPackage(tmp, config);
            var result = ExportWriter.WriteOutputPost(
                tmp, package, config, null,
                new List<Artifacts.ArtifactIR>(), "code", "overview");

            // Find the pack directory (the directory containing MASTER_FLATTEN.md)
            var masterPath = result.OutputPaths.FirstOrDefault(p =>
                Path.GetFileName(p).Equals("MASTER_FLATTEN.md", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(masterPath);

            var outDir = Path.GetDirectoryName(masterPath)!;
            Assert.True(Directory.Exists(outDir), $"Output directory not found: {outDir}");

            // MASTER_FLATTEN.md must be present
            Assert.True(File.Exists(masterPath), $"MASTER_FLATTEN.md not found in pack: {outDir}");

            // MASTER_FLATTEN.md must be non-empty
            Assert.True(new FileInfo(masterPath).Length > 0, "MASTER_FLATTEN.md is empty");

            // MASTER_FLATTEN.md must be the first OutputPath entry (before the pack dir)
            var paths = result.OutputPaths.ToList();
            Assert.Equal(0, paths.IndexOf(masterPath));
        }
        finally
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void PackOutputDirectory_UsesAiSeedNaming()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_naming_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "file.cs"), "class A {}");
            var config = AppConfig.FromArgs(new[]
            {
                "--input", tmp,
                "--format", "pack",
                "--output-layout", "pack"
            });
            var package = BuildMinimalPackage(tmp, config);
            var result = ExportWriter.WriteOutputPost(tmp, package, config, null,
                new List<Artifacts.ArtifactIR>(), "code", "overview");

            // Pack dir path contains _AI_SEED_ somewhere in its hierarchy
            var packPath = result.OutputPaths.Last(); // pack dir is added last
            Assert.Contains("_AI_SEED_", packPath);
        }
        finally
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void SingleFileOutput_UsesAiSeedMasterNaming()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_master_naming_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "notes.md"), "# Notes");
            var config = AppConfig.FromArgs(new[]
            {
                "--input", tmp,
                "--format", "md",
                "--output-layout", "single"
            });
            var package = BuildMinimalPackage(tmp, config);
            var result = ExportWriter.WriteOutputPost(tmp, package, config, null,
                new List<Artifacts.ArtifactIR>(), "code", "overview");

            Assert.Single(result.OutputPaths);
            var outFile = result.OutputPaths[0];
            Assert.Contains("_AI_SEED_MASTER_", Path.GetFileName(outFile));
            Assert.EndsWith(".md", outFile, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void MultiSelectSingleFileOutput_UsesSelectionNameAndWritesOneFile()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_multi_master_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "one.md"), "# One");
            File.WriteAllText(Path.Combine(tmp, "two.md"), "# Two");
            var config = AppConfig.FromArgs(new[]
            {
                "--input", tmp,
                "--format", "md",
                "--output-layout", "single"
            });
            config.IsMultiSelect = true;

            var package = BuildMinimalPackage(tmp, config);
            var result = ExportWriter.WriteOutputPost(tmp, package, config, null,
                new List<Artifacts.ArtifactIR>(), "code", "overview");

            Assert.Single(result.OutputPaths);
            Assert.StartsWith("SELECTION_AI_SEED_MASTER_", Path.GetFileName(result.OutputPaths[0]));
        }
        finally
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, recursive: true);
        }
    }

    private static ContextPackage BuildMinimalPackage(string inputDir, AppConfig config)
    {
        return new ContextPackage(
            SourceRoot: inputDir,
            RootName: new DirectoryInfo(inputDir).Name,
            Mode: config.Mode,
            Format: config.Format,
            ContentLevel: "full",
            GeneratedAt: DateTimeOffset.UtcNow,
            Files: Array.Empty<FileSnapshot>(),
            Skipped: Array.Empty<SkippedFile>(),
            ProjectSignals: Array.Empty<string>(),
            SuggestedCommands: Array.Empty<string>(),
            RedactionEnabled: false,
            LineNumbersEnabled: false,
            PromptInjectionScanEnabled: false,
            MaxTotalChars: 2_000_000,
            SplitSizeChars: 300_000,
            AppVersion: Program.AppVersion,
            Git: null
        );
    }
}
