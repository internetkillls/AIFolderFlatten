using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Modeling;

internal sealed record ModelAuditWarning(
    string ModelDomain,
    string Severity,
    string Message,
    string MitigationHint
);

internal static class ModelingSignalAnalyzer
{
    private static readonly Dictionary<string, string[]> DomainKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "thermal", new[] { "heat", "temperature", "conduction", "convection", "radiation", "thermal conductivity", "flux" } },
        { "economics", new[] { "utility", "elasticity", "demand", "supply", "equilibrium", "welfare", "regression" } },
        { "statistics", new[] { "hypothesis", "p-value", "confidence interval", "likelihood", "posterior", "Bayesian", "estimator" } },
        { "optimization", new[] { "objective", "constraint", "minimize", "maximize", "linear programming", "Lagrangian", "penalty" } },
        { "simulation", new[] { "monte carlo", "agent-based", "system dynamics", "sensitivity", "random seed", "stochastic", "calibration" } }
    };

    public static string ClassifyDomain(string content, out Dictionary<string, int> scores)
    {
        scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "thermal", 0 }, { "economics", 0 }, { "statistics", 0 }, { "optimization", 0 }, { "simulation", 0 }
        };

        foreach (var dom in DomainKeywords.Keys)
        {
            foreach (var word in DomainKeywords[dom])
            {
                if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    scores[dom]++;
                }
            }
        }

        string best = "unknown";
        int max = 0;
        foreach (var kv in scores)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                best = kv.Key;
            }
        }

        return best;
    }

    public static List<ModelAuditWarning> AuditModel(string domain, string content)
    {
        var list = new List<ModelAuditWarning>();

        if (domain == "thermal")
        {
            if (!content.Contains("boundary condition", StringComparison.OrdinalIgnoreCase) && !content.Contains("boundary", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("thermal", "high", "Missing boundary conditions. A PDE thermal model requires Dirichlet, Neumann, or Robin boundary specifications.", "Declare boundary temperature values or heat flux equations."));
            }
            if (!content.Contains("initial condition", StringComparison.OrdinalIgnoreCase) && !content.Contains("initial state", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("thermal", "medium", "Missing initial conditions. Transient thermal simulation requires initial temperature profile.", "Set initial temperature T(x,0) = T_0."));
            }
        }

        if (domain == "economics")
        {
            if (!content.Contains("data provenance", StringComparison.OrdinalIgnoreCase) && !content.Contains("source", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("economics", "medium", "Economics model lacks explicit data provenance statements.", "Add data source citation or census collection metadata."));
            }
            if (!content.Contains("robustness", StringComparison.OrdinalIgnoreCase) && !content.Contains("standard error", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("economics", "low", "Lacks standard error robustness checks or panel clustering.", "Include clustered standard errors or alternate sample filters."));
            }
        }

        if (domain == "optimization")
        {
            if (!content.Contains("objective", StringComparison.OrdinalIgnoreCase) && !content.Contains("minimize", StringComparison.OrdinalIgnoreCase) && !content.Contains("maximize", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("optimization", "high", "Optimization model does not define a clear objective function.", "Specify minimize/maximize f(x) target."));
            }
            if (!content.Contains("constraint", StringComparison.OrdinalIgnoreCase) && !content.Contains("subject to", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("optimization", "high", "Optimization model does not declare decision variables constraints.", "Define inequality constraints g(x) <= 0."));
            }
        }

        if (domain == "simulation")
        {
            if (!content.Contains("seed", StringComparison.OrdinalIgnoreCase) && !content.Contains("random", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("simulation", "medium", "Simulation does not declare a static random seed, reducing reproducibility.", "Set static random seed in simulation initialization."));
            }
            if (!content.Contains("uncertainty", StringComparison.OrdinalIgnoreCase) && !content.Contains("sensitivity", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ModelAuditWarning("simulation", "low", "Lacks sensitivity analysis on model parameter ranges.", "Run sensitivity parameter range sweeps."));
            }
        }

        return list;
    }
}
