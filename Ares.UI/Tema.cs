using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Ortak renk şeması yardımcıları. Tüm bileşenler renklerini buradan alır.
/// </summary>
public static class Tema
{
    public static ColorScheme RenkSemasi(Color on, Color arka)
    {
        var ozellik = Application.Driver.MakeAttribute(on, arka);
        return new ColorScheme { Normal = ozellik, Focus = ozellik, HotNormal = ozellik, HotFocus = ozellik };
    }

    public static ColorScheme MetinSemasi(Color on) => RenkSemasi(on, Color.Black);
}
