# Complete eligible-source snapshot (v0.2.5)

## Explorer integration (v0.3.1-preview)

All AI Flatten right-click actions now use this snapshot writer. Select a folder
for its eligible source, a file for that file, or several items for their combined
selection. Use `AI Flatten 0.3.1 > Complete Source Snapshot`; the same action is
available under Show more options, including on a folder background. The result
dialog reports the output location and verification status. No prompt-starter
footer is appended. Old context-pack and task-context commands remain CLI-only.

## Why this is a separate output mode

The default AI context pack is relevance-/budget-limited, even in `--mode full`.
Its default `--max-total-chars 2000000` may omit major runtime modules.
`--split true` on the legacy path does not override that prior selection.
Do not mistake a context pack for a complete representation of the working tree.

`--snapshot-complete true` uses `FolderScanner` policies and captures **all eligible
text candidates** without `BudgetSelector`, `--max-total-chars`, or `--max-file-bytes`.
It does **not** mean every filesystem object: hidden entries, ignored directories,
unsupported formats, binaries, and selected-input restrictions still apply and
are reported. It rejects `--git-ref`, `--git-changed`, and `--git-staged`; capture
the actual working folder instead. It fails the export if an eligible source
cannot be read or changes during capture. File content may be privacy-redacted.

## Windows PowerShell

```powershell
AIFolderFlatten.exe --input "D:\work\project" --snapshot-complete true `
  --output "D:\exports" --split-size-chars 300000 `
  --redact true --open-after-export never
```

To capture unredacted source for **trusted local storage only**, explicitly pass
`--redact false --redact-paths false`. The raw original file SHA256 hashes
recorded in the manifest are different from the exported text hashes if a file
was redacted or its original byte encoding differs from UTF-8 representation.
Review for secrets before uploading anywhere.

Snapshot outputs are written to a **new directory** outside the input tree.
`--output` means the destination **parent directory**, not a filename. This
mode always emits a directory, regardless of `--format`/`--output-layout`.
`--dry-run true` scans files without writing them. Use `--split-size-chars`
(minimum 4096; default 300000) to cap each individual `.jsonl` or `.md` part.

The output contains:

- `INDEX.md`: readable index, scope and policy exclusions;
- `PART_00001.md`, ...: readable flattened code chunks (formatting separators
  are *not* part of source text);
- `PART_00001.jsonl`, ...: authoritative structured chunks; join each file's
  JSON `Content` values by `Index` to reconstruct exactly the exported text;
- `SOURCE_MANIFEST.json`: original byte hash, exported-text hash, chunk hashes,
  coverage status and policy omissions;
- `.ai-seed-output.json`: generated-output marker.

For large individual files, chunks may span multiple parts. No file content is
dropped simply because a part would exceed the size limit.

## Verification

```powershell
AIFolderFlatten.exe --verify-snapshot "D:\exports\project_SOURCE_SNAPSHOT_20261013_210000_0000000"
```

The verifier independently reads `.jsonl` chunks, checks their order, number,
content hashes, complete per-file exported-text hashes, and detects missing or
extra JSONL part files. The source-byte SHA256 was recorded during capture;
this *offline verification* cannot independently prove that the original source
has not changed since capture, or that intentionally excluded files are present.
Markdown is for human/AI readability; the `.jsonl` text is the source of truth.

`CoverageStatus=COMPLETE` is issued only if there were no policy exclusions;
`COMPLETE_ELIGIBLE_WITH_POLICY_EXCLUSIONS` explicitly tells you the snapshot
is **not** complete coverage of the filesystem, despite complete capture of
eligible sources. If any eligible source cannot be captured, the write fails
instead of issuing a misleading success marker.

### Practical boundaries

- Processing is bounded by a **single file** in memory: `FileText.ReadTextFile`
  and privacy redaction still load one complete file before chunking. Very large
  individual files may require further streaming refactoring.
- UTF-8 files are read with strict UTF-8 decoding; the existing fallback is
  Latin-1. BOM-marked UTF-16 text is allowed past the binary probe and is
  decoded by the existing BOM-aware reader. Exports are UTF-8 text, not
  byte-identical file archives.
- Concurrent file edits may create inconsistent source content. Before/after
  source byte hashes detect changed content in most cases; use a quiescent tree
  for reliable capture. The program cannot guarantee filesystem-atomic snapshots.
- A `PART*.md` fenced block adds display-only newlines and should not be used
  for byte-exact round-trip; reconstruct exported text from JSONL instead.
- Ignore-rule behavior is inherited from `FolderScanner` and `IgnoreRules`.
  Negated `.gitignore` patterns are not currently implemented; review policy
  omissions and explicitly adjust your source inclusion policy when needed.
