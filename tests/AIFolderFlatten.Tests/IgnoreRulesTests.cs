using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

public class IgnoreRulesTests : IDisposable
{
    private readonly string _tempDir;

    public IgnoreRulesTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlattenIgnoreTests_" + Guid.NewGuid().ToString("N"));
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
    public void Test_IgnoreRules_RespectsAiIgnore()
    {
        File.WriteAllText(Path.Combine(_tempDir, ".aiignore"), "*.env\nsecret/");
        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var rules = IgnoreRules.Load(_tempDir, config);

        Assert.True(rules.IsIgnored("prod.env"));
        Assert.True(rules.IsIgnored("secret/keys.txt"));
        Assert.False(rules.IsIgnored("public.txt"));
    }

    [Fact]
    public void Test_IgnoreRules_RespectsGitIgnore()
    {
        File.WriteAllText(Path.Combine(_tempDir, ".gitignore"), "tmp/\n*.log");
        var config = AppConfig.FromArgs(new[] { "--input", _tempDir });
        var rules = IgnoreRules.Load(_tempDir, config);

        Assert.True(rules.IsIgnored("tmp/file.txt"));
        Assert.True(rules.IsIgnored("error.log"));
        Assert.False(rules.IsIgnored("src/Program.cs"));
    }

    [Fact]
    public void Test_IgnoreRules_RespectsExcludeGlobs()
    {
        var config = AppConfig.FromArgs(new[] { "--input", _tempDir, "--exclude", "**/private/**;**/*.tmp" });
        
        Assert.True(Glob.AnyMatch("src/private/secret.cs", config.ExcludeGlobs));
        Assert.True(Glob.AnyMatch("sub/test.tmp", config.ExcludeGlobs));
        Assert.False(Glob.AnyMatch("src/public/class.cs", config.ExcludeGlobs));
    }
}
