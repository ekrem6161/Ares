using System;
using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// LED chaser dalga animasyonu. 10 kutu karakter üzerinde çift yönlü
/// tarama efekti. <see cref="Baslat"/> ile başlar, <see cref="Durdur"/>
/// ile durur; animasyon ana döngü zamanlayıcısıyla ilerler (UI thread).
/// </summary>
public sealed class SpinnerGorunumu : View
{
    private const int KutuSayisi = 10;
    private int _pozisyon;
    private int _yon = 1;
    private object? _zamanlayici;

    public bool Aktif => _zamanlayici is not null;

    public SpinnerGorunumu()
    {
        CanFocus = false;
        Width = KutuSayisi * 3;
        Height = 1;
    }

    public void Baslat()
    {
        Durdur();
        _zamanlayici = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), _ =>
        {
            _pozisyon += _yon;
            if (_pozisyon >= KutuSayisi - 1)
                _yon = -1;
            else if (_pozisyon <= 0)
                _yon = 1;
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
        if (Bounds.Width < KutuSayisi * 3)
            return;

        for (int i = 0; i < KutuSayisi; i++)
        {
            bool aktif = i == _pozisyon;
            var renk = aktif ? Color.BrightCyan : Color.DarkGray;
            surucu.SetAttribute(surucu.MakeAttribute(renk, Color.Black));
            Move(i * 3, 0);
            surucu.AddStr(aktif ? "[■]" : "[□]");
        }
    }
}
