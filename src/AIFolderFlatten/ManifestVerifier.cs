using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIFolderFlatten;

internal static class ManifestVerifier
{
    public static ManifestVerificationResult Verify(string manifestPath, string appVersion)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            errors.Add("Manifest path is empty.");
            return new ManifestVerificationResult(false, errors, warnings);
        }

        if (!File.Exists(manifestPath))
        {
            errors.Add($"Manifest file does not exist: {manifestPath}");
            return new ManifestVerificationResult(false, errors, warnings);
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check kind
            if (!root.TryGetProperty("kind", out var kindProp) || kindProp.GetString() != "ai-folder-context-pack")
            {
                errors.Add("Manifest 'kind' is invalid or missing. Expected 'ai-folder-context-pack'.");
            }

            // Check version
            if (!root.TryGetProperty("version", out var versionProp))
            {
                errors.Add("Manifest 'version' is missing.");
            }
            else
            {
                var version = versionProp.GetString();
                if (version != appVersion)
                {
                    errors.Add($"Manifest version '{version}' does not match application version '{appVersion}'.");
                }
            }

            // ── Expert-System-Only field validation (v0.2.2+) ──────────────────
            // Every pack must statically attest that no AI/LLM/network was used.
            if (!root.TryGetProperty("expertSystemOnly", out var esProp) || esProp.ValueKind != JsonValueKind.True)
            {
                errors.Add("Manifest 'expertSystemOnly' is missing or not true. All packs must declare expertSystemOnly=true.");
            }

            if (!root.TryGetProperty("aiCore", out var aiCoreProp) || aiCoreProp.ValueKind != JsonValueKind.False)
            {
                errors.Add("Manifest 'aiCore' is missing or not false. Core pipeline must have aiCore=false.");
            }

            if (!root.TryGetProperty("llmUsed", out var llmProp) || llmProp.ValueKind != JsonValueKind.False)
            {
                errors.Add("Manifest 'llmUsed' is missing or not false. No LLM may be used in the core pipeline.");
            }

            if (!root.TryGetProperty("networkUsed", out var netProp) || netProp.ValueKind != JsonValueKind.False)
            {
                errors.Add("Manifest 'networkUsed' is missing or not false. Network access is forbidden in the core pipeline.");
            }

            if (!root.TryGetProperty("embeddingsUsed", out var embeddingsProp) || embeddingsProp.ValueKind != JsonValueKind.False)
            {
                errors.Add("Manifest 'embeddingsUsed' is missing or not false. Embeddings are forbidden in the core pipeline.");
            }
            // ───────────────────────────────────────────────────────────────────


            // Verify pack files exist in the same directory
            var dir = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? "";
            var expectedPackFiles = new List<string>
            {
                "CONTEXT.md",
                "FILES.md",
                "SYMBOLS.md",
                "DEPENDENCIES.md",
                "RISKS.md",
                "SKIPPED.md",
                "REDACTIONS.md",
                "USER_PROMPT.md",
                "EXPERT_SYSTEM_PROVENANCE.md"
            };

            if (root.TryGetProperty("git", out var gitProp) && gitProp.ValueKind != JsonValueKind.Null)
            {
                expectedPackFiles.Add("GIT.md");
            }

            foreach (var file in expectedPackFiles)
            {
                var filePath = Path.Combine(dir, file);
                if (!File.Exists(filePath))
                {
                    errors.Add($"Required pack file is missing: {file}");
                }
            }

            // Parse included_files
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (root.TryGetProperty("included_files", out var filesProp) && filesProp.ValueKind == JsonValueKind.Array)
            {
                var filesMdContent = "";
                var filesMdPath = Path.Combine(dir, "FILES.md");
                if (File.Exists(filesMdPath))
                {
                    filesMdContent = File.ReadAllText(filesMdPath);
                }

                foreach (var fileObj in filesProp.EnumerateArray())
                {
                    if (!fileObj.TryGetProperty("id", out var idVal))
                    {
                        errors.Add("An included file is missing its 'id'.");
                        continue;
                    }
                    var id = idVal.GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        errors.Add("An included file has an empty 'id'.");
                        continue;
                    }

                    if (!ids.Add(id))
                    {
                        errors.Add($"Duplicate file ID found in manifest: {id}");
                    }

                    if (fileObj.TryGetProperty("path", out var pathVal))
                    {
                        var path = pathVal.GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(path) && !paths.Add(path))
                        {
                            errors.Add($"Duplicate file path found in manifest: {path}");
                        }
                    }
                    else
                    {
                        errors.Add($"Included file {id} is missing its 'path'.");
                    }

                    // Verify FILES.md has the corresponding block
                    if (!string.IsNullOrEmpty(filesMdContent))
                    {
                        var pattern = $"id=\"{id}\"";
                        if (!filesMdContent.Contains(pattern))
                        {
                            errors.Add($"File ID '{id}' listed in manifest but not found in FILES.md.");
                        }
                    }
                }
            }
            else
            {
                errors.Add("Manifest 'included_files' is missing or not an array.");
            }

            if (root.TryGetProperty("files", out var manifestFiles) && manifestFiles.ValueKind == JsonValueKind.Array)
            {
                var packRoot = Path.GetFullPath(dir);
                foreach (var fileObj in manifestFiles.EnumerateArray())
                {
                    if (!fileObj.TryGetProperty("path", out var pathProp) ||
                        string.IsNullOrWhiteSpace(pathProp.GetString()))
                    {
                        errors.Add("A manifest file entry is missing its 'path'.");
                        continue;
                    }

                    var relativePath = pathProp.GetString()!;
                    var fullPath = Path.GetFullPath(Path.Combine(packRoot, relativePath));
                    if (!IsWithinDirectory(packRoot, fullPath))
                    {
                        errors.Add($"Manifest file path escapes the pack directory: {relativePath}");
                        continue;
                    }

                    if (!File.Exists(fullPath))
                    {
                        errors.Add($"Manifest-listed file is missing: {relativePath}");
                        continue;
                    }

                    if (!fileObj.TryGetProperty("sizeBytes", out var sizeProp) ||
                        !sizeProp.TryGetInt64(out var expectedSize))
                    {
                        errors.Add($"Manifest file '{relativePath}' is missing a valid sizeBytes value.");
                    }
                    else
                    {
                        var actualSize = new FileInfo(fullPath).Length;
                        if (actualSize != expectedSize)
                        {
                            errors.Add($"Size mismatch for '{relativePath}': expected {expectedSize}, actual {actualSize}.");
                        }
                    }

                    if (!fileObj.TryGetProperty("sha256", out var hashProp) ||
                        string.IsNullOrWhiteSpace(hashProp.GetString()))
                    {
                        errors.Add($"Manifest file '{relativePath}' is missing a sha256 value.");
                    }
                    else
                    {
                        var expectedHash = hashProp.GetString()!;
                        var actualHash = CalculateSha256(fullPath);
                        if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add($"Hash mismatch for '{relativePath}'.");
                        }
                    }
                }
            }
            else
            {
                errors.Add("Manifest 'files' is missing or not an array.");
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Manifest is not a valid JSON file: {ex.Message}");
        }
        catch (Exception ex)
        {
            errors.Add($"Error reading manifest: {ex.Message}");
        }

        return new ManifestVerificationResult(errors.Count == 0, errors, warnings);
    }

    private static bool IsWithinDirectory(string directory, string candidate)
    {
        var root = Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar;
        return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static string CalculateSha256(string path)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(path);
        var hash = sha256.ComputeHash(stream);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var value in hash)
        {
            sb.Append(value.ToString("x2"));
        }
        return sb.ToString();
    }
}
