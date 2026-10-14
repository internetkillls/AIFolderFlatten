using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIFolderFlatten;

internal sealed class AppConfig
{
    public string? InputPath { get; private init; }
    public string? GitRef { get; private init; }
    public bool GitChanged { get; private init; }
    public bool GitStaged { get; private init; }
    public bool GitSummary { get; private init; }
    public bool PrivacyStrict { get; private init; }
    public bool DryRun { get; private init; }
    public string? TaskContext { get; private init; }
    public long TaskBudgetChars { get; private init; } = 180_000;
    public string? TaskPins { get; private init; }
    public string? VerifyTaskContext { get; private init; }
    public bool TaskCockpit { get; private init; } = true;
    // Opt-in lossless eligible-source export, independent of AI context budgeting.
    public bool SnapshotComplete { get; private init; }
    public string? VerifySnapshotPath { get; private init; }
    public string? VerifyManifestPath { get; private init; }
    public bool IsMultiSelect { get; set; }
    public List<string> SelectedPaths { get; set; } = new();
    public string Format { get; private init; } = "md"; // md | txt | pack
    public string Mode { get; private init; } = "overview";
    public string OutputPath { get; private init; } = "auto";
    public string ContentLevel { get; private init; } = "full"; // full | outline | signatures
    public long MaxFileBytes { get; private init; } = 1_048_576;
    public long MaxTotalChars { get; private init; } = 2_000_000;
    public long SplitSizeChars { get; private init; } = 300_000;
    public bool SplitOutput { get; private init; }
    public bool IncludeLineNumbers { get; private init; }
    public bool RedactSecrets { get; private init; } = true;
    public bool RedactLocalPaths { get; private init; }
    public bool RespectAiIgnore { get; private init; } = true;
    public bool RespectGitIgnore { get; private init; } = true;
    public bool IncludeHidden { get; private init; }
    public bool ScanPromptInjection { get; private init; } = true;
    public bool OpenAfterExport { get; private init; }
    public bool Notify { get; private init; }
    public bool ShowHelp { get; private init; }
    public bool ShowVersion { get; private init; }
    public string? PromptStarter { get; private init; }
    public HashSet<string> IncludeExtensions { get; private init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludeDirs { get; private init; } = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<string> IncludeGlobs { get; private init; } = Array.Empty<string>();
    public IReadOnlyList<string> ExcludeGlobs { get; private init; } = Array.Empty<string>();

    // v0.2.4b Master Flatten & Output behavior
    public OutputLayout OutputLayout { get; private init; } = OutputLayout.Single;
    public string MasterFormat { get; private init; } = "md";
    public OutputRootMode OutputRootMode { get; private init; } = OutputRootMode.SingleFileNextToSource;
    public OutputRootMode PackOutputRootMode { get; private init; } = OutputRootMode.DedicatedOutputFolder;
    public bool IncludeGenerated { get; private init; } = false;
    public bool AllowOutputInsideInput { get; private init; } = false;
    public string OpenAfterExportMode { get; private init; } = "never"; // never, folder, file, ask
    public string? OpenOutput { get; private init; }

    // Consolidated POST-specific Options
    public bool Auto { get; private init; }
    public string Domain { get; private init; } = "auto"; // auto | code | obsidian | research | documents | modeling | creative | mixed | unknown
    public string Lens { get; private init; } = "auto";   // auto | overview | debug | architecture | obsidian
    public string? Orientation { get; private init; }
    public string? Preset { get; private init; }
    public string Detail { get; private init; } = "standard"; // basic | standard | full

    // Expert-System-Only boundary flags (v0.2.2)
    /// <summary>When true (default), enforces the deterministic expert-system boundary. No LLM/network/AI calls allowed.</summary>
    public bool ExpertSystemOnly { get; private init; } = true;
    /// <summary>Must be false in all production use cases. Setting to true in expert-system mode throws at runtime.</summary>
    public bool AllowLLM { get; private init; } = false;
    /// <summary>Must be false in all production use cases. Setting to true in expert-system mode throws at runtime.</summary>
    public bool AllowNetwork { get; private init; } = false;
    /// <summary>Must be false in all production use cases. Setting to true in expert-system mode throws at runtime.</summary>
    public bool AllowEmbeddings { get; private init; } = false;
    /// <summary>When true, runs toolchain doctor and exits without generating a pack.</summary>
    public bool ToolchainDoctor { get; private init; } = false;

    public static AppConfig FromArgs(string[] args)
    {
        var cliValues = ParseArgs(args);
        var inputPath = GetNullable(cliValues, "input", cliValues.TryGetValue("_0", out var first) ? first : null);
        
        var fileValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(inputPath) && Directory.Exists(inputPath))
        {
            fileValues = ProfileConfig.LoadFromFile(inputPath);
        }
        else
        {
            fileValues = ProfileConfig.LoadFromFile(Directory.GetCurrentDirectory());
        }

        var profile = (GetNullable(cliValues, "profile", null) ?? GetNullable(fileValues, "profile", "")) ?? "";
        profile = profile.Trim().ToLowerInvariant();

        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // 1. Profile defaults (lowest precedence)
        ApplyProfileDefaults(profile, resolved);

        // 2. Overwrite with .ai-flatten.json values (medium precedence)
        foreach (var pair in fileValues)
        {
            resolved[pair.Key] = pair.Value;
        }

        // 3. Overwrite with CLI args (highest precedence)
        foreach (var pair in cliValues)
        {
            resolved[pair.Key] = pair.Value;
        }

        // Explorer offers one action. Ignore routing settings saved by older
        // context-pack profiles, while preserving inclusion and privacy policies.
        if (ParseBool(Get(cliValues, "explorer-snapshot", "false"), "explorer-snapshot"))
        {
            resolved["snapshot-complete"] = "true";
            resolved["output"] = "auto";
            resolved["split-size-chars"] = "300000";
            foreach (var key in new[] { "task-context", "verify-task-context", "verify-snapshot", "verify-manifest", "git-ref", "prompt-starter", "preset", "open-output", "toolchain-doctor", "help", "h", "?", "version" })
                resolved.Remove(key);
            foreach (var key in new[] { "git-changed", "git-staged", "dry-run" })
                resolved[key] = "false";
        }

        // Apply preset mapping shortcuts
        var preset = GetNullable(resolved, "preset", null)?.ToLowerInvariant();
        if (preset != null)
        {
            resolved["format"] = "pack";
            switch (preset)
            {
                case "research-intelligence":
                    resolved["domain"] = "research";
                    break;
                case "creative-reference":
                    resolved["domain"] = "creative";
                    break;
                case "math-modeling":
                    resolved["domain"] = "modeling";
                    break;
                case "document-ledger":
                    resolved["domain"] = "documents";
                    break;
            }
        }

        var include = new HashSet<string>(Program.DefaultIncludeExtensions, StringComparer.OrdinalIgnoreCase);
        var exclude = new HashSet<string>(Program.DefaultExcludeDirs, StringComparer.OrdinalIgnoreCase);

        foreach (var ext in SplitList(Get(resolved, "include-ext", "")))
        {
            include.Add(ext.StartsWith('.') ? ext : "." + ext);
        }

        foreach (var dir in SplitList(Get(resolved, "exclude-dir", "")))
        {
            exclude.Add(dir);
        }

        var format = Get(resolved, "format", "md").ToLowerInvariant();
        if (format is not ("md" or "txt" or "xml" or "json" or "jsonl" or "yaml" or "html" or "pack" or "zip")) 
            throw new ArgumentException("--format must be md, txt, xml, json, jsonl, yaml, html, pack, or zip.");

        var outputLayoutStr = Get(resolved, "output-layout", "single").ToLowerInvariant();
        var outputLayout = OutputLayout.Single;
        if (format == "pack")
        {
            outputLayout = OutputLayout.Pack;
            format = "md";
        }
        else if (format == "zip")
        {
            outputLayout = OutputLayout.Zip;
            format = "md";
        }
        else if (outputLayoutStr == "pack") outputLayout = OutputLayout.Pack;
        else if (outputLayoutStr == "zip") outputLayout = OutputLayout.Zip;
        else if (outputLayoutStr == "both") outputLayout = OutputLayout.Both;
        else if (outputLayoutStr == "single") outputLayout = OutputLayout.Single;
        else throw new ArgumentException("--output-layout must be single, pack, zip, or both.");

        var masterFormat = Get(resolved, "master-format", format).ToLowerInvariant();

        var openAfterExportMode = Get(resolved, "open-after-export", "never").ToLowerInvariant();
        var openLegacy = GetNullable(resolved, "open", null);
        if (openLegacy != null)
        {
            openAfterExportMode = ParseBool(openLegacy, "open") ? "folder" : "never";
        }

        var mode = Get(resolved, "mode", "overview").ToLowerInvariant();
        if (mode is not ("post" or "overview" or "debug" or "refactor" or "architecture" or "code-review" or "obsidian" or "research" or "prompt-pack" or "full"))
        {
            throw new ArgumentException("--mode must be post, overview, debug, refactor, architecture, code-review, obsidian, research, prompt-pack, or full.");
        }

        var contentLevel = Get(resolved, "content-level", mode == "architecture" ? "signatures" : "full").ToLowerInvariant();
        if (contentLevel is not ("full" or "outline" or "signatures")) throw new ArgumentException("--content-level must be full, outline, or signatures.");

        var domain = Get(resolved, "domain", "auto").ToLowerInvariant();
        if (domain is not ("auto" or "code" or "obsidian" or "research" or "documents" or "modeling" or "creative" or "mixed" or "unknown"))
        {
            throw new ArgumentException("--domain must be auto, code, obsidian, research, documents, modeling, creative, mixed, or unknown.");
        }

        var lens = Get(resolved, "lens", "auto").ToLowerInvariant();
        if (lens is not ("auto" or "overview" or "debug" or "architecture" or "obsidian"))
        {
            throw new ArgumentException("--lens must be auto, overview, debug, architecture, or obsidian.");
        }

        var detail = Get(resolved, "detail", "standard").ToLowerInvariant();
        if (detail is not ("basic" or "standard" or "full"))
        {
            throw new ArgumentException("--detail must be basic, standard, or full.");
        }

        return new AppConfig
        {
            InputPath = GetNullable(resolved, "input", resolved.TryGetValue("_0", out var firstPos) ? firstPos : null),
            GitRef = GetNullable(resolved, "git-ref", null),
            GitChanged = ParseBool(Get(resolved, "git-changed", "false"), "git-changed"),
            GitStaged = ParseBool(Get(resolved, "git-staged", "false"), "git-staged"),
            GitSummary = ParseBool(Get(resolved, "git-summary", "false"), "git-summary"),
            PrivacyStrict = ParseBool(Get(resolved, "privacy-strict", "false"), "privacy-strict"),
            DryRun = ParseBool(Get(resolved, "dry-run", "false"), "dry-run"),
            TaskContext = GetNullable(resolved, "task-context", null),
            TaskBudgetChars = ParseLong(Get(resolved, "task-budget-chars", "180000"), "task-budget-chars"),
            TaskPins = GetNullable(resolved, "task-pins", null),
            VerifyTaskContext = GetNullable(resolved, "verify-task-context", null),
            TaskCockpit = ParseBool(Get(resolved, "task-cockpit", "true"), "task-cockpit"),
            SnapshotComplete = ParseBool(Get(resolved, "snapshot-complete", "false"), "snapshot-complete"),
            VerifySnapshotPath = GetNullable(resolved, "verify-snapshot", null),
            VerifyManifestPath = GetNullable(resolved, "verify-manifest", null),
            Format = format,
            MasterFormat = masterFormat,
            OutputLayout = outputLayout,
            Mode = mode,
            OutputPath = Get(resolved, "output", "auto"),
            ContentLevel = contentLevel,
            MaxFileBytes = ParseLong(Get(resolved, "max-file-bytes", "1048576"), "max-file-bytes"),
            MaxTotalChars = ParseLong(Get(resolved, "max-total-chars", "2000000"), "max-total-chars"),
            SplitOutput = ParseBool(Get(resolved, "split", "false"), "split"),
            SplitSizeChars = ParseLong(Get(resolved, "split-size-chars", "300000"), "split-size-chars"),
            IncludeLineNumbers = ParseBool(Get(resolved, "line-numbers", mode is "debug" or "code-review" ? "true" : "false"), "line-numbers"),
            RedactSecrets = ParseBool(Get(resolved, "redact", "true"), "redact"),
            RedactLocalPaths = ParseBool(Get(resolved, "redact-paths", "false"), "redact-paths"),
            RespectAiIgnore = ParseBool(Get(resolved, "respect-aiignore", "true"), "respect-aiignore"),
            RespectGitIgnore = ParseBool(Get(resolved, "respect-gitignore", "true"), "respect-gitignore"),
            IncludeHidden = ParseBool(Get(resolved, "include-hidden", "false"), "include-hidden"),
            ScanPromptInjection = ParseBool(Get(resolved, "scan-prompt-injection", "true"), "scan-prompt-injection"),
            OpenAfterExportMode = openAfterExportMode,
            OpenAfterExport = openAfterExportMode != "never",
            OpenOutput = GetNullable(resolved, "open-output", null),
            IncludeGenerated = ParseBool(Get(resolved, "include-generated", "false"), "include-generated"),
            AllowOutputInsideInput = ParseBool(Get(resolved, "allow-output-inside-input", "false"), "allow-output-inside-input"),
            Notify = ParseBool(Get(resolved, "notify", "false"), "notify"),
            ShowHelp = resolved.ContainsKey("help") || resolved.ContainsKey("h") || resolved.ContainsKey("?"),
            ShowVersion = resolved.ContainsKey("version") || resolved.ContainsKey("v"),
            PromptStarter = GetNullable(resolved, "prompt-starter", null),
            IncludeExtensions = include,
            ExcludeDirs = exclude,
            IncludeGlobs = SplitList(Get(resolved, "include", "")).ToList(),
            ExcludeGlobs = SplitList(Get(resolved, "exclude", "")).ToList(),

            // POST consolidated options
            Auto = ParseBool(Get(resolved, "auto", (profile == "post" || mode == "post" || resolved.ContainsKey("auto")) ? "true" : "false"), "auto"),
            Domain = domain,
            Lens = lens,
            Orientation = GetNullable(resolved, "orientation", null),
            Preset = preset,
            Detail = detail,

            // Expert-System-Only boundary flags
            ExpertSystemOnly = ParseBool(Get(resolved, "expert-system-only", "true"), "expert-system-only"),
            AllowLLM = ParseBool(Get(resolved, "allow-llm", "false"), "allow-llm"),
            AllowNetwork = ParseBool(Get(resolved, "allow-network", "false"), "allow-network"),
            AllowEmbeddings = ParseBool(Get(resolved, "allow-embeddings", "false"), "allow-embeddings"),
            ToolchainDoctor = resolved.ContainsKey("toolchain-doctor")
        };

        // Note: Expert-System boundary guard is applied in Program.cs
        // after construction via ExpertSystemBoundary.AssertExpertSystemMode()
    }

