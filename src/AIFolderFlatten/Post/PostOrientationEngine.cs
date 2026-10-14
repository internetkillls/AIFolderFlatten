using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Post;

internal static class PostOrientationEngine
{
    private static readonly Dictionary<string, string[]> DomainLexicalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "code", new[] { "class ", "function ", "namespace ", "using ", "import ", "public ", "private ", "return " } },
        { "obsidian", new[] { "[[", "tags:", "callout", "dataview", "kanban" } },
        { "research", new[] { "method", "hypothesis", "provenance", "citation", "bibliography", "literature review", "openalex", "doi:", "et al." } },
        { "modeling", new[] { "equation", "variable", "boundary condition", "initial condition", "optimization", "simulation", "constraint", "objective function" } },
        { "creative", new[] { "storyboard", "moodboard", "timbre", "bpm", "midi", "keyframes", "rendering", "motion reference" } }
    };

    public static OrientationState Orient(string inputDir, AppConfig config, ScanState scanState, List<string> textFileContents)
    {
        var hypotheses = new List<OrientationHypothesis>();
        var evidence = new List<string>();
        var alternatives = new List<string>();

        // 1. File Type Scoring
        double codeScore = scanState.CodeSignals * 3.0;
        double obsidianScore = scanState.ObsidianSignals * 3.0;
        double researchScore = scanState.ResearchSignals * 4.0;
        double docScore = scanState.DocumentSignals * 2.0;
        double modelScore = scanState.ModelingSignals * 4.0;
        double creativeScore = scanState.CreativeSignals * 4.0;

        evidence.Add($"File count matches: Code signals={scanState.CodeSignals}, Obsidian signals={scanState.ObsidianSignals}, Research signals={scanState.ResearchSignals}, Document signals={scanState.DocumentSignals}, Modeling signals={scanState.ModelingSignals}, Creative signals={scanState.CreativeSignals}");

        // 2. Lexical Keyword Scanning from sample file contents
        var keywordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "code", 0 }, { "obsidian", 0 }, { "research", 0 }, { "modeling", 0 }, { "creative", 0 }
        };

        foreach (var content in textFileContents)
        {
            var sample = content.Length > 5000 ? content[..5000] : content;
            foreach (var domain in DomainLexicalKeywords.Keys)
            {
                foreach (var word in DomainLexicalKeywords[domain])
                {
                    if (sample.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        keywordCounts[domain]++;
                    }
                }
            }
        }

        codeScore += keywordCounts["code"] * 0.5;
        obsidianScore += keywordCounts["obsidian"] * 0.5;
        researchScore += keywordCounts["research"] * 0.8;
        modelScore += keywordCounts["modeling"] * 0.8;
        creativeScore += keywordCounts["creative"] * 0.8;

        evidence.Add($"Lexical signal matches: Code={keywordCounts["code"]}, Obsidian={keywordCounts["obsidian"]}, Research={keywordCounts["research"]}, Modeling={keywordCounts["modeling"]}, Creative={keywordCounts["creative"]}");

        // Special directory layout detections
        if (Directory.Exists(Path.Combine(inputDir, ".obsidian")))
        {
            obsidianScore += 25.0;
            evidence.Add("Directory '.obsidian' exists, strongly suggesting Obsidian notes vault.");
        }
        if (Directory.Exists(Path.Combine(inputDir, ".git")))
        {
            codeScore += 5.0;
            evidence.Add("Directory '.git' exists, indicating a software repository.");
        }
        if (Directory.Exists(Path.Combine(inputDir, "src")) || Directory.Exists(Path.Combine(inputDir, "Source")))
        {
            codeScore += 10.0;
            evidence.Add("Source directory ('src' or 'Source') exists.");
        }

        // Add user-defined orientation bias if provided
        if (!string.IsNullOrWhiteSpace(config.Orientation))
        {
            evidence.Add($"User-defined orientation hint active: '{config.Orientation}'");
            var hint = config.Orientation.ToLowerInvariant();
            if (hint.Contains("code") || hint.Contains("software") || hint.Contains("program")) codeScore += 20.0;
            if (hint.Contains("note") || hint.Contains("vault") || hint.Contains("obsidian")) obsidianScore += 20.0;
            if (hint.Contains("research") || hint.Contains("paper") || hint.Contains("academic")) researchScore += 20.0;
            if (hint.Contains("math") || hint.Contains("model") || hint.Contains("equation")) modelScore += 20.0;
            if (hint.Contains("creative") || hint.Contains("music") || hint.Contains("video") || hint.Contains("sound")) creativeScore += 20.0;
        }

        // Build hypotheses
        hypotheses.Add(new OrientationHypothesis("code", codeScore, $"Score based on source files/directories and keywords ({codeScore:F1})"));
        hypotheses.Add(new OrientationHypothesis("obsidian", obsidianScore, $"Score based on note links, tags, and structure ({obsidianScore:F1})"));
        hypotheses.Add(new OrientationHypothesis("research", researchScore, $"Score based on research vocabulary, bib references, and papers ({researchScore:F1})"));
        hypotheses.Add(new OrientationHypothesis("modeling", modelScore, $"Score based on formulas, variables, and units ({modelScore:F1})"));
        hypotheses.Add(new OrientationHypothesis("creative", creativeScore, $"Score based on sound/motion files and media terms ({creativeScore:F1})"));

        var sorted = hypotheses.OrderByDescending(h => h.Score).ToList();
        var primaryHypothesis = sorted.First();
        var secondHypothesis = sorted.Skip(1).First();

        double totalScore = sorted.Sum(h => h.Score);
        double confidence = 0.0;

        if (totalScore > 0)
        {
            // Confidence is the difference between top two divided by total positive scores, or scaled
            confidence = primaryHypothesis.Score / totalScore;
        }

        string selectedDomain = "unknown";
        if (primaryHypothesis.Score > 0 && confidence >= 0.5)
        {
            selectedDomain = primaryHypothesis.Domain;
        }
        else if (primaryHypothesis.Score > 0 && confidence < 0.5)
        {
            selectedDomain = "mixed";
        }

        // Apply config domain overrides if specified
        if (!string.IsNullOrWhiteSpace(config.Domain) && !config.Domain.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            selectedDomain = config.Domain.ToLowerInvariant();
            evidence.Add($"User override forced domain selection to: '{selectedDomain}'");
        }

        // Lens auto-detection
        string selectedLens = "overview";
        if (config.Lens != null && !config.Lens.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            selectedLens = config.Lens.ToLowerInvariant();
        }
        else
        {
            // Auto lens selection based on domain
            selectedLens = selectedDomain switch
            {
                "code" => "architecture",
                "obsidian" => "obsidian",
                "research" => "overview",
                "modeling" => "architecture",
                "creative" => "overview",
                _ => "overview"
            };
        }

        // Source State
        string selectedSourceState = "working_folder";
        if (config.GitStaged) selectedSourceState = "git_staged";
        else if (config.GitChanged) selectedSourceState = "git_changed";
        else if (config.GitRef != null) selectedSourceState = "git_head";

        // Alternatives
        foreach (var hyp in sorted.Skip(1).Where(h => h.Score > 0))
        {
            alternatives.Add($"{hyp.Domain} (score: {hyp.Score:F1})");
        }

        return new OrientationState(
            SelectedDomain: selectedDomain,
            SelectedLens: selectedLens,
            SelectedSourceState: selectedSourceState,
            Confidence: confidence,
            Hypotheses: sorted,
            Evidence: evidence,
            Alternatives: alternatives
        );
    }

    public static string RenderOrientationMarkdown(OrientationState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# POST Orientation Report");
        sb.AppendLine();
        sb.AppendLine($"*   **Selected Domain**: `{state.SelectedDomain}`");
        sb.AppendLine($"*   **Selected Lens**: `{state.SelectedLens}`");
        sb.AppendLine($"*   **Source State**: `{state.SelectedSourceState}`");
        sb.AppendLine($"*   **Confidence**: `{state.Confidence:P1}`");
        sb.AppendLine();

        sb.AppendLine("## Hypotheses Scores");
        foreach (var hyp in state.Hypotheses)
        {
            sb.AppendLine($"*   **{hyp.Domain}**: {hyp.Score:F2} — *{hyp.Reason}*");
        }
        sb.AppendLine();

        sb.AppendLine("## Observed Evidence");
        foreach (var ev in state.Evidence)
        {
            sb.AppendLine($"*   {ev}");
        }
        sb.AppendLine();

        if (state.Alternatives.Count > 0)
        {
            sb.AppendLine("## Alternatives");
            foreach (var alt in state.Alternatives)
            {
                sb.AppendLine($"*   {alt}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
