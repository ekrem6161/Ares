using System;
using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// Knight Rider tarayıcı animasyonu (blocks stili): 8 hücre x 2
/// kolon genişlik, çift yönlü tarama, uçlarda bekleme (30/9), arkada
/// gradyan kuyruk. Aktif ■■ parlak kırmızı, kuyruk soluklaşır,
/// pasif ·· koyu. Glifler cmd uyumlu (braille cmd'de render
/// edilmez). Her Baslat çerçeveyi baştan alır.
/// </summary>
public sealed class SpinnerGorunumu : View
{
    private const int Hucresayisi = 8;
    private const int Ileri = Hucresayisi;
    private const int Geri = Hucresayisi - 1;
    private const int BekleBas = 30;
    private const int BekleSon = 9;
    private const int Toplam = Ileri + BekleSon + Geri + BekleBas;
    private const int HucreGenisligi = 2;

    private int _cerceve;
    private object? _zamanlayici;

    public bool Aktif => _zamanlayici is not null;

    public SpinnerGorunumu()
    {
        CanFocus = false;
        Width = Hucresayisi * HucreGenisligi;
        Height = 1;
    }

    public void Baslat()
    {
        Durdur();
        _cerceve = 0;
        _zamanlayici = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(80), _ =>
        {
            _cerceve = (_cerceve + 1) % Toplam;
            SetNeedsDisplay();
            return true;
        });
    }

    public void Durdur()
    {
        if (_zamanlayici is null)
            return;
        Application.MainLoop.RemoveTimeout(_zamanlayici);
        _zamanlayici = null;
        SetNeedsDisplay();
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        if (!Aktif || Bounds.Width < Hucresayisi * HucreGenisligi)
        {
            surucu.SetAttribute(surucu.MakeAttribute(Color.Black, Color.Black));
            Move(0, 0);
            surucu.AddStr(new string(' ', Bounds.Width));
            return;
        }

        for (int i = 0; i < Hucresayisi; i++)
        {
            int indeks = RenkIndeksi(i);
            string glif = indeks < 0 ? "··" : "■■";
            var renk = indeks switch
            {
                0 => Color.BrightRed,
                1 => Color.Red,
                2 => Color.Brown,
                _ => Color.DarkGray,
            };
            surucu.SetAttribute(surucu.MakeAttribute(renk, Color.Black));
            Move(i * HucreGenisligi, 0);
            surucu.AddStr(glif);
        }
    }

    private int RenkIndeksi(int i)
    {
        bool hareket;
        bool ileri;
        int konum;
        int fade;
        int f = _cerceve;

        if (f < Ileri)
        {
            hareket = true; ileri = true; konum = f; fade = 0;
        }
        else if (f < Ileri + BekleSon)
        {
            hareket = false; ileri = true; konum = Hucresayisi - 1; fade = f - Ileri;
        }
        else if (f < Ileri + BekleSon + Geri)
        {
            hareket = true; ileri = false; konum = Hucresayisi - 2 - (f - Ileri - BekleSon); fade = 0;
        }
        else
        {
            hareket = false; ileri = false; konum = 0; fade = f - Ileri - BekleSon - Geri;
        }

        int mesafe = ileri ? konum - i : i - konum;
        if (!hareket && mesafe < 0)
            return -1;
        int indeks = hareket ? mesafe : mesafe + fade;
        if (indeks >= 3)
            return -1;
        return indeks < 0 ? -1 : indeks;
    }
}
