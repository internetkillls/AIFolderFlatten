using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIFolderFlatten.Post;

namespace AIFolderFlatten.Output;

internal static class MasterFlattenRenderer
{
    public static string RenderMarkdown(
        ContextPackage package, 
        AppConfig config, 
        OrientationState? orientation,
        string domain)
    {
        var sb = new StringBuilder();

        // 1. Header / metadata
        sb.AppendLine($"# AI Seeder Master Flatten - {package.RootName}");
        sb.AppendLine($"Generated: {package.GeneratedAt:O}");
        sb.AppendLine($"App Version: {package.AppVersion}");
        sb.AppendLine();

        // 2. Security notice
        sb.AppendLine("> [!WARNING]");
        sb.AppendLine("> This context package may contain sensitive information. Please review before uploading.");
        sb.AppendLine();

        // 3. Expert-system attestation
        sb.AppendLine("## Expert System Attestation");
        sb.AppendLine("```text");
        sb.AppendLine("expertSystemOnly = true");
        sb.AppendLine("aiCore = false");
        sb.AppendLine("llmUsed = false");
        sb.AppendLine("embeddingUsed = false");
        sb.AppendLine("networkUsed = false");
        sb.AppendLine("```");
        sb.AppendLine();

        // 4. Selection report
        sb.AppendLine(SelectionReportRenderer.Render(config, package));
        sb.AppendLine("---");

        // 5. Git selection report
        sb.AppendLine(GitSelectionReportRenderer.Render(config, package));
        sb.AppendLine("---");

        // 6. POST orientation summary
        if (orientation != null)
        {
            sb.AppendLine(PostOrientationEngine.RenderOrientationMarkdown(orientation));
            sb.AppendLine("---");
        }

        // 7. Source tree
        sb.AppendLine(SourceTreeBuilder.Render(package));
        sb.AppendLine("---");

        // 8. File index
        sb.AppendLine(FileIndexBuilder.Render(package));
        sb.AppendLine("---");
        // 9. Completeness report
        sb.AppendLine(CompletenessReportRenderer.Render(package));
        sb.AppendLine("---");

        // 10. Omission/skipped report
        sb.AppendLine(OmissionReportRenderer.Render(package));
        sb.AppendLine("---");
        
        // 11. Parser capability summary
        // 12. Redaction report
        // 13. Risk/hotspot summary
        // 14. Project intelligence
        // 15. Dependency/import hints
        // 16. Symbol map
        
        // 17. File contents
        var legacyContent = config.Format == "md" ? MarkdownRenderer.RenderSingle(package) : TextRenderer.Render(package);
        sb.AppendLine(legacyContent);

        // 18. Manifest summary
        // 19. Follow-up instruction block

        return sb.ToString();
    }
}
