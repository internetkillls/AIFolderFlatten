using Xunit;

namespace AIFolderFlatten.Tests;

public class AnalyzerTests
{
    [Fact]
    public void Test_FileAnalyzer_ExtractsCsharpImportsAndSymbols()
    {
        var content = @"
using System.Text;
using System.IO;

namespace MyNamespace;

public class MyClass
{
    public void MyMethod()
    {
    }
}
";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var analysis = FileAnalyzer.Analyze("MyClass.cs", content, config);

        Assert.Equal("source", analysis.Kind);
        Assert.Equal("csharp", analysis.Language);
        Assert.Contains("System.Text", analysis.Imports);
        Assert.Contains("System.IO", analysis.Imports);
        Assert.Contains("class: MyClass", analysis.Symbols);
        Assert.Contains("method: MyMethod", analysis.Symbols);
    }

    [Fact]
    public void Test_FileAnalyzer_ExtractsMarkdownHeadingsAndTags()
    {
        var content = @"
# Heading 1
## Heading 2
Some text #tag1 #tag2.
- [ ] Task 1
- [x] Task 2
";
        var config = AppConfig.FromArgs(new[] { "--input", "." });

        var analysis = FileAnalyzer.Analyze("doc.md", content, config);

        Assert.Equal("note", analysis.Kind);
        Assert.Equal("markdown", analysis.Language);
        Assert.Contains("L2 # Heading 1", analysis.Headings);
        Assert.Contains("L3 ## Heading 2", analysis.Headings);
        Assert.Contains("#tag1", analysis.Tags);
        Assert.Contains("#tag2", analysis.Tags);
        Assert.Contains("[ ] Task 1", analysis.Tasks);
        Assert.Contains("[x] Task 2", analysis.Tasks);
    }
}
