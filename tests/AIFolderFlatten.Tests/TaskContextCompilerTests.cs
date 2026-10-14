using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using AIFolderFlatten.TaskContext;
using Xunit;

namespace AIFolderFlatten.Tests;

public sealed class TaskContextCompilerTests
{
    [Fact]
    public void CliRejectsConflictingSnapshotAndTaskExportModes()
    {
        using var fx = new Fixture();
        var result = Program.Main(new[] { "--input", fx.Input, "--output", fx.ParentOutput,
            "--snapshot-complete", "true", "--task-context", "settings" });
        Assert.Equal(ExitCodes.ArgumentError, result);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fx.ParentOutput));
    }

    [Fact]
    public void TaskSelection_PrefersRelevantFile_RecordsEvidence_AndDoesNotClaimFullCoverage()
    {
        using var fx = new Fixture();
        fx.Write("src/SettingsTab.cs", "public class SettingsTab { void RenderControls(){} }\n");
        fx.Write("src/SettingsTabTests.cs", "class SettingsTabTests {}\n");
        fx.Write("src/UnrelatedMath.cs", "class RandomArithmetic {}\n");
        var result = TaskContextCompiler.Compile(fx.Input, fx.Config("--task-context", "Fix SettingsTab controls"));
        Assert.Contains("Task context created", result);
        var bundle = fx.Data();
        Assert.Equal("TASK_SCOPED_NOT_A_COMPLETE_REPOSITORY_SNAPSHOT", bundle.Scope);
        Assert.Contains(bundle.Files, x => x.Path == "src/SettingsTab.cs" && x.State == "SELECTED" && x.Score > 0);
        Assert.Contains(bundle.Files, x => x.Path == "src/SettingsTabTests.cs" && x.State == "SELECTED" && x.Reasons.Length > 0);
        Assert.Contains(bundle.Files, x => x.Path == "src/UnrelatedMath.cs" && x.State != "SELECTED");
        Assert.True(TaskContextCompiler.Verify(fx.Output).Passed);
        Assert.True(TaskContextCompiler.Verify(fx.Output, fx.Input).Passed);
        Assert.True(File.Exists(Path.Combine(fx.Output, "CONTEXT_REVIEW.html")));
    }

    [Fact]
    public void TaskBudget_ExplicitlyReportsOmissions_DoesNotCutFileContent()
    {
        using var fx = new Fixture();
        fx.Write("src/ConfigOne.cs", "// config settings\n" + new string('A', 850));
        fx.Write("src/ConfigTwo.cs", "// config settings\n" + new string('B', 850));
        TaskContextCompiler.Compile(fx.Input, fx.Config("--task-context", "config settings", "--task-budget-chars", "1024"));
        var data = fx.Data();
        Assert.Equal(1, data.SelectedCount);
        Assert.Equal(1, data.BudgetOmissions);
        Assert.Contains(data.Files, x => x.State == "OMITTED_BUDGET");
        Assert.True(data.UsedChars <= data.BudgetChars);
        Assert.Contains("Coverage limitations", File.ReadAllText(Path.Combine(fx.Output, "CONTEXT.md")));
    }

    [Fact]
    public void PinsIncludeOtherwiseIrrelevantFiles_AndUnknownPinsFailClosed()
    {
        using var fx = new Fixture();
        fx.Write("src/CompletelyUnrelated.cs", "class Calibrations {}\n");
        TaskContextCompiler.Compile(fx.Input,
            fx.Config("--task-context", "settings controls", "--task-pins", "src/CompletelyUnrelated.cs"));
        Assert.Contains(fx.Data().Files, x => x.Path == "src/CompletelyUnrelated.cs" && x.State == "SELECTED" && x.Reasons.Contains("Explicit human pin"));
        Assert.Throws<ArgumentException>(() => TaskContextCompiler.Compile(fx.Input,
            fx.Config("--task-context", "settings controls", "--task-pins", "src/Missing.cs")));
    }

    [Fact]
    public void VerifierRejectsTamperingAndDetectsStaleSource()
    {
        using var fx = new Fixture();
        fx.Write("src/Settings.cs", "class Settings { }\n");
        TaskContextCompiler.Compile(fx.Input, fx.Config("--task-context", "settings"));
        Assert.True(TaskContextCompiler.Verify(fx.Output, fx.Input).Passed);
        fx.Write("src/Settings.cs", "class Settings { int Changed; }\n");
        Assert.False(TaskContextCompiler.Verify(fx.Output, fx.Input).Passed);
        Assert.True(TaskContextCompiler.Verify(fx.Output).Passed);
        File.AppendAllText(Path.Combine(fx.Output, "CONTEXT.md"), "\nTAMPERED");
        Assert.False(TaskContextCompiler.Verify(fx.Output).Passed);
    }

    [Fact]
    public void CockpitEscapesSourceTagsAndDoesNotInvokeRemoteResources()
    {
        using var fx = new Fixture();
        fx.Write("Settings.cs", "class Settings {} // </script><script>alert(1)</script>\n");
        TaskContextCompiler.Compile(fx.Input, fx.Config("--task-context", "settings"));
        var html = File.ReadAllText(Path.Combine(fx.Output, "CONTEXT_REVIEW.html"));
        Assert.DoesNotContain("</script><script>alert(1)", html);
        Assert.Contains("\\u003C/script\\u003E", html);
        Assert.DoesNotContain("https://", html);
        Assert.True(TaskContextCompiler.Verify(fx.Output).Passed);
    }

    [Fact]
    public void DryRunCreatesNoFiles_AndRejectsOutputWithinInput()
    {
        using var fx = new Fixture();
        fx.Write("Settings.cs", "class Settings {}\n");
        var msg = TaskContextCompiler.Compile(fx.Input,
            fx.Config("--task-context", "settings", "--dry-run", "true"));
        Assert.Contains("DRY RUN", msg);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fx.ParentOutput));
        var bad = AppConfig.FromArgs(new[] { "--input", fx.Input, "--task-context", "settings",
            "--output", Path.Combine(fx.Input, "out") });
        Assert.Throws<ArgumentException>(() => TaskContextCompiler.Compile(fx.Input, bad));
    }

    [Fact]
    public void RedactionPreservesProvenanceOfBothOriginalAndExportedText()
    {
        using var fx = new Fixture();
        fx.Write("Settings.cs", "// settings\napi_key=abcdef123456\n");
        TaskContextCompiler.Compile(fx.Input, fx.Config("--task-context", "settings"));
        var file = Assert.Single(fx.Data().Files);
        Assert.True(file.Redacted);
        Assert.NotEqual(file.ExportSha256, file.OriginalSha256);
        Assert.DoesNotContain("abcdef123456", File.ReadAllText(Path.Combine(fx.Output, "CONTEXT.md")));
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "aif-task-test-" + Guid.NewGuid().ToString("N"));
        public string Input => Path.Combine(_root, "repo");
        public string ParentOutput => Path.Combine(_root, "out");
        public string Output => Assert.Single(Directory.GetDirectories(ParentOutput));
        public Fixture() { Directory.CreateDirectory(Input); Directory.CreateDirectory(ParentOutput); }
        public AppConfig Config(params string[] extra) => AppConfig.FromArgs(new[] { "--input", Input, "--output", ParentOutput }.Concat(extra).ToArray());
        public void Write(string path, string content)
        {
            var full = Path.Combine(Input, path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, content);
        }
        public TaskContextCompiler.TaskBundle Data() => JsonSerializer.Deserialize<TaskContextCompiler.TaskBundle>(
            File.ReadAllText(Path.Combine(Output, "CONTEXT_DATA.json")))!;
        public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
    }
}
