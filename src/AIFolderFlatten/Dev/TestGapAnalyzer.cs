using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Dev;

/// <summary>
/// Deterministic test gap analyzer.
/// No AI, network, or LLM runtime dependency.
/// </summary>
internal static class TestGapAnalyzer
{
    public sealed record TestGapResult(
        string TestMapMarkdown,
        string TestGapMarkdown,
        string HotspotsMarkdown,
        string FixtureCoverageMarkdown
    );

    public static TestGapResult Analyze(string inputDir, IReadOnlyList<string> includedFiles)
    {
        // 1. Gather all files in the input directory recursively (respecting basic filters)
        var allFiles = new List<string>();
        try
        {
            if (Directory.Exists(inputDir))
            {
                allFiles = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories)
                    .Select(p => Path.GetRelativePath(inputDir, p).Replace('\\', '/'))
                    .Where(p => !p.Contains("/bin/") && !p.Contains("/obj/") && !p.Contains("/.git/") && !p.Contains("/.gemini/"))
                    .ToList();
            }
        }
        catch { }

        // Classify source files and test files
        var sourceFiles = new List<string>();
        var testFiles = new List<string>();

        var sourceExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".py", ".js", ".ts" };

        foreach (var file in allFiles)
        {
            var ext = Path.GetExtension(file);
            if (!sourceExtensions.Contains(ext)) continue;

            var name = Path.GetFileNameWithoutExtension(file);
            if (name.Contains("Test", StringComparison.OrdinalIgnoreCase) || 
                name.Contains("Spec", StringComparison.OrdinalIgnoreCase) || 
                file.Contains("/tests/", StringComparison.OrdinalIgnoreCase) ||
                file.Contains("/specs/", StringComparison.OrdinalIgnoreCase))
            {
                testFiles.Add(file);
            }
            else
            {
                sourceFiles.Add(file);
            }
        }

        // Map sources to tests
        var testMap = new Dictionary<string, List<string>>();
        foreach (var src in sourceFiles)
        {
            var srcName = Path.GetFileNameWithoutExtension(src);
            var matches = testFiles.Where(t => Path.GetFileNameWithoutExtension(t).Contains(srcName, StringComparison.OrdinalIgnoreCase)).ToList();
            testMap[src] = matches;
        }

        // Line counts and complexity hotspots (> 300 lines)
        var hotspots = new List<(string File, int Lines)>();
        foreach (var src in sourceFiles)
        {
            try
            {
                var fullPath = Path.Combine(inputDir, src);
                if (File.Exists(fullPath))
                {
                    var lines = File.ReadLines(fullPath).Count();
                    if (lines > 300)
                    {
                        hotspots.Add((src, lines));
                    }
                }
            }
            catch { }
        }
        hotspots = hotspots.OrderByDescending(h => h.Lines).ToList();

        // Specific gaps: Renderers, Redactors, Parsers
        var rendererGaps = new List<string>();
        var redactorGaps = new List<string>();

        foreach (var src in sourceFiles)
        {
            var name = Path.GetFileNameWithoutExtension(src);
            if (name.EndsWith("Renderer", StringComparison.OrdinalIgnoreCase))
            {
                var tests = testMap[src];
                // Check if any test file contains "golden" or "render" in its content
                var hasGolden = false;
                foreach (var t in tests)
                {
                    try
                    {
                        var tPath = Path.Combine(inputDir, t);
                        if (File.Exists(tPath))
                        {
                            var content = File.ReadAllText(tPath);
                            if (content.Contains("golden", StringComparison.OrdinalIgnoreCase) || 
                                content.Contains("Render(") ||
                                content.Contains(".Render"))
                            {
                                hasGolden = true;
                                break;
                            }
                        }
                    }
                    catch { }
                }
                if (!hasGolden) rendererGaps.Add(src);
            }

            if (name.Contains("Redact", StringComparison.OrdinalIgnoreCase))
            {
                var tests = testMap[src];
                var hasNegative = false;
                foreach (var t in tests)
                {
                    try
                    {
                        var tPath = Path.Combine(inputDir, t);
                        if (File.Exists(tPath))
                        {
                            var content = File.ReadAllText(tPath);
                            if (content.Contains("negative", StringComparison.OrdinalIgnoreCase) || 
                                content.Contains("empty", StringComparison.OrdinalIgnoreCase) || 
                                content.Contains("null", StringComparison.OrdinalIgnoreCase) ||
                                content.Contains("Redact(") ||
                                content.Contains(".Redact"))
                            {
                                hasNegative = true;
                                break;
                            }
                        }
                    }
                    catch { }
                }
                if (!hasNegative) redactorGaps.Add(src);
            }
        }

        // Fixture coverage
        var parsers = sourceFiles.Where(s => Path.GetFileNameWithoutExtension(s).Contains("Parser", StringComparison.OrdinalIgnoreCase)).ToList();
        var fixtureCoverage = new List<(string Parser, bool HasFixture, string ExpectedExtension)>();
        foreach (var parser in parsers)
        {
            var pName = Path.GetFileNameWithoutExtension(parser);
            // Guess target extension from parser name, e.g., HtmlDocumentParser -> .html
            var targetExt = GuessExtensionFromParserName(pName);
            var hasFixture = false;
            if (targetExt != null)
            {
                hasFixture = allFiles.Any(f => f.Contains("/fixtures/") && Path.GetExtension(f).Equals(targetExt, StringComparison.OrdinalIgnoreCase));
                fixtureCoverage.Add((parser, hasFixture, targetExt));
            }
        }

        return new TestGapResult(
            TestMapMarkdown: RenderTestMap(testMap),
            TestGapMarkdown: RenderTestGap(testMap, rendererGaps, redactorGaps),
            HotspotsMarkdown: RenderHotspots(hotspots),
            FixtureCoverageMarkdown: RenderFixtureCoverage(fixtureCoverage)
        );
    }

    private static string? GuessExtensionFromParserName(string parserName)
    {
        if (parserName.Contains("Html", StringComparison.OrdinalIgnoreCase)) return ".html";
        if (parserName.Contains("Latex", StringComparison.OrdinalIgnoreCase) || parserName.Contains("Equation", StringComparison.OrdinalIgnoreCase)) return ".tex";
        if (parserName.Contains("Pdf", StringComparison.OrdinalIgnoreCase)) return ".pdf";
        if (parserName.Contains("Epub", StringComparison.OrdinalIgnoreCase)) return ".epub";
        if (parserName.Contains("CSharp", StringComparison.OrdinalIgnoreCase)) return ".cs";
        if (parserName.Contains("Python", StringComparison.OrdinalIgnoreCase)) return ".py";
        if (parserName.Contains("Js", StringComparison.OrdinalIgnoreCase)) return ".js";
        if (parserName.Contains("TypeScript", StringComparison.OrdinalIgnoreCase)) return ".ts";
        if (parserName.Contains("Markdown", StringComparison.OrdinalIgnoreCase)) return ".md";
        return null;
    }

    private static string RenderTestMap(Dictionary<string, List<string>> testMap)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Test Map");
        sb.AppendLine();
        sb.AppendLine("> Mapping of source files to candidate test files detected by name correlation.");
        sb.AppendLine();
        sb.AppendLine("| Source File | Candidate Test Files | Count |");
        sb.AppendLine("| :--- | :--- | :---: |");
        foreach (var kv in testMap.OrderBy(k => k.Key))
        {
            var tests = kv.Value.Count > 0 ? string.Join("<br/>", kv.Value.Select(t => $"`{t}`")) : "_None_";
            sb.AppendLine($"| `{kv.Key}` | {tests} | {kv.Value.Count} |");
        }
        return sb.ToString();
    }

    private static string RenderTestGap(Dictionary<string, List<string>> testMap, List<string> rendererGaps, List<string> redactorGaps)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Test Gap Analysis");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: Gaps listed below are candidates identified by name heuristics.");
        sb.AppendLine();

        sb.AppendLine("## Untested Source Files");
        sb.AppendLine();
        var untested = testMap.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key).ToList();
        if (untested.Count == 0)
        {
            sb.AppendLine("All source files have at least one candidate test file correlation.");
        }
        else
        {
            foreach (var f in untested)
            {
                sb.AppendLine($"*   `{f}`");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Renderer Golden Test Gaps");
        sb.AppendLine();
        sb.AppendLine("Renderers should have a test confirming golden outputs (using `golden` or `Render` patterns).");
        sb.AppendLine();
        if (rendererGaps.Count == 0)
        {
            sb.AppendLine("No golden test gaps detected for renderer classes.");
        }
        else
        {
            foreach (var f in rendererGaps)
            {
                sb.AppendLine($"*   `{f}` (Renderer missing golden assertions in tests)");
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Redactor Negative Test Gaps");
        sb.AppendLine();
        sb.AppendLine("Redactors should have tests validating negative or edge cases (empty, null, or redactions).");
        sb.AppendLine();
        if (redactorGaps.Count == 0)
        {
            sb.AppendLine("No negative test gaps detected for redactor classes.");
        }
        else
        {
            foreach (var f in redactorGaps)
            {
                sb.AppendLine($"*   `{f}` (Redactor missing negative scenario tests)");
            }
        }

        return sb.ToString();
    }

    private static string RenderHotspots(List<(string File, int Lines)> hotspots)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Complexity Hotspots");
        sb.AppendLine();
        sb.AppendLine("> Source files containing more than 300 lines are flagged as high complexity and prioritized for deep testing.");
        sb.AppendLine();
        if (hotspots.Count == 0)
        {
            sb.AppendLine("No files exceeding the 300 lines complexity threshold were detected.");
        }
        else
        {
            sb.AppendLine("| File | Line Count | Priority |");
            sb.AppendLine("| :--- | :---: | :--- |");
            foreach (var h in hotspots)
            {
                var priority = h.Lines > 500 ? "🔴 High" : "🟡 Medium";
                sb.AppendLine($"| `{h.File}` | {h.Lines} | {priority} |");
            }
        }
        return sb.ToString();
    }

    private static string RenderFixtureCoverage(List<(string Parser, bool HasFixture, string ExpectedExtension)> coverage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Fixture Coverage");
        sb.AppendLine();
        sb.AppendLine("> Verifies if parsers have test fixtures for their matching file extensions in the project.");
        sb.AppendLine();
        if (coverage.Count == 0)
        {
            sb.AppendLine("No parser classes were detected for fixture analysis.");
        }
        else
        {
            sb.AppendLine("| Parser Class | Expected Extension | Has Fixture? | Status |");
            sb.AppendLine("| :--- | :---: | :---: | :--- |");
            foreach (var c in coverage)
            {
                var status = c.HasFixture ? "✅ Covered" : "❌ Missing Fixture File";
                sb.AppendLine($"| `{c.Parser}` | `{c.ExpectedExtension}` | {(c.HasFixture ? "Yes" : "No")} | {status} |");
            }
        }
        return sb.ToString();
    }
}
