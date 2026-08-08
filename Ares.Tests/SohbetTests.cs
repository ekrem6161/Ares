using System.Collections.Generic;
using Ares.Core;
using Ares.Core.Sohbet;
using Xunit;

namespace Ares.Tests;

public class SohbetTests
{
    [Fact]
    public void KullaniciMesajiEkle_SiraliEkler()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("ilk");
        sohbet.KullaniciMesajiEkle("ikinci");

        var mesajlar = sohbet.Mesajlar();
        Assert.Equal(2, mesajlar.Count);
        Assert.Equal(RolTipi.User, mesajlar[0].Rol);
        Assert.Equal("ilk", mesajlar[0].Icerik);
        Assert.Equal("ikinci", mesajlar[1].Icerik);
    }

    [Fact]
    public void AsistanYaniti_Bitir_OnceYazilmazSonraYazilir()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("selam");
        var yanit = sohbet.AsistanYanitiniBaslat();

        Assert.Single(sohbet.Mesajlar());

        yanit.ParcaEkle("Mer");
        yanit.ParcaEkle("haba");
        var yazildi = yanit.Bitir();

        Assert.True(yazildi);
        var mesajlar = sohbet.Mesajlar();
        Assert.Equal(2, mesajlar.Count);
        Assert.Equal(RolTipi.Assistant, mesajlar[1].Rol);
        Assert.Equal("Merhaba", mesajlar[1].Icerik);
    }

    [Fact]
    public void AsistanYaniti_HataParcasi_GecmiseYazilmaz()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("selam");
        var yanit = sohbet.AsistanYanitiniBaslat();

        yanit.ParcaEkle("[HATA] OpenAI isteği başarısız: HTTP 401");
        var yazildi = yanit.Bitir();

        Assert.False(yazildi);
        Assert.Single(sohbet.Mesajlar());
    }

    [Fact]
    public void AsistanYaniti_BosBirikim_Yazmaz()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("selam");
        var yanit = sohbet.AsistanYanitiniBaslat();

        Assert.False(yanit.Bitir());
        Assert.Single(sohbet.Mesajlar());
    }

    [Fact]
    public void Mesajlar_KopyaDondurur()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("selam");

        var kopya = sohbet.Mesajlar();
        kopya.Add(new Mesaj(RolTipi.User, "dışarıdan"));

        Assert.Single(sohbet.Mesajlar());
    }

    [Fact]
    public void Temizle_Bosaltir()
    {
        var sohbet = new Sohbet();
        sohbet.KullaniciMesajiEkle("selam");
        sohbet.KullaniciMesajiEkle("selam 2");

        sohbet.Temizle();

        Assert.Empty(sohbet.Mesajlar());
    }
}
