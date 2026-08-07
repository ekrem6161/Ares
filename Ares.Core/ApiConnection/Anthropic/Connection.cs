using System.Text;
using System.Text.Json;

namespace Ares.Core.Anthropic;

public static class Connection
{
    private static readonly HttpClient _http = new();
    private const int MaksToken = 2048;

    /// <summary>
    /// Anthropic API'ye streaming isteği gönderir (stream: true).
    /// Metin parçalarını akış olarak döner; hata durumlarında "[HATA] ..."
    /// tek parça olarak akar.
    /// </summary>
    public static IAsyncEnumerable<string> IstekGonder(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        if (string.IsNullOrWhiteSpace(anahtar))
            return AkisUretici.TekParca(Hata.Doner("Anthropic", "Anahtar boş"));
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
            return AkisUretici.TekParca(Hata.Doner("Anthropic", "URL veya model boş"));
        if (mesajlar is null || mesajlar.Count == 0)
            return AkisUretici.TekParca(Hata.Doner("Anthropic", "Mesaj listesi boş"));
        return AkisUretici.Guvenli(AkisiUret(anahtar, url, model, mesajlar), "Anthropic");
    }

    private static async IAsyncEnumerable<string> AkisiUret(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        var icerikMesajlari = mesajlar.Where(m => m.Rol != RolTipi.System).ToList();
        if (icerikMesajlari.Count == 0)
        {
            yield return Hata.Doner("Anthropic", "İçerik mesajı yok");
            yield break;
        }

        var sistem = string.Join("\n", mesajlar.Where(m => m.Rol == RolTipi.System).Select(m => m.Icerik));
        var govde = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["max_tokens"] = MaksToken,
            ["stream"] = true,
            ["messages"] = icerikMesajlari.Select(m => new { role = m.Rol.ToString().ToLowerInvariant(), content = m.Icerik }),
        };
        if (!string.IsNullOrWhiteSpace(sistem))
            govde["system"] = sistem;

        using var istek = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json"),
        };
        istek.Headers.Add("x-api-key", anahtar);
        istek.Headers.Add("anthropic-version", "2023-06-01");

        using var yanit = await _http.SendAsync(istek, HttpCompletionOption.ResponseHeadersRead);
        if (!yanit.IsSuccessStatusCode)
        {
            var hataMetni = await yanit.Content.ReadAsStringAsync();
            Console.Error.WriteLine($"[Anthropic.Connection] HTTP {(int)yanit.StatusCode}: {hataMetni}");
            yield return $"[HATA] Anthropic HTTP {(int)yanit.StatusCode}";
            yield break;
        }

        using var akim = await yanit.Content.ReadAsStreamAsync();
        using var okuyucu = new StreamReader(akim, Encoding.UTF8);
        await foreach (var parca in SseOkuyucu.DataParcalari(okuyucu))
        {
            var metin = CevapParser.AkisParcasiCikar(parca);
            if (metin.Length > 0)
                yield return metin;
        }
    }
}
