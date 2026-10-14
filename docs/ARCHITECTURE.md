# Architecture

## Boundary

AIFolderFlatten is a Windows-local deterministic expert system. The CLI,
Explorer menu, and notification dialogs are adapters around the same core.
They do not own analysis policy and cannot enable LLM, embedding, agent, or
network execution.

## Pipeline

```text
CLI / Explorer
  -> AppConfig and expert-system guard
  -> ArtifactScanner and parser capability registry
  -> POST orientation (domain, lens, source state)
  -> FolderScanner and ignore rules
  -> File reading, redaction, and privacy audit
  -> FileAnalyzer and budget selection
  -> Domain analyzers
       documents / research / modeling / creative / development / Git
  -> Markdown, text, or pack rendering
  -> provenance and manifest attestation
```

## Layers

```text
interface       Program.cs, Explorer installer, notification dialogs
configuration   AppConfig, ProfileConfig
scanning        FolderScanner, ArtifactScanner, IgnoreRules, FileText
analysis        FileAnalyzer and domain-specific deterministic analyzers
rendering       MarkdownRenderer, TextRenderer, domain renderers
integrity       Redactor, PrivacyScanner, ExpertSystemBoundary,
                ExpertSystemProvenance, ManifestVerifier
delivery        ExportWriter and release scripts
```

## Core Models

- `ArtifactIR`: every discovered artifact, including binary and unsupported files.
- `OrientationState`: selected domain, lens, source state, evidence, and alternatives.
- `FileSnapshot`: included and redacted text with analysis metadata.
- `ContextPackage`: renderer input for the complete context export.
- `ManifestIR`: expert-system and pack-integrity attestation.

## Citation Contract

Included text files receive stable IDs such as `F0001`. `FILES.md` wraps content
in `BEGIN_FILE` and `END_FILE` markers. With line numbering enabled, consumers
can cite anchors such as `F0001:L10-L25`.
