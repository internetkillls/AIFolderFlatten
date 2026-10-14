using System;
using System.Collections.Generic;
using Xunit;

namespace AIFolderFlatten.Tests;

public class RendererTests
{
    private ContextPackage CreateTestPackage(string content1, string content2, long splitSize = 300_000)
    {
        var files = new List<FileSnapshot>
        {
            new FileSnapshot("F0001", "src/file1.cs", content1.Length, DateTime.UtcNow, content1,
                new FileAnalysis("source", "csharp", "sha1", 10, 50, new[]{"class C1"}, new[]{"System"}, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
                new Dictionary<string, int>()),
            new FileSnapshot("F0002", "src/file2.cs", content2.Length, DateTime.UtcNow, content2,
                new FileAnalysis("source", "csharp", "sha2", 20, 60, new[]{"class C2"}, new[]{"System.IO"}, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
                new Dictionary<string, int>())
        };

        return new ContextPackage(
            SourceRoot: ".",
            RootName: "test_project",
            Mode: "overview",
            Format: "md",
            ContentLevel: "full",
            GeneratedAt: DateTimeOffset.UtcNow,
            Files: files,
            Skipped: Array.Empty<SkippedFile>(),
            ProjectSignals: new[] { "C# project found" },
            SuggestedCommands: new[] { "dotnet build" },
            RedactionEnabled: false,
            LineNumbersEnabled: false,
            PromptInjectionScanEnabled: false,
            MaxTotalChars: 1000000,
            SplitSizeChars: splitSize,
            AppVersion: "0.2.1"
        );
    }

    [Fact]
    public void Test_RenderSingle_ContainsBoundaryAnchors()
    {
        var package = CreateTestPackage("content of file 1", "content of file 2");

        var rendered = MarkdownRenderer.RenderSingle(package);

        Assert.Contains("<!-- BEGIN_FILE id=\"F0001\" path=\"src/file1.cs\"", rendered);
        Assert.Contains("content of file 1", rendered);
        Assert.Contains("<!-- END_FILE id=\"F0001\" -->", rendered);
    }

    [Fact]
    public void Test_RenderSplit_SplitsCorrectlyBasedOnSplitSize()
    {
        var content1 = new string('a', 60_000);
        var content2 = new string('b', 60_000);
        var package = CreateTestPackage(content1, content2, splitSize: 50_000);

        var parts = MarkdownRenderer.RenderSplit(package, "base");

        Assert.Equal(2, parts.Count);
        Assert.Contains("part_001", parts[0].FileName);
        Assert.Contains("part_002", parts[1].FileName);
    }

    [Fact]
    public void Test_RenderPack_CreatesAllHandoffFiles()
    {
        var package = CreateTestPackage("content 1", "content 2");

        var pack = MarkdownRenderer.RenderPack(package);

        Assert.True(pack.ContainsKey("CONTEXT.md"));
        Assert.True(pack.ContainsKey("FILES.md"));
        Assert.True(pack.ContainsKey("MANIFEST.json"));
    }

    [Fact]
    public void Test_CompletenessReport_DoesNotPresentInventoryRatioAsContextCoverage()
    {
        var basePackage = CreateTestPackage("content 1", "content 2");
        var package = basePackage with
        {
            Skipped = new[]
            {
                new SkippedFile("assets/image.png", "unsupported extension/name"),
                new SkippedFile("node_modules/", "default/user excluded directory"),
                new SkippedFile("src/large.cs", "too large: 100 bytes > 50 bytes")
            }
        };

        var rendered = Output.CompletenessReportRenderer.Render(package);

        Assert.Contains("Raw Inventory Inclusion Ratio", rendered);
        Assert.Contains("(diagnostic only)", rendered);
        Assert.Contains("Coverage-Limiting Omissions**: `1`", rendered);
        Assert.Contains("Excluded Directory Entries**: `1`", rendered);
        Assert.DoesNotContain("**Context Coverage**", rendered);
    }
}
