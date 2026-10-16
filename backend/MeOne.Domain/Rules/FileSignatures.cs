namespace MeOne.Domain.Rules;

public static class FileSignatures
{
    public const int HeaderLength = 16;

    public static bool IsPdf(ReadOnlySpan<byte> header) =>
        header.Length >= 5 && header[..5].SequenceEqual("%PDF-"u8);

    public static string? ImageContentTypeOf(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";

        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return "image/jpeg";

        if (header.Length >= 6 && (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)))
            return "image/gif";

        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";

        return null;
    }
}
