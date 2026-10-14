using System;
using Xunit;
using AIFolderFlatten.Creative;

namespace AIFolderFlatten.Tests;

public class CreativeAffordanceTests
{
    [Fact]
    public void Test_CreativeAffordance_SniffsTargetScriptAndExcludesPerceptualClaims()
    {
        var affordance = CreativeAffordanceAnalyzer.InferAffordance(".wav", "sound/vocals.wav");

        Assert.Contains("vocals.wav", affordance);
        Assert.Contains("SuperCollider", affordance);

        // Assert it does not make perceptual claims (fast tempo, bright timbre)
        Assert.DoesNotContain("bright", affordance);
        Assert.DoesNotContain("fast", affordance);
    }
}
