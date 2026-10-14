using System.Text.RegularExpressions;

namespace AIFolderFlatten;

internal static class Redactor
{
    private sealed record Rule(string Category, Regex Pattern, Func<Match, string> Replacement);

    private static readonly IReadOnlyList<Rule> Rules = new List<Rule>
    {
        new("private_key", new Regex("-----BEGIN (RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----[\\s\\S]*?-----END (RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "<PRIVATE_KEY_REDACTED>"),
        new("named_api_key", new Regex("(?i)\\b(OPENAI_API_KEY|ANTHROPIC_API_KEY|GOOGLE_API_KEY|GITHUB_TOKEN|GITLAB_TOKEN|DISCORD_TOKEN)\\b\\s*[:=]\\s*['\"]?[^'\"`\\s]+", RegexOptions.Compiled | RegexOptions.CultureInvariant), m => KeepKeyName(m.Value, "<REDACTED>")),
        new("generic_secret_assignment", new Regex("(?i)\\b(api[_-]?key|secret|token|password|passwd|pwd|session|cookie)\\s*[:=]\\s*['\"]?[^\\s'\"`]+", RegexOptions.Compiled | RegexOptions.CultureInvariant), m => KeepAssignmentKey(m.Value)),
        new("openai_key", new Regex("\\bsk-[A-Za-z0-9_-]{20,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "sk-<REDACTED>"),
        new("github_token", new Regex("\\bgh[pousr]_[A-Za-z0-9_]{20,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "gh_<REDACTED>"),
        new("aws_access_key", new Regex("\\bAKIA[0-9A-Z]{16}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "AKIA<REDACTED>"),
        new("aws_secret_access_key", new Regex("(?i)aws_secret_access_key\\s*[:=]\\s*['\"]?[^'\"`\\s]+", RegexOptions.Compiled | RegexOptions.CultureInvariant), m => KeepAssignmentKey(m.Value)),
        new("jwt", new Regex("\\beyJ[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\.[A-Za-z0-9_-]{10,}\\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "<JWT_REDACTED>"),
        new("connection_string", new Regex("(?i)(Server|Host|Data Source)\\s*=.+?;(Database|Initial Catalog)\\s*=.+?;(User Id|Uid|User|Username)\\s*=.+?;Password\\s*=.+?(?=\\r?\\n|$)", RegexOptions.Compiled | RegexOptions.CultureInvariant), _ => "<CONNECTION_STRING_REDACTED>"),
        new("database_url", new Regex("(?i)\\b(postgres|postgresql|mysql|mongodb|redis)://[^\\s'\"`]+", RegexOptions.Compiled | RegexOptions.CultureInvariant), m => m.Groups[1].Value + "://<REDACTED>"),
    };

    private static readonly Rule LocalPathRule = new(
        "local_path",
        new Regex(@"(?i)\b[A-Z]:\\(?:[^\\\r\n\t:*?""<>|]+\\?)+", RegexOptions.Compiled | RegexOptions.CultureInvariant),
        _ => "<LOCAL_PATH_REDACTED>");

    public static (string Content, IReadOnlyDictionary<string, int> Counts) Redact(string content, AppConfig config)
    {
        if (!config.RedactSecrets && !config.RedactLocalPaths) return (content, new Dictionary<string, int>());

        var result = content;
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (config.RedactSecrets)
        {
            foreach (var rule in Rules)
            {
                result = Apply(result, rule, counts);
            }
        }

        if (config.RedactLocalPaths)
        {
            result = Apply(result, LocalPathRule, counts);
        }

        return (result, counts);
    }

    private static string Apply(string input, Rule rule, Dictionary<string, int> counts)
    {
        var count = 0;
        var output = rule.Pattern.Replace(input, match =>
        {
            count++;
            return rule.Replacement(match);
        });

        if (count > 0) counts[rule.Category] = counts.GetValueOrDefault(rule.Category) + count;
        return output;
    }

    private static string KeepAssignmentKey(string value)
    {
        var idx = value.IndexOf('=');
        var sep = '=';
        if (idx < 0)
        {
            idx = value.IndexOf(':');
            sep = ':';
        }
        if (idx < 0) return "<REDACTED>";
        return value[..idx].TrimEnd() + sep + "<REDACTED>";
    }

    private static string KeepKeyName(string value, string replacement)
    {
        var idx = value.IndexOf('=');
        var sep = '=';
        if (idx < 0)
        {
            idx = value.IndexOf(':');
            sep = ':';
        }
        if (idx < 0) return replacement;
        return value[..idx].TrimEnd() + sep + replacement;
    }
}
