using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AIFolderFlatten.Post;
using AIFolderFlatten.Artifacts;
using AIFolderFlatten.Documents;
using AIFolderFlatten.Research;
using AIFolderFlatten.Modeling;
using AIFolderFlatten.Creative;
using AIFolderFlatten.ExpertSystem;

namespace AIFolderFlatten;

internal static class Program
{
    public const string AppVersion = "0.3.1-preview";

    internal static readonly HashSet<string> DefaultIncludeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".markdown", ".txt", ".cs", ".csproj", ".sln", ".json", ".jsonl",
        ".yaml", ".yml", ".xml", ".html", ".htm", ".css", ".scss", ".js", ".jsx",
        ".ts", ".tsx", ".py", ".ps1", ".bat", ".cmd", ".sh", ".sql", ".ini", ".cfg",
        ".toml", ".csv", ".log", ".java", ".kt", ".kts", ".swift", ".cpp", ".c", ".h",
        ".hpp", ".go", ".rs", ".php", ".rb", ".r", ".dockerfile", ".gitignore", ".aiignore",
        ".tex", ".bib", ".ris", ".ipynb", ".wl", ".wls"
    };

    internal static readonly HashSet<string> DefaultSpecialFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dockerfile", "Makefile", "README", "LICENSE", "CHANGELOG", "Cargo.toml", "go.mod", "package.json", "tsconfig.json", "pyproject.toml", "requirements.txt"
    };

    internal static readonly HashSet<string> DefaultExcludeDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".svn", ".hg", ".vs", ".idea", ".vscode", "bin", "obj", "node_modules",
        "dist", "build", "out", "target", ".next", ".nuxt", ".cache", "__pycache__",
        ".pytest_cache", ".mypy_cache", ".venv", "venv", "env", "coverage", ".turbo",
        "_ai_flatten_exports", "release", "publish", "artifacts"
    };

    [STAThread]
    public static int Main(string[] args)
    {
        AppConfig config;
        try
        {
            args = ExpandInputListFile(args);
            config = AppConfig.FromArgs(args);
        }
        catch (Exception ex)
        {
            return Fail($"Argument error: {ex.Message}", notify: true, exitCode: ExitCodes.ArgumentError);
        }

        try
        {
            if (config.ShowHelp)
            {
                Console.WriteLine(HelpText());
                return ExitCodes.Success;
            }

            if (config.ShowVersion)
            {
                Console.WriteLine($"AIFolderFlatten {AppVersion}");
                return ExitCodes.Success;
            }

            // Expert-System boundary enforcement (v0.2.2)
            ExpertSystemBoundary.AssertExpertSystemMode(
                config.ExpertSystemOnly,
                config.AllowLLM,
                config.AllowNetwork,
                config.AllowEmbeddings);

            // Toolchain doctor subcommand
            if (config.ToolchainDoctor || (args.Length >= 2 && args[0] == "toolchain" && args[1] == "doctor"))
            {
                var doctorOutDir = !string.IsNullOrWhiteSpace(config.OutputPath) && !config.OutputPath.Equals("auto", StringComparison.OrdinalIgnoreCase)
                    ? config.OutputPath
                    : Directory.GetCurrentDirectory();
                var statusMd = ToolchainDoctor.Run(doctorOutDir);
                Console.WriteLine(statusMd);
                Console.WriteLine($"[AI Seeder] TOOLCHAIN_STATUS.md written to: {doctorOutDir}");
                return ExitCodes.Success;
            }

            if (config.SnapshotComplete && config.TaskContext != null)
                throw new AIFolderFlattenException("Choose either --snapshot-complete or --task-context for an export.", ExitCodes.ArgumentError);

            if (config.VerifyTaskContext != null)
            {
                var report = TaskContext.TaskContextCompiler.Verify(config.VerifyTaskContext, config.InputPath);
                foreach (var error in report.Errors) Console.Error.WriteLine("- " + error);
                Console.WriteLine(report.Passed ? "Task context verification PASSED" : "Task context verification FAILED");
                return report.Passed ? ExitCodes.Success : ExitCodes.ManifestVerificationFailure;
            }

            if (config.VerifySnapshotPath != null)
            {
                var verification = Output.CompleteSnapshotWriter.Verify(config.VerifySnapshotPath);
                foreach (var error in verification.Errors) Console.Error.WriteLine($"- ERROR: {error}");
                Console.WriteLine(verification.Passed ? "Complete snapshot verification PASSED." : "Complete snapshot verification FAILED.");
                return verification.Passed ? ExitCodes.Success : ExitCodes.ManifestVerificationFailure;
            }

            if (config.VerifyManifestPath != null)
            {
                var verifyResult = ManifestVerifier.Verify(config.VerifyManifestPath, AppVersion);
                if (verifyResult.Passed)
                {
                    Console.WriteLine("Manifest verification PASSED.");
                    return ExitCodes.Success;
                }
                Console.Error.WriteLine("Manifest verification FAILED:");
                foreach (var err in verifyResult.Errors) Console.Error.WriteLine($"- ERROR: {err}");
                foreach (var wrn in verifyResult.Warnings) Console.Error.WriteLine($"- WARNING: {wrn}");
                return ExitCodes.ManifestVerificationFailure;
            }

            if (string.IsNullOrWhiteSpace(config.InputPath))
            {
                return Fail("Missing input folder. Use --input <folder-path>.", config.Notify, ExitCodes.ArgumentError);
            }

            var selection = Output.SelectionInputResolver.Resolve(config.InputPath);
            config.IsMultiSelect = selection.IsMultiSelect;
            config.SelectedPaths = selection.SelectedAbsolutePaths;

            var inputDir = selection.CommonRoot;

            if (config.TaskContext != null)
            {
                if (string.IsNullOrWhiteSpace(config.TaskContext))
                    return Fail("--task-context requires a task description.", config.Notify, ExitCodes.ArgumentError);
                if (config.GitRef != null || config.GitChanged || config.GitStaged)
                    return Fail("--task-context does not support Git subset flags; inspect the working tree directly.", config.Notify, ExitCodes.ArgumentError);
                Console.WriteLine(TaskContext.TaskContextCompiler.Compile(inputDir, config));
                return ExitCodes.Success;
            }

            if (config.SnapshotComplete)
            {
                if (config.GitRef != null || config.GitChanged || config.GitStaged)
                    throw new AIFolderFlattenException("--snapshot-complete does not support --git-ref, --git-changed, or --git-staged. Export the actual working folder without Git selection filters.", ExitCodes.ArgumentError);
                var result = Output.CompleteSnapshotWriter.Write(inputDir, config);
                Console.WriteLine(result);
                if (config.Notify)
                    MessageBox.Show(result, $"AI Flatten {AppVersion} — Complete Source Snapshot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return ExitCodes.Success;
            }

            if (config.GitRef != null)
            {
                return GitContext.RunGitCompile(inputDir, config, RunCompile, Fail);
            }

            return RunCompile(inputDir, config);
        }
        catch (AIFolderFlattenException ex)
        {
            return Fail(ex.Message, config?.Notify ?? false, ex.ExitCode);
        }
        catch (Exception ex)
        {
            return Fail($"Unexpected error: {ex.Message}", config?.Notify ?? false, ExitCodes.GeneralFailure);
        }
    }

    internal static string[] ExpandInputListFile(string[] args)
    {
        var expanded = new List<string>(args.Length);

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            string? listPath = null;

            if (argument.Equals("--input-list-file", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException("--input-list-file requires a file path.");
                }
                listPath = args[++index];
            }
            else if (argument.StartsWith("--input-list-file=", StringComparison.OrdinalIgnoreCase))
            {
                listPath = argument[(argument.IndexOf('=') + 1)..];
            }

            if (listPath == null)
            {
                expanded.Add(argument);
                continue;
            }

            try
            {
                if (!File.Exists(listPath))
                {
                    throw new ArgumentException($"Input list file not found: {listPath}");
                }

                var paths = File.ReadAllLines(listPath)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => path.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (paths.Length == 0)
                {
                    throw new ArgumentException("Input list file did not contain any paths.");
                }

                expanded.Add("--input");
                expanded.Add(string.Join('|', paths));
            }
            finally
            {
                try { File.Delete(listPath); } catch { }
            }
        }

        return expanded.ToArray();
    }

    private static OrientationState? _lastOrientation;
    private static List<ArtifactIR>? _lastArtifacts;
    private static string? _lastDomain;
    private static string? _lastLens;

    private static ContextPackage BuildPackage(string inputDir, AppConfig config)
    {
        var skipped = new List<SkippedFile>();
        var artifactSkipped = new List<SkippedFile>();
        var artifacts = ArtifactScanner.Scan(inputDir, config, artifactSkipped, out var textContents);
        var totalFiles = artifacts.Count + artifactSkipped
            .Select(s => s.RelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var scanState = new ScanState(
            TotalFilesSeen: totalFiles,
            IncludedTextFiles: artifacts.Count(a => a.Status == "parsed"),
            SkippedFiles: artifactSkipped
                .Select(s => s.RelativePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            UnknownArtifacts: artifacts.Count(a => a.ArtifactType == "unknown"),
            MediaArtifacts: artifacts.Count(a => a.ArtifactType == "creative" && a.Extension != ".pd" && a.Extension != ".scd"),
            ResearchSignals: artifacts.Count(a => a.ArtifactType == "research" || a.Extension == ".bib" || a.Extension == ".ris"),
            CreativeSignals: artifacts.Count(a => a.ArtifactType == "creative"),
            CodeSignals: artifacts.Count(a => a.ArtifactType == "code"),
            ObsidianSignals: artifacts.Count(a => a.Extension == ".md" && a.Path.Contains(".obsidian")),
            DocumentSignals: artifacts.Count(a => a.ArtifactType == "document"),
            ModelingSignals: artifacts.Count(a => a.ArtifactType == "modeling")
        );

        var orientation = PostOrientationEngine.Orient(inputDir, config, scanState, textContents);
        var domain = config.Auto ? orientation.SelectedDomain : config.Domain;
        var lens = config.Auto ? orientation.SelectedLens : config.Lens;

        _lastOrientation = orientation;
        _lastArtifacts = artifacts;
        _lastDomain = domain;
        _lastLens = lens;

        var candidates = FolderScanner.EnumerateCandidates(inputDir, config, skipped);
        var raw = new List<(string RelativePath, long Bytes, DateTime ModifiedUtc, string Content, FileAnalysis Analysis, IReadOnlyDictionary<string, int> Redactions)>();

        foreach (var candidate in candidates)
        {
            if (config.GitStaged || config.GitChanged)
            {
                var gitStagedPaths = config.GitStaged ? GitContext.GetStagedFiles(inputDir).Select(p => p.Replace('\\', '/')).ToHashSet() : new HashSet<string>();
                var gitChangedPaths = config.GitChanged ? GitContext.GetChangedFiles(inputDir).Select(p => p.Replace('\\', '/')).ToHashSet() : new HashSet<string>();
                if (!(config.GitStaged && gitStagedPaths.Contains(candidate.RelativePath)) && !(config.GitChanged && gitChangedPaths.Contains(candidate.RelativePath)))
                    continue;
            }

            try
            {
                var fileInfo = new FileInfo(candidate.AbsolutePath);
                if (!fileInfo.Exists) continue;
                if (fileInfo.Length > config.MaxFileBytes)
                {
                    skipped.Add(new SkippedFile(candidate.RelativePath, $"too large: {fileInfo.Length} bytes > {config.MaxFileBytes} bytes"));
                    continue;
                }
                if (FileText.LooksBinary(candidate.AbsolutePath))
                {
                    skipped.Add(new SkippedFile(candidate.RelativePath, "binary or non-text signature"));
                    continue;
                }

                var content = FileText.ReadTextFile(candidate.AbsolutePath);
                var redacted = Redactor.Redact(content, config);
                content = redacted.Content;

                if (config.PrivacyStrict && PrivacyScanner.ContainsRawSecrets(content, out var reason))
                    throw new PrivacyLeakException($"Privacy violation in '{candidate.RelativePath}': {reason}");

                var analysis = FileAnalyzer.Analyze(candidate.RelativePath, content, config);
                raw.Add((candidate.RelativePath, fileInfo.Length, fileInfo.LastWriteTimeUtc, content, analysis, redacted.Counts));
            }
            catch (AIFolderFlattenException) { throw; }
            catch { skipped.Add(new SkippedFile(candidate.RelativePath, "access/read error")); }
        }

        var selected = BudgetSelector.SelectByBudget(raw, skipped, config);
        var snapshots = selected
            .OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select((x, index) => new FileSnapshot(
                Id: $"F{index + 1:0000}",
                RelativePath: x.RelativePath,
                ByteLength: x.Bytes,
                ModifiedUtc: x.ModifiedUtc,
                Content: x.Content,
                Analysis: x.Analysis,
                Redactions: x.Redactions))
            .ToList();

        var normalizedSkipped = skipped
            .GroupBy(
                s => s.RelativePath.Replace('\\', '/'),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new SkippedFile(
                group.Key,
                string.Join("; ", group
                    .Select(s => s.Reason)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(reason => reason, StringComparer.OrdinalIgnoreCase))))
            .OrderBy(s => s.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ContextPackage(
            SourceRoot: inputDir,
            RootName: new DirectoryInfo(inputDir).Name,
            Mode: config.Mode,
            Format: config.Format,
            ContentLevel: config.ContentLevel,
            GeneratedAt: DateTimeOffset.Now,
            Files: snapshots,
            Skipped: normalizedSkipped,
            ProjectSignals: FileAnalyzer.BuildProjectSignals(snapshots),
            SuggestedCommands: FileAnalyzer.SuggestedCommands(snapshots),
            RedactionEnabled: config.RedactSecrets || config.RedactLocalPaths,
            LineNumbersEnabled: config.IncludeLineNumbers,
            PromptInjectionScanEnabled: config.ScanPromptInjection,
            MaxTotalChars: config.MaxTotalChars,
            SplitSizeChars: config.SplitSizeChars,
            AppVersion: AppVersion,
            Git: GitContext.BuildGitInfo(inputDir, config, snapshots.Select(s => s.RelativePath).ToList())
        );
    }

    private static int RunCompile(string inputDir, AppConfig config)
    {
        if (!Directory.Exists(inputDir))
            return Fail($"Input folder not found: {inputDir}", config.Notify, ExitCodes.ScanFailure);

        // Pre-flight Output Self-Exclusion Check
        var paths = Output.OutputNamingPolicy.DeterminePaths(
            inputDir, 
            new DirectoryInfo(inputDir).Name, 
            config.OutputPath, 
            config.OutputLayout, 
            config.MasterFormat,
            null, // gitState
            config.IsMultiSelect
        );
        string targetOutDir = paths.PackDirectoryPath ?? Path.GetDirectoryName(paths.MasterFilePath) ?? inputDir;
        Output.OutputSelfExclusionPolicy.ValidateAndExclude(inputDir, targetOutDir, config, out var exclusionWarning);
        if (exclusionWarning != null) Console.WriteLine(exclusionWarning);

        var package = BuildPackage(inputDir, config);
        var orientation = _lastOrientation!;
        var artifacts = _lastArtifacts!;
        var domain = _lastDomain!;
        var lens = _lastLens!;

        if (config.DryRun)
        {
            Console.WriteLine("--- DRY RUN ACTIVE: No files will be written ---");
            Console.WriteLine($"Mode: {package.Mode}");
            Console.WriteLine($"Format: {package.Format}");
            Console.WriteLine($"Resolved Domain: {domain}");
            Console.WriteLine($"Resolved Lens: {lens}");
            Console.WriteLine($"Files candidate to include: {package.Files.Count}");
            Console.WriteLine($"Files skipped: {package.Skipped.Count}");
            Console.WriteLine($"Total estimated characters: {package.Files.Sum(f => f.Content.Length):N0}");
            return ExitCodes.Success;
        }

        var result = ExportWriter.WriteOutputPost(inputDir, package, config, orientation, artifacts, domain, lens);

        var mode = config.OpenAfterExportMode.ToLowerInvariant();
        if (mode != "never" && result.OutputPaths.Count > 0)
        {
            var primaryPath = result.OutputPaths[0]; // Master file or zip
            var packDir = result.OutputPaths.Count > 1 ? result.OutputPaths[1] : null;

            string targetToOpen = primaryPath;

            if (mode == "folder")
            {
                targetToOpen = packDir ?? Path.GetDirectoryName(primaryPath) ?? primaryPath;
            }
            else if (mode == "file")
            {
                targetToOpen = primaryPath;
            }
            else if (mode == "ask")
            {
                var askResult = MessageBox.Show($"Export complete.\n\nOpen output folder/file?", "AI Folder Flatten", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (askResult == DialogResult.Yes)
                {
                    targetToOpen = packDir ?? Path.GetDirectoryName(primaryPath) ?? primaryPath;
                }
                else
                {
                    targetToOpen = null;
                }
            }

            if (targetToOpen != null)
            {
                ExportWriter.OpenInExplorer(targetToOpen);
            }
        }

        var message = $"AI folder context compile complete.\n\nOutput:\n{string.Join("\n", result.OutputPaths)}\n\nFiles included: {result.IncludedCount}\nFiles skipped: {result.SkippedCount}\nCharacters: {result.TotalCharacters:N0}";
        Console.WriteLine(message);

        if (config.Notify)
            MessageBox.Show(message, "AI Folder Flatten", MessageBoxButtons.OK, MessageBoxIcon.Information);

        return ExitCodes.Success;
    }

    private static int Fail(string message, bool notify, int exitCode)
    {
        Console.Error.WriteLine(message);
        if (notify)
        {
            MessageBox.Show(message, "AI Folder Flatten Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return exitCode;
    }

    private static string HelpText() => $"""
AIFolderFlatten v{AppVersion} - local-first expert-system context compiler

Usage:
  AIFolderFlatten.exe --version
  AIFolderFlatten.exe --input <folder> --profile post
  AIFolderFlatten.exe --input <folder> --profile overview --format md
  AIFolderFlatten.exe --input <folder> --profile debug
  AIFolderFlatten.exe --verify-manifest <path>

Core options:
  --profile <name>                  post, overview, debug, architecture, obsidian, research, prompt-pack.
  --mode <name>                     post, overview, debug, refactor, architecture, code-review,
                                    obsidian, research, prompt-pack, full.
  --format <name>                   md, txt, or pack.
  --output <path>                   Output file/directory, or auto.
  --domain <name>                   Target domains: auto, code, obsidian, research, documents, modeling, creative, mixed, unknown.
  --lens <name>                     Target lenses: auto, overview, debug, architecture, obsidian.
  --orientation "<hint>"            A text hint describing folder purpose.
  --preset <preset>                 Presets: research-intelligence, creative-reference, math-modeling, document-ledger.
  --detail <detail>                 Level of detail: basic, standard, full.
  --git-ref <ref>|choose            Compile a Git revision snapshot.
  --git-changed true|false          Limit context to changed files.
  --git-staged true|false           Limit context to staged files.
  --redact true|false               Redact recognized secrets (default: true).
  --redact-paths true|false         Redact absolute Windows paths.
  --privacy-strict true|false       Fail before writing if raw secrets remain.
  --dry-run true|false              Verify and scan folders without exporting.
  --verify-manifest <path>          Verify integrity of the exported MANIFEST.json file.
  --snapshot-complete true          Export every eligible source file, without context budget or file-size cap,
                                    into chunked Markdown + exact JSONL; --output is a destination PARENT folder.
  --split-size-chars <number>       Maximum characters per snapshot part (minimum 4096; default 300000).
  --verify-snapshot <folder>        Reassemble and hash-check SOURCE_MANIFEST.json and all JSONL parts.
                                    Snapshot-complete ignores --max-total-chars and --max-file-bytes.
                                    Exclusions/unsupported/binary files remain policy-scoped; audit INDEX.md.
                                    Redaction defaults ON: use --redact false only for trusted local storage.
  --explorer-snapshot true          Complete snapshot of Explorer selection, beside its source root.
  --toolchain-doctor                Write local parser/tool availability status.

Task context and human review (deterministic, local only):
  --task-context "<goal>"           Compile source evidence relevant to a task.
  --task-budget-chars <number>      Character budget for task source (default 180000).
  --task-pins <path,path>           Prioritize exact relative source paths.
  --task-cockpit true|false         Generate offline CONTEXT_REVIEW.html (default true).
  --verify-task-context <folder>   Verify task artifacts; --input also checks source freshness.

Boundary options:
  --expert-system-only true         Required; false is rejected.
  --allow-llm false                 Required; true is rejected.
  --allow-network false             Required; true is rejected.
  --allow-embeddings false          Required; true is rejected.
""";
}
