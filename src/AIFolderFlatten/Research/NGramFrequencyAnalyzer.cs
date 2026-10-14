using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Research;

internal static class NGramFrequencyAnalyzer
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at",
        "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "can", "can't", "cannot",
        "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during", "each", "few",
        "for", "from", "further", "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll",
        "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how", "how's", "i", "i'd", "i'll",
        "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most",
        "mustn't", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our",
        "ours", "ourselves", "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't",
        "so", "some", "such", "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there",
        "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too",
        "under", "until", "up", "very", "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't",
        "what", "what's", "when", "when's", "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's",
        "with", "won't", "would", "wouldn't", "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself",
        "yourselves"
    };

    public sealed record NGramResult(
        IReadOnlyList<KeyValuePair<string, int>> Unigrams,
        IReadOnlyList<KeyValuePair<string, int>> Bigrams,
        IReadOnlyList<KeyValuePair<string, int>> Trigrams
    );

    public static NGramResult Analyze(string content, int topCount = 15)
    {
        // Tokenize words
        var words = Regex.Matches(content.ToLowerInvariant(), @"\b[a-z]{3,15}\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(w => !StopWords.Contains(w))
            .ToList();

        var unigrams = new Dictionary<string, int>();
        var bigrams = new Dictionary<string, int>();
        var trigrams = new Dictionary<string, int>();

        // Count Unigrams
        foreach (var w in words)
        {
            unigrams[w] = unigrams.GetValueOrDefault(w) + 1;
        }

        // Count Bigrams
        for (int i = 0; i < words.Count - 1; i++)
        {
            var bg = $"{words[i]} {words[i + 1]}";
            bigrams[bg] = bigrams.GetValueOrDefault(bg) + 1;
        }

        // Count Trigrams
        for (int i = 0; i < words.Count - 2; i++)
        {
            var tg = $"{words[i]} {words[i + 1]} {words[i + 2]}";
            trigrams[tg] = trigrams.GetValueOrDefault(tg) + 1;
        }

        return new NGramResult(
            Unigrams: unigrams.OrderByDescending(kv => kv.Value).Take(topCount).ToList(),
            Bigrams: bigrams.OrderByDescending(kv => kv.Value).Take(topCount).ToList(),
            Trigrams: trigrams.OrderByDescending(kv => kv.Value).Take(topCount).ToList()
        );
    }
}
