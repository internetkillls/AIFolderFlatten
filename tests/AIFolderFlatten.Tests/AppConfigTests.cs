using System;
using AIFolderFlatten.ExpertSystem;
using Xunit;

namespace AIFolderFlatten.Tests;

public sealed class AppConfigTests
{
    [Fact]
    public void Version_flag_is_recognized_without_input()
    {
        var config = AppConfig.FromArgs(new[] { "--version" });

        Assert.True(config.ShowVersion);
        Assert.Null(config.InputPath);
    }

    [Fact]
    public void Expert_system_mode_cannot_be_disabled()
    {
        var config = AppConfig.FromArgs(new[] { "--expert-system-only", "false" });

        Assert.Throws<InvalidOperationException>(() =>
            ExpertSystemBoundary.AssertExpertSystemMode(
                config.ExpertSystemOnly,
                config.AllowLLM,
                config.AllowNetwork,
                config.AllowEmbeddings));
    }
}
