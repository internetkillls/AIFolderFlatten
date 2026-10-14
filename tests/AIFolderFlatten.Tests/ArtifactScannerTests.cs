using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AIFolderFlatten.Artifacts;

namespace AIFolderFlatten.Tests;

public class ArtifactScannerTests : IDisposable
{
    private readonly string _tempDir;

    public ArtifactScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ArtifactScannerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Test_ArtifactScanner_RegistersUnknownExtensions()
    {
        File.WriteAllBytes(Path.Combine(_tempDir, "file.xyz"), new byte[100]); // xyz is unknown
        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var skipped = new List<SkippedFile>();

        var artifacts = ArtifactScanner.Scan(_tempDir, config, skipped, out _);

        Assert.Single(artifacts);
        Assert.Equal(".xyz", artifacts[0].Extension);
        Assert.Equal("unknown", artifacts[0].ArtifactType);
        Assert.Equal("ledger_only", artifacts[0].Status);
    }

    [Fact]
    public void Test_ArtifactClassifier_ClassifiesCorrectly()
    {
        Assert.Equal("code", ArtifactClassifier.Classify(".cs"));
        Assert.Equal("document", ArtifactClassifier.Classify(".pdf"));
        Assert.Equal("research", ArtifactClassifier.Classify(".bib"));
        Assert.Equal("modeling", ArtifactClassifier.Classify(".tex"));
        Assert.Equal("creative", ArtifactClassifier.Classify(".wav"));
        Assert.Equal("unknown", ArtifactClassifier.Classify(".xyz"));
    }
}
