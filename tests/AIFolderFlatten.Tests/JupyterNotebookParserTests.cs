using AIFolderFlatten.Documents;
using Xunit;

namespace AIFolderFlatten.Tests;

public sealed class JupyterNotebookParserTests
{
    [Fact]
    public void Parser_extracts_cells_headings_imports_and_equations()
    {
        var report = JupyterNotebookParser.Parse("""
            {
              "cells": [
                {
                  "cell_type": "markdown",
                  "source": ["# Model Notes\n", "$$E = mc^2$$"]
                },
                {
                  "cell_type": "code",
                  "source": ["import numpy as np\n", "from scipy import stats\n"]
                }
              ]
            }
            """);

        Assert.Equal(1, report.MarkdownCells);
        Assert.Equal(1, report.CodeCells);
        Assert.Contains("Model Notes", report.Headings);
        Assert.Contains("import numpy as np", report.Imports);
        Assert.Contains("E = mc^2", report.EquationCandidates);
    }
}
