using System.Text.Json;

namespace Ares.Core.Anthropic;

internal static class CevapParser
{
    public static string MetniCikar(string yanitJson)
    {
        using var belge = JsonDocument.Parse(yanitJson);
        foreach (var blok in belge.RootElement.GetProperty("content").EnumerateArray())
        {
            if (blok.GetProperty("type").GetString() == "text")
                return blok.GetProperty("text").GetString() ?? "";
        }
        return "[HATA] Anthropic cevabında metin yok.";
    }
}
