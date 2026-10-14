using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AIFolderFlatten.Artifacts;

internal static class ArtifactScanner
{
    public static List<ArtifactIR> Scan(string inputDir, AppConfig config, List<SkippedFile> skipped, out List<string> textContentsForScoring)
    {
        var list = new List<ArtifactIR>();
        textContentsForScoring = new List<string>();

        // Enumerate all files under inputDir, respecting exclusions but including binaries
        var files = EnumerateAllFiles(inputDir, config, skipped);
        int index = 1;

        foreach (var file in files)
        {
            try
            {
                var relativePath = Path.GetRelativePath(inputDir, file).Replace('\\', '/');
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var fileInfo = new FileInfo(file);

                var artifactType = ArtifactClassifier.Classify(ext);
                var reg = ParserCapabilityRegistry.GetEntry(ext);

                string status = "ledger_only";
                if (reg.ParserTier == "T2_text_extract" || reg.ParserTier == "T3_structural_extract" || reg.ParserTier == "T4_domain_ir")
                {
                    status = "parsed";
                }
                else if (reg.ParserTier == "T1_metadata")
                {
                    status = "metadata_only";
                }
                if (reg.ExternalToolHint != null)
                {
                    status = "external_tool_needed";
                }

                // If it is a text file, let's read some lines for scoring evidence
                if (status == "parsed" && fileInfo.Length < 1_048_576 && !FileText.LooksBinary(file))
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        textContentsForScoring.Add(content);
                    }
                    catch { }
                }

                var id = $"A{index++:0000}";
                var hash = CalculateSHA256(file);

                var artifact = new ArtifactIR(
                    Id: id,
                    Path: relativePath,
                    Extension: ext,
                    ArtifactType: artifactType,
                    ParserTier: reg.ParserTier,
                    Status: status,
                    SizeBytes: fileInfo.Length,
                    Sha256: hash,
                    ModifiedUtc: fileInfo.LastWriteTimeUtc,
                    Capabilities: reg.Capabilities,
                    Warnings: new List<string>()
                );

                list.Add(artifact);
            }
            catch { }
        }

        return list;
    }

    private static List<string> EnumerateAllFiles(string inputDir, AppConfig config, List<SkippedFile> skipped)
    {
        var result = new List<string>();
        if (!Directory.Exists(inputDir)) return result;

        var ignoreRules = IgnoreRules.Load(inputDir, config);

        var queue = new Queue<string>();
        queue.Enqueue(inputDir);

        while (queue.Count > 0)
        {
            var dir = queue.Dequeue();

            // Skip excluded directories
            var dirName = Path.GetFileName(dir);
            if (config.ExcludeDirs.Contains(dirName))
            {
                continue;
            }

            try
            {
                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    var name = Path.GetFileName(subDir);
                    // respect hidden folders policy
                    if (name.StartsWith('.') && !config.IncludeHidden && name != ".obsidian" && name != ".git")
                    {
                        continue;
                    }
                    queue.Enqueue(subDir);
                }

                foreach (var file in Directory.GetFiles(dir))
                {
                    var name = Path.GetFileName(file);
                    if (name.StartsWith('.') && !config.IncludeHidden && name != ".aiignore" && name != ".gitignore")
                    {
                        continue;
                    }

                    var relPath = Path.GetRelativePath(inputDir, file).Replace('\\', '/');

                    // Check glob matches if include/exclude patterns exist
                    if (config.IncludeGlobs.Count > 0)
                    {
                        bool matchesInclude = false;
                        foreach (var glob in config.IncludeGlobs)
                        {
                            if (Glob.IsMatch(relPath, glob)) { matchesInclude = true; break; }
                        }
                        if (!matchesInclude)
                        {
                            skipped.Add(new SkippedFile(relPath, "excluded by include globs"));
                            continue;
                        }
                    }

                    bool matchesExclude = false;
                    foreach (var glob in config.ExcludeGlobs)
                    {
                        if (Glob.IsMatch(relPath, glob)) { matchesExclude = true; break; }
                    }
                    if (matchesExclude)
                    {
                        skipped.Add(new SkippedFile(relPath, "excluded by exclude globs"));
                        continue;
                    }

                    // Respect ignore rules loaded
                    if (ignoreRules.IsIgnored(relPath))
                    {
                        skipped.Add(new SkippedFile(relPath, "ignored by project ignore patterns"));
                        continue;
                    }

                    result.Add(file);
                }
            }
            catch { }
        }

        return result;
    }

    private static string CalculateSHA256(string filepath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filepath);
            var hash = sha256.ComputeHash(stream);
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        catch
        {
            return "hash_unavailable";
        }
    }
}
