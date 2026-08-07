using Terminal.Gui;
using Ares.UI.Bilesenler;

namespace Ares.UI;

/// <summary>
/// Hoş geldin ekranı (welcome screen). Sadece görsel: logo, başlık, açıklama,
/// kısayol listesi ve alt bilgi çubuğunu dizer. Fonksiyonellik bu adımda yok.
/// Ctrl+Q basılınca <see cref="CikisIstendi"/> tetiklenir.
/// </summary>
public sealed class HosGeldinEkrani : View
{
    public event Action? CikisIstendi;

    public HosGeldinEkrani()
    {
        CanFocus = true;
        BilesenleriKur();
        KeyPress += TusBasildi;
    }

    private void BilesenleriKur()
    {
        var logo = new LogoGorunumu { X = 0, Y = 0 };

        var surum = new Label("Ares v0.1")
        {
            X = 14, Y = 0, Height = 1,
            ColorScheme = Tema.MetinSemasi(Color.BrightCyan),
        };
        var tanim = new Label("terminal AI assistant")
        {
            X = 14, Y = 1, Height = 1,
            ColorScheme = Tema.MetinSemasi(Color.Gray),
        };

        var baslik = new Label("Welcome to Ares")
        {
            X = 0, Y = 5, Width = Dim.Fill(), Height = 1,
            AutoSize = false,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = Tema.MetinSemasi(Color.BrightCyan),
        };
        var aciklama = new Label("Type a message and press Enter to chat.\n(Chat screen comes in the next step.)")
        {
            X = 0, Y = 7, Width = Dim.Fill(), Height = 2,
            AutoSize = false,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = Tema.MetinSemasi(Color.Gray),
        };
        var kisaYol = new Label("Shortcuts:\n  Ctrl+Q   quit")
        {
            X = 0, Y = 10, Width = Dim.Fill(), Height = 2,
            AutoSize = false,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = Tema.MetinSemasi(Color.DarkGray),
        };

        var altBilgi = new AltBilgi
        {
            X = 0, Y = Pos.AnchorEnd(2), Width = Dim.Fill(), Height = 2,
        };
        altBilgi.Ayarla("Ctrl+Q quit", "ready", Color.BrightGreen);

        Add(logo, surum, tanim, baslik, aciklama, kisaYol, altBilgi);
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
