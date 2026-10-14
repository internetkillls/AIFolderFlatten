using System;
using System.Collections.Generic;
using System.IO;

namespace AIFolderFlatten.Post;

internal static class AffordanceDetector
{
    public static List<string> Detect(string inputDir, ScanState scan)
    {
        var affordances = new List<string>();

        if (scan.CodeSignals > 0)
        {
            affordances.Add("Software Engineering: Exposes source code structures suitable for compilation or review.");
        }

        if (Directory.Exists(Path.Combine(inputDir, ".git")))
        {
            affordances.Add("Git Version Control: Allows tracking repository commits, staged changes, and revision snapshots.");
        }

        if (scan.ObsidianSignals > 0 || Directory.Exists(Path.Combine(inputDir, ".obsidian")))
        {
            affordances.Add("Knowledge Vault: Contains hyperlinked notes, tags, and structure ideal for wiki index analysis.");
        }

        if (scan.ResearchSignals > 0)
        {
            affordances.Add("Scholarly Resource: Features research vocabulary, paper structures, and bibliographical maps.");
        }

        if (scan.ModelingSignals > 0)
        {
            affordances.Add("Mathematical Modeling: Exposes formulas, variables, and mathematical assumptions suitable for simulation audits.");
        }

        if (scan.CreativeSignals > 0 || scan.MediaArtifacts > 0)
        {
            affordances.Add("Creative Reference Asset: Includes audio, video, image, or 3D models suitable for asset ledgers.");
        }

        if (scan.DocumentSignals > 0)
        {
            affordances.Add("Document Archive: Contains digital books (EPUB), publications (PDF), or web pages (HTML).");
        }

        if (affordances.Count == 0)
        {
            affordances.Add("Generic File Seed: Basic file collection without specialized structural signals.");
        }

        return affordances;
    }

    public static string RenderAffordancesMarkdown(List<string> affordances)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Folder Affordances Ledger");
        sb.AppendLine();
        sb.AppendLine("The seed folder exposes the following capabilities:");
        sb.AppendLine();
        foreach (var aff in affordances)
        {
            sb.AppendLine($"*   **{aff}**");
        }
        return sb.ToString();
    }
}
