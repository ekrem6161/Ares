using System.Collections.Generic;

namespace Ares.Core.Sohbet;

/// <summary>
/// Çalışma zamanı sohbet geçmişi: User/Assistant mesajlarını sırayla tutar,
/// Router'a verilecek kopya listeyi üretir. Geçmiş sınırsızdır; kayıt/yükleme
/// (Session) ayrı bir adımdır.
/// </summary>
public sealed class Sohbet
{
    private readonly List<Mesaj> _mesajlar = new();

    public void KullaniciMesajiEkle(string icerik)
    {
        _mesajlar.Add(new Mesaj(RolTipi.User, icerik));
    }

    public AsistanYaniti AsistanYanitiniBaslat()
    {
        return new AsistanYaniti(this);
    }

    /// <summary>API'ye gönderilecek mesajların kopyası (dışarıdan değiştirilemez).</summary>
    public List<Mesaj> Mesajlar()
    {
        return new List<Mesaj>(_mesajlar);
    }

    public void Temizle()
    {
        _mesajlar.Clear();
    }

    internal void AsistanMesajiEkle(string icerik)
    {
        _mesajlar.Add(new Mesaj(RolTipi.Assistant, icerik));
    }
}
