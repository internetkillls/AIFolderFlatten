using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Tests that verify <see cref="ManifestVerifier.Verify"/> correctly validates
/// expert-system fields (expertSystemOnly, llmUsed, networkUsed, aiCore).
/// </summary>
public class ManifestExpertSystemTests : IDisposable
{
    private readonly string _tempDir;

    public ManifestExpertSystemTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "AIFolderFlatten_ESManifestTests_" + Guid.NewGuid().ToString("N"));
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

    // ── Helpers ──────────────────────────────────────────────────────────────

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

    private string WriteManifest(string json)
    {
        var path = Path.Combine(_tempDir, "MANIFEST.json");
        File.WriteAllText(path, json);
        return path;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Valid_ES_manifest_passes()
    {
        CreateDummyPackFiles();
        var manifestPath = WriteManifest("""
            {
              "kind": "ai-folder-context-pack",
              "version": "0.2.3",
              "expertSystemOnly": true,
              "llmUsed": false,
              "networkUsed": false,
              "embeddingsUsed": false,
              "aiCore": false,
              "included_files": [
                { "id": "F0001", "path": "Program.cs" }
              ],
              "files": [
                {
                  "path": "CONTEXT.md",
                  "sizeBytes": 0,
                  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                }
              ]
            }
            """);

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.True(result.Passed, string.Join("; ", result.Errors));
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Manifest_missing_expertSystemOnly_fails()
    {
        CreateDummyPackFiles();
        var manifestPath = WriteManifest("""
            {
              "kind": "ai-folder-context-pack",
              "version": "0.2.3",
              "llmUsed": false,
              "networkUsed": false,
              "embeddingsUsed": false,
              "aiCore": false,
              "included_files": [
                { "id": "F0001", "path": "Program.cs" }
              ],
              "files": [
                {
                  "path": "CONTEXT.md",
                  "sizeBytes": 0,
                  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                }
              ]
            }
            """);

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e =>
            e.Contains("expertSystemOnly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Manifest_with_llmUsed_true_fails()
    {
        CreateDummyPackFiles();
        var manifestPath = WriteManifest("""
            {
              "kind": "ai-folder-context-pack",
              "version": "0.2.3",
              "expertSystemOnly": true,
              "llmUsed": true,
              "networkUsed": false,
              "embeddingsUsed": false,
              "aiCore": false,
              "included_files": [
                { "id": "F0001", "path": "Program.cs" }
              ],
              "files": [
                {
                  "path": "CONTEXT.md",
                  "sizeBytes": 0,
                  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                }
              ]
            }
            """);

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e =>
            e.Contains("llmUsed", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Manifest_with_networkUsed_true_fails()
    {
        CreateDummyPackFiles();
        var manifestPath = WriteManifest("""
            {
              "kind": "ai-folder-context-pack",
              "version": "0.2.3",
              "expertSystemOnly": true,
              "llmUsed": false,
              "networkUsed": true,
              "embeddingsUsed": false,
              "aiCore": false,
              "included_files": [
                { "id": "F0001", "path": "Program.cs" }
              ],
              "files": [
                {
                  "path": "CONTEXT.md",
                  "sizeBytes": 0,
                  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                }
              ]
            }
            """);

        var result = ManifestVerifier.Verify(manifestPath, "0.2.3");

        Assert.False(result.Passed);
        Assert.Contains(result.Errors, e =>
            e.Contains("networkUsed", StringComparison.OrdinalIgnoreCase));
    }
}
