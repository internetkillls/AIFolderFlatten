using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIFolderFlatten.Output;

internal static class CommonRootResolver
{
    public static string Resolve(List<string> absolutePaths)
    {
        if (absolutePaths == null || absolutePaths.Count == 0)
            throw new ArgumentException("Cannot resolve common root for an empty list of paths.");

        if (absolutePaths.Count == 1)
        {
            return Normalize(absolutePaths[0]);
        }

        var normalizedPaths = absolutePaths.Select(Normalize).ToList();

        // Split each path into segments
        var pathSegments = normalizedPaths.Select(p => p.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)).ToList();

        var minLength = pathSegments.Min(p => p.Length);
        var commonLength = 0;

        for (int i = 0; i < minLength; i++)
        {
            var firstSegment = pathSegments[0][i];
            if (pathSegments.All(p => string.Equals(p[i], firstSegment, StringComparison.OrdinalIgnoreCase)))
            {
                commonLength++;
            }
            else
            {
                break;
            }
        }

        if (commonLength == 0)
        {
            // Usually happens if paths are on different drives on Windows (e.g., C:\ and D:\)
            throw new AIFolderFlattenException("Paths are on different drives. Cannot resolve a common root.", ExitCodes.ArgumentError);
        }

        var commonSegments = pathSegments[0].Take(commonLength).ToArray();
        
        // Handle Windows Drive letter (e.g. C: -> C:\)
        var root = string.Join(Path.DirectorySeparatorChar.ToString(), commonSegments);
        if (commonLength == 1 && root.EndsWith(":"))
        {
            root += Path.DirectorySeparatorChar;
        }

        return root;
    }

    private static string Normalize(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            fullPath = Path.GetDirectoryName(fullPath) ?? fullPath;
        }
        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
