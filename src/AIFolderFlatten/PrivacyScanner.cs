using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten;

internal static class PrivacyScanner
{
    private static readonly IReadOnlyList<Regex> SecretPatterns = new List<Regex>
    {
        new Regex("-----BEGIN (RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----", RegexOptions.Compiled | RegexOptions.CultureInvariant),
        new Regex("\\bsk-[A-Za-z0-9_-]{20,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant),
        new Regex("\\bgh[pousr]_[A-Za-z0-9_]{20,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant),
        new Regex("\\bAKIA[0-9A-Z]{16}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant),
        new Regex("\\beyJ[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant),
    };

    public static bool ContainsRawSecrets(string content, out string? matchedPatternReason)
    {
        matchedPatternReason = null;
        foreach (var pattern in SecretPatterns)
        {
            var match = pattern.Match(content);
            if (match.Success)
            {
                matchedPatternReason = $"Matched pattern: '{pattern}' at index {match.Index}";
                return true;
            }
        }
        return false;
    }
}
