# Getting started on Windows

## Requirements

- Windows 11 x64 for the modern Explorer menu.
- .NET 8 SDK or a compatible newer SDK.
- Visual Studio with Desktop development with C++ and a Windows SDK for the native menu.
- Node.js only when running the cockpit JavaScript smoke test.

The executable is a command-line application. Double-clicking it without input
does not launch an installer or a desktop editor.

## Build and install

Clone this repository, then double-click `build.bat` at its root. The build creates
`release\win-x64`. Double-click `install.bat` inside that directory to register
the per-user Explorer menu. Keep the release directory in place after installation.

The native build script currently searches common Visual Studio 2022 and Visual
Studio 18 Insiders installation paths. If your installation is elsewhere, update
the candidate paths in `scripts/build-modern-context-menu.ps1`.

## Create a complete snapshot

Right-click the repository folder, then select **AI Flatten 0.3.1 > Complete Source
Snapshot**. The completion dialog reports the output directory and integrity result.
For the folder background, use **Show more options**. Single-file and multi-select
actions capture the selected scope rather than the entire repository.

Alternatively, from PowerShell at the repository root:

```powershell
.\release\win-x64\AIFolderFlatten.exe --input 'C:\Projects\Example' `
  --snapshot-complete true --output 'C:\Exports' --split-size-chars 300000
```

The snapshot contains `INDEX.md`, `SOURCE_MANIFEST.json` and numbered Markdown/JSONL
parts. Keep all parts together. Check the manifest exclusions before treating the
export as sufficient for your task. Redaction defaults to enabled; the exported
text can differ from the original bytes.

```powershell
.\release\win-x64\AIFolderFlatten.exe --verify-snapshot 'C:\Exports\Example_SOURCE_SNAPSHOT_...'
```

## Validate a development checkout

```powershell
dotnet test .\tests\AIFolderFlatten.Tests\AIFolderFlatten.Tests.csproj
node .\scripts\smoke-cockpit-ui.js
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\smoke-complete-snapshot.ps1 -Executable .\release\win-x64\AIFolderFlatten.exe
```

## Optional task review

Use `--task-context 'Describe the task'` to generate `CONTEXT_REVIEW.html` for
offline review of selected evidence. This mode has a budget and does not guarantee
complete source coverage. See the [cockpit guide](TASK_CONTEXT_COCKPIT.md).
