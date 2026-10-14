using System;
using Xunit;
using AIFolderFlatten.Modeling;

namespace AIFolderFlatten.Tests;

public class ModelingAnalyzerTests
{
    [Fact]
    public void Test_ModelingAnalyzer_ExtractsEquationsAndVariables()
    {
        var content = @"The pressure model is defined by block equation: $$ p = \rho R T $$";
        var equations = EquationLiteExtractor.Extract("F0001", content);

        Assert.Single(equations);
        Assert.Equal(@"p = \rho R T", equations[0].Formula);
        
        // Assert variables
        Assert.Contains("p", equations[0].ExtractedVariables);
        Assert.Contains(@"\rho", equations[0].ExtractedVariables);
        Assert.Contains("T", equations[0].ExtractedVariables);

        var vars = EquationLiteExtractor.BuildVariableDictionary(equations);
        var tempVar = vars.FirstOrDefault(v => v.Symbol == "T");
        Assert.NotNull(tempVar);
        Assert.Equal("Temperature", tempVar.Name);
    }

    [Fact]
    public void Test_ModelingAnalyzer_AuditsThermalPDE()
    {
        var content = "We define a thermal convection model.";
        var domain = ModelingSignalAnalyzer.ClassifyDomain(content, out var scores);
        var warnings = ModelingSignalAnalyzer.AuditModel(domain, content);

        Assert.Equal("thermal", domain);
        // Thermal model without boundary/initial condition keywords should trigger warnings
        Assert.NotEmpty(warnings);
        Assert.Contains(warnings, w => w.Severity == "high");
    }
}
