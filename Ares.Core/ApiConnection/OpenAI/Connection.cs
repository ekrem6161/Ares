using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ares.Core.OpenAI;

public static class Connection
{
    private static readonly HttpClient _http = new();

    public static async Task<string> IstekGonder(string anahtar, string url, string model, List<Mesaj> mesajlar)
    {
        if (string.IsNullOrWhiteSpace(anahtar))
            return BosHata("Anahtar boş");
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
            return BosHata("URL veya model boş");
        if (mesajlar is null || mesajlar.Count == 0)
            return BosHata("Mesaj listesi boş");

        var govde = new
        {
            model,
            messages = mesajlar.Select(m => new { role = m.Rol.ToString().ToLowerInvariant(), content = m.Icerik }),
        };
        using var istek = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json"),
        };
        istek.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anahtar);
        try
        {
            using var yanit = await _http.SendAsync(istek);
            var metin = await yanit.Content.ReadAsStringAsync();
            if (!yanit.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[OpenAI.Connection] HTTP {(int)yanit.StatusCode}: {metin}");
                return $"[HATA] OpenAI HTTP {(int)yanit.StatusCode}";
            }
            using var belge = JsonDocument.Parse(metin);
            return belge.RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content").GetString() ?? "";
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[OpenAI.Connection] {e.Message}");
            return "[HATA] OpenAI isteği başarısız: " + e.Message;
        }
    }

    private static string BosHata(string sebep)
    {
        Console.Error.WriteLine($"[OpenAI.Connection] {sebep}");
        return $"[HATA] OpenAI {sebep}.";
    }
}
