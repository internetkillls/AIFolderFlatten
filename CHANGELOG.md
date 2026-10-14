# Changelog

## v0.3.1-preview — 2026-10-14

- All Explorer actions now offer only Complete Source Snapshot, including folder background and legacy fallback.
- Removed context-pack, starter and Git-subset actions from Explorer menus.
- Added completion notifications and a versioned native menu title.
- Explorer snapshots override saved export routing while retaining inclusion and privacy policies.
- Fixed single-file snapshot selection to avoid including unselected sibling files.
- Added menu-to-CLI coverage tests for large source, agent history, saved profiles and multi-select.

## v0.3.0-preview — 2026-10-14

- Added deterministic task-aware context selection, pinned paths, and character budgets.
- Added offline `CONTEXT_REVIEW.html` for source review and Markdown download.
- Added task-context integrity and source-freshness verification.
- Retained v0.2.5 complete-source snapshots; conflicting export modes are rejected.
- Included Windows batch launchers for build and Explorer-menu installation.

## v0.2.5 — 2026-10-13

Added opt-in complete eligible-source snapshots alongside the existing context pack modes.

### Added
- `--snapshot-complete true` captures all eligible text source without total-character or file-size limits.
- Chunked Markdown and JSONL parts, `SOURCE_MANIFEST.json`, and `--verify-snapshot` for coverage and integrity checks.
- Explicit policy exclusions, fail-on-read-error behavior, and BOM-marked Unicode text support.
- Six snapshot regression tests and a PowerShell smoke test covering export, verification, and tamper detection.

## v0.2.4 — 2026-06-13

Master Flatten as primary output, prompt starter templates, and cascading Explorer menu.

### Added
- Single-file master output (`*_AI_SEED_MASTER_*`) as default via `--output-layout single|pack|zip|both`.
- `--format xml|json|jsonl` support in addition to md/txt.
- `--prompt-starter` flag with built-in templates: debug, mockup-review, vibe-continue, explain, notebooklm.
- Cascading Explorer menu with 5 groups: Flatten Single File, Flatten with Starter, Flatten Pack, Git Flatten, Advanced.
- No auto-open Explorer by default (`--open-after-export never`).
- Reflatten safety: skips `*_AI_SEED_MASTER_*` files and `*_AI_SEED_*/` directories.

## v0.2.3 — 2026-06-13

Added deterministic development analysis and Git delta outputs.

### Added
- Test maps, test-gap reports, hotspot reports, and fixture coverage.
- Contract/version/docs drift reports.
- Dependency maps, build command detection, and dependency risk reports.
- Git changed/staged summaries, delta context, and patch handoff prompts.

## v0.2.2 — 2026-06-13

Hardened the expert-system-only boundary and provenance contract.

### Added
- Runtime rejection of LLM, network, and embedding modes.
- Expert-system manifest attestation.
- `EXPERT_SYSTEM_PROVENANCE.md` and toolchain status output.
- Parser capability registries and candidate-language output naming.

## v0.2.1 — 2026-06-13

Hardened the compiler, improved Windows Explorer integration, added git modes, manifest verification, and a comprehensive xUnit test suite.

### Added
- Grouped Explorer cascading submenu for 9 task-based profiles.
- Native keyboard access keys (hotkeys) for all submenu items.
- Git revision snapshot (`--git-ref`), staged changes (`--git-staged`), and unstaged changed files (`--git-changed`) support.
- Dry run / preview mode (`--dry-run`).
- Local config file (`.ai-flatten.json`).
- Strict privacy fail-safe checks (`--privacy-strict`).
- MANIFEST.json verification command (`--verify-manifest`).
- Parity for TextRenderer with full symbols, dependencies, risks, skips, and redactions.
- xUnit automated test suite project.
- Typed stable exit code contract.
- Added documentation for config, security, testing, explorer menu, and roadmap.

### Changed
- Output files are now placed directly in the parent directory (next to the target folder, WinRAR-style) by default.
- Default exclude filters to ignore build/release output and binaries (`release/`, `*.zip`, `*.exe`, etc.).
- Deriving split file target sizes from `--split-size-chars`.
- Refactored Program.cs to focus on CLI orchestration under 250 lines.

## v0.2.0 — 2026-06-13

Rebuilt from a basic folder flattener into an AI folder context compiler.

### Added

- Context profiles: overview, debug, architecture, obsidian, research, prompt-pack.
- Context modes: overview, debug, refactor, architecture, code-review, obsidian, research, prompt-pack, full.
- Pack output with CONTEXT.md, FILES.md, SYMBOLS.md, DEPENDENCIES.md, RISKS.md, SKIPPED.md, REDACTIONS.md, USER_PROMPT.md, MANIFEST.json.
- Stable file IDs.
- Strong BEGIN_FILE / END_FILE boundaries.
- Optional line numbers.
- File classifier.
- SHA256 and line count per file.
- Symbol extraction for C#, TypeScript/JavaScript, Python, Markdown headings.
- Import extraction for C#, TypeScript/JavaScript, Python, Java/Kotlin, Swift.
- Markdown/Obsidian extraction: frontmatter keys, headings, wikilinks, tags, tasks.
- Project intelligence and suggested verification commands.
- Prompt-injection-like text warnings.
- Expanded redaction categories.
- .aiignore, .gitignore, include/exclude glob support.
- Priority-based budget selection.
- Explorer context menu profile installer.

### Known limitations

- Regex-based parser, not Roslyn.
- Simple gitignore parser.
- No GUI preview yet.
- Source package only; compile on Windows with .NET SDK.
