using Terminal.Gui;

namespace Ares.UI.Bilesenler;

/// <summary>
/// Alt durum çubuğu: üst kenarda ayraç çizgisi, altında solda kısayol metni,
/// sağda durum metni (sağa yaslı). 2 satır yükseklik için tasarlanmıştır.
/// Yeniden kullanılabilir bileşen: tüm ekranların altında kullanılır.
/// </summary>
public sealed class AltBilgi : View
{
    private string _solMetin = "";
    private string _sagMetin = "";
    private Color _sagRenk = Color.BrightGreen;

    public AltBilgi()
    {
        CanFocus = false;
    }

    public void Ayarla(string sol, string sag, Color sagRenk)
    {
        _solMetin = sol;
        _sagMetin = sag;
        _sagRenk = sagRenk;
        SetNeedsDisplay();
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        if (genislik <= 0 || Bounds.Height <= 0)
            return;

        surucu.SetAttribute(surucu.MakeAttribute(Color.DarkGray, Color.Black));
        Move(0, 0);
        surucu.AddStr(new string('─', genislik));

        Move(0, 1);
        surucu.AddStr(Kirp(_solMetin, genislik));

        surucu.SetAttribute(surucu.MakeAttribute(_sagRenk, Color.Black));
        int sagBaslangic = Math.Max(0, genislik - _sagMetin.Length);
        Move(sagBaslangic, 1);
        surucu.AddStr(Kirp(_sagMetin, genislik - sagBaslangic));
    }

    private static string Kirp(string metin, int en)
    {
        if (metin.Length <= en)
            return metin;
        return metin.Substring(0, Math.Max(0, en));
    }
}
