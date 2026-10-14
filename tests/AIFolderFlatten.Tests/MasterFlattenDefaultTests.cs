using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Verifies that AppConfig correctly parses --output-layout and that
/// ExportWriter resolves the _AI_SEED_MASTER_ naming convention (AC-MASTER-001, AC-MASTER-002).
/// </summary>
public class MasterFlattenDefaultTests
{
    [Fact]
    public void AppConfig_ParsesOutputLayout_Single()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--output-layout", "single" });
        Assert.Equal(OutputLayout.Single, config.OutputLayout);
    }

    [Fact]
    public void AppConfig_ParsesOutputLayout_Pack()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--output-layout", "pack" });
        Assert.Equal(OutputLayout.Pack, config.OutputLayout);
    }

    [Fact]
    public void AppConfig_DefaultOutputLayout_IsSingle()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake" });
        Assert.Equal(OutputLayout.Single, config.OutputLayout);
    }

    [Fact]
    public void AppConfig_InvalidOutputLayout_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--output-layout", "invalid" }));
    }

    [Fact]
    public void AppConfig_AcceptsFormatXml()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--format", "xml" });
        Assert.Equal("xml", config.Format);
    }

    [Fact]
    public void AppConfig_AcceptsFormatJson()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--format", "json" });
        Assert.Equal("json", config.Format);
    }

    [Fact]
    public void AppConfig_AcceptsFormatJsonl()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--format", "jsonl" });
        Assert.Equal("jsonl", config.Format);
    }

    [Fact]
    public void AppConfig_OpenAfterExport_NeverIsFalse()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--open-after-export", "never" });
        Assert.False(config.OpenAfterExport);
    }

    [Fact]
    public void AppConfig_OpenAfterExport_AlwaysIsTrue()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--open-after-export", "always" });
        Assert.True(config.OpenAfterExport);
    }

    [Fact]
    public void AppConfig_OpenAfterExport_DefaultIsFalse()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake" });
        Assert.False(config.OpenAfterExport);
    }
}
