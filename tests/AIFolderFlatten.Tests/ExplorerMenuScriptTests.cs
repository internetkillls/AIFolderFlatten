using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIFolderFlatten.Output;

namespace AIFolderFlatten.Tests;

public sealed class ExplorerMenuScriptTests
{
    [Theory]
    [InlineData("folder")]
    [InlineData("file")]
    [InlineData("multi")]
    public void NativeMenuCapturesSelectionDespiteLargeHistoryAndOldProfile(string selection)
    {
        var root = FindRepositoryRoot();
        var native = File.ReadAllText(Path.Combine(root, "native", "AIFolderFlatten.ContextMenu", "dllmain.cpp"));
        var menu = Regex.Matches(native, "CommandSpec\\{ L\"([^\"]+)\", L\"([^\"]+)\"");
        var command = Assert.Single(menu.Cast<Match>());
        Assert.Equal("Complete Source Snapshot", command.Groups[1].Value);
        var scratch = Path.Combine(Path.GetTempPath(), "aif-menu-test-" + Guid.NewGuid().ToString("N"));
        var source = Path.Combine(scratch, "repo");
        Directory.CreateDirectory(Path.Combine(source, "src"));
        Directory.CreateDirectory(Path.Combine(source, ".devspace"));
        try
        {
            var main = Path.Combine(source, "src", "main.ts");
            var index = Path.Combine(source, "src", "index.ts");
            File.WriteAllText(main, "export class Main {} /*" + new string('x', 1_100_000) + "*/");
            File.WriteAllText(index, "export * from './main';");
            for (var i = 0; i < 4; i++)
                File.WriteAllText(Path.Combine(source, ".devspace", $"receipt{i}.json"), "{\"receipt\":\"" + new string('y', 600000) + "\"}");
            File.WriteAllText(Path.Combine(source, ".ai-flatten.json"),
                """{"max-total-chars":100,"max-file-bytes":1,"prompt-starter":"vibe-continue","task-context":"settings","git-changed":true,"dry-run":true}""");
            var list = Path.Combine(scratch, "selection.paths");
            var paths = selection == "folder" ? new[] { source } : selection == "file" ? new[] { main } : new[] { main, index };
            File.WriteAllLines(list, paths, Encoding.Unicode);
            var args = new[] { "--input-list-file", list }.Concat(command.Groups[2].Value.Split(' '))
                .Concat(new[] { "--notify", "false" }).ToArray();
            Assert.Equal(ExitCodes.Success, Program.Main(args));
            var parent = selection == "folder" ? scratch : source;
            var folder = Assert.Single(Directory.GetDirectories(parent, "*_SOURCE_SNAPSHOT_*"));
            var manifest = JsonSerializer.Deserialize<CompleteSnapshotWriter.SnapshotManifest>(
                File.ReadAllText(Path.Combine(folder, "SOURCE_MANIFEST.json")))!;
            Assert.Contains(manifest.Files, f => f.Path == (selection == "folder" ? "src/main.ts" : "main.ts"));
            if (selection == "file") Assert.Single(manifest.Files);
            if (selection == "multi") Assert.Equal(2, manifest.IncludedFiles);
            if (selection == "folder") Assert.Contains(manifest.Files, f => f.Path == ".devspace/receipt3.json");
            Assert.Equal(0, manifest.LimitingOmissions);
            Assert.True(CompleteSnapshotWriter.Verify(folder).Passed);
            Assert.All(Directory.GetFiles(folder, "PART_*.md"), part =>
                Assert.DoesNotContain("## TASK: Continue Building This", File.ReadAllText(part)));
        }
        finally { Directory.Delete(scratch, true); }
    }

    [Fact]
    public void BackgroundAndLegacyMenuUseOnlySnapshotWrapper()
    {
        var root = FindRepositoryRoot();
        var installer = File.ReadAllText(Path.Combine(root, "scripts", "install-context-menu.ps1"));
        var wrapper = File.ReadAllText(Path.Combine(root, "scripts", "invoke-multi-flatten.ps1"));
        Assert.Contains("Complete Source Snapshot", installer);
        Assert.Contains(@"Directory\Background\shell", installer);
        Assert.Contains("invoke-multi-flatten.ps1", installer);
        Assert.Contains("\"--explorer-snapshot\", \"true\"", wrapper);
        Assert.DoesNotContain("--prompt-starter", wrapper);
        Assert.DoesNotContain("--git-ref", wrapper);
        Assert.DoesNotContain("--output-layout", wrapper);
        Assert.Contains("Collect-ExplorerSelection", wrapper);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "README.md")) && Directory.Exists(Path.Combine(current.FullName, "src")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
