using System.Text;

namespace AIFolderFlatten;

internal static class FileText
{
    public static string ReadTextFile(string path)
    {
        try
        {
            return File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
        catch (DecoderFallbackException)
        {
            return File.ReadAllText(path, Encoding.Latin1);
        }
    }

    public static bool LooksBinary(string path)
    {
        Span<byte> buffer = stackalloc byte[4096];
        using var stream = File.OpenRead(path);
        var read = stream.Read(buffer);
        if (read == 0) return false;

        var controlCount = 0;
        for (var i = 0; i < read; i++)
        {
            var b = buffer[i];
            if (b == 0) return true;
            var isAllowedControl = b is 9 or 10 or 13;
            if (b < 32 && !isAllowedControl) controlCount++;
        }

        return controlCount / (double)read > 0.10;
    }

    public static string AddLineNumbers(string content)
    {
        var lines = NormalizeLineEndings(content).Split('\n');
        var width = Math.Max(4, lines.Length.ToString().Length);
        var sb = new StringBuilder(content.Length + lines.Length * (width + 2));
        for (var i = 0; i < lines.Length; i++)
        {
            sb.Append((i + 1).ToString().PadLeft(width, '0'));
            sb.Append(' ');
            sb.AppendLine(lines[i].TrimEnd('\r'));
        }
        return sb.ToString().TrimEnd();
    }

    public static string NormalizeLineEndings(string content)
    {
        return content.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    public static int CountLines(string content)
    {
        if (content.Length == 0) return 0;
        return NormalizeLineEndings(content).Count(ch => ch == '\n') + 1;
    }
}
