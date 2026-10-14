using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Dev;

/// <summary>
/// Deterministic build dependency and framework analyzer.
/// No AI, network, or LLM runtime dependency.
/// </summary>
internal static class DependencyAnalyzer
{
    public sealed record DependencyResult(
        string DependencyMapMarkdown,
        string BuildCommandsMarkdown,
        string DependencyRisksMarkdown
    );

    public static DependencyResult Analyze(string inputDir)
    {
        var packages = new List<(string Language, string ProjectName, string DependencyName, string Version)>();
        var buildCommands = new List<string>();
        var risks = new List<string>();

        var csprojs = new List<string>();
        var packageJsons = new List<string>();
        var requirementsTxts = new List<string>();
        var pyprojects = new List<string>();

        try
        {
            if (Directory.Exists(inputDir))
            {
                csprojs.AddRange(Directory.GetFiles(inputDir, "*.csproj", SearchOption.AllDirectories));
                packageJsons.AddRange(Directory.GetFiles(inputDir, "package.json", SearchOption.AllDirectories));
                requirementsTxts.AddRange(Directory.GetFiles(inputDir, "requirements.txt", SearchOption.AllDirectories));
                pyprojects.AddRange(Directory.GetFiles(inputDir, "pyproject.toml", SearchOption.AllDirectories));
            }
        }
        catch { }

        // 1. Analyze .NET project dependencies
        foreach (var csproj in csprojs)
        {
            var projName = Path.GetFileName(csproj);
            try
            {
                var content = File.ReadAllText(csproj);
                
                // Target Framework
                var tfMatch = Regex.Match(content, @"<TargetFramework>([^<]+)</TargetFramework>");
                if (tfMatch.Success)
                {
                    var tf = tfMatch.Groups[1].Value.Trim();
                    packages.Add(("C#", projName, "Target Framework: " + tf, ""));
                    if (tf.Contains("netcoreapp") || tf.Contains("net5") || tf.Contains("net4"))
                    {
                        risks.Add($"Project `{projName}` targets older framework version `{tf}`. Consider upgrading to modern .NET 8.0 or newer.");
                    }
                }

                // PackageReferences
                var matches = Regex.Matches(content, @"<PackageReference\s+Include=""([^""]+)""\s+Version=""([^""]+)""\s*/>|<PackageReference\s+Version=""([^""]+)""\s+Include=""([^""]+)""\s*/>");
                foreach (Match m in matches)
                {
                    var name = m.Groups[1].Value;
                    var ver = m.Groups[2].Value;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = m.Groups[4].Value;
                        ver = m.Groups[3].Value;
                    }
                    packages.Add(("C#", projName, name, ver));
                }

                // ProjectReferences
                var projRefs = Regex.Matches(content, @"<ProjectReference\s+Include=""([^""]+)""\s*/>");
                foreach (Match m in projRefs)
                {
                    var refPath = m.Groups[1].Value;
                    packages.Add(("C#", projName, "Project Ref: " + Path.GetFileName(refPath), ""));
                }
            }
            catch (Exception ex)
            {
                risks.Add($"Error parsing .csproj `{projName}`: {ex.Message}");
            }
        }
        if (csprojs.Count > 0)
        {
            buildCommands.Add("dotnet restore");
            buildCommands.Add("dotnet build");
            buildCommands.Add("dotnet test");
        }

