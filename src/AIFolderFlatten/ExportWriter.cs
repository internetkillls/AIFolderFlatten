using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AIFolderFlatten.Post;
using AIFolderFlatten.Artifacts;
using AIFolderFlatten.Documents;
using AIFolderFlatten.Research;
using AIFolderFlatten.Modeling;
using AIFolderFlatten.Creative;
using AIFolderFlatten.ExpertSystem;

namespace AIFolderFlatten;

internal static class ExportWriter
{
    public static ExportResult WriteOutput(string inputDir, ContextPackage package, AppConfig config)
    {
        // Fallback for legacy format packs if called
        return WriteOutputPost(inputDir, package, config, null, new List<ArtifactIR>(), "code", "overview");
    }

    public static ExportResult WriteOutputPost(
        string inputDir, 
        ContextPackage package, 
        AppConfig config, 
        OrientationState? orientation, 
        List<ArtifactIR> artifacts,
        string domain,
        string lens)
    {
        try
        {
            var outputPaths = new List<string>();
            var safeName = MakeSafeFileName(package.RootName);
            var stamp = package.GeneratedAt.ToString("yyyyMMdd_HHmmss");

            // Compute orientation state if null (e.g. from legacy tests)
            if (orientation == null)
            {
                var scanState = new ScanState(artifacts.Count, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                orientation = PostOrientationEngine.Orient(inputDir, config, scanState, new List<string>());
            }

            // Perform document parsing for Ledger
            var htmlReports = new List<HtmlDocumentParser.HtmlReport>();
            var pdfReports = new List<PdfMetadataAdapter.PdfReport>();
            var epubReports = new List<EpubMetadataAdapter.EpubReport>();
            var latexReports = new List<LatexLiteParser.LatexReport>();
            var notebookReports = new List<JupyterNotebookParser.NotebookReport>();
            var citationEntries = new List<CitationEntry>();

            foreach (var art in artifacts)
            {
                var fullPath = Path.Combine(inputDir, art.Path);
                if (!File.Exists(fullPath)) continue;

                if (art.Extension == ".html" || art.Extension == ".htm")
                {
                    try { htmlReports.Add(HtmlDocumentParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".pdf")
                {
                    pdfReports.Add(PdfMetadataAdapter.Parse(fullPath));
                }
                else if (art.Extension == ".epub")
                {
                    epubReports.Add(EpubMetadataAdapter.Parse(fullPath));
                }
                else if (art.Extension == ".tex")
                {
                    try { latexReports.Add(LatexLiteParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".ipynb")
                {
                    try { notebookReports.Add(JupyterNotebookParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".bib")
                {
                    try { citationEntries.AddRange(CitationLiteParser.ParseBibTeX(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".ris")
                {
                    try { citationEntries.AddRange(CitationLiteParser.ParseRIS(File.ReadAllText(fullPath))); } catch { }
                }
            }

            // Delegate to MasterFlattenWriter for v0.2.4b Output Layout handling
            return Output.MasterFlattenWriter.Write(inputDir, package, config, orientation, artifacts, domain, lens);
        }
        catch (Exception ex)
        {
            throw new OutputWriteException($"Failed to write output: {ex.Message}");
        }
    }

    public static void WritePackSidecars(
        string outDir,
        string inputDir, 
        ContextPackage package, 
        AppConfig config, 
        OrientationState? orientation, 
        List<ArtifactIR> artifacts,
        string domain,
        string lens,
        string masterFilePath)
    {
        try
        {
            var outputPaths = new List<string>();
            var writtenFiles = new Dictionary<string, string>();

            void WritePackFile(string relativeName, string content)
            {
                var path = Path.Combine(outDir, relativeName);
                EnsureParentDirectory(path);
                File.WriteAllText(path, content, Utf8NoBom());
                outputPaths.Add(path);
                writtenFiles[relativeName] = content;
            }

            // Perform document parsing for Ledger
            var htmlReports = new List<HtmlDocumentParser.HtmlReport>();
            var pdfReports = new List<PdfMetadataAdapter.PdfReport>();
            var epubReports = new List<EpubMetadataAdapter.EpubReport>();
            var latexReports = new List<LatexLiteParser.LatexReport>();
            var notebookReports = new List<JupyterNotebookParser.NotebookReport>();
            var citationEntries = new List<CitationEntry>();

            foreach (var art in artifacts)
            {
                var fullPath = Path.Combine(inputDir, art.Path);
                if (!File.Exists(fullPath)) continue;

                if (art.Extension == ".html" || art.Extension == ".htm")
                {
                    try { htmlReports.Add(HtmlDocumentParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".pdf")
                {
                    pdfReports.Add(PdfMetadataAdapter.Parse(fullPath));
                }
                else if (art.Extension == ".epub")
                {
                    epubReports.Add(EpubMetadataAdapter.Parse(fullPath));
                }
                else if (art.Extension == ".tex")
                {
                    try { latexReports.Add(LatexLiteParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".ipynb")
                {
                    try { notebookReports.Add(JupyterNotebookParser.Parse(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".bib")
                {
                    try { citationEntries.AddRange(CitationLiteParser.ParseBibTeX(File.ReadAllText(fullPath))); } catch { }
                }
                else if (art.Extension == ".ris")
                {
                    try { citationEntries.AddRange(CitationLiteParser.ParseRIS(File.ReadAllText(fullPath))); } catch { }
                }
            }

            // Core Lenses files
            var packDocs = MarkdownRenderer.RenderPack(package);
            foreach (var doc in packDocs.Where(d => d.Key != "MANIFEST.json"))
            {
                WritePackFile(doc.Key, doc.Value);
            }

            if (orientation != null)
            {
                WritePackFile("ORIENTATION.md", PostOrientationEngine.RenderOrientationMarkdown(orientation));
                WritePackFile("MODE_GUIDE.md", ModeGuideRenderer.Render(orientation));
                WritePackFile("POST_ORIENTATION.json", JsonSerializer.Serialize(orientation, new JsonSerializerOptions { WriteIndented = true }));
            }

            var affordanceScan = new ScanState(
                artifacts.Count,
                package.Files.Count,
                package.Skipped.Count,
                artifacts.Count(a => a.ArtifactType == "unknown"),
                artifacts.Count(a => a.ArtifactType == "creative"),
                artifacts.Count(a => a.ArtifactType == "research"),
                artifacts.Count(a => a.ArtifactType == "creative"),
                artifacts.Count(a => a.ArtifactType == "code"),
                artifacts.Count(a => a.Path.Contains(".obsidian", StringComparison.OrdinalIgnoreCase)),
                artifacts.Count(a => a.ArtifactType == "document"),
                artifacts.Count(a => a.ArtifactType == "modeling"));
            WritePackFile("AFFORDANCES.md", AffordanceDetector.RenderAffordancesMarkdown(
                AffordanceDetector.Detect(inputDir, affordanceScan)));

            // Ledgers
            WritePackFile("ARTIFACT_LEDGER.md", ArtifactLedgerRenderer.RenderLedger(artifacts));
            WritePackFile("PARSER_CAPABILITY.md", ArtifactLedgerRenderer.RenderParserCapabilities(artifacts));
            WritePackFile("UNKNOWN_ARTIFACTS.md", ArtifactLedgerRenderer.RenderUnknownArtifacts(artifacts));

            // Documents & Citations
            if (htmlReports.Count > 0 || pdfReports.Count > 0 || epubReports.Count > 0 || latexReports.Count > 0 || notebookReports.Count > 0)
            {
                WritePackFile("DOCUMENTS.md", DocumentLedgerRenderer.RenderDocuments(htmlReports, pdfReports, epubReports, latexReports, notebookReports));
            }
            if (citationEntries.Count > 0)
            {
                WritePackFile("CITATIONS.md", DocumentLedgerRenderer.RenderCitations(citationEntries));
            }

            // Research signal subpack
            if (domain == "research" || domain == "mixed")
            {
                var combinedText = string.Join(" ", package.Files.Select(f => f.Content));
                var ngrams = NGramFrequencyAnalyzer.Analyze(combinedText);
                var methods = new List<MethodSignal>();
                var inferences = new List<InferenceSignal>();
                foreach (var file in package.Files)
                {
                    methods.AddRange(ResearchSignalAnalyzer.ExtractMethods(file.Id, file.Content));
                    inferences.AddRange(ResearchSignalAnalyzer.ExtractInferences(file.Id, file.Content));
                }
                var queries = ResearchSignalAnalyzer.GenerateQueries(ngrams.Unigrams, ngrams.Bigrams);

                WritePackFile("RESEARCH/RESEARCH_SIGNAL_REPORT.md", ResearchRenderer.RenderResearchSignalReport(methods, inferences));
                WritePackFile("RESEARCH/TERM_BANK.md", ResearchRenderer.RenderTermBank(ngrams));
                WritePackFile("RESEARCH/METHOD_PHRASE_MAP.md", ResearchRenderer.RenderMethodMap(methods, inferences));
                WritePackFile("RESEARCH/GAP_QUESTIONS.md", ResearchRenderer.RenderGapQuestions(queries));
                WritePackFile("RESEARCH/PROVENANCE_HINTS.md", ResearchRenderer.RenderProvenanceHints(methods, inferences));
            }

            // Modeling
            if (domain == "modeling" || domain == "mixed")
            {
                var equations = new List<EquationInfo>();
                foreach (var file in package.Files)
                {
                    equations.AddRange(EquationLiteExtractor.Extract(file.Id, file.Content));
                }
                var variables = EquationLiteExtractor.BuildVariableDictionary(equations);
                var combinedText2 = string.Join(" ", package.Files.Select(f => f.Content));
                var modelDomain = ModelingSignalAnalyzer.ClassifyDomain(combinedText2, out var scores);
                var warnings = ModelingSignalAnalyzer.AuditModel(modelDomain, combinedText2);

                WritePackFile("MODELING/MODELING_ORIENTATION.md", ModelingRenderer.RenderModelingOrientation(modelDomain, scores, warnings));
                WritePackFile("MODELING/EQUATION_CANDIDATES.md", ModelingRenderer.RenderEquations(equations));
                WritePackFile("MODELING/VARIABLE_CANDIDATES.md", ModelingRenderer.RenderVariables(variables));
                WritePackFile("MODELING/ASSUMPTION_MARKERS.md", ModelingRenderer.RenderAssumptions(warnings));
                WritePackFile("MODELING/PARAMETER_CANDIDATES.md", ModelingRenderer.RenderParameters(variables));
            }

            // Creative
            if (domain == "creative" || domain == "mixed")
            {
                var mediaList = MediaLedgerBuilder.Build(artifacts);
                WritePackFile("CREATIVE/CREATIVE_ASSET_LEDGER.md", CreativeRenderer.RenderCreativeAssetLedger(mediaList));
                WritePackFile("CREATIVE/MEDIA_LEDGER.md", CreativeRenderer.RenderMediaLedger(mediaList));
                WritePackFile("CREATIVE/SCRIPT_TARGET_CANDIDATES.md", CreativeRenderer.RenderScriptTargetCandidates(mediaList));
            }

            // Dev / Coding subpack
            if (domain == "dev" || domain == "code" || domain == "mixed")
            {
                var testGapResult = AIFolderFlatten.Dev.TestGapAnalyzer.Analyze(inputDir, package.Files.Select(f => f.RelativePath).ToList());
                WritePackFile("TESTS/TEST_MAP.md", testGapResult.TestMapMarkdown);
                WritePackFile("TESTS/TEST_GAP.md", testGapResult.TestGapMarkdown);
                WritePackFile("TESTS/HOTSPOTS.md", testGapResult.HotspotsMarkdown);
                WritePackFile("TESTS/FIXTURE_COVERAGE.md", testGapResult.FixtureCoverageMarkdown);

                var contractDriftResult = AIFolderFlatten.Dev.ContractDriftChecker.Check(inputDir);
                WritePackFile("CONTRACT/CONTRACT_DRIFT.md", contractDriftResult.ContractDriftMarkdown);
                WritePackFile("CONTRACT/VERSION_DRIFT.md", contractDriftResult.VersionDriftMarkdown);
                WritePackFile("CONTRACT/DOCS_VS_CODE.md", contractDriftResult.DocsVsCodeMarkdown);

                var dependencyResult = AIFolderFlatten.Dev.DependencyAnalyzer.Analyze(inputDir);
                WritePackFile("DEV/DEPENDENCY_MAP.md", dependencyResult.DependencyMapMarkdown);
                WritePackFile("DEV/BUILD_COMMANDS.md", dependencyResult.BuildCommandsMarkdown);
                WritePackFile("DEV/DEPENDENCY_RISKS.md", dependencyResult.DependencyRisksMarkdown);
            }

            // Write Git info
            if (package.Git != null && package.Git.IsGitRepo)
            {
                var gitSb = new StringBuilder();
                gitSb.AppendLine("# Git State Report");
                gitSb.AppendLine();
                gitSb.AppendLine($"*   **Is Git Repo**: `true`");
                gitSb.AppendLine($"*   **Branch**: `{package.Git.Branch}`");
                gitSb.AppendLine($"*   **HEAD SHA**: `{package.Git.HeadSha}`");
                gitSb.AppendLine($"*   **Mode**: `{package.Git.Mode}`");
                gitSb.AppendLine($"*   **Target Ref**: `{package.Git.Ref}`");
                gitSb.AppendLine();
                gitSb.AppendLine("## Included Files under Git");
                foreach (var f in package.Git.IncludedPaths)
                {
                    gitSb.AppendLine($"*   `{f}`");
                }
                WritePackFile("GIT/GIT_STATE.md", gitSb.ToString());

                var changedFiles = GitContext.GetChangedFiles(inputDir);
                var stagedFiles = GitContext.GetStagedFiles(inputDir);

                WritePackFile("GIT/DELTA_CONTEXT.md", GitContext.RenderDeltaContext(
                    inputDir, 
                    package.Git.Branch ?? "unknown", 
                    package.Git.HeadSha ?? "unknown", 
                    package.Git.Mode ?? "git_working_folder", 
                    package.Git.Ref, 
                    changedFiles, 
                    stagedFiles));

                WritePackFile("GIT/CHANGED_FILES.md", GitContext.RenderChangedFiles(changedFiles));
                WritePackFile("GIT/STAGED_FILES.md", GitContext.RenderStagedFiles(stagedFiles));
                WritePackFile("GIT/DIFF_SUMMARY.md", GitContext.RenderDiffSummary(inputDir));
                WritePackFile("GIT/PATCH_HANDOFF_PROMPT.md", GitContext.RenderPatchHandoffPrompt(
                    package.Git.Branch ?? "unknown", 
                    package.Git.HeadSha ?? "unknown", 
                    package.Git.Mode ?? "git_working_folder", 
                    changedFiles, 
                    stagedFiles));
            }

            // Expert-System provenance
            var tiersUsed = artifacts.Select(a => a.ParserTier).Distinct().ToList();
            var toolsUsed = new List<string>();
            var toolsMissing = new List<string>();
            var fallbacksUsed = new List<string> { "internal_regex_parser" };
            var limitations = new List<string>
            {
                "Regex-only structural extraction — not full compiler semantics.",
                "PDF/EPUB: metadata only, no text extraction without external tool.",
                "Media files: hash/size/path only — no perceptual or content analysis."
            };

            var provenanceIR = ExpertSystemProvenance.BuildIR(
                config,
                toolsUsed,
                toolsMissing,
                fallbacksUsed,
                tiersUsed,
                limitations);

            WritePackFile("EXPERT_SYSTEM_PROVENANCE.md", ExpertSystemProvenance.Render(provenanceIR));

            var toolchainStatus = ToolchainDoctor.BuildMinimalStatus();
            WritePackFile("TOOLCHAIN_STATUS.md", toolchainStatus);

            // Calculate Hash of Master file
            string? masterSha256 = null;
            long? masterBytes = null;
            if (File.Exists(masterFilePath))
            {
                masterBytes = new FileInfo(masterFilePath).Length;
                masterSha256 = CalculateSHA256String(File.ReadAllText(masterFilePath));
            }

            // Build dynamic MANIFEST.json
            var manifestObj = new Dictionary<string, object>
            {
                { "kind", "ai-folder-context-pack" },
                { "version", Program.AppVersion },
                { "rootName", package.RootName },
                { "appVersion", Program.AppVersion },
                { "generatedAt", package.GeneratedAt.ToString("o") },
                { "mode", package.Mode },
                { "expertSystemOnly", ExpertSystemBoundary.ExpertSystemOnly },
                { "aiCore", ExpertSystemBoundary.AiCore },
                { "llmUsed", ExpertSystemBoundary.LlmUsed },
                { "networkUsed", ExpertSystemBoundary.NetworkUsed },
                { "embeddingsUsed", ExpertSystemBoundary.EmbeddingsUsed },
                { "toolchains", tiersUsed },
                { "parserTiers", tiersUsed },
                { "warnings", limitations },
                { "masterFlatten", new Dictionary<string, object> {
                    { "path", Path.GetFileName(masterFilePath) },
                    { "format", config.MasterFormat },
                    { "sha256", masterSha256 ?? "" },
                    { "bytes", masterBytes ?? 0 },
                    { "isPrimaryArtifact", true }
                }},
                { "outputLayout", config.OutputLayout.ToString().ToLowerInvariant() },
                { "generatedOutputMarker", ".ai-seed-output.json" },
                { "includeGenerated", config.IncludeGenerated },
                { "included_files", package.Files.Select(f => new Dictionary<string, object>
                    {
                        { "id", f.Id },
                        { "path", f.RelativePath }
                    }).ToList()
                },
                { "files", writtenFiles.Select(wf => new Dictionary<string, object>
                    {
                        { "path", wf.Key },
                        { "sizeBytes", Encoding.UTF8.GetByteCount(wf.Value) },
                        { "sha256", CalculateSHA256String(wf.Value) }
                    }).ToList()
                }
            };
            var manifestText = JsonSerializer.Serialize(manifestObj, new JsonSerializerOptions { WriteIndented = true });
            WritePackFile("MANIFEST.json", manifestText);
        }
        catch (Exception ex)
        {
            throw new OutputWriteException($"Failed to write output: {ex.Message}");
        }
    }

    private static string ResolveOutputDirectory(string inputDir, AppConfig config, string safeName, string stamp)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputPath) && !config.OutputPath.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFullPath(config.OutputPath);
        }
        var parent = Path.GetDirectoryName(inputDir);
        var targetDir = string.IsNullOrWhiteSpace(parent) ? inputDir : parent;
        return Path.Combine(targetDir, $"{safeName}_{stamp}_{config.Mode}_pack");
    }

    private static string ResolveOutputFile(string inputDir, AppConfig config, string safeName, string stamp, string ext)
    {
        if (!string.IsNullOrWhiteSpace(config.OutputPath) && !config.OutputPath.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFullPath(config.OutputPath);
        }
        var parent = Path.GetDirectoryName(inputDir);
        var targetDir = string.IsNullOrWhiteSpace(parent) ? inputDir : parent;
        return Path.Combine(targetDir, $"{safeName}_{stamp}_{config.Mode}{ext}");
    }

    private static void EnsureParentDirectory(string outputPath)
    {
        var parent = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrWhiteSpace(parent)) Directory.CreateDirectory(parent);
    }

    private static Encoding Utf8NoBom() => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private static string MakeSafeFileName(string raw)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = raw.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var safe = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "folder" : safe;
    }

    internal static string CalculateSHA256String(string text)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = sha256.ComputeHash(bytes);
        var sb = new StringBuilder();
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static void OpenInExplorer(string outputPath)
    {
        try
        {
            var target = Directory.Exists(outputPath) ? outputPath : Path.GetDirectoryName(outputPath)!;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = Directory.Exists(outputPath) ? $"\"{target}\"" : $"/select,\"{outputPath}\"",
                UseShellExecute = true
            });
        }
        catch { }
    }
}
