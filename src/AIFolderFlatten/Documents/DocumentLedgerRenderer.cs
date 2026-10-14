using System;
using System.Collections.Generic;
using System.Text;

namespace AIFolderFlatten.Documents;

internal static class DocumentLedgerRenderer
{
    public static string RenderDocuments(
        List<HtmlDocumentParser.HtmlReport> htmls,
        List<PdfMetadataAdapter.PdfReport> pdfs,
        List<EpubMetadataAdapter.EpubReport> epubs,
        List<LatexLiteParser.LatexReport> latexes,
        List<JupyterNotebookParser.NotebookReport> notebooks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Documents Analysis Report");
        sb.AppendLine();
        sb.AppendLine("Summarizing metadata and structure for discovered document archives:");
        sb.AppendLine();

        if (htmls.Count > 0)
        {
            sb.AppendLine("## HTML Web Documents");
            sb.AppendLine();
            foreach (var h in htmls)
            {
                sb.AppendLine($"### Title: *{h.Title}*");
                sb.AppendLine($"*   **Description**: {h.Description}");
                sb.AppendLine($"*   **Headings**: {h.Headings.Count} found");
                foreach (var hd in h.Headings.Take(5)) sb.AppendLine($"    *   {hd}");
                sb.AppendLine($"*   **Links**: {h.Links.Count} outbound found");
                sb.AppendLine($"*   **Text Preview**: {h.TextPreview}");
                sb.AppendLine();
            }
        }

        if (pdfs.Count > 0)
        {
            sb.AppendLine("## PDF Document Sniffers (T1 metadata)");
            sb.AppendLine();
            sb.AppendLine("| Title | Author | Valid PDF | Status |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (var p in pdfs)
            {
                sb.AppendLine($"| {p.Title} | {p.Author} | {p.IsValidPdf} | {p.MetadataStatus} |");
            }
            sb.AppendLine();
        }

        if (epubs.Count > 0)
        {
            sb.AppendLine("## EPUB Digital Books (T1 container/OPF metadata)");
            sb.AppendLine();
            sb.AppendLine("| Title | Author | Language | Status |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (var e in epubs)
            {
                sb.AppendLine($"| {e.Title} | {e.Author} | {e.Language} | {e.MetadataStatus} |");
            }
            sb.AppendLine();
        }

        if (latexes.Count > 0)
        {
            sb.AppendLine("## LaTeX Documents (T3 structure)");
            sb.AppendLine();
            foreach (var l in latexes)
            {
                sb.AppendLine($"*   **Document Class**: `{l.DocumentClass}`");
                sb.AppendLine($"*   **Packages**: {string.Join(", ", l.Packages)}");
                sb.AppendLine($"*   **Sections**: {l.Sections.Count} found");
                foreach (var sec in l.Sections.Take(5)) sb.AppendLine($"    *   {sec}");
                sb.AppendLine($"*   **Equations**: {l.Equations.Count} formulas extracted");
                sb.AppendLine($"*   **Citations**: {l.Citations.Count} bib keys referenced");
                sb.AppendLine();
            }
        }

        if (notebooks.Count > 0)
        {
            sb.AppendLine("## Jupyter Notebooks (T3 structure)");
            sb.AppendLine();
            foreach (var notebook in notebooks)
            {
                sb.AppendLine($"*   **Markdown cells**: {notebook.MarkdownCells}");
                sb.AppendLine($"*   **Code cells**: {notebook.CodeCells}");
                sb.AppendLine($"*   **Headings**: {notebook.Headings.Count}");
                sb.AppendLine($"*   **Imports**: {notebook.Imports.Count}");
                sb.AppendLine($"*   **Equation candidates**: {notebook.EquationCandidates.Count}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public static string RenderCitations(List<CitationEntry> citations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Citations Reference Index");
        sb.AppendLine();
        sb.AppendLine("Tracked bibliographic citation index entries:");
        sb.AppendLine();

        if (citations.Count == 0)
        {
            sb.AppendLine("No bibliographic citation reference entries found.");
            return sb.ToString();
        }

        sb.AppendLine("| Key | Type | Title | Author | Year |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
        foreach (var c in citations)
        {
            sb.AppendLine($"| `{c.Key}` | `{c.Type}` | {c.Title} | {c.Author} | {c.Year} |");
        }

        return sb.ToString();
    }
}
