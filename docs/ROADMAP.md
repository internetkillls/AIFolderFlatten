# Project Roadmap

This document outlines the planned feature set, structural improvements, and future iterations for **AIFolderFlatten**.

---

## 🚀 Version 0.4.0: Codebase Intelligence & Interactive CLI

Focuses on deeper code understanding, multi-language support, and improving the interactive console experience.

### Deeper Code Analysis
*   **AST Parsers**: Support parsing and extraction of symbols, functions, and imports for Python (`.py`), TypeScript (`.ts`/`.tsx`), Go (`.go`), and Rust (`.rs`).
*   **Dependency Visualizer**: Generate Mermaid-style dependency graphs directly inside the context output.
*   **Token Estimator**: Add real-time token estimation (cl100k_base / tiktoken) to warn users if their output fits within specific LLM context windows (e.g. Claude 3.5 Sonnet, GPT-4o).

### Interactive Console Enhancements
*   **Interactive Revision Chooser**: Replace the text-based interactive git picker with a rich terminal UI (TUI) to easily browse recent commits, tags, and branches.
*   **Selective File Toggles**: Allow users to check/uncheck specific files from the scan via arrow keys and spacebar before output is finalized.

---

## 🚀 Version 0.5.0: Smart Chunking & Integration

Focuses on output structure optimizations and integrations with editors and build systems.

### Content Chunking
*   **Semantic Chunking**: Split output text not just by character limits, but dynamically along class, function, or module boundaries.
*   **Deterministic Chunk Metadata**: Emit JSON Lines with stable chunk IDs and
    source anchors. Vector generation remains outside the expert-system core.

### IDE Integrations
*   **VS Code Extension**: Trigger directory flattening from the VS Code Explorer sidebar.
*   **Cursor IDE / Windsurf Integration**: Built-in prompt recipes to automatically inject compiled context.

---

## 🚀 Version 1.0.0: Enterprise & Cross-Platform UI

Focuses on enterprise-grade security and cross-platform native interfaces.

### Native GUI / System Tray
*   **Cross-platform Tray App**: A lightweight system tray utility built with .NET MAUI / Photino, offering visual configuration toggles.
*   **Drag-and-Drop Compiler**: A small, floating desktop widget where users can drop folders to get instant output.

### Enterprise Compliance
*   **Custom Redactor Rules**: Allow organizations to define custom regular expressions or dictionary-based redact rules via local/global config files.
*   **Metadata Cleaners**: Strip metadata from binary assets and documents (e.g. PDF author names, EXIF data from images) before packaging.
