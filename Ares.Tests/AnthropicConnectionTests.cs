using System.Collections.Generic;
using System.Threading.Tasks;
using Ares.Core;
using Ares.Core.Anthropic;
using Xunit;

namespace Ares.Tests;

public class AnthropicConnectionTests
{
    private static readonly List<Mesaj> Mesajlar = new() { new(RolTipi.User, "selam") };
    private const string TestURL = "https://api.anthropic.com/v1/messages";
    private const string TestModel = "claude-sonnet-4-20250514";

    [Fact]
    public async Task IstekGonder_BosAnahtar_AglIsistemiYapilmaz()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(Connection.IstekGonder("", TestURL, TestModel, Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("Anahtar", sonuc);
    }

    [Fact]
    public async Task IstekGonder_BosURL_HataDoner()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(Connection.IstekGonder("sk-ant-test", "", TestModel, Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("URL", sonuc);
    }

    [Fact]
    public async Task IstekGonder_BosModel_HataDoner()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(Connection.IstekGonder("sk-ant-test", TestURL, "", Mesajlar));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("model", sonuc, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IstekGonder_SadeceSystem_Reddedilir()
    {
        var sonuc = await TestYardimcilari.AkisiTopla(Connection.IstekGonder("sk-ant-test", TestURL, TestModel,
            new List<Mesaj> { new(RolTipi.System, "Sen Ares'sin") }));
        Assert.StartsWith("[HATA]", sonuc);
        Assert.Contains("İçerik", sonuc);
    }

    [Fact]
    public void CevapMetni_ThinkingBloguVarsaTextiBulur()
    {
        var json = "{\"content\":[" +
            "{\"type\":\"thinking\",\"thinking\":\"dusunuyorum...\"}," +
            "{\"type\":\"text\",\"text\":\"Merhaba!\"}]}";
        Assert.Equal("Merhaba!", CevapParser.MetniCikar(json));
    }

    [Fact]
    public void CevapMetni_TextBloguYoksaHataDoner()
    {
        var json = "{\"content\":[{\"type\":\"thinking\",\"thinking\":\"dusunuyorum...\"}]}";
        Assert.StartsWith("[HATA]", CevapParser.MetniCikar(json));
    }
}
