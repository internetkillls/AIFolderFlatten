using System;
using Xunit;
using AIFolderFlatten.Research;

namespace AIFolderFlatten.Tests;

public class ResearchSignalTests
{
    [Fact]
    public void Test_ResearchSignal_ExtractsMethodCues()
    {
        var text = "In this research, semi-structured interviews were conducted to collect text.";
        var methods = ResearchSignalAnalyzer.ExtractMethods("F0001", text);

        Assert.Single(methods);
        Assert.Equal("semi-structured interview", methods[0].Signal);
        Assert.Equal("F0001:L1", methods[0].Evidence);
    }

    [Fact]
    public void Test_ResearchSignal_ComputesAcademicFeasibility()
    {
        var feasibility = ResearchSignalAnalyzer.ComputeFeasibility(researchDensity: 4.5, codeDensity: 1.0);
        
        var academic = feasibility.FirstOrDefault(f => f.Dimension == "Academic");
        Assert.NotNull(academic);
        Assert.True(academic.Score > 0.5);
    }
}
