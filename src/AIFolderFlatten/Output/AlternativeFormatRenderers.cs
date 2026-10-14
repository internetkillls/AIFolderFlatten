using System.Text;
using System.Text.Json;
using System.Linq;
using System.Security;

namespace AIFolderFlatten.Output;

internal static class XmlRenderer
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<AIContextPack>");
        sb.AppendLine($"  <Metadata>");
        sb.AppendLine($"    <GeneratedAt>{SecurityElement.Escape(package.GeneratedAt.ToString("o"))}</GeneratedAt>");
        sb.AppendLine($"    <RootName>{SecurityElement.Escape(package.RootName)}</RootName>");
        sb.AppendLine($"    <Mode>{SecurityElement.Escape(package.Mode)}</Mode>");
        sb.AppendLine($"  </Metadata>");
        sb.AppendLine("  <Files>");
        foreach (var file in package.Files)
        {
            sb.AppendLine($"    <File path=\"{SecurityElement.Escape(file.RelativePath)}\">");
            sb.AppendLine($"      <Content><![CDATA[{file.Content}]]></Content>");
            sb.AppendLine($"    </File>");
        }
        sb.AppendLine("  </Files>");
        sb.AppendLine("</AIContextPack>");
        return sb.ToString();
    }
}

internal static class JsonRenderer
{
    public static string Render(ContextPackage package)
    {
        var obj = new {
            metadata = new {
                generatedAt = package.GeneratedAt.ToString("o"),
                rootName = package.RootName,
                mode = package.Mode
            },
            files = package.Files.Select(f => new {
                path = f.RelativePath,
                content = f.Content
            })
        };
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }
}

internal static class JsonlRenderer
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        foreach (var file in package.Files)
        {
            var obj = new {
                path = file.RelativePath,
                content = file.Content
            };
            sb.AppendLine(JsonSerializer.Serialize(obj));
        }
        return sb.ToString();
    }
}

internal static class YamlRenderer
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("metadata:");
        sb.AppendLine($"  generatedAt: \"{package.GeneratedAt:o}\"");
        sb.AppendLine($"  rootName: \"{EscapeYaml(package.RootName)}\"");
        sb.AppendLine($"  mode: \"{package.Mode}\"");
        sb.AppendLine("files:");
        foreach (var file in package.Files)
        {
            sb.AppendLine($"  - path: \"{EscapeYaml(file.RelativePath)}\"");
            sb.AppendLine($"    content: |");
            
            using var reader = new System.IO.StringReader(file.Content);
            while (reader.ReadLine() is string line)
            {
                sb.AppendLine($"      {line}");
            }
        }
        return sb.ToString();
    }

    private static string EscapeYaml(string s) => s.Replace("\"", "\\\"");
}

internal static class HtmlPreviewRenderer
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>AI Context Pack</title>");
        sb.AppendLine("<style>body{font-family:sans-serif;} .file{margin-bottom:2em;} .content{background:#f4f4f4;padding:1em;white-space:pre-wrap;border:1px solid #ddd;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>AI Context Pack: {SecurityElement.Escape(package.RootName)}</h1>");
        foreach (var file in package.Files)
        {
            sb.AppendLine($"<div class=\"file\"><h2>{SecurityElement.Escape(file.RelativePath)}</h2>");
            sb.AppendLine($"<div class=\"content\">{SecurityElement.Escape(file.Content)}</div></div>");
        }
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
