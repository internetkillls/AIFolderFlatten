using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIFolderFlatten.Artifacts;

internal static class ArtifactLedgerRenderer
{
    public static string RenderLedger(List<ArtifactIR> artifacts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Artifact Ledger");
        sb.AppendLine();
        sb.AppendLine("This ledger lists all tracked files, including documents, code, media, and unknown data structures.");
        sb.AppendLine();
        sb.AppendLine("| ID | Relative Path | Type/Category | Size (Bytes) | SHA-256 Hash | Status | Parser Tier |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var art in artifacts)
        {
            sb.AppendLine($"| `{art.Id}` | `{art.Path}` | {art.ArtifactType} | {art.SizeBytes:N0} | `{art.Sha256}` | {art.Status} | `{art.ParserTier}` |");
        }

        return sb.ToString();
    }

    public static string RenderParserCapabilities(List<ArtifactIR> artifacts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Parser Capability Registry");
        sb.AppendLine();
        sb.AppendLine("Registry mappings for file formats encountered in this workspace:");
        sb.AppendLine();
        sb.AppendLine("| Extension | Tier | Parser Component | Tool Suggestion | Capabilities |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        var extensions = artifacts.Select(a => a.Extension).Distinct().OrderBy(e => e).ToList();
        foreach (var ext in extensions)
        {
            var reg = ParserCapabilityRegistry.GetEntry(ext);
            var caps = string.Join(", ", reg.Capabilities);
            sb.AppendLine($"| `{ext}` | `{reg.ParserTier}` | {reg.ParserName ?? "None"} | {reg.ExternalToolHint ?? "None"} | {caps} |");
        }

        return sb.ToString();
    }

    public static string RenderUnknownArtifacts(List<ArtifactIR> artifacts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Unknown Artifacts Ledger");
        sb.AppendLine();
        sb.AppendLine("The following files have unrecognized formats and were preserved as ledger-only entries to prevent silent context loss:");
        sb.AppendLine();

        var unknown = artifacts.Where(a => a.ArtifactType == "unknown").ToList();
        if (unknown.Count == 0)
        {
            sb.AppendLine("No unknown artifacts detected in this scan.");
            return sb.ToString();
        }

        sb.AppendLine("| ID | Relative Path | Size (Bytes) | SHA-256 Hash | Modified Time (UTC) |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var art in unknown)
        {
            sb.AppendLine($"| `{art.Id}` | `{art.Path}` | {art.SizeBytes:N0} | `{art.Sha256}` | {art.ModifiedUtc:yyyy-MM-dd HH:mm:ss} |");
        }

        return sb.ToString();
    }
}
