using Ares.Core.OpenAI;

namespace Ares.Core;

public enum ProviderTipi { OpenAI, Anthropic }

public static class Router
{
    /// <summary>Varsayılan sağlayıcıya tek kullanıcı mesajıyla streaming istek gönderir.</summary>
    public static IAsyncEnumerable<string> IstekGonder(string mesaj)
    {
        var provider = Config.DefaultProvider == "Anthropic" ? ProviderTipi.Anthropic : ProviderTipi.OpenAI;
        return IstekGonder(provider, new List<Mesaj> { new(RolTipi.User, mesaj) });
    }

    /// <summary>Varsayılan sağlayıcıya mesaj geçmişiyle streaming istek gönderir.</summary>
    public static IAsyncEnumerable<string> IstekGonder(List<Mesaj> mesajlar)
    {
        var provider = Config.DefaultProvider == "Anthropic" ? ProviderTipi.Anthropic : ProviderTipi.OpenAI;
        return IstekGonder(provider, mesajlar);
    }

    /// <summary>Belirtilen sağlayıcıya mesaj geçmişiyle streaming istek gönderir.</summary>
    public static IAsyncEnumerable<string> IstekGonder(ProviderTipi provider, List<Mesaj> mesajlar)
    {
        if (mesajlar is null || mesajlar.Count == 0)
            return AkisUretici.TekParca(Hata.Doner("Router", "Mesaj listesi boş."));
        return provider switch
        {
            ProviderTipi.OpenAI => Connection.IstekGonder(Config.OpenAIKey, Config.OpenAIURL, Config.OpenAIModel, mesajlar),
            ProviderTipi.Anthropic => Anthropic.Connection.IstekGonder(Config.AnthropicKey, Config.AnthropicURL, Config.AnthropicModel, mesajlar),
            _ => AkisUretici.TekParca("[HATA] Bilinmeyen sağlayıcı: " + provider),
        };
    }
}
