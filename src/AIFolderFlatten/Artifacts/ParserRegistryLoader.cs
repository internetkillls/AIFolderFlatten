using System;
using System.Collections.Generic;

namespace AIFolderFlatten.Artifacts;

/// <summary>
/// An entry in the parser registry describing how a file extension is handled.
/// </summary>
internal sealed record ParserRegistryEntry(
    string Extension,
    string Category,
    string Tier,
    string Parser,
    bool RequiresExternalTool,
    bool RequiresAI,        // always false — expert-system boundary
    bool NetworkRequired,   // always false — expert-system boundary
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Limitations
);

/// <summary>
/// Provides the hardcoded parser registry for all supported file extensions.
/// All entries have RequiresAI=false and NetworkRequired=false (expert-system boundary).
/// </summary>
internal static class ParserRegistryLoader
{
    /// <summary>
    /// Returns the complete parser registry as an immutable list.
    /// </summary>
    public static IReadOnlyList<ParserRegistryEntry> GetAllEntries() => _entries;

    private static readonly IReadOnlyList<ParserRegistryEntry> _entries = new List<ParserRegistryEntry>
    {
        // ── Source code ──────────────────────────────────────────────────────
        new(
            Extension: ".cs",
            Category: "source",
            Tier: "T3_structural_extract",
            Parser: "CSharpLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "symbol_extract", "import_extract", "heading_detect" },
            Limitations: new[] { "regex_or_ast_lite_only", "not_full_compiler_semantics" }
        ),
        new(
            Extension: ".csproj",
            Category: "build",
            Tier: "T3_structural_extract",
            Parser: "CsprojXmlParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "package_reference_extract", "target_framework_detect" },
            Limitations: new[] { "xml_parse_only", "no_transitive_resolution" }
        ),
        new(
            Extension: ".sln",
            Category: "build",
            Tier: "T1_metadata",
            Parser: "SolutionFileReader",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "project_reference_detect" },
            Limitations: new[] { "text_parse_only", "no_build_graph_resolution" }
        ),
        new(
            Extension: ".py",
            Category: "source",
            Tier: "T3_structural_extract",
            Parser: "PythonLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "import_extract", "def_detect" },
            Limitations: new[] { "regex_only", "no_type_inference" }
        ),
        new(
            Extension: ".js",
            Category: "source",
            Tier: "T3_structural_extract",
            Parser: "JsLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "import_extract", "function_detect" },
            Limitations: new[] { "regex_only", "no_jsx_ast" }
        ),
        new(
            Extension: ".ts",
            Category: "source",
            Tier: "T3_structural_extract",
            Parser: "TypeScriptLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "import_extract", "type_detect" },
            Limitations: new[] { "regex_only", "no_type_checker" }
        ),

        // ── Config / Data ────────────────────────────────────────────────────
        new(
            Extension: ".json",
            Category: "config",
            Tier: "T2_text_extract",
            Parser: "JsonTextReader",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "text_read", "key_detect" },
            Limitations: new[] { "no_schema_validation", "no_semantic_query" }
        ),
        new(
            Extension: ".yaml",
            Category: "config",
            Tier: "T2_text_extract",
            Parser: "YamlTextReader",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "text_read" },
            Limitations: new[] { "no_schema_validation" }
        ),

        // ── Research ─────────────────────────────────────────────────────────
        new(
            Extension: ".md",
            Category: "research",
            Tier: "T3_structural_extract",
            Parser: "MarkdownLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "heading_extract", "frontmatter_detect", "wikilink_detect", "citation_key_detect" },
            Limitations: new[] { "no_semantic_analysis", "citation_heuristic_only" }
        ),
        new(
            Extension: ".bib",
            Category: "research",
            Tier: "T3_structural_extract",
            Parser: "BibTexLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "citation_key_extract", "author_detect", "year_detect" },
            Limitations: new[] { "regex_only", "no_doi_resolution" }
        ),

        // ── Documents ────────────────────────────────────────────────────────
        new(
            Extension: ".html",
            Category: "document",
            Tier: "T2_text_extract",
            Parser: "HtmlDocumentParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "title_extract", "heading_detect", "link_detect" },
            Limitations: new[] { "no_js_execution", "no_css_rendering" }
        ),
        new(
            Extension: ".htm",
            Category: "document",
            Tier: "T2_text_extract",
            Parser: "HtmlDocumentParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "title_extract", "heading_detect", "link_detect" },
            Limitations: new[] { "no_js_execution", "no_css_rendering" }
        ),
        new(
            Extension: ".pdf",
            Category: "document",
            Tier: "T1_metadata",
            Parser: "PdfMetadataAdapter",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "header_sniff" },
            Limitations: new[] { "no_text_extraction_without_external_tool", "metadata_only" }
        ),
        new(
            Extension: ".epub",
            Category: "document",
            Tier: "T1_metadata",
            Parser: "EpubMetadataAdapter",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "opf_metadata_extract", "title_extract", "author_extract" },
            Limitations: new[] { "no_full_text_extraction", "metadata_only" }
        ),
        new(
            Extension: ".tex",
            Category: "document",
            Tier: "T3_structural_extract",
            Parser: "LatexLiteParser",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "section_detect", "equation_candidate_detect", "citation_key_detect" },
            Limitations: new[] { "regex_only", "no_latex_compiler" }
        ),

        // ── Plain text / data ────────────────────────────────────────────────
        new(
            Extension: ".txt",
            Category: "text",
            Tier: "T2_text_extract",
            Parser: "PlainTextReader",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "text_read" },
            Limitations: new[] { "no_structure_detection", "no_symbol_extract" }
        ),
        new(
            Extension: ".csv",
            Category: "data",
            Tier: "T2_text_extract",
            Parser: "CsvTextReader",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "line_count", "hash", "text_read", "column_detect" },
            Limitations: new[] { "no_type_inference", "no_statistics" }
        ),

        // ── Creative / Media ─────────────────────────────────────────────────
        new(
            Extension: ".wav",
            Category: "creative",
            Tier: "T0_ledger_only",
            Parser: "AudioLedgerOnly",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "path_record" },
            Limitations: new[] { "no_audio_analysis", "no_duration", "no_bpm", "no_perceptual_claims", "external_ffprobe_needed_for_metadata" }
        ),
        new(
            Extension: ".mp3",
            Category: "creative",
            Tier: "T0_ledger_only",
            Parser: "AudioLedgerOnly",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "path_record" },
            Limitations: new[] { "no_audio_analysis", "no_duration", "no_bpm", "no_perceptual_claims" }
        ),
        new(
            Extension: ".mp4",
            Category: "creative",
            Tier: "T0_ledger_only",
            Parser: "VideoLedgerOnly",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "path_record" },
            Limitations: new[] { "no_frame_analysis", "no_motion_detection", "no_perceptual_claims", "external_ffprobe_needed" }
        ),
        new(
            Extension: ".png",
            Category: "creative",
            Tier: "T0_ledger_only",
            Parser: "ImageLedgerOnly",
            RequiresExternalTool: false,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "path_record" },
            Limitations: new[] { "no_image_analysis", "no_color_extraction", "no_perceptual_claims" }
        ),
        new(
            Extension: ".blend",
            Category: "creative",
            Tier: "T0_ledger_only",
            Parser: "BlendLedgerOnly",
            RequiresExternalTool: true,
            RequiresAI: false,
            NetworkRequired: false,
            Capabilities: new[] { "hash", "size_bytes", "path_record" },
            Limitations: new[] { "binary_format", "blender_python_needed_for_extraction" }
        ),
    }.AsReadOnly();
}
