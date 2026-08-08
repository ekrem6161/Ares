using System;
using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// opencode tarzı braille nokta spinner'ı. 10 çerçeveli klasik dots
/// animasyonu: ⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏. <see cref="Baslat"/> ile başlar,
/// <see cref="Durdur"/> ile durur; animasyon ana döngü zamanlayıcısıyla
/// ilerler (UI thread). Her <see cref="Baslat"/> çerçeveyi baştan alır.
/// </summary>
public sealed class SpinnerGorunumu : View
{
    private const string Cerceveler = "⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏";
    private int _cerceve;
    private object? _zamanlayici;

    public bool Aktif => _zamanlayici is not null;

    public SpinnerGorunumu()
    {
        CanFocus = false;
        Width = 1;
        Height = 1;
    }

    public void Baslat()
    {
        Durdur();
        _cerceve = 0;
        _zamanlayici = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(80), _ =>
        {
            _cerceve = (_cerceve + 1) % Cerceveler.Length;
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
        if (!Aktif || Bounds.Width < 1)
        {
            surucu.SetAttribute(surucu.MakeAttribute(Color.Black, Color.Black));
            Move(0, 0);
            surucu.AddStr(" ");
            return;
        }

        surucu.SetAttribute(surucu.MakeAttribute(Color.Green, Color.Black));
        Move(0, 0);
        surucu.AddStr(Cerceveler[_cerceve].ToString());
    }
}
