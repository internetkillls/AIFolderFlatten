using System.IO;

namespace AIFolderFlatten.Output;

internal enum GitSourceState
{
    Folder,
    MultiSelection,
    GitWorking,
    GitStaged,
    GitChanged,
    GitCommit
}

internal static class GitSelectionResolver
{
    public static GitSourceState Resolve(AppConfig config, bool isGitRepo)
    {
        if (config.IsMultiSelect)
        {
            return GitSourceState.MultiSelection;
        }

        if (config.GitRef != null)
        {
            return GitSourceState.GitCommit;
        }

        if (config.GitStaged)
        {
            return GitSourceState.GitStaged;
        }

        if (config.GitChanged)
        {
            return GitSourceState.GitChanged;
        }

        if (isGitRepo)
        {
            return GitSourceState.GitWorking;
        }

        return GitSourceState.Folder;
    }
}
