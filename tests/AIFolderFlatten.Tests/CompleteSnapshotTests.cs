using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using AIFolderFlatten.Output;

namespace AIFolderFlatten.Tests;

public sealed class CompleteSnapshotTests
{
    [Fact]
    public void UnbudgetedSnapshotIncludesEveryEligibleFileAndVerifiesAcrossParts()
    {
        using var fixture = new Fixture();
        Directory.CreateDirectory(Path.Combine(fixture.Source, "src"));
        for (var i = 0; i < 23; i++)
            File.WriteAllText(Path.Combine(fixture.Source, "src", $"Module{i:00}.cs"),
                $"// module {i}\n" + new string('X', 12000) + "\r\nEND\n");
        File.WriteAllText(Path.Combine(fixture.Source, "src", "ExactlyPreserved.txt"), "no-final-newline");
        File.WriteAllText(Path.Combine(fixture.Source, "src", "Unicode.txt"), string.Concat(Enumerable.Repeat("🚀", 8000)));
        File.WriteAllText(Path.Combine(fixture.Source, "src", "Empty.txt"), "");
        File.WriteAllText(Path.Combine(fixture.Source, "src", "Utf16.cs"), "class UnicodeSource {}", System.Text.Encoding.Unicode);

        var config = fixture.Config("--max-total-chars", "100", "--max-file-bytes", "1", "--split-size-chars", "8000", "--redact", "false");
        var output = CompleteSnapshotWriter.Write(fixture.Source, config);
        Assert.Contains("verification: PASSED", output);
        var folder = fixture.SnapshotFolder;
        var manifest = ReadManifest(folder);
        Assert.Equal(27, manifest.IncludedFiles);
        Assert.Equal(27, manifest.EligibleFiles);
        Assert.Equal(0, manifest.LimitingOmissions);
        Assert.True(manifest.Parts.Count > 1);
        Assert.True(CompleteSnapshotWriter.Verify(folder).Passed);
        Assert.All(manifest.Files, f => Assert.NotEmpty(f.Chunks));
        Assert.Contains(manifest.Files, f => f.Path == "src/ExactlyPreserved.txt");
        Assert.All(manifest.Parts, p =>
        {
            Assert.True(File.Exists(Path.Combine(folder, p)));
            Assert.True(File.Exists(Path.Combine(folder, Path.ChangeExtension(p, ".md"))));
            Assert.True(File.ReadAllText(Path.Combine(folder, p)).Length <= 8000);
            Assert.True(File.ReadAllText(Path.Combine(folder, Path.ChangeExtension(p, ".md"))).Length <= 8000);
        });
    }

    [Fact]
    public void VerificationRejectsTamperedContentAndMissingPart()
    {
        using var fixture = new Fixture();
        File.WriteAllText(Path.Combine(fixture.Source, "README.md"), new string('A', 15000));
        CompleteSnapshotWriter.Write(fixture.Source, fixture.Config("--split-size-chars", "4096", "--redact", "false"));
        var folder = fixture.SnapshotFolder;
        var part = ReadManifest(folder).Parts[0];
        File.AppendAllText(Path.Combine(folder, part), "{}\n");
        Assert.False(CompleteSnapshotWriter.Verify(folder).Passed);
        File.Delete(Path.Combine(folder, part));
        Assert.False(CompleteSnapshotWriter.Verify(folder).Passed);
    }

    [Fact]
    public void ReportsPolicyExclusionsRatherThanClaimingFullFilesystemCoverage()
    {
        using var fixture = new Fixture();
        File.WriteAllText(Path.Combine(fixture.Source, "Main.cs"), "class Main {}\n");
        File.WriteAllText(Path.Combine(fixture.Source, "ignored.dll"), "not source");
        Directory.CreateDirectory(Path.Combine(fixture.Source, "node_modules"));
        File.WriteAllText(Path.Combine(fixture.Source, "node_modules", "ignored.js"), "code");
        CompleteSnapshotWriter.Write(fixture.Source, fixture.Config());
        var manifest = ReadManifest(fixture.SnapshotFolder);
        Assert.Equal(1, manifest.IncludedFiles);
        Assert.Equal("COMPLETE_ELIGIBLE_WITH_POLICY_EXCLUSIONS", manifest.CoverageStatus);
        Assert.Contains(manifest.Exclusions, e => e.RelativePath.Contains("node_modules"));
        Assert.Contains(manifest.Exclusions, e => e.RelativePath == "ignored.dll");
    }

    [Fact]
    public void RedactionIsExplicitAndHashOfExportedTextIsVerifiable()
    {
        using var fixture = new Fixture();
        File.WriteAllText(Path.Combine(fixture.Source, "config.txt"), "api_key=abcdef1234\n");
        CompleteSnapshotWriter.Write(fixture.Source, fixture.Config());
        var folder = fixture.SnapshotFolder;
        var file = Assert.Single(ReadManifest(folder).Files);
        Assert.True(file.Redacted);
        Assert.NotEqual(file.SourceSha256, file.ExportedTextSha256);
        Assert.True(CompleteSnapshotWriter.Verify(folder).Passed);
        Assert.DoesNotContain("abcdef1234", File.ReadAllText(Path.Combine(folder, ReadManifest(folder).Parts[0])));
    }

    [Fact]
    public void RejectsOutputInsideInputAndGitSubsetFlags()
    {
        using var fixture = new Fixture();
        File.WriteAllText(Path.Combine(fixture.Source, "Main.cs"), "class Main {}\n");
        var inside = AppConfig.FromArgs(new[] { "--input", fixture.Source, "--snapshot-complete", "true", "--output", Path.Combine(fixture.Source, "exports") });
        Assert.Throws<ArgumentException>(() => CompleteSnapshotWriter.Write(fixture.Source, inside));
        var gitSubset = fixture.Config("--git-changed", "true");
        Assert.True(gitSubset.GitChanged);
        Assert.Equal(ExitCodes.ArgumentError, Program.Main(new[]
        {
            "--input", fixture.Source, "--snapshot-complete", "true", "--git-changed", "true"
        }));
    }

    [Fact]
    public void DryRunDoesNotWriteAnySnapshots()
    {
        using var fixture = new Fixture();
        File.WriteAllText(Path.Combine(fixture.Source, "Main.cs"), "class Main {}\n");
        var result = CompleteSnapshotWriter.Write(fixture.Source, fixture.Config("--dry-run", "true"));
        Assert.Contains("DRY RUN", result);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Destination));
    }

    private static CompleteSnapshotWriter.SnapshotManifest ReadManifest(string dir) =>
        JsonSerializer.Deserialize<CompleteSnapshotWriter.SnapshotManifest>(
            File.ReadAllText(Path.Combine(dir, "SOURCE_MANIFEST.json")))!;

    private sealed class Fixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "AIFolderFlatten-Snapshot-Test-" + Guid.NewGuid().ToString("N"));
        public string Source => Path.Combine(_root, "source");
        public string Destination => Path.Combine(_root, "exports");
        public string SnapshotFolder => Assert.Single(Directory.EnumerateDirectories(Destination));
        public Fixture() { Directory.CreateDirectory(Source); Directory.CreateDirectory(Destination); }
        public AppConfig Config(params string[] flags) =>
            AppConfig.FromArgs(new[] { "--input", Source, "--output", Destination, "--snapshot-complete", "true" }.Concat(flags).ToArray());
        public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
    }
}
