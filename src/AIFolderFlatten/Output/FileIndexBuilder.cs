using System.Linq;
using System.Text;

namespace AIFolderFlatten.Output;

internal static class FileIndexBuilder
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## File Index");
        sb.AppendLine();
        sb.AppendLine("| ID | Relative Path | Bytes | Characters |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        foreach (var file in package.Files.OrderBy(f => f.RelativePath))
        {
            var chars = file.Content.Length;
            sb.AppendLine($"| `{file.Id}` | `{file.RelativePath}` | {file.ByteLength} | {chars} |");
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
