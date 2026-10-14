using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIFolderFlatten.Artifacts;

namespace AIFolderFlatten.Creative;

internal sealed record MediaAssetInfo(
    string Id,
    string Path,
    string Category, // Audio, Video, Image, 3D Model, Audio Patch
    long SizeBytes,
    string Sha256,
    string InferredAffordance,
    string ParserTier,
    string Status,
    string Warning
);

internal static class MediaLedgerBuilder
{
    public static List<MediaAssetInfo> Build(List<ArtifactIR> artifacts)
    {
        var list = new List<MediaAssetInfo>();
        var mediaArtifacts = artifacts.Where(a => a.ArtifactType == "creative").ToList();

        foreach (var art in mediaArtifacts)
        {
            var category = GetCategory(art.Extension);
            var affordance = CreativeAffordanceAnalyzer.InferAffordance(art.Extension, art.Path);

            string warning = "Duration, channels, sample rate, fps, and keyframes are unavailable in the deterministic core. Sniffing requires local ffprobe/ffmpeg tooling.";

            list.Add(new MediaAssetInfo(
                Id: art.Id,
                Path: art.Path,
                Category: category,
                SizeBytes: art.SizeBytes,
                Sha256: art.Sha256,
                InferredAffordance: affordance,
                ParserTier: art.ParserTier,
                Status: art.Status,
                Warning: warning
            ));
        }

        return list;
    }

    private static string GetCategory(string extension)
    {
        var ext = extension.ToLowerInvariant().Trim();
        return ext switch
        {
            ".wav" or ".mp3" or ".flac" => "Audio",
            ".mp4" or ".mov" or ".webm" => "Video",
            ".png" or ".jpg" or ".jpeg" or ".webp" or ".svg" => "Image",
            ".blend" => "3D Model",
            ".pd" or ".scd" => "Audio Patch",
            _ => "Creative Reference File"
        };
    }
}
