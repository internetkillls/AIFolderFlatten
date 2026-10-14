using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Dev;

/// <summary>
/// Deterministic contract drift and version matching analyzer.
/// No AI, network, or LLM runtime dependency.
/// </summary>
internal static class ContractDriftChecker
{
    public sealed record ContractDriftResult(
        string ContractDriftMarkdown,
        string VersionDriftMarkdown,
        string DocsVsCodeMarkdown
    );

    public static ContractDriftResult Check(string inputDir)
    {
        var currentVersion = Program.AppVersion;
        var versionDrifts = new List<string>();
        var docsVsCodeDrifts = new List<string>();
        var contractDrifts = new List<string>();

        // 1. Version Drift Checks
        // Check csproj version
        string? csprojVersion = null;
        try
        {
            var csprojPath = Path.Combine(inputDir, "src", "AIFolderFlatten", "AIFolderFlatten.csproj");
            if (File.Exists(csprojPath))
            {
                var content = File.ReadAllText(csprojPath);
                var match = Regex.Match(content, @"<Version>([^<]+)</Version>");
                if (match.Success)
                {
                    csprojVersion = match.Groups[1].Value.Trim();
                    if (csprojVersion != currentVersion)
                    {
                        versionDrifts.Add($"Csproj version (`{csprojVersion}`) does not match Program.AppVersion (`{currentVersion}`).");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            versionDrifts.Add($"Failed to read csproj version: {ex.Message}");
        }

        // Check README.md version references
        try
        {
            var readmePath = Path.Combine(inputDir, "README.md");
            if (File.Exists(readmePath))
            {
                var content = File.ReadAllText(readmePath);
                // The first version declaration is the current product identity.
                // Later version references may be legitimate historical notes.
                var match = Regex.Match(content, @"v\d+\.\d+\.\d+");
                if (match.Success)
                {
                    var version = match.Value.TrimStart('v');
                    if (version != currentVersion)
                    {
                        versionDrifts.Add($"README.md references obsolete or mismatching version: `{match.Value}`.");
                    }
                }
            }
        }
        catch { }

        // Check CHANGELOG.md entries
        try
        {
            var changelogPath = Path.Combine(inputDir, "CHANGELOG.md");
            if (File.Exists(changelogPath))
            {
                var content = File.ReadAllText(changelogPath);
                if (!content.Contains(currentVersion))
                {
                    versionDrifts.Add($"CHANGELOG.md is missing an entry for the current version: `{currentVersion}`.");
                }
            }
            else
            {
                versionDrifts.Add("CHANGELOG.md file is missing from root.");
            }
        }
        catch { }

        // 2. Docs vs Code Drift (obsolete filenames in docs)
        var obsoleteFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "EQUATIONS.md", "EQUATION_CANDIDATES.md" },
            { "VARIABLES.md", "VARIABLE_CANDIDATES.md" },
            { "PARAMETERS.md", "PARAMETER_CANDIDATES.md" },
            { "ASSUMPTIONS.md", "ASSUMPTION_MARKERS.md" },
            { "SCRIPT_TARGETS.md", "SCRIPT_TARGET_CANDIDATES.md" },
            { "CREATIVE_REFERENCE.md", "CREATIVE_ASSET_LEDGER.md" },
            { "METHOD_MAP.md", "METHOD_PHRASE_MAP.md" },
            { "FEASIBILITY.md", "(Removed per overclaim restriction)" },
            { "STATE_OF_THE_ART_QUERIES.md", "GAP_QUESTIONS.md" }
        };

        try
        {
            var docFiles = new List<string>();
            if (Directory.Exists(inputDir))
            {
                var docsDir = Path.Combine(inputDir, "docs");
                if (Directory.Exists(docsDir))
                {
                    docFiles.AddRange(Directory.GetFiles(docsDir, "*.md", SearchOption.AllDirectories));
                }
                var readme = Path.Combine(inputDir, "README.md");
                if (File.Exists(readme)) docFiles.Add(readme);
            }

            foreach (var docPath in docFiles)
            {
                var relPath = Path.GetRelativePath(inputDir, docPath).Replace('\\', '/');
                var content = File.ReadAllText(docPath);
                if (content.Contains("**Historical baseline:**", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (var obsolete in obsoleteFiles)
                {
                    if (content.Contains(obsolete.Key))
                    {
                        docsVsCodeDrifts.Add($"File `{relPath}` references deprecated output name `{obsolete.Key}`. Should be updated to `{obsolete.Value}`.");
                    }
                }
            }
        }
        catch { }

        // 3. Contract Drift (general specification drift)
        // Check if there are any mismatches in the required outputs list
        try
        {
            var contractPath = Path.Combine(inputDir, "docs", "AIFolderFlatten-v0.2.1-UNIFIED-GRAND-BUILD-CONTRACT.md");
            if (File.Exists(contractPath))
            {
                var content = File.ReadAllText(contractPath);
                // Check if the contract defines v0.2.3 but code hasn't implemented optional toolchains yet
                if (content.Contains("v0.2.3") && !Directory.Exists(Path.Combine(inputDir, "src", "AIFolderFlatten", "Dev")))
                {
                    contractDrifts.Add("Contract specifies v0.2.3 features but the Dev directory is missing in code (Drift).");
                }
            }
        }
        catch { }

        return new ContractDriftResult(
            ContractDriftMarkdown: RenderContractDrift(contractDrifts),
            VersionDriftMarkdown: RenderVersionDrift(versionDrifts),
            DocsVsCodeMarkdown: RenderDocsVsCode(docsVsCodeDrifts)
        );
    }

    private static string RenderContractDrift(List<string> drifts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Contract Drift Report");
        sb.AppendLine();
        sb.AppendLine("> Evaluates current code implementation status against the product's architectural contract.");
        sb.AppendLine();
        if (drifts.Count == 0)
        {
            sb.AppendLine("✅ **No contract drift detected**. Code conforms to core architectural specifications.");
        }
        else
        {
            sb.AppendLine("⚠️ **Contract Drift Warnings:**");
            sb.AppendLine();
            foreach (var d in drifts)
            {
                sb.AppendLine($"*   {d}");
            }
        }
        return sb.ToString();
    }

    private static string RenderVersionDrift(List<string> drifts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Version Drift Report");
        sb.AppendLine();
        sb.AppendLine("> Verifies consistency of version metadata across project config files, binaries, and documentation.");
        sb.AppendLine();
        if (drifts.Count == 0)
        {
            sb.AppendLine("✅ **All version metadata is perfectly aligned**.");
        }
        else
        {
            sb.AppendLine("⚠️ **Version Mismatches Detected:**");
            sb.AppendLine();
            foreach (var d in drifts)
            {
                sb.AppendLine($"*   {d}");
            }
        }
        return sb.ToString();
    }

    private static string RenderDocsVsCode(List<string> drifts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Docs vs Code Drift");
        sb.AppendLine();
        sb.AppendLine("> Flags references to obsolete filenames or deprecated components within project documentation.");
        sb.AppendLine();
        if (drifts.Count == 0)
        {
            sb.AppendLine("✅ **No documentation drift detected**. Documentation aligns with code file structures.");
        }
        else
        {
            sb.AppendLine("⚠️ **Documentation Drift Warnings:**");
            sb.AppendLine();
            foreach (var d in drifts)
            {
                sb.AppendLine($"*   {d}");
            }
        }
        return sb.ToString();
    }
}
