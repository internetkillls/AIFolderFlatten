using System;
using Xunit;
using AIFolderFlatten.Research;

namespace AIFolderFlatten.Tests;

public class NGramAnalyzerTests
{
    [Fact]
    public void Test_NGramAnalyzer_CountsKeywordsAndExcludesStopwords()
    {
        var text = "purposive sampling is important. purposive sampling provides qualitative results. we use purposive sampling.";
        var ngrams = NGramFrequencyAnalyzer.Analyze(text);

        // check bigram
        var purposiveSampling = ngrams.Bigrams.FirstOrDefault(b => b.Key == "purposive sampling");
        Assert.Equal(3, purposiveSampling.Value);

        // stopword check
        var isWord = ngrams.Unigrams.FirstOrDefault(u => u.Key == "is");
        Assert.Equal(default, isWord);
    }
}
