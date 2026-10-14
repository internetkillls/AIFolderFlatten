# Codex Review Prompt

## Exact Scope

Review the AIFolderFlatten v0.2.2 implementation for regressions in the expert-system-only boundary, deterministic behavior, manifest integrity, provenance completeness, heuristic labeling, and DLC packaging.

## Forbidden Changes

- Do not approve code that introduces an LLM, agent, embedding, network, telemetry, or cloud dependency.
- Do not accept missing or permissive manifest attestation.
- Do not accept provenance that hides external tools, fallbacks, parser tiers, or limitations.
- Do not broaden the review into unrelated feature redesign.

## Tests To Run

```powershell
dotnet test tests/AIFolderFlatten.Tests/AIFolderFlatten.Tests.csproj
powershell -ExecutionPolicy Bypass -File scripts/validate-dlc.ps1
```

Review the release build script and confirm the validated DLC tree is copied into the deliverable.

## Expected Files

- `src/AIFolderFlatten/ExpertSystem/ExpertSystemBoundary.cs`
- `src/AIFolderFlatten/ExpertSystem/ExpertSystemProvenance.cs`
- `src/AIFolderFlatten/ExportWriter.cs`
- `src/AIFolderFlatten/ManifestVerifier.cs`
- `scripts/validate-dlc.ps1`
- `scripts/build-release.ps1`
- `dlc/v0.2.2/CODEX_IMPLEMENTATION_PROMPT.md`
- `dlc/v0.2.2/CODEX_TEST_PROMPT.md`
- `dlc/v0.2.2/CODEX_REVIEW_PROMPT.md`
- `dlc/v0.2.2/ACCEPTANCE_CRITERIA.md`
- `dlc/v0.2.2/STOP_CONDITIONS.md`

## Manifest And Provenance Requirements

- Confirm manifest values are generated from immutable expert-system boundary constants.
- Confirm verification rejects missing, malformed, or boundary-violating values.
- Confirm provenance is human-readable and records execution facts without claiming AI generation.
- Confirm every release DLC includes the five governance files.

## Expert-System-Only Boundary

Reject the change if core execution can invoke LLMs, embeddings, agents, or network services, or if heuristic candidate output is presented as authoritative AI output.

