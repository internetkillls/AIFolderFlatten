using System;
using System.IO;

namespace AIFolderFlatten.Creative;

internal static class CreativeAffordanceAnalyzer
{
    public static string InferAffordance(string extension, string relativePath)
    {
        var ext = extension.ToLowerInvariant().Trim();
        var folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/').ToLowerInvariant() ?? "";

        // Heuristics based on folder path names
        string prefix = "";
        if (folder.Contains("mood") || folder.Contains("reference") || folder.Contains("board"))
        {
            prefix = "Creative Reference Card: ";
        }
        else if (folder.Contains("sound") || folder.Contains("audio") || folder.Contains("synth"))
        {
            prefix = "Audio Design Asset: ";
        }
        else if (folder.Contains("video") || folder.Contains("motion") || folder.Contains("anim"))
        {
            prefix = "Motion Video Reference: ";
        }

        var fileName = Path.GetFileName(relativePath);
        switch (ext)
        {
            case ".wav":
            case ".mp3":
            case ".flac":
                return prefix + $"Audio waveform stream candidate ({fileName}). Target script VST mapping or SuperCollider synthesis.";
            case ".mp4":
            case ".mov":
            case ".webm":
                return prefix + $"Video timeline motion candidate ({fileName}). Target keyframe extraction or motion vector analysis.";
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".webp":
            case ".svg":
                return prefix + $"Static visual reference ({fileName}). Target moodboard index or layout color scheme extraction.";
            case ".blend":
                return $"3D Model / Scene file ({fileName}). Target Blender Python keyframe script automation.";
            case ".pd":
                return $"Modular audio patch file ({fileName}). Target PureData canvas synthesis.";
            case ".scd":
                return $"SuperCollider code ({fileName}). Target real-time audio server synthesis.";
            default:
                return $"Generic creative asset ({fileName}). Target index ledger preservation.";
        }
    }
}
