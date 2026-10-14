using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIFolderFlatten.ExpertSystem;

/// <summary>
/// Intermediate representation for expert-system provenance metadata.
/// </summary>
internal sealed record ExpertSystemProvenanceIR(
    string GeneratedAt,
    string ToolVersion,
    string SourceRoot,
    bool AiCore,
    bool NetworkUsed,
    bool LlmUsed,
    bool EmbeddingsUsed,
    bool ExpertSystemOnly,
    IReadOnlyList<string> ExternalToolsUsed,
    IReadOnlyList<string> ExternalToolsMissing,
    IReadOnlyList<string> FallbacksUsed,
    IReadOnlyList<string> ParserTiersUsed,
    IReadOnlyList<string> KnownLimitations
);

/// <summary>
/// Builds and renders the Expert-System provenance document.
/// This document certifies that no AI, LLMs, embeddings, or network calls
/// were made during the scan. All outputs are deterministic.
/// </summary>
internal static class ExpertSystemProvenance
{
    /// <summary>
    /// Builds an <see cref="ExpertSystemProvenanceIR"/> from pipeline context.
    /// </summary>
    /// <param name="config">Application configuration (for source root).</param>
    /// <param name="toolsUsed">External tools that were found and invoked.</param>
    /// <param name="toolsMissing">External tools that were probed but not found.</param>
    /// <param name="fallbacksUsed">Internal fallbacks activated when tools were missing.</param>
    /// <param name="parserTiersUsed">Parser tier identifiers used during the scan.</param>
    /// <param name="knownLimitations">Known limitations of this scan run.</param>
    public static ExpertSystemProvenanceIR BuildIR(
        AppConfig config,
        IReadOnlyList<string> toolsUsed,
        IReadOnlyList<string> toolsMissing,
        IReadOnlyList<string> fallbacksUsed,
        IReadOnlyList<string> parserTiersUsed,
        IReadOnlyList<string> knownLimitations)
    {
        // Assert boundary before recording provenance
        ExpertSystemBoundary.AssertExpertSystemMode(
            expertSystemOnly: ExpertSystemBoundary.ExpertSystemOnly,
            allowLlm: ExpertSystemBoundary.LlmUsed,
            allowNetwork: ExpertSystemBoundary.NetworkUsed,
            allowEmbeddings: ExpertSystemBoundary.EmbeddingsUsed);

        return new ExpertSystemProvenanceIR(
            GeneratedAt: DateTimeOffset.UtcNow.ToString("o"),
            ToolVersion: Program.AppVersion,
            SourceRoot: config.InputPath ?? string.Empty,
            AiCore: ExpertSystemBoundary.AiCore,
            NetworkUsed: ExpertSystemBoundary.NetworkUsed,
            LlmUsed: ExpertSystemBoundary.LlmUsed,
            EmbeddingsUsed: ExpertSystemBoundary.EmbeddingsUsed,
            ExpertSystemOnly: ExpertSystemBoundary.ExpertSystemOnly,
            ExternalToolsUsed: toolsUsed ?? Array.Empty<string>(),
            ExternalToolsMissing: toolsMissing ?? Array.Empty<string>(),
            FallbacksUsed: fallbacksUsed ?? Array.Empty<string>(),
            ParserTiersUsed: parserTiersUsed ?? Array.Empty<string>(),
            KnownLimitations: knownLimitations ?? Array.Empty<string>()
        );
    }

    /// <summary>
    /// Renders a <see cref="ExpertSystemProvenanceIR"/> as a Markdown provenance document.
    /// </summary>
    public static string Render(ExpertSystemProvenanceIR ir)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Expert-System Provenance");
        sb.AppendLine();
        sb.AppendLine("> ⚠️ **Required Warning**: Review outputs before uploading to any cloud AI.");
        sb.AppendLine("> The scanner is deterministic and cannot guarantee perfect privacy.");
        sb.AppendLine();
        sb.AppendLine("## Boundary Attestation");
        sb.AppendLine();
        sb.AppendLine("| Property | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| AI used | {BoolStr(ir.AiCore)} |");
        sb.AppendLine($"| Network used | {BoolStr(ir.NetworkUsed)} |");
        sb.AppendLine($"| LLM used | {BoolStr(ir.LlmUsed)} |");
        sb.AppendLine($"| Embeddings used | {BoolStr(ir.EmbeddingsUsed)} |");
        sb.AppendLine($"| Expert-System-Only | {BoolStr(ir.ExpertSystemOnly)} |");
        sb.AppendLine();
        sb.AppendLine("## Generation Metadata");
        sb.AppendLine();
        sb.AppendLine($"- **Generated at**: {ir.GeneratedAt}");
        sb.AppendLine($"- **Tool version**: {ir.ToolVersion}");
        sb.AppendLine($"- **Source root**: `{ir.SourceRoot}`");
        sb.AppendLine();
        sb.AppendLine("## External Tools Used");
        sb.AppendLine();
        if (ir.ExternalToolsUsed.Count == 0)
        {
            sb.AppendLine("_None — all parsing performed by internal expert-system routines._");
        }
        else
        {
            foreach (var tool in ir.ExternalToolsUsed)
                sb.AppendLine($"- {tool}");
        }
        sb.AppendLine();
        sb.AppendLine("## External Tools Missing");
        sb.AppendLine();
        if (ir.ExternalToolsMissing.Count == 0)
        {
            sb.AppendLine("_None._");
        }
        else
        {
            foreach (var tool in ir.ExternalToolsMissing)
                sb.AppendLine($"- {tool}");
        }
        sb.AppendLine();
        sb.AppendLine("## Fallbacks Used");
        sb.AppendLine();
        if (ir.FallbacksUsed.Count == 0)
        {
            sb.AppendLine("_None._");
        }
        else
        {
            foreach (var fb in ir.FallbacksUsed)
                sb.AppendLine($"- {fb}");
        }
        sb.AppendLine();
        sb.AppendLine("## Parser Tiers Used");
        sb.AppendLine();
        if (ir.ParserTiersUsed.Count == 0)
        {
            sb.AppendLine("_None recorded._");
        }
        else
        {
            foreach (var tier in ir.ParserTiersUsed.Distinct())
                sb.AppendLine($"- {tier}");
        }
        sb.AppendLine();
        sb.AppendLine("## Known Limitations");
        sb.AppendLine();
        if (ir.KnownLimitations.Count == 0)
        {
            sb.AppendLine("_None specified._");
        }
        else
        {
            foreach (var lim in ir.KnownLimitations)
                sb.AppendLine($"- {lim}");
        }

        return sb.ToString();
    }

    private static string BoolStr(bool value) => value ? "true" : "false";
}
