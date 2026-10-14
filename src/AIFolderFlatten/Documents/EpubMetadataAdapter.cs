using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AIFolderFlatten.Documents;

internal static class EpubMetadataAdapter
{
    public sealed record EpubReport(
        bool IsValid,
        string Title,
        string Author,
        string Language,
        string MetadataStatus
    );

    public static EpubReport Parse(string filepath)
    {
        try
        {
            if (!File.Exists(filepath))
            {
                return new EpubReport(false, "Unknown", "Unknown", "Unknown", "File does not exist.");
            }

            using var archive = ZipFile.OpenRead(filepath);

            // 1. Locate META-INF/container.xml
            var containerEntry = archive.GetEntry("META-INF/container.xml");
            if (containerEntry == null)
            {
                return new EpubReport(false, "Unknown", "Unknown", "Unknown", "Missing META-INF/container.xml.");
            }

            string opfPath = "";
            using (var stream = containerEntry.Open())
            {
                var doc = XDocument.Load(stream);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                var rootfile = doc.Descendants(ns + "rootfile").FirstOrDefault();
                opfPath = rootfile?.Attribute("full-path")?.Value ?? "";
            }

            if (string.IsNullOrWhiteSpace(opfPath))
            {
                return new EpubReport(false, "Unknown", "Unknown", "Unknown", "Root OPF file path not found in container.xml.");
            }

            // 2. Load OPF metadata
            var opfEntry = archive.GetEntry(opfPath);
            if (opfEntry == null)
            {
                // Try case-insensitive lookup
                opfEntry = archive.Entries.FirstOrDefault(e => e.FullName.Equals(opfPath, StringComparison.OrdinalIgnoreCase));
            }

            if (opfEntry == null)
            {
                return new EpubReport(false, "Unknown", "Unknown", "Unknown", $"OPF entry not found at path: {opfPath}");
            }

            string title = "Unknown Title";
            string author = "Unknown Author";
            string lang = "Unknown Language";

            using (var stream = opfEntry.Open())
            {
                var doc = XDocument.Load(stream);
                var metadata = doc.Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "metadata");
                if (metadata != null)
                {
                    var titleEl = metadata.Elements().FirstOrDefault(e => e.Name.LocalName == "title");
                    if (titleEl != null) title = titleEl.Value;

                    var creatorEl = metadata.Elements().FirstOrDefault(e => e.Name.LocalName == "creator");
                    if (creatorEl != null) author = creatorEl.Value;

                    var langEl = metadata.Elements().FirstOrDefault(e => e.Name.LocalName == "language");
                    if (langEl != null) lang = langEl.Value;
                }
            }

            return new EpubReport(true, title, author, lang, "T1_metadata Parsed");
        }
        catch (Exception ex)
        {
            return new EpubReport(false, "Error", "Error", "Error", $"Failed to parse: {ex.Message}");
        }
    }
}
