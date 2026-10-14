# AIFolderFlatten v0.3.1-preview

A Windows-local tool that turns a selected repository into verified Markdown and
JSONL source snapshots. Every Explorer context-menu action creates a **Complete
Source Snapshot**. An optional CLI mode creates task-scoped context packs with an
offline HTML review cockpit.

[Getting started](docs/GETTING_STARTED.md) · [Documentation](docs/README.md) ·
[Snapshot contract](docs/COMPLETE_SOURCE_SNAPSHOT.md) · [Changelog](CHANGELOG.md)

## Complete snapshots

- Capture every eligible text file in the selected working-tree scope.
- No total-character budget or per-file size cap in complete-snapshot mode.
- Split large source across numbered Markdown and JSONL parts.
- Record source-byte and exported-text SHA-256 hashes in `SOURCE_MANIFEST.json`.
- Verify chunks, part inventory and exported content before publishing the output directory.
- Report ignore rules, binary exclusions and unsupported formats explicitly.

“Complete” means complete eligible text, not every filesystem object. Redaction
is enabled by default, and exported text may differ from original bytes. Review
`INDEX.md` and the manifest exclusions before using an export for a major refactor.

## Windows build and installation

Build requirements: Windows, .NET 8 or a compatible newer SDK, and Visual Studio
with the Desktop development with C++ workload and a Windows SDK. The modern
Explorer extension targets Windows 11 x64. See [setup details](docs/GETTING_STARTED.md).

1. Clone this repository.
2. Double-click `build.bat` in the repository root.
3. Double-click `release\win-x64\install.bat`.
4. Right-click the source folder and select **AI Flatten 0.3.1 > Complete Source Snapshot**.

The same snapshot action is available for selected files, multi-select, and the
folder background under **Show more options**. A completion dialog reports the
output directory and verification result. Keep the release directory in place
after installation. `AIFolderFlatten.exe` is a CLI, not a setup installer.

## CLI usage

```powershell
.\release\win-x64\AIFolderFlatten.exe --input 'C:\Projects\Example' `
  --snapshot-complete true --output 'C:\Exports' --split-size-chars 300000

.\release\win-x64\AIFolderFlatten.exe --verify-snapshot 'C:\Exports\Example_SOURCE_SNAPSHOT_...'
```

Output includes `INDEX.md`, `SOURCE_MANIFEST.json`, paired `PART_*.md` and
`PART_*.jsonl` files, and an output marker. Keep the whole directory together.
JSONL is authoritative for reconstructing exported text; Markdown is for reading.

For a task-scoped review instead:

```powershell
.\release\win-x64\AIFolderFlatten.exe --input 'C:\Projects\Example' `
  --task-context 'Review settings migration behavior' --output 'C:\Exports'
```

Open the generated `CONTEXT_REVIEW.html` locally to inspect evidence and download
a reviewed feed. Task mode uses a budget and does not provide complete coverage.
Legacy context-pack formats remain available through the CLI. Use `--help` for options.

## Validation

```powershell
dotnet test .\tests\AIFolderFlatten.Tests\AIFolderFlatten.Tests.csproj
node .\scripts\smoke-cockpit-ui.js
```

The Windows CI workflow runs these checks. Release smoke tests cover snapshot
verification, tampering, task-source freshness, and the compiled native menu.
See [Testing](docs/TESTING.md) and [Contributing](CONTRIBUTING.md).

## Privacy and limitations

Processing is deterministic and local: no LLM or network service is used by the
application. Redaction is best-effort; inspect exports before sharing. A snapshot
is not filesystem-atomic, so use a stable working tree. Large individual files
are loaded into memory before chunking. See [Security](docs/SECURITY.md) and the
[snapshot boundaries](docs/COMPLETE_SOURCE_SNAPSHOT.md).

## License

No open-source license has been selected. Public availability does not grant an
open-source license.