    private static void ApplyProfileDefaults(string? profile, Dictionary<string, string> values)
    {
        if (string.IsNullOrWhiteSpace(profile)) return;

        void SetDefault(string key, string value)
        {
            if (!values.ContainsKey(key)) values[key] = value;
        }

        switch (profile)
        {
            case "post":
                SetDefault("auto", "true");
                SetDefault("format", "pack");
                SetDefault("mode", "post");
                break;
            case "overview":
                SetDefault("mode", "overview");
                SetDefault("format", "md");
                break;
            case "debug":
                SetDefault("mode", "debug");
                SetDefault("format", "pack");
                SetDefault("line-numbers", "true");
                SetDefault("split", "true");
                break;
            case "architecture":
                SetDefault("mode", "architecture");
                SetDefault("format", "pack");
                SetDefault("content-level", "signatures");
                break;
            case "obsidian":
                SetDefault("mode", "obsidian");
                SetDefault("format", "pack");
                SetDefault("line-numbers", "true");
                break;
            case "research":
                SetDefault("mode", "research");
                SetDefault("format", "pack");
                break;
            case "prompt-pack":
                SetDefault("mode", "prompt-pack");
                SetDefault("format", "pack");
                break;
            default:
                throw new ArgumentException($"Unknown --profile '{profile}'. Use post, overview, debug, architecture, obsidian, research, or prompt-pack.");
        }
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var positionalIndex = 0;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var key = arg[2..].Trim();
                if (string.IsNullOrWhiteSpace(key)) continue;

                var eq = key.IndexOf('=');
                if (eq >= 0)
                {
                    result[key[..eq]] = key[(eq + 1)..];
                    continue;
                }

                if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    result[key] = args[++i];
                }
                else
                {
                    result[key] = "true";
                }
            }
            else if (arg.StartsWith("/", StringComparison.Ordinal) && arg.Length > 1)
            {
                result[arg[1..]] = "true";
            }
            else
            {
                result[$"_{positionalIndex++}"] = arg;
            }
        }

        return result;
    }

    private static string Get(Dictionary<string, string> values, string key, string fallback)
    {
        return values.TryGetValue(key, out var value) ? value : fallback;
    }

    private static string? GetNullable(Dictionary<string, string> values, string key, string? fallback = null)
    {
        return values.TryGetValue(key, out var value) ? value : fallback;
    }

    private static long ParseLong(string? value, string key)
    {
        return long.TryParse(value, out var parsed) && parsed >= 0
            ? parsed
            : throw new ArgumentException($"--{key} must be a non-negative integer.");
    }

    private static bool ParseBool(string? value, string key)
    {
        if (bool.TryParse(value, out var parsed)) return parsed;
        var normalized = value?.Trim().ToLowerInvariant();
        if (normalized is "1" or "yes" or "y" or "on") return true;
        if (normalized is "0" or "no" or "n" or "off") return false;
        throw new ArgumentException($"--{key} must be true or false.");
    }

    internal static IEnumerable<string> SplitList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) yield break;

        foreach (var item in value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(item)) yield return item;
        }
    }
}
