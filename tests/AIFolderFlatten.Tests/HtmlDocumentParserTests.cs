using System;
using Xunit;
using AIFolderFlatten.Documents;

namespace AIFolderFlatten.Tests;

public class HtmlDocumentParserTests
{
    [Fact]
    public void Test_HtmlDocumentParser_ExtractsElementsAndStripsScripts()
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Sample Document Title</title>
    <meta name='description' content='Meta Description Content'>
    <script>alert('dangerous script');</script>
</head>
<body>
    <h1>Header 1 text</h1>
    <h2>Header 2 text</h2>
    <p>Read more at <a href='https://example.com'>example</a>.</p>
</body>
</html>
";
        var report = HtmlDocumentParser.Parse(html);

        Assert.Equal("Sample Document Title", report.Title);
        Assert.Equal("Meta Description Content", report.Description);
        Assert.Contains("H1: Header 1 text", report.Headings);
        Assert.Contains("H2: Header 2 text", report.Headings);
        Assert.Contains("[example](https://example.com)", report.Links);
        
        // Assert scripts are stripped
        Assert.DoesNotContain("alert", report.TextPreview);
        Assert.DoesNotContain("dangerous script", report.TextPreview);
    }
}
