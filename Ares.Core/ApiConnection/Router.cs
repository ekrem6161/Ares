using Ares.Core.OpenAI;

namespace Ares.Core;

public enum ProviderTipi { OpenAI, Anthropic }

public static class Router
{
    public static Task<string> IstekGonder(string mesaj)
    {
        var provider = Config.DefaultProvider == "Anthropic" ? ProviderTipi.Anthropic : ProviderTipi.OpenAI;
        return IstekGonder(provider, new List<Mesaj> { new(RolTipi.User, mesaj) });
    }

    public static Task<string> IstekGonder(ProviderTipi provider, List<Mesaj> mesajlar)
    {
        if (mesajlar is null || mesajlar.Count == 0)
        {
            Console.Error.WriteLine("[Router] Mesaj listesi boş");
            return Task.FromResult("[HATA] Mesaj listesi boş.");
        }
        return provider switch
        {
            ProviderTipi.OpenAI => Connection.IstekGonder(Config.OpenAIKey, Config.OpenAIURL, Config.OpenAIModel, mesajlar),
            ProviderTipi.Anthropic => Anthropic.Connection.IstekGonder(Config.AnthropicKey, Config.AnthropicURL, Config.AnthropicModel, mesajlar),
            _ => Task.FromResult("[HATA] Bilinmeyen sağlayıcı: " + provider),
        };
    }
}
