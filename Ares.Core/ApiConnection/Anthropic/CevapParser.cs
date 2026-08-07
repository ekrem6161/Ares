using System.Text.Json;

namespace Ares.Core.Anthropic;

/// <summary>
/// Anthropic yanıt ayrıştırıcısı. Tek parça yanıt <see cref="MetniCikar"/>,
/// streaming parçaları <see cref="AkisParcasiCikar"/> ile çıkarılır.
/// </summary>
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

    /// <summary>Streaming parçası: content_block_delta içindeki text_delta metni — yoksa boş.</summary>
    public static string AkisParcasiCikar(string dataJson)
    {
        using var belge = JsonDocument.Parse(dataJson);
        if (!belge.RootElement.TryGetProperty("type", out var tip) ||
            tip.GetString() != "content_block_delta")
            return "";
        var delta = belge.RootElement.GetProperty("delta");
        if (!delta.TryGetProperty("type", out var deltaTip) ||
            deltaTip.GetString() != "text_delta")
            return "";
        if (!delta.TryGetProperty("text", out var metin) || metin.ValueKind != JsonValueKind.String)
            return "";
        return metin.GetString() ?? "";
    }
}
