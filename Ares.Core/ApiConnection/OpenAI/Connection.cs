using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ares.Core.OpenAI;

public static class Connection
{
    private static readonly HttpClient _http = new();

    /// <summary>
    /// OpenAI-uyumlu endpoint'e streaming isteği gönderir (stream: true).
    /// Metin parçalarını akış olarak döner; hata durumlarında "[HATA] ..."
    /// tek parça olarak akar.
    /// </summary>
    public static IAsyncEnumerable<string> IstekGonder(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        if (string.IsNullOrWhiteSpace(anahtar))
            return AkisUretici.TekParca(Hata.Doner("OpenAI", "Anahtar boş"));
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
            return AkisUretici.TekParca(Hata.Doner("OpenAI", "URL veya model boş"));
        if (mesajlar is null || mesajlar.Count == 0)
            return AkisUretici.TekParca(Hata.Doner("OpenAI", "Mesaj listesi boş"));
        return AkisUretici.Guvenli(AkisiUret(anahtar, url, model, mesajlar), "OpenAI");
    }

    private static async IAsyncEnumerable<string> AkisiUret(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        var govde = new
        {
            model,
            stream = true,
            messages = mesajlar.Select(m => new { role = m.Rol.ToString().ToLowerInvariant(), content = m.Icerik }),
        };
        using var istek = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json"),
        };
        istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anahtar);

        using var yanit = await _http.SendAsync(istek, HttpCompletionOption.ResponseHeadersRead);
        if (!yanit.IsSuccessStatusCode)
        {
            var hataMetni = await yanit.Content.ReadAsStringAsync();
            Console.Error.WriteLine($"[OpenAI.Connection] HTTP {(int)yanit.StatusCode}: {hataMetni}");
            yield return $"[HATA] OpenAI HTTP {(int)yanit.StatusCode}";
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
