using System.Text;

namespace AIFolderFlatten.Output;

internal static class GitSelectionReportRenderer
{
    public static string Render(AppConfig config, ContextPackage package)
    {
        var isGitRepo = package.Git != null && package.Git.IsGitRepo;
        var sourceKind = GitSelectionResolver.Resolve(config, isGitRepo);

        var sb = new StringBuilder();

        switch (sourceKind)
        {
            case GitSourceState.GitWorking:
                sb.AppendLine("## Git Context: Working Folder");
                sb.AppendLine();
                sb.AppendLine("> Git repository detected, but using standard folder snapshot (uncommitted + committed together).");
                sb.AppendLine("> No specific Git branch, commit, or changed-file filter was applied.");
                break;
            case GitSourceState.GitStaged:
                sb.AppendLine("## Git Context: Staged Files Only");
                sb.AppendLine();
                sb.AppendLine("> Output is strictly limited to files currently staged in Git (`--git-staged true`).");
                break;
            case GitSourceState.GitChanged:
                sb.AppendLine("## Git Context: Changed Files Only");
                sb.AppendLine();
                sb.AppendLine("> Output is strictly limited to files currently changed in the Git working directory (`--git-changed true`).");
                break;
            case GitSourceState.GitCommit:
                sb.AppendLine($"## Git Context: Specific Commit / Ref (`{config.GitRef}`)");
                sb.AppendLine();
                sb.AppendLine($"> Output represents a historical Git snapshot at ref `{config.GitRef}`.");
                break;
            case GitSourceState.Folder:
                sb.AppendLine("## Source Context: Standard Folder");
                sb.AppendLine();
                sb.AppendLine("> No Git repository detected. Standard folder snapshot applied.");
                break;
            case GitSourceState.MultiSelection:
                // If it's multi-select, Git context is less relevant, but we might still mention it.
                if (isGitRepo)
                {
                    sb.AppendLine("## Git Context: Active Repository");
                    sb.AppendLine();
                    sb.AppendLine("> A Git repository exists at the common root, but a specific multi-path selection is being compiled.");
                }
                break;
        }

        if (isGitRepo && sourceKind != GitSourceState.Folder && sourceKind != GitSourceState.MultiSelection)
        {
            sb.AppendLine();
            sb.AppendLine($"- **Branch**: `{package.Git!.Branch}`");
            sb.AppendLine($"- **HEAD SHA**: `{package.Git.HeadSha}`");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
