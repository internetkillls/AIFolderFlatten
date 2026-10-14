using System;
using System.Collections.Generic;

namespace AIFolderFlatten.Artifacts;

internal sealed record RegistryEntry(
    string ParserTier,
    string? ParserName,
    string? ExternalToolHint,
    IReadOnlyList<string> Capabilities
);

internal static class ParserCapabilityRegistry
{
    private static readonly Dictionary<string, RegistryEntry> Registry = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".md", new RegistryEntry("T3_structural_extract", "MarkdownRenderer", null, new[] { "can_parse_headings", "can_parse_frontmatter", "can_parse_wikilinks" }) },
        { ".markdown", new RegistryEntry("T3_structural_extract", "MarkdownRenderer", null, new[] { "can_parse_headings", "can_parse_frontmatter", "can_parse_wikilinks" }) },
        { ".txt", new RegistryEntry("T2_text_extract", "TextRenderer", null, new[] { "can_extract_text" }) },
        { ".cs", new RegistryEntry("T3_structural_extract", "FileAnalyzer", null, new[] { "can_parse_classes", "can_parse_methods", "can_parse_imports" }) },
        { ".py", new RegistryEntry("T3_structural_extract", "FileAnalyzer", null, new[] { "can_parse_functions", "can_parse_imports" }) },
        { ".js", new RegistryEntry("T3_structural_extract", "FileAnalyzer", null, new[] { "can_parse_functions", "can_parse_imports" }) },
        { ".ts", new RegistryEntry("T3_structural_extract", "FileAnalyzer", null, new[] { "can_parse_classes", "can_parse_imports" }) },
        { ".html", new RegistryEntry("T3_structural_extract", "HtmlDocumentParser", null, new[] { "can_parse_title", "can_parse_headings", "can_parse_links", "strip_scripts" }) },
        { ".htm", new RegistryEntry("T3_structural_extract", "HtmlDocumentParser", null, new[] { "can_parse_title", "can_parse_headings", "can_parse_links", "strip_scripts" }) },
        { ".pdf", new RegistryEntry("T1_metadata", "PdfMetadataAdapter", null, new[] { "sniff_metadata", "size_hash_lookup" }) },
        { ".epub", new RegistryEntry("T1_metadata", "EpubMetadataAdapter", null, new[] { "parse_container_xml", "parse_opf_metadata", "sniff_metadata" }) },
        { ".mobi", new RegistryEntry("T0_ledger_only", null, "ebook-convert", new[] { "ledger_only", "external_tool_suggested" }) },
        { ".azw", new RegistryEntry("T0_ledger_only", null, "ebook-convert", new[] { "ledger_only", "external_tool_suggested" }) },
        { ".azw3", new RegistryEntry("T0_ledger_only", null, "ebook-convert", new[] { "ledger_only", "external_tool_suggested" }) },
        { ".tex", new RegistryEntry("T3_structural_extract", "LatexLiteParser", null, new[] { "can_parse_sections", "can_parse_equations", "can_parse_citations" }) },
        { ".bib", new RegistryEntry("T3_structural_extract", "CitationLiteParser", null, new[] { "can_parse_citations", "parse_bibtex_entries" }) },
        { ".ris", new RegistryEntry("T3_structural_extract", "CitationLiteParser", null, new[] { "can_parse_citations", "parse_ris_entries" }) },
        { ".ipynb", new RegistryEntry("T3_structural_extract", "JupyterNotebookParser", null, new[] { "can_parse_cells", "can_parse_headings", "can_parse_equations", "can_parse_imports" }) },
        { ".wl", new RegistryEntry("T2_text_extract", "FileText", null, new[] { "can_extract_text", "wolfram_script" }) },
        { ".wls", new RegistryEntry("T2_text_extract", "FileText", null, new[] { "can_extract_text", "wolfram_script" }) },
        { ".nb", new RegistryEntry("T0_ledger_only", null, "wolframscript", new[] { "ledger_only", "external_tool_suggested" }) },
        { ".wav", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".mp3", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".flac", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".mp4", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".mov", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".webm", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "ffprobe", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".png", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", null, new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".jpg", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", null, new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".jpeg", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", null, new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".webp", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", null, new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".svg", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", null, new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".blend", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "blender-python", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".pd", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "puredata", new[] { "ledger_only", "creative_affordance_sniffing" }) },
        { ".scd", new RegistryEntry("T1_metadata", "MediaLedgerBuilder", "supercollider", new[] { "ledger_only", "creative_affordance_sniffing" }) }
    };

    public static RegistryEntry GetEntry(string extension)
    {
        var ext = extension.ToLowerInvariant().Trim();
        if (Registry.TryGetValue(ext, out var entry))
        {
            return entry;
        }
        return new RegistryEntry("T0_ledger_only", null, null, new[] { "ledger_only_fallback" });
    }
}
