using SvadbeniSalon.Model.Exceptions;

namespace SvadbeniSalon.Services;

public static class ImageContentValidator
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Gif87 = "GIF87a"u8.ToArray();
    private static readonly byte[] Gif89 = "GIF89a"u8.ToArray();
    private static readonly byte[] WebpRiff = "RIFF"u8.ToArray();
    private static readonly byte[] WebpWebp = "WEBP"u8.ToArray();

    public static void EnsureValidImageBase64(string? base64, int maxBytes = 2_000_000)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        var payload = base64.Contains(',') ? base64[(base64.IndexOf(',') + 1)..] : base64;
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload.Trim());
        }
        catch (FormatException)
        {
            throw new ClientException("Profilna slika nije validan Base64 sadržaj.");
        }

        if (bytes.Length == 0)
        {
            throw new ClientException("Profilna slika je prazna.");
        }

        if (bytes.Length > maxBytes)
        {
            throw new ClientException($"Profilna slika ne smije biti veća od {maxBytes / 1_000_000} MB.");
        }

        if (!IsAllowedImage(bytes))
        {
            throw new ClientException(
                "Profilna slika mora biti JPEG, PNG, GIF ili WEBP (provjera po sadržaju datoteke).");
        }
    }

    private static bool IsAllowedImage(ReadOnlySpan<byte> data)
    {
        if (StartsWith(data, Jpeg)) return true;
        if (StartsWith(data, Png)) return true;
        if (StartsWith(data, Gif87) || StartsWith(data, Gif89)) return true;
        if (data.Length >= 12 && StartsWith(data, WebpRiff) && data[8..12].SequenceEqual(WebpWebp))
        {
            return true;
        }

        return false;
    }

    private static bool StartsWith(ReadOnlySpan<byte> data, ReadOnlySpan<byte> prefix)
        => data.Length >= prefix.Length && data[..prefix.Length].SequenceEqual(prefix);
}
