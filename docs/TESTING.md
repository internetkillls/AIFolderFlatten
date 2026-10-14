# Testing Documentation

AIFolderFlatten includes a comprehensive test suite built with **xUnit** to verify the functionality of all key pipeline steps, from file scanning to ignore rule resolution, symbol analysis, secret redaction, manifest verification, and output rendering.

---

## 1. Test Project Architecture

The test suite resides in the `tests/AIFolderFlatten.Tests` directory:

*   **Target Framework**: `net8.0-windows`
*   **Dependency settings**: Uses `<UseWindowsForms>true</UseWindowsForms>` to maintain dependency compatibility with the parent project, enabling full cross-compilation and testing.
*   **Accessing Internal Members**: The main project includes `[assembly: InternalsVisibleTo("AIFolderFlatten.Tests")]` inside its properties, allowing unit tests to directly target internal utility classes (`Redactor`, `IgnoreRules`, `PrivacyScanner`, etc.) without exposing them publicly.

---

## 2. Test Suites Reference

The tests are organized into the following logical files:

### `ScannerTests.cs`
*   **`Test_Scanner_Excludes_BinaryAndReleaseFiles`**: Verifies that `.exe`, `.dll`, `.pdb`, `.zip`, `.deps.json`, and `.runtimeconfig.json` files are automatically filtered out during scanning and marked with correct skipped reasons.
*   **`Test_Scanner_Excludes_ExcludeDirectoriesByDefault`**: Verifies that standard output/dependency directories like `bin`, `obj`, `.git`, `release`, and `publish` are skipped.
*   **`Test_Scanner_RespectsMaxFileBytes`**: Ensures files exceeding the maximum file byte size are skipped and reported.

### `IgnoreRulesTests.cs`
*   **`Test_Glob_Simple_Extension`**: Verifies pattern matching like `*.tmp`.
*   **`Test_Glob_Recursive_Match`**: Verifies pattern matching for recursive directory globs like `**/*.tmp` under subfolders.
*   **`Test_IgnoreRules_ReadIgnoreFile_RespectsPrecedence`**: Validates the precedence of `.aiignore` and `.gitignore` file overrides.

### `RedactorTests.cs`
*   **`Test_Redactor_Masks_APIKeys`**: Assures that OpenAI keys (`sk-...`), GitHub tokens (`ghp_...`), JWTs, and AWS access keys are redacted.
*   **`Test_Redactor_Masks_ConnectionStrings`**: Assures SQL connection strings and connection URLs with passwords are redacted.
*   **`Test_Redactor_Masks_LocalPaths`**: Assures Windows local paths are correctly redacted when path redaction is enabled.
*   **`Test_PrivacyScanner_Detects_RawSecrets`**: Verifies that raw unredacted credentials throw a `PrivacyLeakException` under `--privacy-strict true`.

### `AnalyzerTests.cs`
*   **`Test_Analyzer_Detects_CSharp_Entrypoint`**: Verifies that a C# file containing `static void Main` is correctly identified as an entry point.
*   **`Test_Analyzer_Detects_DotNet_Project`**: Verifies that a folder containing `.csproj` is correctly flagged as a `.NET` project class.
*   **`Test_Analyzer_Extracts_Dependencies`**: Validates that package references, imports, and dependencies are correctly extracted from files.

### `RendererTests.cs`
*   **`Test_MarkdownRenderer_Generates_Valid_Structure`**: Checks that the Markdown renderer produces a structured header, file metadata blocks, and file content citation wrappers.
*   **`Test_TextRenderer_Generates_Plaintext`**: Validates plain text rendering mode parity (including symbol mappings, dependencies, and warning indicators).
*   **`Test_MarkdownRenderer_SplitSize_CalculatesCorrectly`**: Ensures split-size character budgets correctly trigger pagination at the specified package limit.

### `ManifestTests.cs`
*   **`Test_ManifestVerifier_Validates_Integrity`**: Confirms that a valid manifest verifies successfully (returning exit code 0).
*   **`Test_ManifestVerifier_Fails_On_MissingFile`**: Validates that missing files in the physical pack folder return exit code 3 (Manifest Error).
*   **`Test_ManifestVerifier_Fails_On_HashMismatch`**: Validates that hash mismatches in files return exit code 3.

Additional suites cover POST orientation, artifacts, documents, citations,
research signals, modeling candidates, creative affordances, expert-system
boundary enforcement, DLC validation contracts, Explorer menu structure,
development reports, and Jupyter notebook extraction.

---

## 3. Running the Tests

To run the full suite, open your terminal at the repository root and execute:

```powershell
dotnet test
```

For detailed outputs or targeting just the test project explicitly:

```powershell
dotnet test tests/AIFolderFlatten.Tests/AIFolderFlatten.Tests.csproj --logger "console;verbosity=detailed"
```

Validate that every release DLC contains the required Codex prompts, acceptance
criteria, stop conditions, and expert-system boundary clauses:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/validate-dlc.ps1
```
