using System;
using System.Text;

namespace AIFolderFlatten.Post;

internal static class ModeGuideRenderer
{
    public static string Render(OrientationState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Context compiler Mode Guide");
        sb.AppendLine();
        sb.AppendLine($"This package was generated using the **v{Program.AppVersion} POST Context Compiler**.");
        sb.AppendLine($"*   **Selected Domain**: `{state.SelectedDomain}`");
        sb.AppendLine($"*   **Selected Lens**: `{state.SelectedLens}`");
        sb.AppendLine($"*   **Source State**: `{state.SelectedSourceState}`");
        sb.AppendLine();

        sb.AppendLine("## Lens Guidance");
        sb.AppendLine();
        sb.AppendLine("AIFolderFlatten supports 4 core cross-domain lenses. Choose the appropriate lens based on your goals:");
        sb.AppendLine();
        sb.AppendLine("*   **Overview**: Best for initial codebase exploration. Emphasizes file indices and summaries.");
        sb.AppendLine("*   **Debug**: Best for deep work or AI coding agents. Includes line numbers, full file contents, and security risks.");
        sb.AppendLine("*   **Architecture**: Best for analyzing code dependencies, import structures, and class/method signatures.");
        sb.AppendLine("*   **Obsidian**: Best for reading markdown note vaults, wikilinks, tags, and frontmatter properties.");
        sb.AppendLine();

        sb.AppendLine("## Domain Guidance");
        sb.AppendLine();
        sb.AppendLine("Each domain triggers specialized analyzers and rendering maps:");
        sb.AppendLine();
        sb.AppendLine("*   **Code**: Optimizes output for code symbols, imports, project types, and entrypoints.");
        sb.AppendLine("*   **Obsidian**: Maps note networks, link graphs, frontmatter keys, and checklist items.");
        sb.AppendLine("*   **Research**: Compiles academic term frequencies, methodology signals, claims maps, and scholarly queries.");
        sb.AppendLine("*   **Modeling**: Extracts formulas, variables, parameters, boundary conditions, and scientific equations.");
        sb.AppendLine("*   **Creative**: Tracks media asset sizes, SHA256 hashes, script target suggestions, and creative affordances.");
        sb.AppendLine();

        sb.AppendLine("## Source State Guidance");
        sb.AppendLine();
        sb.AppendLine("*   **working_folder**: Includes all untracked and modified files currently in the filesystem.");
        sb.AppendLine("*   **git_head**: Filters context to only files currently tracked in the Git HEAD commit.");
        sb.AppendLine("*   **git_changed**: Filters context to files modified since the last Git commit.");
        sb.AppendLine("*   **git_staged**: Filters context to files staged for the next Git commit.");
        sb.AppendLine();

        sb.AppendLine("## Alternative Recommendations for this Folder");
        if (state.Alternatives.Count > 0)
        {
            sb.AppendLine("Based on our scoring, you might want to try these alternative configurations:");
            sb.AppendLine();
            foreach (var alt in state.Alternatives)
            {
                sb.AppendLine($"*   `--domain {alt.Split(' ')[0]}`");
            }
        }
        else
        {
            sb.AppendLine("This folder exhibits extremely clear signals matching the chosen configuration. No alternative domains scored positive.");
        }
        sb.AppendLine();

        return sb.ToString();
    }
}
