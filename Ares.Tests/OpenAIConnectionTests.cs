using System.Collections.Generic;
using System.Threading.Tasks;
using Ares.Core;
using Ares.Core.OpenAI;
using Xunit;

namespace Ares.Tests;

public class OpenAIConnectionTests
{
    private static readonly List<Mesaj> Mesajlar = new() { new(RolTipi.User, "selam") };

    [Fact]
    public async Task IstekGonder_BosAnahtar_AglIsistemiYapilmaz()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(
            Connection.IstekGonder("", "https://example.com/v1/chat/completions", "gpt-4o", Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("Anahtar", sonuc);
    }

    [Fact]
    public async Task IstekGonder_BosURL_HataDoner()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(
            Connection.IstekGonder("sk-test", "", "gpt-4o", Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("URL", sonuc);
    }

    [Fact]
    public async Task IstekGonder_BosModel_HataDoner()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(
            Connection.IstekGonder("sk-test", "https://example.com/v1/chat/completions", "", Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("model", sonuc, System.StringComparison.OrdinalIgnoreCase);
    }
}
