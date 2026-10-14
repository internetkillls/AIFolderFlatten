using System;
using System.Collections.Generic;
using Xunit;
using AIFolderFlatten.Post;

namespace AIFolderFlatten.Tests;

public class PostOrientationTests
{
    [Fact]
    public void Test_PostOrientation_Scoring_CalculatesCorrectly()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "." });
        var scan = new ScanState(
            TotalFilesSeen: 10,
            IncludedTextFiles: 5,
            SkippedFiles: 0,
            UnknownArtifacts: 0,
            MediaArtifacts: 0,
            ResearchSignals: 8, // high research signals
            CreativeSignals: 0,
            CodeSignals: 1,
            ObsidianSignals: 0,
            DocumentSignals: 3,
            ModelingSignals: 0
        );

        var textContents = new List<string> { "methods, hypothesis, literature review, provenance" };
        var state = PostOrientationEngine.Orient(".", config, scan, textContents);

        Assert.Equal("research", state.SelectedDomain);
        Assert.True(state.Confidence > 0.5);
    }

    [Fact]
    public void Test_PostOrientation_LowConfidence_FallsBackToUnknown()
    {
        var config = AppConfig.FromArgs(new[] { "--input", ".", "--auto", "true" });
        var scan = new ScanState(10, 5, 0, 5, 0, 0, 0, 0, 0, 0, 0); // mixed/unknown files
        var textContents = new List<string>();

        var state = PostOrientationEngine.Orient(".", config, scan, textContents);

        // Low scoring should fall back to unknown or mixed
        Assert.True(state.SelectedDomain == "unknown" || state.SelectedDomain == "mixed");
    }

    [Fact]
    public void Test_PostOrientation_RespectsUserOrientationHint()
    {
        var config = AppConfig.FromArgs(new[] { "--input", ".", "--orientation", "this is for creative sound design" });
        var scan = new ScanState(10, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var textContents = new List<string>();

        var state = PostOrientationEngine.Orient(".", config, scan, textContents);

        Assert.Equal("creative", state.SelectedDomain);
    }
}
