using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

public class ManifestTests : IDisposable
{
    private readonly string _tempDir;

    public ManifestTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlattenManifestTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }

    private void CreateDummyPackFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "CONTEXT.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "FILES.md"), "<!-- BEGIN_FILE id=\"F0001\" -->");
        File.WriteAllText(Path.Combine(_tempDir, "SYMBOLS.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "DEPENDENCIES.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "RISKS.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "SKIPPED.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "REDACTIONS.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "USER_PROMPT.md"), "");
        File.WriteAllText(Path.Combine(_tempDir, "EXPERT_SYSTEM_PROVENANCE.md"), "");
    }

    private static string MakeValidManifest(string version = "0.2.3", string fileId = "F0001") =>
        "{" +
        $"\"kind\":\"ai-folder-context-pack\"," +
        $"\"version\":\"{version}\"," +
        "\"expertSystemOnly\":true," +
        "\"aiCore\":false," +
        "\"llmUsed\":false," +
        "\"networkUsed\":false," +
        "\"embeddingsUsed\":false," +
        $"\"included_files\":[{{\"id\":\"{fileId}\",\"path\":\"Program.cs\"}}]," +
        "\"files\":[{\"path\":\"CONTEXT.md\",\"sizeBytes\":0," +
        "\"sha256\":\"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\"}]" +
        "}";

    [Fact]
    public void Test_ManifestVerifier_ValidManifest_Passes()
    {
        CreateDummyPackFiles();
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath, MakeValidManifest());
        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");
        Assert.True(result.Passed, string.Join("; ", result.Errors));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Test_ManifestVerifier_MismatchedVersion_Fails()
    {
        CreateDummyPackFiles();
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath, MakeValidManifest(version: "0.2.0"));
        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");
        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("version"));
    }

    [Fact]
    public void Test_ManifestVerifier_DuplicateFileIds_Fails()
    {
        CreateDummyPackFiles();
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath,
            "{\"kind\":\"ai-folder-context-pack\",\"version\":\"0.2.3\"," +
            "\"expertSystemOnly\":true,\"aiCore\":false,\"llmUsed\":false,\"networkUsed\":false,\"embeddingsUsed\":false," +
            "\"included_files\":[{\"id\":\"F0001\",\"path\":\"f1.cs\"},{\"id\":\"F0001\",\"path\":\"f2.cs\"}]," +
            "\"files\":[{\"path\":\"CONTEXT.md\",\"sizeBytes\":0," +
            "\"sha256\":\"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\"}]}");
        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");
        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate file ID"));
    }

    [Fact]
    public void Test_ManifestVerifier_MissingPackFiles_Fails()
    {
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath, MakeValidManifest());
        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");
        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("Required pack file is missing"));
    }

    [Fact]
    public void Test_ManifestVerifier_Missing_ExpertSystemOnly_Fails()
    {
        CreateDummyPackFiles();
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath,
            "{\"kind\":\"ai-folder-context-pack\",\"version\":\"0.2.3\"," +
            "\"aiCore\":false,\"llmUsed\":false,\"networkUsed\":false,\"embeddingsUsed\":false," +
            "\"included_files\":[{\"id\":\"F0001\",\"path\":\"Program.cs\"}]," +
            "\"files\":[{\"path\":\"CONTEXT.md\",\"sizeBytes\":0," +
            "\"sha256\":\"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\"}]}");
        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");
        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("expertSystemOnly"));
    }

    [Fact]
    public void Test_ManifestVerifier_HashMismatch_Fails()
    {
        CreateDummyPackFiles();
        File.WriteAllText(Path.Combine(_tempDir, "CONTEXT.md"), "tampered");
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath, MakeValidManifest());

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("Hash mismatch", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Test_ManifestVerifier_MissingEmbeddingsAttestation_Fails()
    {
        CreateDummyPackFiles();
        var manifestPath = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(manifestPath, MakeValidManifest().Replace("\"embeddingsUsed\":false,", ""));

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e => e.Contains("embeddingsUsed", StringComparison.OrdinalIgnoreCase));
    }
}
