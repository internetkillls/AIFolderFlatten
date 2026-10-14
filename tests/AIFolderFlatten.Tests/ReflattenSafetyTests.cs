using System;
using System.IO;
using System.Linq;
using Xunit;
using AIFolderFlatten.Output;

namespace AIFolderFlatten.Tests;

public class ReflattenSafetyTests : IDisposable
{
    private readonly string _testDir;

    public ReflattenSafetyTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "AIFolderFlatten_ReflattenTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void GeneratedOutputDetector_SkipsAiSeedOutputMarker()
    {
        // Arrange
        var subDir = Path.Combine(_testDir, "some-pack-folder");
        Directory.CreateDirectory(subDir);
        
        var filePath = Path.Combine(subDir, ".ai-seed-output.json");
        File.WriteAllText(filePath, "{\"generator\":\"AIFolderFlatten\"}");

        // Act
        var result = GeneratedOutputDetector.IsGeneratedOutput(subDir, true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GeneratedOutputDetector_DoesNotSkipNormalFolders()
    {
        // Arrange
        var subDir = Path.Combine(_testDir, "some-normal-folder");
        Directory.CreateDirectory(subDir);
        
        // Act
        var result = GeneratedOutputDetector.IsGeneratedOutput(subDir, true);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OutputSelfExclusionPolicy_ThrowsWhenTryingToWriteToSelfWithoutFlag()
    {
        // Arrange
        var config = new AppConfig();
        // Since AllowOutputInsideInput is false by default in AppConfig.cs, we can use an empty one.
        
        // Act & Assert
        Assert.Throws<AIFolderFlattenException>(() => 
            OutputSelfExclusionPolicy.ValidateAndExclude(_testDir, _testDir, config, out _)
        );
    }
}
