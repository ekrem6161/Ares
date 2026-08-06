using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

[Trait("Kategori", "Canli")]
public class CanliApiTests
{
    private static bool CanliAktif() =>
        Environment.GetEnvironmentVariable("ARES_CANLI") == "1";

    private static async Task<string> Iste(ProviderTipi provider)
    {
        Assert.True(Config.Yukle(), "Config yuklenemedi (ARES_CANLI=1 ile calistirildi mi?)");
        return await Router.IstekGonder(provider, new List<Mesaj>
        {
            new(RolTipi.User, "Hangi model oldugunu tek kelimeyle soyle."),
        });
    }

    [Fact]
    public async Task OpenAI_DeepSeek_CanliCevapDoner()
    {
        if (!CanliAktif())
            return;
        var cevap = await Iste(ProviderTipi.OpenAI);
        Assert.False(string.IsNullOrWhiteSpace(cevap));
        Assert.False(cevap.StartsWith("[HATA]", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Anthropic_Haiku_CanliCevapDoner()
    {
        if (!CanliAktif())
            return;
        var cevap = await Iste(ProviderTipi.Anthropic);
        Assert.False(string.IsNullOrWhiteSpace(cevap));
        Assert.False(cevap.StartsWith("[HATA]", StringComparison.Ordinal));
    }
}
