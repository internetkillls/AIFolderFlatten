namespace AIFolderFlatten;

internal sealed record FileCandidate(string AbsolutePath, string RelativePath);

internal sealed record SkippedFile(string RelativePath, string Reason);

internal sealed record RedactionFinding(string FileId, string RelativePath, string Category, int Count);

internal sealed record FileAnalysis(
    string Kind,
    string Language,
    string Sha256,
    int LineCount,
    int PriorityScore,
    IReadOnlyList<string> Symbols,
    IReadOnlyList<string> Imports,
    IReadOnlyList<string> Headings,
    IReadOnlyList<string> FrontmatterKeys,
    IReadOnlyList<string> WikiLinks,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Tasks,
    IReadOnlyList<string> Todos,
    IReadOnlyList<string> Warnings);

internal sealed record FileSnapshot(
    string Id,
    string RelativePath,
    long ByteLength,
    DateTime ModifiedUtc,
    string Content,
    FileAnalysis Analysis,
    IReadOnlyDictionary<string, int> Redactions);

internal sealed record ContextPackage(
    string SourceRoot,
    string RootName,
    string Mode,
    string Format,
    string ContentLevel,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<FileSnapshot> Files,
    IReadOnlyList<SkippedFile> Skipped,
    IReadOnlyList<string> ProjectSignals,
    IReadOnlyList<string> SuggestedCommands,
    bool RedactionEnabled,
    bool LineNumbersEnabled,
    bool PromptInjectionScanEnabled,
    long MaxTotalChars,
    long SplitSizeChars,
    string AppVersion,
    GitInfo? Git = null);

internal sealed record ExportResult(IReadOnlyList<string> OutputPaths, int IncludedCount, int SkippedCount, int TotalCharacters);

internal class AIFolderFlattenException : Exception
{
    public int ExitCode { get; }
    public AIFolderFlattenException(string message, int exitCode) : base(message)
    {
        ExitCode = exitCode;
    }
}

internal sealed class ExportException : AIFolderFlattenException
{
    public ExportException(string message) : base(message, ExitCodes.OutputWriteFailure) { }
}

internal sealed class PrivacyLeakException : AIFolderFlattenException
{
    public PrivacyLeakException(string message) : base(message, ExitCodes.PrivacyLeakFailure) { }
}

internal sealed class GitException : AIFolderFlattenException
{
    public GitException(string message) : base(message, ExitCodes.GitFailure) { }
}

internal sealed class ScanException : AIFolderFlattenException
{
    public ScanException(string message) : base(message, ExitCodes.ScanFailure) { }
}

internal sealed class OutputWriteException : AIFolderFlattenException
{
    public OutputWriteException(string message) : base(message, ExitCodes.OutputWriteFailure) { }
}

internal sealed record GitInfo(
    bool IsGitRepo,
    string? Branch,
    string? HeadSha,
    string? Mode,
    string? Ref,
    IReadOnlyList<string> IncludedPaths,
    IReadOnlyList<string> Warnings
);

internal sealed record ManifestVerificationResult(
    bool Passed,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings
);

// ── Expert-System-Only records ──────────────────────────────────────────────
// Note: ExpertSystemProvenanceIR is defined in ExpertSystem/ExpertSystemProvenance.cs
// Note: ToolchainStatusEntry is defined in ExpertSystem/ToolchainDoctor.cs

/// <summary>
/// Full manifest IR including all v0.2.2 expert-system fields.
/// </summary>
internal sealed record ManifestIR(
    string Kind,
    string Version,
    string RootName,
    string AppVersion,
    DateTimeOffset GeneratedAt,
    string Mode,
    bool AiCore,
    bool NetworkUsed,
    bool LlmUsed,
    bool EmbeddingsUsed,
    bool ExpertSystemOnly,
    IReadOnlyList<string> Toolchains,
    IReadOnlyList<string> ParserTiers,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<ManifestFileEntry> IncludedFiles,
    IReadOnlyList<ManifestFileEntry> Files
);

internal sealed record ManifestFileEntry(string Id, string Path);

// ── v0.2.4b Master Flatten & Reflatten Safety ───────────────────────────────

internal enum OutputLayout
{
    Single,
    Pack,
    Zip,
    Both
}

internal enum FileStatus
{
    IncludedContent,
    IncludedLedgerOnly,
    SkippedByIgnore,
    SkippedBySize,
    SkippedByBudget,
    SkippedByBinaryPolicy,
    SkippedByUnsupportedPolicy,
    SkippedByPrivacyPolicy,
    SkippedGeneratedOutput,
    ErrorUnreadable,
    ErrorPathMissing
}

internal enum SourceKind
{
    SourceInput,
    SelectedInput,
    GitInput,
    GeneratedAiSeedOutput,
    GeneratedPackFile,
    ExternalToolOutput,
    UnknownInput
}

internal enum OutputRootMode
{
    SingleFileNextToSource,
    DedicatedOutputFolder,
    CustomOutputPath
}

internal sealed record MasterFlattenIdentity(
    string Path,
    string Format,
    string? Sha256,
    long? Bytes,
    int? Lines,
    bool IsPrimaryArtifact
);
