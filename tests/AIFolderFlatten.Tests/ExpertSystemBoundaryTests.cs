using System;
using Xunit;
using AIFolderFlatten.ExpertSystem;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Tests that verify the compile-time constants and runtime guard of
/// <see cref="ExpertSystemBoundary"/> are correct and cannot be violated.
/// </summary>
public class ExpertSystemBoundaryTests
{
    [Fact]
    public void AiCore_is_always_false()
    {
        Assert.False(ExpertSystemBoundary.AiCore);
    }

    [Fact]
    public void NetworkUsed_is_always_false()
    {
        Assert.False(ExpertSystemBoundary.NetworkUsed);
    }

    [Fact]
    public void LlmUsed_is_always_false()
    {
        Assert.False(ExpertSystemBoundary.LlmUsed);
    }

    [Fact]
    public void EmbeddingsUsed_is_always_false()
    {
        Assert.False(ExpertSystemBoundary.EmbeddingsUsed);
    }

    [Fact]
    public void ExpertSystemOnly_is_always_true()
    {
        Assert.True(ExpertSystemBoundary.ExpertSystemOnly);
    }

    [Fact]
    public void AssertExpertSystemMode_allows_valid_config()
    {
        // All false → no violation → must not throw
        var exception = Record.Exception(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                expertSystemOnly: true,
                allowLlm: false,
                allowNetwork: false,
                allowEmbeddings: false));

        Assert.Null(exception);
    }

    [Fact]
    public void AssertExpertSystemMode_throws_if_allowLlm_true()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                expertSystemOnly: true,
                allowLlm: true,
                allowNetwork: false,
                allowEmbeddings: false));
    }

    [Fact]
    public void AssertExpertSystemMode_throws_if_allowNetwork_true()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                expertSystemOnly: true,
                allowLlm: false,
                allowNetwork: true,
                allowEmbeddings: false));
    }

    [Fact]
    public void AssertExpertSystemMode_throws_if_allowEmbeddings_true()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                expertSystemOnly: true,
                allowLlm: false,
                allowNetwork: false,
                allowEmbeddings: true));
    }

    [Fact]
    public void AssertExpertSystemMode_throws_if_expertSystemOnly_false()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                expertSystemOnly: false,
                allowLlm: false,
                allowNetwork: false,
                allowEmbeddings: false));
    }
}
