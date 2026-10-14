using System;
using System.IO;
using Xunit;

namespace AIFolderFlatten.Tests;

public sealed class InputListFileTests
{
    [Fact]
    public void ExpandInputListFile_replaces_file_with_single_input_and_deletes_it()
    {
        var listFile = Path.Combine(Path.GetTempPath(), $"aiff-input-{Guid.NewGuid():N}.paths");
        File.WriteAllLines(listFile, new[]
        {
            @"C:\Folder One",
            @"C:\Folder Two",
            @"C:\Folder One"
        });

        var result = Program.ExpandInputListFile(new[]
        {
            "--input-list-file",
            listFile,
            "--format",
            "md"
        });

        Assert.Equal("--input", result[0]);
        Assert.Equal(@"C:\Folder One|C:\Folder Two", result[1]);
        Assert.Equal("--format", result[2]);
        Assert.Equal("md", result[3]);
        Assert.False(File.Exists(listFile));
    }
}
