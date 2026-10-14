using System.Text;

namespace AIFolderFlatten.Output;

internal static class SelectionReportRenderer
{
    public static string Render(AppConfig config, ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Selection Report");
        
        if (config.IsMultiSelect)
        {
            sb.AppendLine();
            sb.AppendLine("This context package was generated from a **multi-path selection** rather than a single folder root.");
            sb.AppendLine();
            sb.AppendLine($"**Common Root**: `{package.SourceRoot}`");
            sb.AppendLine();
            sb.AppendLine("### Explicitly Selected Paths");
            foreach (var path in config.SelectedPaths)
            {
                // Show paths relative to the common root for brevity
                var relative = System.IO.Path.GetRelativePath(package.SourceRoot, path).Replace('\\', '/');
                sb.AppendLine($"* `{relative}`");
            }
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("This context package was generated from a **single folder root**.");
            sb.AppendLine();
            sb.AppendLine($"**Source Root**: `{package.SourceRoot}`");
        }
        sb.AppendLine();
        return sb.ToString();
    }
}
