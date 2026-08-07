using System.Text;

namespace Ares.Core;

/// <summary>
/// SSE (Server-Sent Events) akış okuyucusu: text/event-stream formatındaki
/// yanıttan "data: ..." içeriklerini ayıklar. OpenAI ve Anthropic streaming
/// yanıtlarının ortak okuyucusudur. "data: [DONE]" görüldüğünde akış biter.
/// </summary>
internal static class SseOkuyucu
{
    public static async IAsyncEnumerable<string> DataParcalari(StreamReader okuyucu)
    {
        while (true)
        {
            var satir = await okuyucu.ReadLineAsync();
            if (satir is null)
                yield break;
            if (!satir.StartsWith("data:", StringComparison.Ordinal))
                continue;
            var veri = satir.Substring(5).Trim();
            if (veri.Length == 0)
                continue;
            if (veri == "[DONE]")
                yield break;
            yield return veri;
        }
    }
}
