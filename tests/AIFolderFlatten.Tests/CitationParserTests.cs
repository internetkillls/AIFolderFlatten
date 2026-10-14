using System;
using Xunit;
using AIFolderFlatten.Documents;

namespace AIFolderFlatten.Tests;

public class CitationParserTests
{
    [Fact]
    public void Test_BibTeXParser_ParsesValidEntries()
    {
        var bib = @"
@article{einstein1905,
  author = {Einstein, Albert},
  title = ""Does the inertia of a body depend upon its energy-content?"",
  journal = {Annalen der Physik},
  year = 1905
}
";
        var entries = CitationLiteParser.ParseBibTeX(bib);

        Assert.Single(entries);
        Assert.Equal("einstein1905", entries[0].Key);
        Assert.Equal("article", entries[0].Type);
        Assert.Equal("Does the inertia of a body depend upon its energy-content?", entries[0].Title);
        Assert.Equal("Einstein, Albert", entries[0].Author);
        Assert.Equal("1905", entries[0].Year);
    }

    [Fact]
    public void Test_RISParser_ParsesValidEntries()
    {
        var ris = @"
TY  - JOUR
TI  - Title of Scholarly Paper
AU  - Scholar, Alice
AU  - Scholar, Bob
PY  - 2026/06/13
ER  - 
";
        var entries = CitationLiteParser.ParseRIS(ris);

        Assert.Single(entries);
        Assert.Equal("JOUR", entries[0].Type);
        Assert.Equal("Title of Scholarly Paper", entries[0].Title);
        Assert.Contains("Scholar, Alice", entries[0].Author);
        Assert.Contains("Scholar, Bob", entries[0].Author);
        Assert.Equal("2026", entries[0].Year);
    }
}
