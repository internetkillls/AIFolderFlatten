namespace AIFolderFlatten;

internal static class FolderScanner
{
    public static IReadOnlyList<FileCandidate> EnumerateCandidates(string inputDir, AppConfig config, List<SkippedFile> skipped)
    {
        var ignoreRules = IgnoreRules.Load(inputDir, config);
        var result = new List<FileCandidate>();
        var stack = new Stack<string>();
        stack.Push(inputDir);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            IEnumerable<string> dirs;
            IEnumerable<string> files;

            try
            {
                dirs = Directory.EnumerateDirectories(current).ToList();
                files = Directory.EnumerateFiles(current).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                if (config.SnapshotComplete)
                    throw new ScanException($"Cannot enumerate source directory: {RelativeOrName(inputDir, current)} (access denied).");
                skipped.Add(new SkippedFile(RelativeOrName(inputDir, current), "directory access denied"));
                continue;
            }
            catch (IOException ex)
            {
                if (config.SnapshotComplete)
                    throw new ScanException($"Cannot enumerate source directory: {RelativeOrName(inputDir, current)}: {ex.Message}");
                skipped.Add(new SkippedFile(RelativeOrName(inputDir, current), $"directory I/O error: {ex.Message}"));
                continue;
            }

            foreach (var dir in dirs)
            {
                DirectoryInfo info;
                try
                {
                    info = new DirectoryInfo(dir);
                    if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        skipped.Add(new SkippedFile(RelativeOrName(inputDir, dir), "directory reparse point skipped"));
                        continue;
                    }

                    if (!config.IncludeHidden && info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        skipped.Add(new SkippedFile(RelativeOrName(inputDir, dir), "hidden directory skipped"));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (config.SnapshotComplete)
                        throw new ScanException($"Cannot inspect directory '{RelativeOrName(inputDir, dir)}': {ex.Message}");
                    continue;
                }

                var relativeDir = RelativeOrName(inputDir, dir).Replace('\\', '/');
                
                if (!config.IncludeGenerated && Output.GeneratedOutputDetector.IsGeneratedOutput(dir, true))
                {
                    skipped.Add(new SkippedFile(relativeDir + "/", "skipped_generated_output"));
                    continue;
                }

                if (config.ExcludeDirs.Contains(info.Name))
                {
                    skipped.Add(new SkippedFile(relativeDir + "/", "default/user excluded directory"));
                    continue;
                }

                if (ignoreRules.IsIgnored(relativeDir + "/"))
                {
                    skipped.Add(new SkippedFile(relativeDir + "/", ".aiignore/.gitignore/exclude pattern"));
                    continue;
                }

                stack.Push(dir);
            }

            foreach (var file in files)
            {
                FileInfo info;
                try
                {
                    info = new FileInfo(file);
                    if (!config.IncludeHidden && info.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        skipped.Add(new SkippedFile(RelativeOrName(inputDir, file), "hidden file skipped"));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (config.SnapshotComplete)
                        throw new ScanException($"Cannot inspect file '{RelativeOrName(inputDir, file)}': {ex.Message}");
                    continue;
                }

                var relative = Path.GetRelativePath(inputDir, file).Replace('\\', '/');

                if (!config.IncludeGenerated && Output.GeneratedOutputDetector.IsGeneratedOutput(file, false))
                {
                    skipped.Add(new SkippedFile(relative, "skipped_generated_output"));
                    continue;
                }

                if (ignoreRules.IsIgnored(relative))
                {
                    skipped.Add(new SkippedFile(relative, ".aiignore/.gitignore/exclude pattern"));
                    continue;
                }

                if (Glob.AnyMatch(relative, config.ExcludeGlobs))
                {
                    skipped.Add(new SkippedFile(relative, "--exclude pattern"));
                    continue;
                }

                var explicitInclude = Glob.AnyMatch(relative, config.IncludeGlobs);
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(file).ToLowerInvariant();

                if (!explicitInclude &&
                    (ext is ".exe" or ".pdb" or ".dll" or ".zip" || 
                     fileName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) || 
                     fileName.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase)))
                {
                    skipped.Add(new SkippedFile(relative, "release artifact excluded"));
                    continue;
                }

                var includeByExtension = !string.IsNullOrWhiteSpace(ext) && config.IncludeExtensions.Contains(ext);
                var includeBySpecialName = Program.DefaultSpecialFileNames.Contains(fileName);

                if (!explicitInclude && !includeByExtension && !includeBySpecialName)
                {
                    skipped.Add(new SkippedFile(relative, "unsupported extension/name"));
                    continue;
                }

                if (config.IsMultiSelect || (config.SnapshotComplete && config.SelectedPaths.Count > 0))
                {
                    bool isSelected = false;
                    foreach (var sp in config.SelectedPaths)
                    {
                        if (string.Equals(file, sp, StringComparison.OrdinalIgnoreCase))
                        {
                            isSelected = true;
                            break;
                        }
                        
                        // Check if it's a descendant of a selected directory
                        var spDir = sp.EndsWith(Path.DirectorySeparatorChar.ToString()) ? sp : sp + Path.DirectorySeparatorChar;
                        if (file.StartsWith(spDir, StringComparison.OrdinalIgnoreCase))
                        {
                            isSelected = true;
                            break;
                        }
                    }

                    if (!isSelected)
                    {
                        skipped.Add(new SkippedFile(relative, "skipped_not_in_selection"));
                        continue;
                    }
                }

                result.Add(new FileCandidate(file, relative));
            }
        }

        return result.OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string RelativeOrName(string root, string path)
    {
        try
        {
            return Path.GetRelativePath(root, path).Replace('\\', '/');
        }
        catch
        {
            return Path.GetFileName(path);
        }
    }
}
