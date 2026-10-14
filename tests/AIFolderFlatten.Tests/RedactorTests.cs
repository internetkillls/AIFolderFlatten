using Xunit;

namespace AIFolderFlatten.Tests;

public class RedactorTests
{
    [Fact]
    public void Test_Redactor_RedactsOpenAiKey()
    {
        var content = "var key = \"sk-1234567890abcdef1234567890abcdef\";";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var result = Redactor.Redact(content, config);

        Assert.Contains("sk-<REDACTED>", result.Content);
        Assert.DoesNotContain("abcdef", result.Content);
        Assert.True(result.Counts.ContainsKey("openai_key"));
    }

    [Fact]
    public void Test_Redactor_RedactsGitHubToken()
    {
        var content = "My token value is ghp_abcdefghijklmnopqrstuvwxyz0123456789";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var result = Redactor.Redact(content, config);

        Assert.Contains("gh_<REDACTED>", result.Content);
        Assert.DoesNotContain("abcdefghijkl", result.Content);
        Assert.True(result.Counts.ContainsKey("github_token"));
    }

    [Fact]
    public void Test_Redactor_RedactsAwsAccessKey()
    {
        var content = "aws_key = \"AKIAIOSFODNN7EXAMPLE\";";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var result = Redactor.Redact(content, config);

        Assert.Contains("AKIA<REDACTED>", result.Content);
        Assert.True(result.Counts.ContainsKey("aws_access_key"));
    }

    [Fact]
    public void Test_Redactor_RedactsJwt()
    {
        var content = "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var result = Redactor.Redact(content, config);

        Assert.Contains("<JWT_REDACTED>", result.Content);
        Assert.True(result.Counts.ContainsKey("jwt"));
    }

    [Fact]
    public void Test_Redactor_RedactsLocalWindowsPaths()
    {
        var content = "The path is C:\\Users\\example-user\\Secrets\\file.txt";
        var config = AppConfig.FromArgs(new[] { "--input", ".", "--redact-paths", "true" });

        var result = Redactor.Redact(content, config);

        Assert.Contains("<LOCAL_PATH_REDACTED>", result.Content);
        Assert.DoesNotContain("example-user", result.Content);
        Assert.True(result.Counts.ContainsKey("local_path"));
    }
}
