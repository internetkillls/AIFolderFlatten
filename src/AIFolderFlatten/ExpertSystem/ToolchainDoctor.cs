using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AIFolderFlatten.ExpertSystem;

/// <summary>
/// Entry in the toolchain status report.
/// </summary>
internal sealed record ToolchainStatusEntry(
    string ToolName,
    string Version,
    bool Found,
    bool RequiresAI,       // always false — this tool is part of the expert-system boundary
    bool RequiresNetwork,  // always false — all probes are local CLI calls
    string Fallback,
    string Capability
);

/// <summary>
/// Intermediate representation for the full toolchain status report.
/// </summary>
internal sealed record ToolchainStatusIR(
    string GeneratedAt,
    IReadOnlyList<ToolchainStatusEntry> Entries
);

/// <summary>
/// Probes optional external CLI tools and reports their availability.
/// No AI, network, or LLM usage — local process calls only.
/// </summary>
internal static class ToolchainDoctor
{
    private sealed record ToolSpec(
        string Name,
        string Command,
        string Args,
        string Fallback,
        string Capability
    );

    private static readonly ToolSpec[] KnownTools =
    {
        new("tree-sitter",          "tree-sitter",  "--version",  "internal_regex_parser",      "structural_code_parse"),
        new("ast-grep",             "ast-grep",     "--version",  "internal_regex_parser",      "pattern_search"),
        new("dependency-cruiser",   "depcruise",    "--version",  "internal_import_regex",      "dependency_graph"),
        new("madge",                "madge",        "--version",  "internal_import_regex",      "module_graph"),
        new("ffprobe",              "ffprobe",      "-version",   "ledger_only_no_media_analysis","media_metadata"),
        new("git",                  "git",          "--version",  "no_git_context",             "vcs_context"),
    };

    /// <summary>
    /// Probes all known optional tools, writes TOOLCHAIN_STATUS.md to <paramref name="outputDir"/>,
    /// and returns the markdown content.
    /// </summary>
    /// <param name="outputDir">Directory in which to write TOOLCHAIN_STATUS.md.</param>
    /// <returns>The markdown content of the status report.</returns>
    public static string Run(string outputDir)
    {
        var markdown = BuildMinimalStatus();

        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, "TOOLCHAIN_STATUS.md");
            File.WriteAllText(outputPath, markdown);
        }

        return markdown;
    }

    /// <summary>
    /// Builds the minimal toolchain status markdown by probing all known tools.
    /// </summary>
    /// <returns>The markdown content of the status report.</returns>
    public static string BuildMinimalStatus()
    {
        var entries = new List<ToolchainStatusEntry>();

        foreach (var spec in KnownTools)
        {
            var entry = ProbeOneTool(spec);
            entries.Add(entry);
        }

        return RenderStatusMarkdown(entries);
    }


    /// <summary>
    /// Renders a Markdown status table from a list of toolchain status entries.
    /// </summary>
    public static string RenderStatusMarkdown(IReadOnlyList<ToolchainStatusEntry> entries)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Toolchain Status");
        sb.AppendLine();
        sb.AppendLine($"> Generated at: {DateTimeOffset.UtcNow:o}");
        sb.AppendLine("> Expert-System-Only: true | Requires AI: false | Requires Network: false");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();

        int found = 0;
        int missing = 0;
        foreach (var e in entries)
        {
            if (e.Found) found++;
            else missing++;
        }

        sb.AppendLine($"- Tools found: **{found}**");
        sb.AppendLine($"- Tools missing: **{missing}**");
        sb.AppendLine($"- All tools optional — fallbacks are available for every missing tool.");
        sb.AppendLine();
        sb.AppendLine("## Tool Status");
        sb.AppendLine();
        sb.AppendLine("| Tool | Found | Version | Capability | Fallback | Requires AI | Requires Network |");
        sb.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var e in entries)
        {
            var foundMark = e.Found ? "✅" : "❌";
            sb.AppendLine($"| {e.ToolName} | {foundMark} | {Escape(e.Version)} | {e.Capability} | {e.Fallback} | {BoolStr(e.RequiresAI)} | {BoolStr(e.RequiresNetwork)} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Notes");
        sb.AppendLine();
        sb.AppendLine("- All tools above are **optional**. The expert-system core always activates its internal fallback parser when a tool is missing.");
        sb.AppendLine("- No tool in this registry requires AI, LLMs, embeddings, or network access.");
        sb.AppendLine("- To install missing tools, consult their official documentation.");

        return sb.ToString();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static ToolchainStatusEntry ProbeOneTool(ToolSpec spec)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = spec.Command,
                Arguments = spec.Args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                return NotFound(spec);
            }

            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit(5000);

            var versionText = (stdout + stderr).Trim();
            if (string.IsNullOrWhiteSpace(versionText))
                versionText = "found (no version output)";

            // Truncate to first line for readability
            var newline = versionText.IndexOf('\n');
            if (newline > 0) versionText = versionText[..newline].TrimEnd('\r');

            return new ToolchainStatusEntry(
                ToolName: spec.Name,
                Version: versionText,
                Found: true,
                RequiresAI: false,
                RequiresNetwork: false,
                Fallback: spec.Fallback,
                Capability: spec.Capability
            );
        }
        catch
        {
            return NotFound(spec);
        }
    }

    private static ToolchainStatusEntry NotFound(ToolSpec spec) =>
        new(
            ToolName: spec.Name,
            Version: "not found",
            Found: false,
            RequiresAI: false,
            RequiresNetwork: false,
            Fallback: spec.Fallback,
            Capability: spec.Capability
        );

    private static string BoolStr(bool value) => value ? "true" : "false";

    private static string Escape(string s) => s.Replace("|", "\\|");
}
