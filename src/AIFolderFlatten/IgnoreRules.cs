using System.Text.RegularExpressions;

namespace AIFolderFlatten;

internal sealed class IgnoreRules
{
    private readonly List<string> _patterns = new();

    public IReadOnlyList<string> Patterns => _patterns;

    public static IgnoreRules Load(string root, AppConfig config)
    {
        var rules = new IgnoreRules();
        if (config.RespectAiIgnore)
        {
            rules.LoadFile(Path.Combine(root, ".aiignore"));
        }

        if (config.RespectGitIgnore)
        {
            rules.LoadFile(Path.Combine(root, ".gitignore"));
        }

        foreach (var pattern in config.ExcludeGlobs)
        {
            rules.Add(pattern);
        }

        return rules;
    }

    private void LoadFile(string path)
    {
        if (!File.Exists(path)) return;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith('!')) continue; // v0.2 simple mode: negated patterns are intentionally ignored.
            Add(line);
        }
    }

    private void Add(string pattern)
    {
        var normalized = pattern.Replace('\\', '/').Trim();
        if (!string.IsNullOrWhiteSpace(normalized)) _patterns.Add(normalized);
    }

    public bool IsIgnored(string relativePath)
    {
        var path = relativePath.Replace('\\', '/').TrimStart('/');
        foreach (var pattern in _patterns)
        {
            if (Glob.IsMatch(path, pattern)) return true;
        }

        return false;
    }
}

internal static class Glob
{
    public static bool AnyMatch(string path, IEnumerable<string> patterns)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        return patterns.Any(pattern => IsMatch(normalized, pattern));
    }

    public static bool IsMatch(string path, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;

        var normalizedPattern = pattern.Replace('\\', '/').TrimStart('/');
        if (normalizedPattern.EndsWith('/'))
        {
            var dir = normalizedPattern.TrimEnd('/');
            return path.Equals(dir, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(dir + "/", StringComparison.OrdinalIgnoreCase)
                || path.Contains("/" + dir + "/", StringComparison.OrdinalIgnoreCase);
        }

        if (!normalizedPattern.Contains('/'))
        {
            var fileName = Path.GetFileName(path);
            if (Wildcard(fileName, normalizedPattern)) return true;
            return path.Split('/').Any(part => Wildcard(part, normalizedPattern));
        }

        return Wildcard(path, normalizedPattern);
    }

    private static bool Wildcard(string input, string pattern)
    {
        var regex = "^" + Regex.Escape(pattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", ".") + "$";
        return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
