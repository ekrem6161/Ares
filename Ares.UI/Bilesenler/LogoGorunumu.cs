using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// "ARES" box-drawing ASCII logosu (somon/turuncu renk). 13 sütun genişlik, 3 satır.
/// Yeniden kullanılabilir bileşen: hoş geldin ve sohbet ekranlarında kullanılır.
/// </summary>
public sealed class LogoGorunumu : View
{
    private static readonly string[] Satirlar =
    {
        "╔═╗╦═╗╔═╗╔═╗",
        "╠═╣╠╦╝╠═ ╚═╗",
        "╩ ╩╩╚═╚═╝╚═╝",
    };

    public LogoGorunumu()
    {
        CanFocus = false;
        Width = 13;
        Height = Satirlar.Length;
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;
        surucu.SetAttribute(surucu.MakeAttribute(Color.BrightRed, Color.Black));
        for (int i = 0; i < Satirlar.Length; i++)
        {
            Move(0, i);
            surucu.AddStr(Satirlar[i]);
        }
    }
}
