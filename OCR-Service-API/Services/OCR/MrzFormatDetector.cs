namespace Task_Manager_API.Services.OCR;

public enum MrzFormat { TD1, TD2, TD3, MRVA, MRVB, Unknown }

public static class MrzFormatDetector
{
    public static MrzFormat DetectFormat(string[] lines)
    {
        var cleanLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

        if (cleanLines.Length == 3 && cleanLines[0].Length >= 30)
            return MrzFormat.TD1;

        if (cleanLines.Length == 2 && cleanLines[0].Length >= 44)
            return cleanLines[0][0] == 'P' ? MrzFormat.TD3 : MrzFormat.MRVA;

        if (cleanLines.Length == 2 && cleanLines[0].Length >= 36)
            return cleanLines[0][0] == 'V' ? MrzFormat.MRVB : MrzFormat.TD2;

        return MrzFormat.Unknown;
    }

    public static string ExtractDocumentNumber(MrzFormat format, string[] lines)
    {
        var cleanLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

        return format switch
        {
            MrzFormat.TD1 => cleanLines[0].Substring(5, Math.Min(9, cleanLines[0].Length - 5)).Replace("<", ""),

            MrzFormat.TD3 => cleanLines[1].Substring(0, 9).Replace("<", ""),

            MrzFormat.TD2 => cleanLines[1].Substring(0, 9).Replace("<", ""),

            MrzFormat.MRVA => cleanLines[1].Substring(0, 9).Replace("<", ""),
            MrzFormat.MRVB => cleanLines[1].Substring(0, 9).Replace("<", ""),

            _ => throw new NotSupportedException($"Cannot extract document number for format {format}")
        };
    }
}