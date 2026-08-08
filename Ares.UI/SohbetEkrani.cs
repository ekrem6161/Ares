using System;
using Ares.Core;
using Ares.Core.Sohbet;
using Ares.UI.Bilesenler;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Sohbet ekranı: üstte logo/versiyon/ayraç, ortada mesaj listesi,
/// altta giriş kutusu ve durum çubuğu. Enter ile gönderim yapılır;
/// yanıt AkisTuketicisi ile parça parça ekrana basılır, akış boyunca
/// giriş kilitlenir. Geçmiş Core'daki Sohbet nesnesinde tutulur ve
/// her istekte API'ye tam geçmiş gider. Ctrl+Q çıkış.
/// </summary>
public sealed class SohbetEkrani : View
{
    public event Action? CikisIstendi;

    private readonly Sohbet _gecmis = new();
    private readonly SohbetGorunumu _sohbet;
    private readonly IstemKutusu _kutu;
    private readonly AltBilgi _altBilgi;
    private readonly SpinnerGorunumu _spinner;

    private const string KisaYollar = "Enter send  Ctrl+Q quit";

    public SohbetEkrani()
    {
        CanFocus = true;

        var logo = new LogoGorunumu { X = 0, Y = 0 };
        var surum = new Label("Ares v0.1")
        {
            X = 14, Y = 0, Height = 1,
            ColorScheme = Tema.MetinSemasi(Color.BrightCyan),
        };
        var ayrac = new Ayrac
        {
            X = 0, Y = 3, Width = Dim.Fill(), Height = 1,
            ColorScheme = Tema.MetinSemasi(Color.DarkGray),
        };

        _sohbet = new SohbetGorunumu
        {
            X = 0, Y = 4, Width = Dim.Fill(), Height = Dim.Fill(4),
        };

        _kutu = new IstemKutusu
        {
            X = 0, Y = Pos.AnchorEnd(4), Width = Dim.Fill(),
            Placeholder = "Type a message and press Enter to chat",
        };

        _altBilgi = new AltBilgi
        {
            X = 0, Y = Pos.AnchorEnd(2), Width = Dim.Fill(), Height = 2,
        };
        _altBilgi.Ayarla(KisaYollar, "", Color.BrightGreen);

        _spinner = new SpinnerGorunumu
        {
            X = 0, Y = Pos.AnchorEnd(1),
        };

        _kutu.MesajGonderildi += MesajGeldi;
        _kutu.IcerikDegisti += YazimDurumu;

        Add(logo, surum, ayrac, _sohbet, _kutu, _altBilgi, _spinner);
        KeyPress += TusBasildi;
    }

    public void OdagiKutuyaVer()
    {
        _kutu.CanFocus = true;
        _kutu.SetFocus();
    }

    private void MesajGeldi(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return;
        _gecmis.KullaniciMesajiEkle(metin);
        _sohbet.Ekle(MesajRol.Kullanici, "You: " + metin);
        _kutu.Icerik = "";
        _kutu.CanFocus = false;
        var yanit = _gecmis.AsistanYanitiniBaslat();
        AkisTuketicisi.Calistir(
            Router.IstekGonder(_gecmis.Mesajlar()),
            parca =>
            {
                yanit.ParcaEkle(parca);
                ParcaGeldi(parca);
            },
            () =>
            {
                yanit.Bitir();
                AkisBitti();
            });
    }

    private void ParcaGeldi(string parca)
    {
        if (parca.StartsWith("[HATA]", StringComparison.Ordinal))
            _sohbet.Ekle(MesajRol.Hata, parca);
        else
            _sohbet.SonunaEkle(parca);
    }

    private void YazimDurumu(string metin)
    {
        if (string.IsNullOrEmpty(metin))
        {
            _spinner.Durdur();
            _altBilgi.Ayarla(KisaYollar, "", Color.BrightGreen);
        }
        else
        {
            _altBilgi.Ayarla("", "", Color.DarkGray);
            _spinner.Baslat();
        }
    }

    private void AkisBitti()
    {
        _spinner.Durdur();
        _altBilgi.Ayarla(KisaYollar, "", Color.BrightGreen);
        OdagiKutuyaVer();
    }

    private void TusBasildi(View.KeyEventEventArgs e)
    {
        if (e.KeyEvent.Key == (Key.Q | Key.CtrlMask))
        {
            e.Handled = true;
            CikisIstendi?.Invoke();
        }
    }
}
