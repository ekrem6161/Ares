using System.Collections.Generic;
using System.Threading.Tasks;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class RouterTests : ConfigTestBase
{
    [Fact]
    public async Task IstekGonder_BosMesajListesi_HataDoner()
    {
        var sonuc = await Router.IstekGonder(ProviderTipi.OpenAI, new List<Mesaj>());
        Assert.StartsWith("[HATA]", sonuc);
    }

    [Fact]
    public async Task IstekGonder_NullMesajListesi_CrashEtmez()
    {
        var sonuc = await Router.IstekGonder(ProviderTipi.OpenAI, null!);
        Assert.StartsWith("[HATA]", sonuc);
    }

    [Fact]
    public async Task IstekGonder_BilinmeyenProvider_HataDoner()
    {
        var sonuc = await Router.IstekGonder((ProviderTipi)999, TekMesaj());
        Assert.StartsWith("[HATA] Bilinmeyen sağlayıcı", sonuc);
    }

    [Fact]
    public async Task IstekGonder_VarsayilanAnthropic_AnthropicKoluDoner()
    {
        Config.DefaultProvider = "Anthropic";
        var sonuc = await Router.IstekGonder("selam");
        Assert.StartsWith("[HATA] Anthropic", sonuc);
        Assert.Contains("Anahtar", sonuc);
    }

    private static List<Mesaj> TekMesaj() => new() { new(RolTipi.User, "selam") };
}
