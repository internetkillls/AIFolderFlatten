# Security and Privacy Reference

AIFolderFlatten uses a **privacy-first architecture** to reduce the chance that
sensitive information is exported. Pattern-based redaction cannot guarantee
that every possible secret format will be detected, so outputs still require
human review before sharing.

This document describes the Redactor engine, the post-redaction Privacy Auditor, and the strict safety modes.

---

## 1. Automated Secret Redaction (`--redact true`)

By default, the tool scans the content of all files for known pattern signatures of API tokens, private keys, database URLs, and connection strings. These patterns are automatically replaced with redact placeholders before being written to the output files.

### Redaction Rules Reference

| Category | Target Patterns / Formats | Replacement Format |
| :--- | :--- | :--- |
| **Private Keys** | PEM formatted Private Keys (`-----BEGIN * PRIVATE KEY-----`) | `<PRIVATE_KEY_REDACTED>` |
| **Named API Keys** | `OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, `GOOGLE_API_KEY`, `GITHUB_TOKEN`, `GITLAB_TOKEN`, `DISCORD_TOKEN` assignments | `KEY_NAME=<REDACTED>` |
| **Generic Secrets** | Assignments matching `api-key`, `secret`, `token`, `password`, `pwd`, `session`, `cookie` | `KEY=<REDACTED>` |
| **OpenAI Keys** | Patterns matching `sk-[A-Za-z0-9_-]{20,}` | `sk-<REDACTED>` |
| **GitHub Tokens** | Patterns matching `ghp_`, `gho_`, `ghu_`, `ghs_`, `ghr_` followed by characters | `gh_<REDACTED>` |
| **AWS Credentials** | AWS access key ID (`AKIA...`) and secret keys | `AKIA<REDACTED>`, `aws_secret_access_key=<REDACTED>` |
| **JSON Web Tokens (JWT)** | Three-part base64 encoded JWT tokens | `<JWT_REDACTED>` |
| **Connection Strings** | Database Connection Strings (`Server=...;Database=...;User Id=...;Password=...`) | `<CONNECTION_STRING_REDACTED>` |
| **Database URLs** | Protocols like `postgres://`, `mysql://`, `mongodb://`, `redis://` containing login details | `protocol://<REDACTED>` |

---

## 2. Local File Path Redaction (`--redact-paths true`)

When compiling codebases to share with public or external models, you might want to hide internal username/directory paths. By enabling `--redact-paths true`, AIFolderFlatten uses a robust matcher to find absolute Windows paths:

*   **Pattern**: Matches Windows drive letters followed by folder paths (e.g. `C:\Users\username\secrets\config.json`).
*   **Replacement**: Replaced with `<LOCAL_PATH_REDACTED>`.

---

## 3. Strict Privacy Mode (`--privacy-strict true`)

For high-security environments, redaction alone might not be enough. If there's an unknown or custom credential pattern, a raw leak could still happen.

To solve this, AIFolderFlatten includes **Strict Privacy Mode** (`--privacy-strict true`):

1.  **Post-Redaction Audit**: After all regular redactors have finished modifying a file's content, a secondary, highly sensitive **Privacy Auditor** scans the resulting string.
2.  **Fail-Safe Termination**: If the auditor finds any remaining unredacted private keys, raw API tokens (like `sk-` or `ghp_` patterns), AWS keys, or JWT tokens, the entire flattening operation is instantly aborted.
3.  **No Partial Output**: The strict audit runs while building the in-memory
    package, before export files are written.
4.  **Exit Code**: The CLI prints a diagnostic alert and terminates with **Exit Code 7 (Privacy Leak)**.

---

## 4. Security Reports in Rendered Outputs

When you generate a project package, a redaction summary is appended to your export output so you can audit what was modified:

*   **Markdown Format (`.md`)**: The Markdown renderer appends a **Redaction Report** section showing a count of redacted items by category.
*   **Plain Text Format (`.txt`)**: The plain text renderer outputs a structured table detailing every redaction occurrence.

## Expert-System Attestation

Pack output includes `EXPERT_SYSTEM_PROVENANCE.md`. `MANIFEST.json` records
`expertSystemOnly=true` and false values for AI core, LLM, network, and
embeddings use. Manifest verification checks required files, byte sizes, and
SHA-256 hashes.
