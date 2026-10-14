using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

public sealed class DocumentationContractTests
{
    [Fact]
    public void Required_current_documentation_files_exist()
    {
        var root = FindRepositoryRoot();
        foreach (var relativePath in new[]
        {
            "README.md",
            "CHANGELOG.md",
            "docs/ARCHITECTURE.md",
            "docs/POST.md",
            "docs/UX_MODE_GUIDE.md",
            "docs/EXPLORER_MENU.md",
            "docs/DOCUMENT_INGESTION.md",
            "docs/RESEARCH_INTELLIGENCE.md",
            "docs/MODELING_INTELLIGENCE.md",
            "docs/CREATIVE_REFERENCE.md",
            "docs/GIT_STATE.md",
            "docs/SECURITY.md",
            "docs/TESTING.md",
            "docs/TROUBLESHOOTING.md",
            "docs/ROADMAP.md"
        })
        {
            Assert.True(File.Exists(Path.Combine(root, relativePath)), $"Missing documentation file: {relativePath}");
        }
    }

    [Fact]
    public void Current_readme_and_changelog_identify_current_version()
    {
        var root = FindRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        var changelog = File.ReadAllText(Path.Combine(root, "CHANGELOG.md"));

        Assert.StartsWith($"# AIFolderFlatten v{Program.AppVersion}", readme, StringComparison.Ordinal);
        Assert.Contains($"## v{Program.AppVersion}", changelog, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "README.md")) &&
                Directory.Exists(Path.Combine(current.FullName, "src")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
