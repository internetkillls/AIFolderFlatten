using System;
using System.IO;
using System.Linq;
using Xunit;
using AIFolderFlatten.Dev;

namespace AIFolderFlatten.Tests;

public sealed class TestGapTests : IDisposable
{
    private readonly string _tempDir;

    public TestGapTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "AIFolderFlatten_TestGapTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }

    [Fact]
    public void TestGapAnalyzer_Identifies_UntestedFiles_And_Hotspots()
    {
        // Create files:
        // 1. A normal source file
        var normalSrc = Path.Combine(_tempDir, "OrderProcessor.cs");
        File.WriteAllText(normalSrc, "public class OrderProcessor { }");

        // 2. A source file with a test file
        var testedSrc = Path.Combine(_tempDir, "PaymentProcessor.cs");
        File.WriteAllText(testedSrc, "public class PaymentProcessor { }");
        var testFile = Path.Combine(_tempDir, "PaymentProcessorTests.cs");
        File.WriteAllText(testFile, "public class PaymentProcessorTests { }");

        // 3. A renderer with golden test gap (has a test file, but without golden/render content)
        var rendererSrc = Path.Combine(_tempDir, "InvoiceRenderer.cs");
        File.WriteAllText(rendererSrc, "public class InvoiceRenderer { }");
        var rendererTest = Path.Combine(_tempDir, "InvoiceRendererTests.cs");
        File.WriteAllText(rendererTest, "public class InvoiceRendererTests { }");

        // 4. A redactor with negative test gap (has a test file, but without negative/empty/null/redact content)
        var redactorSrc = Path.Combine(_tempDir, "SecretRedactor.cs");
        File.WriteAllText(redactorSrc, "public class SecretRedactor { }");
        var redactorTest = Path.Combine(_tempDir, "SecretRedactorTests.cs");
        File.WriteAllText(redactorTest, "public class SecretRedactorTests { }");

        // 5. Complexity hotspot file (> 300 lines)
        var largeSrc = Path.Combine(_tempDir, "BigController.cs");
        File.WriteAllText(largeSrc, string.Join(Environment.NewLine, Enumerable.Repeat("// line", 350)));

        // 6. Parser fixture coverage check (need tests/fixtures/ directory)
        var parserSrc = Path.Combine(_tempDir, "HtmlParser.cs");
        File.WriteAllText(parserSrc, "public class HtmlParser { }");

        var result = TestGapAnalyzer.Analyze(_tempDir, new[] { "OrderProcessor.cs", "PaymentProcessor.cs", "InvoiceRenderer.cs", "SecretRedactor.cs", "BigController.cs", "HtmlParser.cs" });

        // Assertions
        Assert.NotNull(result);
        
        // Test gap report should list untested files
        Assert.Contains("`OrderProcessor.cs`", result.TestGapMarkdown);
        Assert.DoesNotContain("`PaymentProcessor.cs`", result.TestGapMarkdown);

        // Renderer and Redactor gaps
        Assert.Contains("`InvoiceRenderer.cs` (Renderer missing golden assertions in tests)", result.TestGapMarkdown);
        Assert.Contains("`SecretRedactor.cs` (Redactor missing negative scenario tests)", result.TestGapMarkdown);

        // Hotspots
        Assert.Contains("`BigController.cs`", result.HotspotsMarkdown);
        Assert.Contains("350", result.HotspotsMarkdown);

        // Parser Coverage status (should be missing fixture since we didn't create html fixture)
        Assert.Contains("`HtmlParser.cs`", result.FixtureCoverageMarkdown);
        Assert.Contains("Missing Fixture File", result.FixtureCoverageMarkdown);
    }
}
