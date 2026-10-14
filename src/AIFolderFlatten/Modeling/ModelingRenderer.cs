using System;
using System.Collections.Generic;
using System.Text;

namespace AIFolderFlatten.Modeling;

internal static class ModelingRenderer
{
    public static string RenderModelingOrientation(string domain, Dictionary<string, int> scores, List<ModelAuditWarning> warnings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Modeling and Scientific Orientation");
        sb.AppendLine();
        sb.AppendLine($"*   **Determined Mathematical Domain**: `{domain}`");
        sb.AppendLine();

        sb.AppendLine("## Domain Signal Counts");
        sb.AppendLine();
        foreach (var kv in scores)
        {
            sb.AppendLine($"*   **{kv.Key}**: {kv.Value} keyword matches");
        }
        sb.AppendLine();

        sb.AppendLine("## Model Completeness Audit");
        sb.AppendLine();
        if (warnings.Count == 0)
        {
            sb.AppendLine("Model audit passed with 0 completeness warnings.");
        }
        else
        {
            sb.AppendLine("| Domain | Severity | Warning Message | Suggested Mitigation |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (var w in warnings)
            {
                sb.AppendLine($"| {w.ModelDomain} | **{w.Severity.ToUpper()}** | {w.Message} | {w.MitigationHint} |");
            }
        }

        return sb.ToString();
    }

    public static string RenderEquations(List<EquationInfo> equations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Extracted Mathematical Formulations");
        sb.AppendLine();
        sb.AppendLine("Parsed LaTeX equations and inline formulas:");
        sb.AppendLine();

        if (equations.Count == 0)
        {
            sb.AppendLine("No explicit LaTeX math equations or inline formulas detected.");
            return sb.ToString();
        }

        foreach (var eq in equations)
        {
            sb.AppendLine($"### Equation at `{eq.Evidence}`");
            sb.AppendLine();
            sb.AppendLine("$$");
            sb.AppendLine(eq.Formula);
            sb.AppendLine("$$");
            sb.AppendLine();
            sb.AppendLine($"*   **Sniffed variable symbols**: {string.Join(", ", eq.ExtractedVariables)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string RenderVariables(List<VariableInfo> variables)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Variables Dictionary");
        sb.AppendLine();
        sb.AppendLine("Mathematical symbols extracted from formulas and mapped to heuristic definitions:");
        sb.AppendLine();

        if (variables.Count == 0)
        {
            sb.AppendLine("No variable symbols extracted.");
            return sb.ToString();
        }

        sb.AppendLine("| Symbol | Inferred Name | Units | Expected Role | First Evidence |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
        foreach (var v in variables)
        {
            sb.AppendLine($"| `{v.Symbol}` | {v.Name} | {v.Unit} | {v.Role} | `{v.Evidence}` |");
        }

        return sb.ToString();
    }

    public static string RenderAssumptions(List<ModelAuditWarning> warnings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Model Assumptions and Bounds");
        sb.AppendLine();
        sb.AppendLine("Tracked boundary limitations and solver assumptions:");
        sb.AppendLine();

        var boundaryWarnings = warnings.Where(w => w.Message.Contains("boundary", StringComparison.OrdinalIgnoreCase)).ToList();
        if (boundaryWarnings.Count > 0)
        {
            sb.AppendLine("> [!WARNING]");
            sb.AppendLine("> Boundary conditions are not fully specified. Numerical PDE solvers assume default values (e.g. insulating bounds) which may alter simulation results.");
            sb.AppendLine();
        }

        sb.AppendLine("### General Assumptions");
        sb.AppendLine("*   Parameters are constant unless time-dependent equations are declared.");
        sb.AppendLine("*   Variable coordinates are bounded by dimensions sniffed in data frames.");
        sb.AppendLine();

        return sb.ToString();
    }

    public static string RenderParameters(List<VariableInfo> variables)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Model Parameters");
        sb.AppendLine();
        sb.AppendLine("Variables acting as constant inputs or system parameters in mathematical formulations:");
        sb.AppendLine();

        var parameters = variables.Where(v => v.Role == "parameter").ToList();
        if (parameters.Count == 0)
        {
            sb.AppendLine("No dedicated parameter variables detected in this scan.");
            return sb.ToString();
        }

        sb.AppendLine("| Parameter | Symbol | Unit | Sniffed Source |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");
        foreach (var p in parameters)
        {
            sb.AppendLine($"| {p.Name} | `{p.Symbol}` | {p.Unit} | `{p.Evidence}` |");
        }

        return sb.ToString();
    }
}
