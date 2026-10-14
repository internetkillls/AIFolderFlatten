using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

public class ScannerTests : IDisposable
{
    private readonly string _tempDir;

    public ScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlattenTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch { }
    }

    [Fact]
    public void Test_Scanner_Excludes_BinaryAndReleaseFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "Program.cs"), "class Program { }");
        File.WriteAllText(Path.Combine(_tempDir, "app.exe"), "binary content");
        File.WriteAllText(Path.Combine(_tempDir, "app.pdb"), "debug symbols");
        File.WriteAllText(Path.Combine(_tempDir, "library.dll"), "library");
        File.WriteAllText(Path.Combine(_tempDir, "archive.zip"), "zip file");
        File.WriteAllText(Path.Combine(_tempDir, "project.deps.json"), "{}");
        File.WriteAllText(Path.Combine(_tempDir, "project.runtimeconfig.json"), "{}");

        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var skipped = new List<SkippedFile>();

        var candidates = FolderScanner.EnumerateCandidates(_tempDir, config, skipped);

        Assert.Single(candidates);
        Assert.Equal("Program.cs", candidates[0].RelativePath);

        Assert.Contains(skipped, s => s.RelativePath == "app.exe" && s.Reason == "release artifact excluded");
        Assert.Contains(skipped, s => s.RelativePath == "app.pdb" && s.Reason == "release artifact excluded");
        Assert.Contains(skipped, s => s.RelativePath == "library.dll" && s.Reason == "release artifact excluded");
        Assert.Contains(skipped, s => s.RelativePath == "archive.zip" && s.Reason == "release artifact excluded");
        Assert.Contains(skipped, s => s.RelativePath == "project.deps.json" && s.Reason == "release artifact excluded");
        Assert.Contains(skipped, s => s.RelativePath == "project.runtimeconfig.json" && s.Reason == "release artifact excluded");
    }

    [Fact]
    public void Test_Scanner_Excludes_ExcludeDirectoriesByDefault()
    {
        var binDir = Path.Combine(_tempDir, "bin");
        var objDir = Path.Combine(_tempDir, "obj");
        var gitDir = Path.Combine(_tempDir, ".git");
        var releaseDir = Path.Combine(_tempDir, "release");
        var publishDir = Path.Combine(_tempDir, "publish");

        Directory.CreateDirectory(binDir);
        Directory.CreateDirectory(objDir);
        Directory.CreateDirectory(gitDir);
        Directory.CreateDirectory(releaseDir);
        Directory.CreateDirectory(publishDir);

        File.WriteAllText(Path.Combine(_tempDir, "main.cs"), "class M { }");
        File.WriteAllText(Path.Combine(binDir, "skipped.cs"), "class S { }");
        File.WriteAllText(Path.Combine(releaseDir, "skipped.cs"), "class S { }");

        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var skipped = new List<SkippedFile>();

        var candidates = FolderScanner.EnumerateCandidates(_tempDir, config, skipped);

        Assert.Single(candidates);
        Assert.Equal("main.cs", candidates[0].RelativePath);
    }

    [Fact]
    public void Test_Scanner_RespectsMaxFileBytes()
    {
        File.WriteAllText(Path.Combine(_tempDir, "small.cs"), "class S { }");
        File.WriteAllText(Path.Combine(_tempDir, "large.cs"), new string('a', 100));

        var config = AppConfig.FromArgs(new[] { "--input", _tempDir, "--max-file-bytes", "50" });
        var method = typeof(Program).GetMethod("BuildPackage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
        
        var package = (ContextPackage)method.Invoke(null, new object[] { _tempDir, config })!;

        Assert.Single(package.Files);
        Assert.Equal("small.cs", package.Files[0].RelativePath);
        Assert.Contains(package.Skipped, s => s.RelativePath == "large.cs" && s.Reason.Contains("too large"));
    }

    [Fact]
    public void Test_BuildPackage_DoesNotDuplicateSkippedPathsAcrossScanners()
    {
        File.WriteAllText(Path.Combine(_tempDir, ".gitignore"), "*.tmp");
        File.WriteAllText(Path.Combine(_tempDir, "ignored.tmp"), "ignored");
        File.WriteAllText(Path.Combine(_tempDir, "main.cs"), "class M { }");

        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var method = typeof(Program).GetMethod("BuildPackage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var package = (ContextPackage)method.Invoke(null, new object[] { _tempDir, config })!;

        Assert.Single(package.Skipped, s => s.RelativePath == "ignored.tmp");
    }
}
