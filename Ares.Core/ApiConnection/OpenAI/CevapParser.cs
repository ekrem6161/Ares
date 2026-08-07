using System.Text.Json;

namespace Ares.Core.OpenAI;

/// <summary>
/// OpenAI-uyumlu yanıt ayrıştırıcısı. Tek parça yanıt <see cref="TekParcaCikar"/>,
/// streaming parçaları <see cref="AkisParcasiCikar"/> ile çıkarılır.
/// </summary>
internal static class CevapParser
{
    public static string TekParcaCikar(string yanitJson)
    {
        using var belge = JsonDocument.Parse(yanitJson);
        return belge.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    /// <summary>Streaming parçası: choices[0].delta.content — yoksa boş döner.</summary>
    public static string AkisParcasiCikar(string dataJson)
    {
        using var belge = JsonDocument.Parse(dataJson);
        if (!belge.RootElement.TryGetProperty("choices", out var secimler) ||
            secimler.GetArrayLength() == 0)
            return "";
        var delta = secimler[0].GetProperty("delta");
        if (!delta.TryGetProperty("content", out var icerik) || icerik.ValueKind != JsonValueKind.String)
            return "";
        return icerik.GetString() ?? "";
    }
}
