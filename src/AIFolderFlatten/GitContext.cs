using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace AIFolderFlatten;

internal static class GitContext
{
    public static bool IsGitRepo(string workingDir)
    {
        try
        {
            RunCommand(workingDir, "git", "rev-parse --is-inside-work-tree", out var exitCode);
            return exitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static string? GetBranch(string workingDir)
    {
        try
        {
            var res = RunCommand(workingDir, "git", "rev-parse --abbrev-ref HEAD", out var exitCode);
            return exitCode == 0 ? res.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetHeadSha(string workingDir)
    {
        try
        {
            var res = RunCommand(workingDir, "git", "rev-parse HEAD", out var exitCode);
            return exitCode == 0 ? res.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    public static IReadOnlyList<string> GetChangedFiles(string workingDir)
    {
        try
        {
            var res = RunCommand(workingDir, "git", "diff --name-only HEAD", out var exitCode);
            if (exitCode != 0) return Array.Empty<string>();
            return res.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static IReadOnlyList<string> GetStagedFiles(string workingDir)
    {
        try
        {
            var res = RunCommand(workingDir, "git", "diff --name-only --cached", out var exitCode);
            if (exitCode != 0) return Array.Empty<string>();
            return res.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static GitInfo BuildGitInfo(string workingDir, AppConfig config, IReadOnlyList<string> includedPaths)
    {
        var isRepo = IsGitRepo(workingDir);
        if (!isRepo)
        {
            return new GitInfo(
                IsGitRepo: false,
                Branch: null,
                HeadSha: null,
                Mode: "none",
                Ref: null,
                IncludedPaths: Array.Empty<string>(),
                Warnings: new[] { "Not a git repository" }
            );
        }

        var branch = GetBranch(workingDir);
        var sha = GetHeadSha(workingDir);
        var mode = "git_working_folder";
        
        if (config.GitRef != null)
        {
            mode = config.GitRef.Equals("HEAD", StringComparison.OrdinalIgnoreCase) ? "git_head" : "git_ref";
        }
        else if (config.GitStaged) mode = "git_staged";
        else if (config.GitChanged) mode = "git_changed";

        return new GitInfo(
            IsGitRepo: true,
            Branch: branch,
            HeadSha: sha,
            Mode: mode,
            Ref: config.GitRef,
            IncludedPaths: includedPaths,
            Warnings: Array.Empty<string>()
        );
    }

    public static string RenderDeltaContext(string workingDir, string branch, string sha, string mode, string? gitRef, IReadOnlyList<string> changed, IReadOnlyList<string> staged)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Delta Context Report");
        sb.AppendLine();
        sb.AppendLine($"- **Branch**: `{branch}`");
        sb.AppendLine($"- **HEAD SHA**: `{sha}`");
        sb.AppendLine($"- **Delta Mode**: `{mode}`");
        if (gitRef != null)
        {
            sb.AppendLine($"- **Target Reference**: `{gitRef}`");
        }
        sb.AppendLine($"- **Staged Files Count**: {staged.Count}");
        sb.AppendLine($"- **Changed Files Count**: {changed.Count}");
        sb.AppendLine();
        return sb.ToString();
    }

    public static string RenderChangedFiles(IReadOnlyList<string> files)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Changed Files (Working Directory)");
        sb.AppendLine();
        if (files.Count == 0)
        {
            sb.AppendLine("No modified files in the working directory.");
        }
        else
        {
            sb.AppendLine("| File Path |");
            sb.AppendLine("| :--- |");
            foreach (var f in files)
            {
                sb.AppendLine($"| `{f}` |");
            }
        }
        return sb.ToString();
    }

    public static string RenderStagedFiles(IReadOnlyList<string> files)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Staged Files (Index)");
        sb.AppendLine();
        if (files.Count == 0)
        {
            sb.AppendLine("No files staged for commit.");
        }
        else
        {
            sb.AppendLine("| File Path |");
            sb.AppendLine("| :--- |");
            foreach (var f in files)
            {
                sb.AppendLine($"| `{f}` |");
            }
        }
        return sb.ToString();
    }

    public static string RenderDiffSummary(string workingDir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Diff Summary");
        sb.AppendLine();
        
        try
        {
            var stat = RunCommand(workingDir, "git", "diff --stat HEAD", out var exitCode);
            if (exitCode == 0 && !string.IsNullOrWhiteSpace(stat))
            {
                sb.AppendLine("## Working Tree Diff Stat");
                sb.AppendLine("```text");
                sb.AppendLine(stat.Trim());
                sb.AppendLine("```");
            }

            var stagedStat = RunCommand(workingDir, "git", "diff --stat --cached", out var stagedExitCode);
            if (stagedExitCode == 0 && !string.IsNullOrWhiteSpace(stagedStat))
            {
                sb.AppendLine("## Staged Diff Stat");
                sb.AppendLine("```text");
                sb.AppendLine(stagedStat.Trim());
                sb.AppendLine("```");
            }

            if (string.IsNullOrWhiteSpace(stat) && string.IsNullOrWhiteSpace(stagedStat))
            {
                sb.AppendLine("No differences detected (working tree and index are clean).");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error retrieving diff stat: {ex.Message}");
        }

        return sb.ToString();
    }

    public static string RenderPatchHandoffPrompt(string branch, string sha, string mode, IReadOnlyList<string> changed, IReadOnlyList<string> staged)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Patch Handoff Prompt");
        sb.AppendLine();
        sb.AppendLine("> This is a deterministic prompt template generated by AI Seeder.");
        sb.AppendLine();
        sb.AppendLine("## Context");
        sb.AppendLine();
        sb.AppendLine($"- **Branch**: `{branch}`");
        sb.AppendLine($"- **HEAD SHA**: `{sha}`");
        sb.AppendLine($"- **Delta Mode**: `{mode}`");
        sb.AppendLine();
        sb.AppendLine("## Changes Included");
        sb.AppendLine();
        if (staged.Count > 0)
        {
            sb.AppendLine("### Staged Files");
            foreach (var f in staged)
            {
                sb.AppendLine($"- `{f}`");
            }
            sb.AppendLine();
        }
        if (changed.Count > 0)
        {
            sb.AppendLine("### Changed Files");
            foreach (var f in changed)
            {
                sb.AppendLine($"- `{f}`");
            }
            sb.AppendLine();
        }
        if (staged.Count == 0 && changed.Count == 0)
        {
            sb.AppendLine("_No changed or staged files in the patch._");
            sb.AppendLine();
        }
        sb.AppendLine("## Suggested Instructions for Developer/LLM");
        sb.AppendLine();
        sb.AppendLine("Review the changes in the provided diffs and staged/changed file listings.");
        sb.AppendLine("Validate that all modifications align with project requirements and coding standards.");
        sb.AppendLine();
        return sb.ToString();
    }


    public static string? ChooseGitRef(string workingDir)
    {
        try
        {
            var refsRaw = RunCommand(workingDir, "git", "for-each-ref --format=\"%(refname:short)\" refs/heads/ refs/tags/", out var exitCode);
            if (exitCode != 0)
            {
                Console.WriteLine("Error: Failed to retrieve git branches/tags.");
                return null;
            }

            var refs = refsRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Distinct()
                .ToList();

            Console.WriteLine("\n--- Select Git Revision to Compile ---");
            Console.WriteLine("[0] HEAD (Current tracked state)");
            for (var i = 0; i < refs.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {refs[i]}");
            }
            Console.Write($"Select a revision [0-{refs.Count}] (Default: 0, or type custom branch/tag/hash): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input == "0")
            {
                return "HEAD";
            }

            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= refs.Count)
            {
                return refs[idx - 1];
            }

            return input;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during git selection: {ex.Message}");
            return null;
        }
    }

    public static int RunGitCompile(string inputDir, AppConfig config, Func<string, AppConfig, int> compileAction, Func<string, bool, int, int> failAction)
    {
        if (!IsGitRepo(inputDir))
        {
            return failAction($"Input folder is not a Git repository: {inputDir}", config.Notify, ExitCodes.GitFailure);
        }

        var selectedRef = config.GitRef!;
        if (selectedRef.Equals("choose", StringComparison.OrdinalIgnoreCase) || selectedRef.Equals("select", StringComparison.OrdinalIgnoreCase))
        {
            var chosen = ChooseGitRef(inputDir);
            if (chosen == null) return ExitCodes.Success;
            selectedRef = chosen;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlatten_" + Guid.NewGuid().ToString("N"));
        var tempZip = tempDir + ".zip";

        try
        {
            Directory.CreateDirectory(tempDir);

            Console.WriteLine($"Archiving git revision '{selectedRef}'...");
            RunCommand(inputDir, "git", $"archive --format=zip --output=\"{tempZip}\" {selectedRef}", out var archiveExitCode);
            if (archiveExitCode != 0)
            {
                return failAction($"Git archive failed for revision '{selectedRef}'. Ensure that the branch, tag, or commit hash exists.", config.Notify, ExitCodes.GitFailure);
            }

            ZipFile.ExtractToDirectory(tempZip, tempDir);

            // Re-run compilation pipeline on the unzipped directory
            // For output directories, redirect output relative to original inputDir parent
            return compileAction(tempDir, config);
        }
        catch (Exception ex)
        {
            return failAction($"Git snapshot compilation failed: {ex.Message}", config.Notify, ExitCodes.GitFailure);
        }
        finally
        {
            try
            {
                if (File.Exists(tempZip)) File.Delete(tempZip);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
            }
            catch { }
        }
    }

    public static string RunCommand(string workingDir, string fileName, string arguments, out int exitCode)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            exitCode = process.ExitCode;
            return stdout;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new ExportException($"Failed to start process '{fileName}'. Ensure it is installed and in your system PATH. Detail: {ex.Message}");
        }
    }
}
