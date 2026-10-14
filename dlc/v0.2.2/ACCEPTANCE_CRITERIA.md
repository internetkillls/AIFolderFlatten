# Acceptance Criteria

- The DLC contains `CODEX_IMPLEMENTATION_PROMPT.md`, `CODEX_TEST_PROMPT.md`, `CODEX_REVIEW_PROMPT.md`, `ACCEPTANCE_CRITERIA.md`, and `STOP_CONDITIONS.md`.
- Each Codex prompt defines exact scope, forbidden changes, tests to run, expected files, manifest and provenance requirements, and the expert-system-only boundary.
- `scripts/validate-dlc.ps1` fails on a missing required file or clause.
- `scripts/build-release.ps1` validates the DLC before publishing and copies it to `release/win-x64/DLC`.
- `MANIFEST.json` attests `aiCore=false`, `networkUsed=false`, `llmUsed=false`, `embeddingsUsed=false`, and `expertSystemOnly=true`.
- Pack generation emits `EXPERT_SYSTEM_PROVENANCE.md`.
- The full automated test suite and DLC validation pass.
- Core execution remains deterministic and uses no LLM, embedding, agent, or network service.
