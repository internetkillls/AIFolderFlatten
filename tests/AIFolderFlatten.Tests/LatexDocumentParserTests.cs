using System;
using Xunit;
using AIFolderFlatten.Documents;

namespace AIFolderFlatten.Tests;

public class LatexDocumentParserTests
{
    [Fact]
    public void Test_LatexDocumentParser_ExtractsMathAndStructure()
    {
        var tex = @"
\documentclass{report}
\usepackage{graphicx}
\usepackage{amsmath,amssymb}
\begin{document}
\section{Governing Equations}
This is a transient model:
\begin{equation}
\frac{\partial T}{\partial t} = \alpha \nabla^2 T
\end{equation}
We cite \cite{einstein1905,shaw2026} for details.
\label{sec:eqs}
\end{document}
";
        var report = LatexLiteParser.Parse(tex);

        Assert.Equal("report", report.DocumentClass);
        Assert.Contains("graphicx", report.Packages);
        Assert.Contains("amsmath", report.Packages);
        Assert.Contains("amssymb", report.Packages);
        Assert.Contains("section: Governing Equations", report.Sections);
        Assert.Contains(@"\frac{\partial T}{\partial t} = \alpha \nabla^2 T", report.Equations);
        Assert.Contains("einstein1905", report.Citations);
        Assert.Contains("shaw2026", report.Citations);
        Assert.Contains("label: sec:eqs", report.Labels);
    }
}
