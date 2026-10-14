using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Verifies that FolderScanner skips previously generated AI Seeder outputs (AC-MASTER-006).
/// </summary>
public class ReflattenSkipsGeneratedOutputTests
{
    private AppConfig MakeConfig(string inputDir) =>
        AppConfig.FromArgs(new[] { "--input", inputDir, "--format", "md" });

    [Fact]
    public void Scanner_Skips_AiSeedMasterMarkdownFile()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_reflatten_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            // Real source file
            File.WriteAllText(Path.Combine(tmp, "source.cs"), "class A {}");
            // Previously generated master file — must be skipped
            var generatedMaster = Path.Combine(tmp, "MyProject_AI_SEED_MASTER_20240101_120000.md");
            File.WriteAllText(generatedMaster, "# Previous output");

            var config = MakeConfig(tmp);
            var skipped = new List<SkippedFile>();
            var candidates = FolderScanner.EnumerateCandidates(tmp, config, skipped);

            var candidateNames = candidates.Select(c => Path.GetFileName(c.AbsolutePath)).ToList();
            Assert.DoesNotContain("MyProject_AI_SEED_MASTER_20240101_120000.md", candidateNames);

            var skipReasons = skipped.Select(s => s.Reason).ToList();
            Assert.Contains("skipped_generated_output", skipReasons);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void Scanner_Skips_AiSeedMasterTxtFile()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_reflatten_txt_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "readme.md"), "# Test");
            File.WriteAllText(Path.Combine(tmp, "Folder_AI_SEED_MASTER_20240101_120000.txt"), "Previous TXT output");

            var config = MakeConfig(tmp);
            var skipped = new List<SkippedFile>();
            var candidates = FolderScanner.EnumerateCandidates(tmp, config, skipped);

            Assert.DoesNotContain(candidates, c => c.RelativePath.Contains("_AI_SEED_MASTER_"));
            Assert.Contains(skipped, s => s.Reason == "skipped_generated_output");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void Scanner_Skips_AiSeedPackDirectory()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_reflatten_dir_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "main.py"), "print('hello')");
            // Previously generated pack directory
            var packDir = Path.Combine(tmp, "MyFolder_AI_SEED_20240101_120000");
            Directory.CreateDirectory(packDir);
            File.WriteAllText(Path.Combine(packDir, "MASTER_FLATTEN.md"), "# Old output");
            File.WriteAllText(Path.Combine(packDir, "CONTEXT.md"), "# Old context");

            var config = MakeConfig(tmp);
            var skipped = new List<SkippedFile>();
            var candidates = FolderScanner.EnumerateCandidates(tmp, config, skipped);

            // MASTER_FLATTEN.md from inside the pack dir should NOT appear in candidates
            Assert.DoesNotContain(candidates, c => c.RelativePath.Contains("_AI_SEED_"));

            // The pack directory itself must be in the skipped list
            Assert.Contains(skipped, s =>
                s.Reason == "skipped_generated_output" &&
                s.RelativePath.Contains("_AI_SEED_"));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void Scanner_Skips_AiSeedOutputJson()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_reflatten_json_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "app.js"), "console.log('hi')");
            File.WriteAllText(Path.Combine(tmp, ".ai-seed-output.json"), "{}");

            var config = MakeConfig(tmp);
            var skipped = new List<SkippedFile>();
            FolderScanner.EnumerateCandidates(tmp, config, skipped);

            Assert.Contains(skipped, s =>
                s.RelativePath.EndsWith(".ai-seed-output.json") &&
                s.Reason == "skipped_generated_output");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void Scanner_DoesNotSkip_NormalFiles()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "aiff_normal_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tmp);
        try
        {
            File.WriteAllText(Path.Combine(tmp, "main.cs"), "class App {}");
            File.WriteAllText(Path.Combine(tmp, "notes.md"), "# Notes");

            var config = MakeConfig(tmp);
            var skipped = new List<SkippedFile>();
            var candidates = FolderScanner.EnumerateCandidates(tmp, config, skipped);

            Assert.Contains(candidates, c => c.RelativePath == "main.cs");
            Assert.Contains(candidates, c => c.RelativePath == "notes.md");
            Assert.DoesNotContain(skipped, s => s.Reason == "skipped_generated_output");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
