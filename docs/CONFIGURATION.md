# Configuration

AIFolderFlatten v0.2.3 supports CLI arguments, local `.ai-flatten.json`
files, and predefined profiles.

## Precedence Order

When resolving configuration options, the tool applies the following priority order (highest to lowest):

1. **CLI Arguments** (e.g. `--mode debug`)
2. **Local Profile File** (`.ai-flatten.json` in the target folder or current working directory)
3. **Predefined Profile Defaults** (e.g. `--profile debug`)
4. **Built-in Defaults** (e.g. `--format md`)

---

## `.ai-flatten.json` Profile Configuration

You can place an `.ai-flatten.json` configuration file at the root of your folder to store default settings. The format is a standard JSON object containing key-value pairs matching CLI options (using kebab-case keys):

```json
{
  "profile": "debug",
  "format": "pack",
  "max-total-chars": 500000,
  "split-size-chars": 150000,
  "line-numbers": true,
  "respect-gitignore": true,
  "privacy-strict": true
}
```

---

## CLI Options Reference

### Core Options
*   `--input <path>`: Path to the directory you want to flatten.
*   `--git-ref <ref>`: Compile from a specific Git branch, tag, or commit instead of the filesystem. Use `choose` for interactive choice.
*   `--git-staged true|false`: Limit file scan only to staged Git changes.
*   `--git-changed true|false`: Limit file scan only to unstaged/modified Git changes.
*   `--git-summary true|false`: Include Git repository details in a dedicated `GIT.md` file.
*   `--version`: Print the product version and exit.
*   `--profile <name>`: `post`, `overview`, `debug`, `architecture`, `obsidian`, `research`, or `prompt-pack`.
*   `--mode <mode>`: `post`, `overview`, `debug`, `refactor`, `architecture`, `code-review`, `obsidian`, `research`, `prompt-pack`, or `full`.
*   `--format md|txt|pack`: Render format. `pack` splits the output into specialized Markdown files and a `MANIFEST.json`.
*   `--output <path>`: Specific file path or directory for output. Defaults to `auto` (parent directory of the target folder).
*   `--domain <name>`: `auto`, `code`, `obsidian`, `research`, `documents`, `modeling`, `creative`, `mixed`, or `unknown`.
*   `--lens <name>`: `auto`, `overview`, `debug`, `architecture`, or `obsidian`.
*   `--preset <name>`: `research-intelligence`, `creative-reference`, `math-modeling`, or `document-ledger`.
*   `--orientation <text>`: Add a deterministic orientation hint.
*   `--detail basic|standard|full`: Select report detail.
*   `--split true|false`: Split Markdown output into multiple parts if it exceeds the split threshold.
*   `--split-size-chars <number>`: Soft threshold character limit for split part files (default: 300,000).

### Scanner Options
*   `--max-file-bytes <number>`: Skip files larger than this byte size (default: 1,048,576).
*   `--max-total-chars <number>`: Limit total included context character budget (default: 2,000,000). Set to 0 for unlimited.
*   `--include <globs>`: Glob patterns of files to explicitly include (e.g. `src/**/*.cs`).
*   `--exclude <globs>`: Glob patterns of files to explicitly exclude (e.g. `**/obj/**`).
*   `--include-ext <extensions>`: Comma-separated list of extra extensions to scan (e.g. `.json,.yaml`).
*   `--exclude-dir <directories>`: Comma-separated list of extra folder names to skip.

### Safety Options
*   `--redact true|false`: Auto-redact secrets like API keys and credentials (default: true).
*   `--redact-paths true|false`: Redact local Windows file system paths (default: false).
*   `--privacy-strict true|false`: Fail and exit with code `7` if any raw secrets are left unredacted.
*   `--dry-run true|false`: Show scan summary without writing any physical files.
*   `--verify-manifest <path>`: Verify integrity of a package `MANIFEST.json`.
*   `--expert-system-only true`: Required. `false` is rejected.
*   `--allow-llm false`: Required. `true` is rejected.
*   `--allow-network false`: Required. `true` is rejected.
*   `--allow-embeddings false`: Required. `true` is rejected.

Unknown keys in `.ai-flatten.json` are ignored. Use only documented options.
