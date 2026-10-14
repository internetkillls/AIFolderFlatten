using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Modeling;

internal sealed record EquationInfo(
    string Formula,
    string Evidence,
    IReadOnlyList<string> ExtractedVariables
);

internal sealed record VariableInfo(
    string Symbol,
    string Name,
    string Unit,
    string Role, // parameter, state, output, unknown
    string Evidence
);

internal static class EquationLiteExtractor
{
    private static readonly Regex BlockMathRegex = new(@"\$\$(.*?)\$\$|\\begin\{(equation|align|gather|eqnarray)\}([\s\S]*?)\\end\{\2\}", RegexOptions.Compiled);
    private static readonly Regex InlineMathRegex = new(@"\$(.*?)\$", RegexOptions.Compiled);
    private static readonly Regex VariableSymbolRegex = new(@"\b([a-zA-Z])\b|\\(alpha|beta|gamma|delta|epsilon|zeta|eta|theta|iota|kappa|lambda|mu|nu|xi|pi|rho|sigma|tau|upsilon|phi|chi|psi|omega)\b", RegexOptions.Compiled);

    public static List<EquationInfo> Extract(string fileId, string content)
    {
        var list = new List<EquationInfo>();

        // Find block math
        var blockMatches = BlockMathRegex.Matches(content);
        foreach (Match match in blockMatches)
        {
            var formula = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
            var lineNum = GetLineNumberAtIndex(content, match.Index);
            var vars = ExtractVariables(formula);
            list.Add(new EquationInfo(formula.Trim(), $"{fileId}:L{lineNum}", vars));
        }

        // Find inline math if no block math matches were found or to gather more
        if (list.Count < 10)
        {
            var inlineMatches = InlineMathRegex.Matches(content);
            foreach (Match match in inlineMatches)
            {
                var formula = match.Groups[1].Value;
                // Avoid matching single letters inside markdown that are just standard characters
                if (formula.Trim().Length <= 1) continue;
                var lineNum = GetLineNumberAtIndex(content, match.Index);
                var vars = ExtractVariables(formula);
                list.Add(new EquationInfo(formula.Trim(), $"{fileId}:L{lineNum}", vars));
            }
        }

        return list;
    }

    public static List<VariableInfo> BuildVariableDictionary(List<EquationInfo> equations)
    {
        var dict = new Dictionary<string, VariableInfo>();
        foreach (var eq in equations)
        {
            foreach (var sym in eq.ExtractedVariables)
            {
                if (!dict.ContainsKey(sym))
                {
                    // Heuristic variable naming and unit mapping
                    var name = GetHeuristicName(sym);
                    var unit = GetHeuristicUnit(sym);
                    var role = GetHeuristicRole(sym);

                    dict[sym] = new VariableInfo(
                        Symbol: sym,
                        Name: name,
                        Unit: unit,
                        Role: role,
                        Evidence: eq.Evidence
                    );
                }
            }
        }
        return new List<VariableInfo>(dict.Values);
    }

    private static List<string> ExtractVariables(string formula)
    {
        var vars = new HashSet<string>();
        var matches = VariableSymbolRegex.Matches(formula);
        foreach (Match match in matches)
        {
            var sym = match.Groups[1].Success ? match.Groups[1].Value : "\\" + match.Groups[2].Value;
            // Filter out common LaTeX keywords like d, e (exponent), i, j (loop indices) if they are lowercase letters
            if (sym == "d" || sym == "e" || sym == "i" || sym == "j" || sym == "x" || sym == "y")
            {
                // We can still keep x and y as coordinates/variables
            }
            vars.Add(sym);
        }
        return new List<string>(vars);
    }

    private static string GetHeuristicName(string symbol)
    {
        return symbol switch
        {
            "T" => "Temperature",
            "t" => "Time",
            "p" => "Pressure",
            "V" => "Volume",
            "v" => "Velocity",
            "m" => "Mass",
            "E" => "Energy",
            "x" => "X Coordinate",
            "y" => "Y Coordinate",
            "z" => "Z Coordinate",
            "f" => "Function",
            "\\theta" => "Angle or Temperature Theta",
            "\\lambda" => "Wavelength or Decay Constant",
            "\\rho" => "Density",
            "\\sigma" => "Standard Deviation or Stress",
            "\\mu" => "Mean or Viscosity",
            _ => $"Variable '{symbol}'"
        };
    }

    private static string GetHeuristicUnit(string symbol)
    {
        return symbol switch
        {
            "T" => "K (Kelvin) or C (Celsius)",
            "t" => "s (seconds)",
            "p" => "Pa (Pascal)",
            "V" => "m^3 (cubic meters)",
            "v" => "m/s (meters per second)",
            "m" => "kg (kilogram)",
            "E" => "J (Joule)",
            _ => "dimensionless or unknown"
        };
    }

    private static string GetHeuristicRole(string symbol)
    {
        return symbol switch
        {
            "T" or "x" or "y" or "z" => "state or coordinate",
            "t" => "independent variable",
            "p" or "V" or "m" => "parameter",
            "f" or "E" => "output or objective function value",
            _ => "unknown/unmapped"
        };
    }

    private static int GetLineNumberAtIndex(string content, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n') line++;
        }
        return line;
    }
}
