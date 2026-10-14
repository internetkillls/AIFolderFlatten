using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AIFolderFlatten.Documents;

internal static class PdfMetadataAdapter
{
    private static readonly Regex TitleRegex = new(@"/Title\s*\((.*?)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AuthorRegex = new(@"/Author\s*\((.*?)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public sealed record PdfReport(
        bool IsValidPdf,
        string Title,
        string Author,
        string MetadataStatus
    );

    public static PdfReport Parse(string filepath)
    {
        try
        {
            if (!File.Exists(filepath))
            {
                return new PdfReport(false, "Unknown", "Unknown", "File does not exist.");
            }

            using var stream = File.OpenRead(filepath);
            var header = new byte[8];
            var bytesRead = stream.Read(header, 0, header.Length);
            if (bytesRead < 4)
            {
                return new PdfReport(false, "Unknown", "Unknown", "Empty or truncated file.");
            }

            var headerStr = Encoding.ASCII.GetString(header, 0, bytesRead);
            bool isValid = headerStr.StartsWith("%PDF", StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                return new PdfReport(false, "Unknown", "Unknown", "Invalid PDF magic header.");
            }

            // Sniff metadata in the first 8KB or last 8KB
            string title = "Unknown Title";
            string author = "Unknown Author";

            var buffer = new byte[8192];
            stream.Seek(0, SeekOrigin.Begin);
            var read = stream.Read(buffer, 0, buffer.Length);
            var startChunk = Encoding.UTF8.GetString(buffer, 0, read);

            var tMatch = TitleRegex.Match(startChunk);
            if (tMatch.Success) title = tMatch.Groups[1].Value;

            var aMatch = AuthorRegex.Match(startChunk);
            if (aMatch.Success) author = aMatch.Groups[1].Value;

            return new PdfReport(true, title, author, "T1_metadata Sniffed");
        }
        catch (Exception ex)
        {
            return new PdfReport(false, "Error", "Error", $"Failed to parse: {ex.Message}");
        }
    }
}
