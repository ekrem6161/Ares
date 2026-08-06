using System.Text;
using System.Text.Json;

namespace Ares.Core.Anthropic;

public static class Connection
{
    private static readonly HttpClient _http = new();
    private const int MaksToken = 2048;

    public static async Task<string> IstekGonder(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        if (string.IsNullOrWhiteSpace(anahtar))
            return Hata.Doner("Anthropic", "Anahtar boş");
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
            return Hata.Doner("Anthropic", "URL veya model boş");
        if (mesajlar is null || mesajlar.Count == 0)
            return Hata.Doner("Anthropic", "Mesaj listesi boş");

        var icerikMesajlari = mesajlar.Where(m => m.Rol != RolTipi.System).ToList();
        if (icerikMesajlari.Count == 0)
            return Hata.Doner("Anthropic", "İçerik mesajı yok");

        var sistem = string.Join("\n", mesajlar.Where(m => m.Rol == RolTipi.System).Select(m => m.Icerik));
        var govde = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["max_tokens"] = MaksToken,
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
        try
        {
            using var yanit = await _http.SendAsync(istek);
            var metin = await yanit.Content.ReadAsStringAsync();
            if (!yanit.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[Anthropic.Connection] HTTP {(int)yanit.StatusCode}: {metin}");
                return $"[HATA] Anthropic HTTP {(int)yanit.StatusCode}";
            }
            return CevapParser.MetniCikar(metin);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Anthropic.Connection] {e.Message}");
            return "[HATA] Anthropic isteği başarısız: " + e.Message;
        }
    }
}
