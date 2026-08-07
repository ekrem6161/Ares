using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class SseOkuyucuTests
{
    private static async Task<List<string>> Oku(string sse)
    {
        using var akim = new MemoryStream(Encoding.UTF8.GetBytes(sse));
        using var okuyucu = new StreamReader(akim, Encoding.UTF8);
        var parcalar = new List<string>();
        await foreach (var parca in SseOkuyucu.DataParcalari(okuyucu))
            parcalar.Add(parca);
        return parcalar;
    }

    [Fact]
    public async Task DataSatirlari_Cikarilir()
    {
        var sonuc = await Oku("data: {\"a\":1}\n\ndata: {\"b\":2}\n\n");
        Assert.Equal(new[] { "{\"a\":1}", "{\"b\":2}" }, sonuc);
    }

    [Fact]
    public async Task Done_Gorulunce_AkisBiter()
    {
        var sonuc = await Oku("data: {\"a\":1}\n\ndata: [DONE]\n\n");
        Assert.Single(sonuc);
    }

    [Fact]
    public async Task DataOlmayanSatirlar_Atlanir()
    {
        var sonuc = await Oku("event: message\ndata: {\"a\":1}\n\n:yorum satiri\n\n");
        Assert.Equal(new[] { "{\"a\":1}" }, sonuc);
    }

    [Fact]
    public async Task BosDataSatiri_Atlanir()
    {
        var sonuc = await Oku("data:\n\ndata: {\"a\":1}\n\n");
        Assert.Single(sonuc);
    }

    [Fact]
    public async Task AkisSonu_BosSonuc()
    {
        var sonuc = await Oku("");
        Assert.Empty(sonuc);
    }
}
