using System;
using System.IO;
using Xunit;
using AIFolderFlatten.Dev;

namespace AIFolderFlatten.Tests;

public sealed class ContractDriftTests : IDisposable
{
    private readonly string _tempDir;

    public ContractDriftTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlatten_ContractDriftTests_" + Guid.NewGuid().ToString("N"));
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

    [Fact]
    public void ContractDriftChecker_Detects_Version_And_Filename_Drifts()
    {
        // 1. Version drift - mock a csproj with version 0.1.0
        var srcDir = Path.Combine(_tempDir, "src", "AIFolderFlatten");
        Directory.CreateDirectory(srcDir);
        var csprojPath = Path.Combine(srcDir, "AIFolderFlatten.csproj");
        File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><Version>0.1.0</Version></PropertyGroup></Project>");

        // Mock README.md with outdated version v0.1.0 and obsolete filename EQUATIONS.md
        var readmePath = Path.Combine(_tempDir, "README.md");
        File.WriteAllText(readmePath, "Product version v0.1.0. See: EQUATIONS.md.");

        // Mock docs folder with another obsolete filename
        var docsDir = Path.Combine(_tempDir, "docs");
        Directory.CreateDirectory(docsDir);
        var docPath = Path.Combine(docsDir, "usage.md");
        File.WriteAllText(docPath, "Usage instructions. Check SCRIPT_TARGETS.md.");

        // Run checker
        var result = ContractDriftChecker.Check(_tempDir);

        Assert.NotNull(result);

        // Version Drift check
        Assert.Contains("Csproj version (`0.1.0`) does not match Program.AppVersion", result.VersionDriftMarkdown);
        Assert.Contains("README.md references obsolete or mismatching version: `v0.1.0`", result.VersionDriftMarkdown);
        Assert.Contains("CHANGELOG.md file is missing from root", result.VersionDriftMarkdown);

        // Docs vs Code filename check
        Assert.Contains("references deprecated output name `EQUATIONS.md`. Should be updated to `EQUATION_CANDIDATES.md`", result.DocsVsCodeMarkdown);
        Assert.Contains("references deprecated output name `SCRIPT_TARGETS.md`. Should be updated to `SCRIPT_TARGET_CANDIDATES.md`", result.DocsVsCodeMarkdown);
    }
}
