using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// Tüm genişliğini kesintisiz "─" ile dolduran yatay ayraç çizgisi.
/// Terminal.Gui v1'de Label.AutoSize varsayılan true olduğundan Width = Dim.Fill()
/// güvenilmez; kendi Redraw'ını yapan bu view her boyutlanmada güncel genişliği
/// çizer (pencere resize olsa bile tam genişlik korunur).
/// </summary>
public sealed class Ayrac : View
{
    public Ayrac()
    {
        CanFocus = false;
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        if (genislik <= 0 || Bounds.Height <= 0)
            return;
        surucu.SetAttribute(ColorScheme?.Normal ?? surucu.MakeAttribute(Color.DarkGray, Color.Black));
        for (int y = 0; y < Bounds.Height; y++)
        {
            Move(0, y);
            surucu.AddStr(new string('─', genislik));
        }
    }
}
