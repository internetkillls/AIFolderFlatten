using System;
using System.Collections.Generic;

namespace AIFolderFlatten.Post;

internal sealed record AppState(
    string AppVersion,
    string SourceRoot,
    DateTimeOffset StartedAt,
    AppConfig Config,
    RuntimeState Runtime,
    ScanState Scan,
    OrientationState Orientation,
    ExportState Export
);

internal sealed record RuntimeState(
    bool IsWindows,
    bool GitAvailable,
    bool IsGitRepo,
    bool FfprobeAvailable,
    bool PythonAvailable,
    IReadOnlyList<string> Warnings
);

internal sealed record ScanState(
    int TotalFilesSeen,
    int IncludedTextFiles,
    int SkippedFiles,
    int UnknownArtifacts,
    int MediaArtifacts,
    int ResearchSignals,
    int CreativeSignals,
    int CodeSignals,
    int ObsidianSignals,
    int DocumentSignals,
    int ModelingSignals
);

internal sealed record OrientationState(
    string SelectedDomain,
    string SelectedLens,
    string SelectedSourceState,
    double Confidence,
    IReadOnlyList<OrientationHypothesis> Hypotheses,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> Alternatives
);

internal sealed record OrientationHypothesis(
    string Domain,
    double Score,
    string Reason
);

internal sealed record ExportState(
    string Format,
    string OutputRoot,
    IReadOnlyList<string> OutputFiles,
    bool OpenAfterExport,
    bool Notify,
    IReadOnlyList<string> Warnings
);
