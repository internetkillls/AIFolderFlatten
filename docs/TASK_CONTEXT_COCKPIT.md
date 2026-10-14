# Task-Aware Context Compiler + Human Context Cockpit (v0.3.0-preview)

## Scope / boundaries

This is a **deterministic, local, task-scoped context feed**. It is not a full
repository snapshot, a semantic call graph, an LLM agent, an IDE, or a proof of
software correctness. It does **not** modify project source files. The legacy
`--mode`/`--format` export path remains unchanged.

The external patch used v0.2.4 as its baseline. This repository integrates it
with v0.2.5 complete snapshots and the Windows build/install launchers.
Task-context and snapshot-complete are mutually exclusive export modes.

### Basic invocation (PowerShell)

```powershell
AIFolderFlatten.exe --input "D:\projects\my-app" `
  --task-context "Remove duplicate Settings controls without breaking migrations" `
  --output "D:\aif-exports" `
  --task-budget-chars 180000 --task-cockpit true
```

Output is a uniquely named folder **outside the scanned source** with:

- `CONTEXT.md` — task-scoped source with paths, reasons, line hints and hashes
- `CONTEXT_DATA.json` — authoritative selected content and candidate metadata
- `SELECTION_REPORT.md` — selected/omitted paths, reasons, and scanner exclusions
- `CONTEXT_REVIEW.html` — static offline cockpit (open locally in a browser)
- `.ai-seed-output.json` — generated-output marker

### Human review loop

1. Read the explicit task, selected file count, budget, and omissions.
2. Search candidates and inspect lexical evidence; optionally deselect files
   already captured in `CONTEXT_DATA.json`.
3. Copy/download the reviewed Markdown feed. UI-selected variations are **not**
   independently verified by the original offline verifier; retain original
   `CONTEXT_DATA.json` to establish source provenance.
4. If missing files matter, copy pinned paths and re-run CLI with
   `--task-pins "src/Settings.cs,src/SettingsMigration.cs"`. A pin is not a
   budget override; raise `--task-budget-chars` to include large pinned sources.
5. Before an AI writes code, verify the original export against the current tree:

```powershell
AIFolderFlatten.exe --verify-task-context "D:\aif-exports\my-app_TASK_CONTEXT_..." --input "D:\projects\my-app"
```

The verifier compares selected text digests against `CONTEXT.md`, checks
manifest counts, and checks original source **bytes** against stored hashes
when `--input` is supplied. It does not verify unselected sources or guarantee
that unrelated code remained unchanged. It does not cryptographically sign
against attackers who can rewrite the manifest and context together.

### Selection method

- Tokenizes task terms and filename identifiers (CamelCase/snake_case).
- Scores path matches, bounded source-line lexical hits, explicit human pins.
- Boosts likely filename-related tests/configurations and source lines that
  mention a selected filename stem. The latter are **unverified text hints**;
  they do not establish a call graph, interface contract or runtime behavior.
- Includes full eligible files until the specified **character budget**; no
  partial file content is silently included. Every budget omission is visible.
- Uses `FolderScanner` inclusion policies, with the same `.aiignore`,
  `.gitignore`, hidden-file, default exclusion and extension limitations.
- Uses `--max-file-bytes` as a task scanning cap (default 1 MiB). A pinned file
  bypasses the task scanner size cap, subject to memory and context budget.
- Reads one file at a time during analysis; selected files and the offline
  review page can still be large. Prefer a moderate budget.
- Privacy redaction is **best effort**, on by default. `--privacy-strict true`
  detects recognized residual secret formats, not arbitrary credentials.
- Scans the current working folder. The task mode rejects `--git-ref`,
  `--git-staged` and `--git-changed` to avoid unsupported historical/subset claims.
- Does not use network, embeddings, an LLM runtime or launch an external app.

### Suggested evaluation

Baseline against legacy export on the same tasks with known relevant files:
relevant-file recall, required-dependency recall (manually labeled), exported
source chars, time to human review, and verified patch/test success. A smaller
feed does not automatically imply better AI decisions.

### Known gaps (future work)

- Cross-language semantic symbol graph using Roslyn / language servers.
- Incremental hash cache, changed-file delta packs, true task dependency closure.
- Capture of a full, chunked, lossless snapshot is a **separate** v0.2.5 change set.
- UI inclusion of unloaded files needs recompile; current static HTML cannot
  access arbitrary local files by design.
- There is no runtime test guarantee until the .NET tests are executed on a
  Windows-capable SDK environment.
