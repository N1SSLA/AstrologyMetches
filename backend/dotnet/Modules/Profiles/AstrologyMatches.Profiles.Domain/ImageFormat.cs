namespace AstrologyMatches.Profiles.Domain;

/// <summary>
/// Определяет формат изображения по первым байтам файла.
/// </summary>
public static class ImageFormat
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string Webp = "image/webp";

    /// <summary>
    /// Сколько первых байт файла нужно, чтобы определить формат.
    /// </summary>
    public const int HeaderLength = 12;

    /// <summary>
    /// Возвращает MIME-тип (jpeg, png или webp) или null, если файл не похож ни на один из них.
    /// </summary>
    /// <remarks>
    /// Content-Type из запроса задаёт клиент и может прислать что угодно,
    /// поэтому тип проверяем по сигнатуре в начале файла.
    /// </remarks>
    public static string? DetectContentType(byte[] header)
    {
        // JPEG начинается с FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return Jpeg;
        }

        // PNG начинается с 89 'P' 'N' 'G' 0D 0A 1A 0A
        byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        if (header.Length >= pngSignature.Length && header.Take(pngSignature.Length).SequenceEqual(pngSignature))
        {
            return Png;
        }

        // WEBP: 'RIFF', 4 байта размера, затем 'WEBP'
        if (header.Length >= 12
            && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
            && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
        {
            return Webp;
        }

        return null;
    }
}
