using System;
using System.Collections.Generic;
using System.Text;

namespace AIFolderFlatten.Research;

internal static class ResearchRenderer
{
    /// <summary>
    /// Expert-System Signal Report — deterministic heuristics only, no feasibility claims.
    /// All items are candidates/hints for human review. No AI interpretation.
    /// </summary>
    public static string RenderResearchSignalReport(List<MethodSignal> methods, List<InferenceSignal> inferences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Research Signal Report (Expert-System)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: All items below are *candidates* and *hints* detected by");
        sb.AppendLine("> deterministic heuristics. None are verified research gaps or validated academic claims.");
        sb.AppendLine("> Human review is required before acting on any signal.");
        sb.AppendLine();
        sb.AppendLine($"*   **Method phrase candidates**: {methods.Count} matches found");
        sb.AppendLine($"*   **Inference phrase candidates**: {inferences.Count} matches found");
        sb.AppendLine();
        return sb.ToString();
    }

    // Kept for backward-compat with single-file (md/txt) output path
    public static string RenderResearchIntelligence(List<MethodSignal> methods, List<InferenceSignal> inferences, List<FeasibilityDimension> feasibility)
        => RenderResearchSignalReport(methods, inferences);

    public static string RenderTermBank(NGramFrequencyAnalyzer.NGramResult ngrams)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Term Bank and Vocabulary Frequencies");
        sb.AppendLine();
        sb.AppendLine("Top unigram vocabulary frequencies (excluding standard English stop words):");
        sb.AppendLine();
        sb.AppendLine("| Unigram | Frequency |");
        sb.AppendLine("| :--- | :---: |");
        foreach (var kv in ngrams.Unigrams)
        {
            sb.AppendLine($"| `{kv.Key}` | {kv.Value} |");
        }
        sb.AppendLine();

        sb.AppendLine("Top bigram collocations:");
        sb.AppendLine();
        sb.AppendLine("| Bigram | Frequency |");
        sb.AppendLine("| :--- | :---: |");
        foreach (var kv in ngrams.Bigrams)
        {
            sb.AppendLine($"| `{kv.Key}` | {kv.Value} |");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    public static string RenderMethodMap(List<MethodSignal> methods, List<InferenceSignal> inferences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Method Phrase Map (Candidates)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: Items below are phrase-match candidates only.");
        sb.AppendLine("> They indicate possible methodology signals but require human verification.");
        sb.AppendLine();

        sb.AppendLine("## Method Phrase Candidates");
        sb.AppendLine();
        if (methods.Count == 0)
        {
            sb.AppendLine("No explicit methodology keyword signals detected.");
        }
        else
        {
            sb.AppendLine("| Method Signal | Evidence | Implicitness | Risk / Warning |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (var m in methods)
            {
                sb.AppendLine($"| {m.Signal} | `{m.Evidence}` | {m.Implicitness} | {m.Risk} |");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Inference Phrase Candidates");
        sb.AppendLine();
        if (inferences.Count == 0)
        {
            sb.AppendLine("No explicit inference claim signals detected.");
        }
        else
        {
            sb.AppendLine("| Claim | Inference Type | Evidence | Strength | Missing Context |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
            foreach (var inf in inferences)
            {
                sb.AppendLine($"| {inf.Claim} | {inf.Type} | `{inf.Evidence}` | {inf.Strength} | {inf.Missing} |");
            }
        }
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Renders gap questions (research queries) with expert-system candidate language.
    /// Replaces: RenderFeasibility (interpretive overclaim) + RenderStateOfTheArtQueries.
    /// </summary>
    public static string RenderGapQuestions(List<string> queries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Gap Questions (Not Verified Gaps)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: These query strings were generated from vocabulary frequency");
        sb.AppendLine("> and phrase patterns only. They are NOT verified research gaps. They are starting");
        sb.AppendLine("> points for human investigation. Validate against primary literature.");
        sb.AppendLine();
        sb.AppendLine("Candidate search strings for academic investigation (Google Scholar, Crossref, etc.):");
        sb.AppendLine();
        foreach (var q in queries)
        {
            sb.AppendLine($"*   `{q}`");
        }
        return sb.ToString();
    }

    // Kept for legacy single-file output compatibility
    public static string RenderFeasibility(List<FeasibilityDimension> feasibility)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Feasibility Markers (Heuristic — Not Validated)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: Scores below are heuristic pattern-match counts, not validated.");
        sb.AppendLine();
        sb.AppendLine("| Dimension | Score | Primary Reason | Risk | Mitigation |");
        sb.AppendLine("| :--- | :---: | :--- | :--- | :--- |");
        foreach (var f in feasibility)
        {
            sb.AppendLine($"| {f.Dimension} | {f.Score:F2} | {f.Reason} | {f.Risk} | {f.Mitigation} |");
        }
        return sb.ToString();
    }

    // Kept for legacy single-file output compatibility
    public static string RenderStateOfTheArtQueries(List<string> queries)
        => RenderGapQuestions(queries);

    /// <summary>
    /// Provenance hints — what the scanner was and wasn't able to determine.
    /// </summary>
    public static string RenderProvenanceHints(List<MethodSignal> methods, List<InferenceSignal> inferences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Provenance Hints");
        sb.AppendLine();
        sb.AppendLine("> This file was produced by AI Seeder expert-system heuristics only.");
        sb.AppendLine("> No AI, LLM, embeddings, or network access was used.");
        sb.AppendLine();
        sb.AppendLine("## What was detected");
        sb.AppendLine();
        sb.AppendLine($"*   Method phrase candidates: {methods.Count}");
        sb.AppendLine($"*   Inference phrase candidates: {inferences.Count}");
        sb.AppendLine();
        sb.AppendLine("## What was NOT determined");
        sb.AppendLine();
        sb.AppendLine("*   No semantic similarity was computed.");
        sb.AppendLine("*   No LLM-based summarisation was performed.");
        sb.AppendLine("*   No literature access was performed.");
        sb.AppendLine("*   No gap validation was performed.");
        sb.AppendLine();
        return sb.ToString();
    }
}

