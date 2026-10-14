using System;
using System.Collections.Generic;

namespace AIFolderFlatten.Artifacts;

internal sealed record ArtifactIR(
    string Id,
    string Path,
    string Extension,
    string ArtifactType, // code, document, research, modeling, creative, unknown
    string ParserTier,   // T0_ledger_only, T1_metadata, T2_text_extract, T3_structural_extract, T4_domain_ir
    string Status,       // parsed, metadata_only, ledger_only, external_tool_needed, unsupported_preserved
    long SizeBytes,
    string Sha256,
    DateTime ModifiedUtc,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Warnings
);
