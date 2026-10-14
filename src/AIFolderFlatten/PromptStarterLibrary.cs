using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AIFolderFlatten;

internal static class PromptStarterLibrary
{
    // Fallback templates — always work even without the config/prompt-starters/ directory
    private static readonly Dictionary<string, string> Builtins = new(StringComparer.OrdinalIgnoreCase)
    {
        ["debug"] = """
            ---

            ## TASK: Debug This Issue

            **Problem:** [DESCRIBE THE BUG OR ERROR HERE]
            **Expected behavior:** [WHAT SHOULD HAPPEN]
            **Actual behavior:** [WHAT ACTUALLY HAPPENS]
            **Error message (if any):** [PASTE ERROR / STACK TRACE HERE]

            Please:
            1. Identify the root cause from the files above
            2. Point to the exact file and line number
            3. Propose a minimal fix — do not refactor unrelated code
            """,

        ["mockup-review"] = """
            ---

            ## TASK: Review This Mockup / Design

            The files above represent a work-in-progress mockup or design.

            Please:
            1. Identify the single biggest structural or architectural issue
            2. Note anything missing, inconsistent, or that will cause problems later
            3. Suggest what to fix first — be specific, not general
            """,

        ["vibe-continue"] = """
            ---

            ## TASK: Continue Building This

            The files above show the current state of the project.

            Please:
            1. Summarize where this is in 2-3 sentences
            2. Identify what is most obviously incomplete or broken
            3. Write the next logical piece of code — pick up exactly where I left off
            """,

        ["explain"] = """
            ---

            ## TASK: Explain This Codebase

            Please explain this project to me as if I am new to it:
            1. What does it do? (1-2 sentences, no jargon)
            2. Walk me through the entry point and the main execution flow
            3. Highlight anything non-obvious, clever, or that I should not touch without understanding first
            """,

        ["notebooklm"] = """
            ---

            ## About This Document

            This document was automatically compiled from a project folder using AIFolderFlatten.
            It contains the project structure, file summaries, and full source text for use as a NotebookLM source.

            Upload this file as a source in NotebookLM, then ask questions about the project.
            NotebookLM will cite specific sections when answering, reducing hallucination.
            """
    };

    public static IReadOnlyCollection<string> BuiltinNames => Builtins.Keys;

    /// <summary>
    /// Resolves a prompt starter by name. Lookup order:
    /// 1. &lt;inputDir&gt;/.ai-starters/&lt;name&gt;.txt  (project-local override)
    /// 2. &lt;exeDir&gt;/config/prompt-starters/&lt;name&gt;.txt  (bundled alongside exe)
    /// 3. Built-in string constants (always available)
    /// 4. Treats value as inline text if it contains spaces (direct --prompt-starter "your text here")
    /// Returns null if not found.
    /// </summary>
    public static string? Resolve(string? starter, string inputDir)
    {
        if (string.IsNullOrWhiteSpace(starter)) return null;

        var name = starter.Trim();

        // 1. Project-local override
        var localPath = Path.Combine(inputDir, ".ai-starters", name + ".txt");
        if (File.Exists(localPath))
            return File.ReadAllText(localPath, Utf8NoBom());

        // 2. Bundled alongside exe (for release builds)
        var exePath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(exePath))
        {
            var exeDir = Path.GetDirectoryName(exePath);
            if (!string.IsNullOrWhiteSpace(exeDir))
            {
                var bundledPath = Path.Combine(exeDir, "config", "prompt-starters", name + ".txt");
                if (File.Exists(bundledPath))
                    return File.ReadAllText(bundledPath, Utf8NoBom());
            }
        }

        // 3. Built-in fallback
        if (Builtins.TryGetValue(name, out var builtin))
            return builtin;

        // 4. Inline text (if value contains spaces it's not a file name)
        if (name.Contains(' ') || name.Contains('\n'))
            return name;

        return null;
    }

    private static Encoding Utf8NoBom() => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}
