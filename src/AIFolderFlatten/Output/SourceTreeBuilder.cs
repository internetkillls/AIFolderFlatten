using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIFolderFlatten.Output;

internal static class SourceTreeBuilder
{
    public static string Render(ContextPackage package)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Source Tree");
        sb.AppendLine();
        sb.AppendLine("```text");
        sb.AppendLine(package.RootName);

        var allPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in package.Files) allPaths.Add(f.RelativePath);
        foreach (var s in package.Skipped) allPaths.Add(s.RelativePath.TrimEnd('/'));

        var root = BuildNodeTree(allPaths.ToList());

        RenderTree(sb, root, "", true, package.Files.Select(f => f.RelativePath).ToHashSet(StringComparer.OrdinalIgnoreCase));
        
        sb.AppendLine("```");
        sb.AppendLine();
        return sb.ToString();
    }

    private class TreeNode
    {
        public string Name { get; set; } = "";
        public string FullRelativePath { get; set; } = "";
        public Dictionary<string, TreeNode> Children { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static TreeNode BuildNodeTree(List<string> relativePaths)
    {
        var root = new TreeNode();
        foreach (var path in relativePaths)
        {
            var segments = path.Split('/');
            var current = root;
            var currentPath = "";

            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (string.IsNullOrEmpty(segment)) continue;

                currentPath = i == 0 ? segment : currentPath + "/" + segment;

                if (!current.Children.TryGetValue(segment, out var child))
                {
                    child = new TreeNode { Name = segment, FullRelativePath = currentPath };
                    current.Children[segment] = child;
                }
                current = child;
            }
        }
        return root;
    }

    private static void RenderTree(StringBuilder sb, TreeNode node, string indent, bool isLast, HashSet<string> includedPaths)
    {
        var children = node.Children.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
        
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var isChildLast = i == children.Count - 1;

            var marker = isChildLast ? "└── " : "├── ";
            var childIndent = indent + (isChildLast ? "    " : "│   ");

            // For leaf nodes (files), show if they are excluded.
            string displaySuffix = "";
            if (child.Children.Count == 0 && !includedPaths.Contains(child.FullRelativePath))
            {
                displaySuffix = " (omitted)";
            }

            sb.AppendLine($"{indent}{marker}{child.Name}{displaySuffix}");

            RenderTree(sb, child, childIndent, isChildLast, includedPaths);
        }
    }
}
