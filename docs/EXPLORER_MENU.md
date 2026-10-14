# Windows Explorer: Complete Source Snapshot

Every AI Flatten context-menu action exports a complete eligible-source snapshot.
The modern menu is `AI Flatten 0.3.1 > Complete Source Snapshot` for selected files,
folders, and multiple selected items. The folder-background command is available
under Show more options as `AI Flatten - Complete Source Snapshot`.

Double-click `release\win-x64\install.bat` after building with `build.bat`.
The installer replaces older AI Flatten actions and adds the same snapshot
action to legacy file/folder menus under Show more options. All registrations are
per-user; unrelated Explorer preferences are preserved.

The completion dialog reports the output folder and integrity result. Output is
placed in a new `*_SOURCE_SNAPSHOT_*` directory beside the common source root,
with `INDEX.md`, `SOURCE_MANIFEST.json`, and numbered Markdown/JSONL parts.
Single-file selection captures that file; multiple selections capture only selected
files and descendants of selected folders. Select the repository folder to capture
all of its eligible source.

No total-character budget, file-size cap, Git subset, or prompt-starter footer is
applied. Old saved export-routing settings cannot redirect the Explorer action.
Ignore rules, extension/binary exclusions and redaction still apply and are reported
in the manifest. Complete eligible text does not mean all filesystem objects.

Verify an export with:

```powershell
.\release\win-x64\AIFolderFlatten.exe --verify-snapshot 'C:\exports\repo_SOURCE_SNAPSHOT_...'
```

Other context-pack and task-context modes remain available through the CLI.
The Explorer menu exposes only complete snapshots.
