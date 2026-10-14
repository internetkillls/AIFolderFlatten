using System.IO;
using System.Linq;
using Xunit;

namespace AIFolderFlatten.Tests;

/// <summary>
/// Verifies that no default Explorer context menu command uses --open true (AC-MASTER-004).
/// </summary>
public class NoAutoOpenExplorerMenuTests
{
    private static string ScriptContent()
    {
        var repo = FindRepoRoot();
        var path = Path.Combine(repo, "scripts", "install-context-menu.ps1");
        Assert.True(File.Exists(path), $"install-context-menu.ps1 not found at: {path}");
        return File.ReadAllText(path);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "scripts", "install-context-menu.ps1")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find repo root containing scripts/install-context-menu.ps1");
    }

    [Fact]
    public void Script_DoesNotContain_OpenTrue()
    {
        // --open true is the old flag that auto-launched Explorer.
        // No default command should use it.
        var content = ScriptContent();
        Assert.DoesNotContain("--open true", content);
    }

    [Fact]
    public void Script_DoesNotContain_OpenAfterExportAlways()
    {
        // --open-after-export always is also forbidden as a default
        Assert.DoesNotContain("--open-after-export always", ScriptContent());
    }

    [Fact]
    public void Script_SnapshotCommands_HaveNoAutoOpen()
    {
        var content = File.ReadAllText(Path.Combine(FindRepoRoot(), "scripts", "invoke-multi-flatten.ps1"));
        var lines = content.Split('\n');

        var commandLines = lines
            .Where(l => l.Contains("--explorer-snapshot"))
            .ToList();

        Assert.NotEmpty(commandLines);
        foreach (var line in commandLines)
        {
            Assert.DoesNotContain("--open true", line);
            Assert.DoesNotContain("--open-after-export always", line);
        }
    }

    [Fact]
    public void AppConfig_OpenAfterExportNever_DoesNotOpen()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake", "--open-after-export", "never" });
        Assert.False(config.OpenAfterExport);
    }

    [Fact]
    public void AppConfig_DefaultOpen_IsFalse()
    {
        var config = AppConfig.FromArgs(new[] { "--input", "C:\\Fake" });
        Assert.False(config.OpenAfterExport);
    }
}