        // 2. Analyze Node.js package.json dependencies
        foreach (var packageJson in packageJsons)
        {
            var dirName = Path.GetFileName(Path.GetDirectoryName(packageJson) ?? "root");
            try
            {
                var content = File.ReadAllText(packageJson);
                
                // Deterministic crude JSON parsing to avoid pulling Newtonian JSON parser
                // Match dependencies section
                ParseJsonDeps(content, "dependencies", "JavaScript/TypeScript", dirName, packages);
                ParseJsonDeps(content, "devDependencies", "JavaScript/TypeScript", dirName, packages);

                // Check for scripts
                var scriptsMatch = Regex.Match(content, @"""scripts""\s*:\s*\{([^}]+)\}");
                if (scriptsMatch.Success)
                {
                    var scriptsBlock = scriptsMatch.Groups[1].Value;
                    var scriptKeys = Regex.Matches(scriptsBlock, @"""([^""]+)""\s*:");
                    foreach (Match m in scriptKeys)
                    {
                        buildCommands.Add($"npm run {m.Groups[1].Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                risks.Add($"Error parsing package.json in `{dirName}`: {ex.Message}");
            }
        }
        if (packageJsons.Count > 0 && !buildCommands.Contains("npm install"))
        {
            buildCommands.Insert(0, "npm install");
        }

        // 3. Analyze Python dependencies
        foreach (var req in requirementsTxts)
        {
            var dirName = Path.GetFileName(Path.GetDirectoryName(req) ?? "root");
            try
            {
                var lines = File.ReadLines(req);
                foreach (var line in lines)
                {
                    var clean = line.Trim();
                    if (string.IsNullOrEmpty(clean) || clean.StartsWith("#")) continue;

                    // Parse e.g. numpy==1.2.3 or numpy>=1.2.3
                    var parts = Regex.Split(clean, @"(==|>=|<=|>)");
                    if (parts.Length >= 3)
                    {
                        packages.Add(("Python", dirName + "/requirements.txt", parts[0].Trim(), parts[2].Trim()));
                    }
                    else
                    {
                        packages.Add(("Python", dirName + "/requirements.txt", clean, "unspecified"));
                    }
                }
            }
            catch (Exception ex)
            {
                risks.Add($"Error parsing requirements.txt in `{dirName}`: {ex.Message}");
            }
        }
        if (requirementsTxts.Count > 0)
        {
            buildCommands.Add("pip install -r requirements.txt");
        }

        // Parse pyproject.toml
        foreach (var pyproj in pyprojects)
        {
            var dirName = Path.GetFileName(Path.GetDirectoryName(pyproj) ?? "root");
            try
            {
                var content = File.ReadAllText(pyproj);
                var matches = Regex.Matches(content, @"dependencies\s*=\s*\[([^\]]+)\]");
                foreach (Match m in matches)
                {
                    var depList = m.Groups[1].Value;
                    var deps = Regex.Matches(depList, @"""([^""]+)""|'([^']+)'");
                    foreach (Match d in deps)
                    {
                        var dep = d.Groups[1].Value;
                        if (string.IsNullOrEmpty(dep)) dep = d.Groups[2].Value;
                        packages.Add(("Python", dirName + "/pyproject.toml", dep, "specified"));
                    }
                }
            }
            catch { }
        }

        // Heuristics for risks
        var uniqueLanguages = packages.Select(p => p.Language).Distinct().ToList();
        if (uniqueLanguages.Count > 1)
        {
            risks.Add($"Multi-language project detected ({string.Join(", ", uniqueLanguages)}). Ensure build toolchains are configured for all environments.");
        }

        return new DependencyResult(
            DependencyMapMarkdown: RenderDependencyMap(packages),
            BuildCommandsMarkdown: RenderBuildCommands(buildCommands),
            DependencyRisksMarkdown: RenderDependencyRisks(risks)
        );
    }

    private static void ParseJsonDeps(string content, string sectionName, string language, string projectName, List<(string Language, string ProjectName, string DependencyName, string Version)> packages)
    {
        var match = Regex.Match(content, @"""" + sectionName + @"""\s*:\s*\{([^}]+)\}");
        if (match.Success)
        {
            var block = match.Groups[1].Value;
            var entries = Regex.Matches(block, @"""([^""]+)""\s*:\s*""([^""]+)""");
            foreach (Match m in entries)
            {
                packages.Add((language, projectName, m.Groups[1].Value, m.Groups[2].Value));
            }
        }
    }

    private static string RenderDependencyMap(List<(string Language, string ProjectName, string DependencyName, string Version)> packages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Dependency Map");
        sb.AppendLine();
        sb.AppendLine("> Structured catalog of dependencies and platforms detected in the workspace.");
        sb.AppendLine();
        if (packages.Count == 0)
        {
            sb.AppendLine("No package managers or build files detected.");
        }
        else
        {
            sb.AppendLine("| Language/Tech | Module/Project | Dependency | Version |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (var p in packages.OrderBy(x => x.Language).ThenBy(x => x.ProjectName))
            {
                sb.AppendLine($"| {p.Language} | `{p.ProjectName}` | `{p.DependencyName}` | `{p.Version}` |");
            }
        }
        return sb.ToString();
    }

    private static string RenderBuildCommands(List<string> commands)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Build Commands");
        sb.AppendLine();
        sb.AppendLine("> Recommended command sequences for compiling, resolving dependencies, and testing the project based on detected platforms.");
        sb.AppendLine();
        if (commands.Count == 0)
        {
            sb.AppendLine("No standard build system detected (C#, JS, Python).");
        }
        else
        {
            sb.AppendLine("## Recommended Setup & Compilation");
            sb.AppendLine();
            sb.AppendLine("```bash");
            foreach (var c in commands.Distinct())
            {
                sb.AppendLine(c);
            }
            sb.AppendLine("```");
        }
        return sb.ToString();
    }

    private static string RenderDependencyRisks(List<string> risks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Dependency Risks");
        sb.AppendLine();
        sb.AppendLine("> Evaluates target frameworks, multi-language architectures, and potential build toolchain conflicts.");
        sb.AppendLine();
        if (risks.Count == 0)
        {
            sb.AppendLine("✅ **No framework or dependency risks detected**.");
        }
        else
        {
            sb.AppendLine("⚠️ **Dependency and Build Risks:**");
            sb.AppendLine();
            foreach (var r in risks)
            {
                sb.AppendLine($"*   {r}");
            }
        }
        return sb.ToString();
    }
}
