# Codex Test Prompt

## Exact Scope

Verify only the AIFolderFlatten v0.2.2 expert-system boundary, manifest attestation, provenance output, deterministic parser behavior, and DLC completeness.

## Forbidden Changes

- Do not change production behavior merely to bypass a failing assertion.
- Do not introduce network, LLM, embedding, agent, or cloud-backed test fixtures.
- Do not relax manifest or provenance assertions.
- Do not modify unrelated features or installer behavior.

## Tests To Run

```powershell
dotnet test tests/AIFolderFlatten.Tests/AIFolderFlatten.Tests.csproj
powershell -ExecutionPolicy Bypass -File scripts/validate-dlc.ps1
powershell -ExecutionPolicy Bypass -File scripts/build-release.ps1
```

After the release build, confirm `release/win-x64/DLC/v0.2.2` contains all five required DLC files.

## Expected Files

- `tests/AIFolderFlatten.Tests/ExpertSystemBoundaryTests.cs`
- `tests/AIFolderFlatten.Tests/ManifestExpertSystemTests.cs`
- `tests/AIFolderFlatten.Tests/ManifestTests.cs`
- `scripts/validate-dlc.ps1`
- `scripts/build-release.ps1`
- `release/win-x64/DLC/v0.2.2/CODEX_IMPLEMENTATION_PROMPT.md`
- `release/win-x64/DLC/v0.2.2/CODEX_TEST_PROMPT.md`
- `release/win-x64/DLC/v0.2.2/CODEX_REVIEW_PROMPT.md`
- `release/win-x64/DLC/v0.2.2/ACCEPTANCE_CRITERIA.md`
- `release/win-x64/DLC/v0.2.2/STOP_CONDITIONS.md`

## Manifest And Provenance Requirements

- Assert all five expert-system manifest fields and their required boolean values.
- Assert provenance is emitted for pack output and identifies deterministic execution.
- Assert verification fails for missing or contradictory expert-system attestation.
- Assert release packaging does not omit the DLC provenance requirements.

## Expert-System-Only Boundary

All tests must run without LLMs, embeddings, agents, or network access. External parser tools may only be optional local tools with explicit provenance and deterministic fallbacks.

