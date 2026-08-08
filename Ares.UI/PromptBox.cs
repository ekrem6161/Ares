using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Ares.UI;

/// <summary>
/// Boşken soluk placeholder gösteren TextField.
///
/// Terminal.Gui v1 TextField'de hazır placeholder özelliği yok.
/// Placeholder'ı input'un kendi <see cref="View.Redraw"/>'ında çiziyoruz —
/// input boşken metni soluk renkte (0,0)'a basar.
/// </summary>
internal sealed class PromptBox : TextField
{
    private readonly string _yerTutucu;
    private readonly Attribute _yerTutucuRengi;

    public PromptBox(string yerTutucu, Attribute yerTutucuRengi)
        : base(string.Empty)
    {
        _yerTutucu = yerTutucu;
        _yerTutucuRengi = yerTutucuRengi;
    }

    public override void Redraw(Rect sinirlar)
    {
        base.Redraw(sinirlar);
        if ((Text is null || Text.IsEmpty) && _yerTutucu.Length > 0)
        {
            Application.Driver.SetAttribute(_yerTutucuRengi);
            Move(0, 0);
            Application.Driver.AddStr(_yerTutucu);
        }
    }
}
