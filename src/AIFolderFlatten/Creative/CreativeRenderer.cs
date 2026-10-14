using System;
using System.Collections.Generic;
using System.Text;

namespace AIFolderFlatten.Creative;

internal static class CreativeRenderer
{
    /// <summary>
    /// Creative Asset Ledger — path, extension, size, category only. No perceptual claims.
    /// Replaces the old RenderCreativeReference which contained subjective classification.
    /// </summary>
    public static string RenderCreativeAssetLedger(List<MediaAssetInfo> media)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Creative Asset Ledger (Expert-System)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: This ledger contains path, extension, size, and SHA-256 only.");
        sb.AppendLine("> No perceptual analysis, content analysis, or AI classification was performed.");
        sb.AppendLine();
        sb.AppendLine($"*   **Total media assets discovered**: {media.Count}");
        sb.AppendLine();

        sb.AppendLine("## Asset Count by Category (Extension-Based)");
        sb.AppendLine();
        var counts = new Dictionary<string, int>();
        foreach (var m in media)
        {
            counts[m.Category] = counts.GetValueOrDefault(m.Category) + 1;
        }

        foreach (var kv in counts)
        {
            sb.AppendLine($"*   **{kv.Key}**: {kv.Value} file(s)");
        }

        sb.AppendLine();
        sb.AppendLine("See MEDIA_LEDGER.md for full path/hash/size listing.");

        return sb.ToString();
    }

    // Kept for backward compatibility
    public static string RenderCreativeReference(List<MediaAssetInfo> media)
        => RenderCreativeAssetLedger(media);

    public static string RenderMediaLedger(List<MediaAssetInfo> media)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Media Ledger");
        sb.AppendLine();
        sb.AppendLine("List of creative media assets discovered in this workspace:");
        sb.AppendLine();
        sb.AppendLine("| ID | Relative Path | Category | Size (Bytes) | SHA-256 | Inferred Affordance | Warning / Limitations |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var m in media)
        {
            sb.AppendLine($"| `{m.Id}` | `{m.Path}` | {m.Category} | {m.SizeBytes:N0} | `{m.Sha256}` | {m.InferredAffordance} | *{m.Warning}* |");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Script Target Candidates — heuristic environment hints per asset category.
    /// Candidate language only. Not validated recommendations.
    /// </summary>
    public static string RenderScriptTargetCandidates(List<MediaAssetInfo> media)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Script Target Candidates (Heuristic)");
        sb.AppendLine();
        sb.AppendLine("> **Expert-System Note**: Environment suggestions below are heuristic mappings from file");
        sb.AppendLine("> extension/category to known script environments. They are *candidates*, not recommendations.");
        sb.AppendLine();
        sb.AppendLine("| Asset Category | Candidate Script Environment | Candidate Role |");
        sb.AppendLine("| :--- | :--- | :--- |");

        var categories = media.Select(m => m.Category).Distinct().ToList();
        foreach (var cat in categories)
        {
            var target = GetRecommendedEnvironment(cat);
            var role = GetTargetRole(cat);
            sb.AppendLine($"| {cat} | `{target}` | {role} |");
        }

        return sb.ToString();
    }

    // Kept for backward compatibility
    public static string RenderScriptTargets(List<MediaAssetInfo> media)
        => RenderScriptTargetCandidates(media);

    private static string GetRecommendedEnvironment(string category)
    {
        return category switch
        {
            "Audio" => "SuperCollider / VST Parameter Map",
            "Video" => "Blender Python / Three.js",
            "Image" => "p5.js / Tailwind CSS",
            "3D Model" => "Blender Python Scripting",
            "Audio Patch" => "PureData / MaxMSP",
            _ => "Generic Python audio/video scripting"
        };
    }

    private static string GetTargetRole(string category)
    {
        return category switch
        {
            "Audio" => "Automate synthesizer params or synthesis engines",
            "Video" => "Parse visual keyframes or timeline timelines",
            "Image" => "Map canvas drawings, coordinate vectors, or moodboards",
            "3D Model" => "Run Blender CLI batch animations",
            "Audio Patch" => "Design real-time modular synthesis blocks",
            _ => "Index asset details for processing"
        };
    }
}
