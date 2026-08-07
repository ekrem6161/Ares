using System;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Bordürlü, placeholder destekli tek satır metin girişi (prompt kutusu).
/// Enter ile <see cref="MesajGonderildi"/> tetiklenir.
/// </summary>
public sealed class IstemKutusu : View
{
    private string _icerik = "";
    private string _placeholder = "";
    private int _imlec;

    public string Icerik
    {
        get => _icerik;
        set { _icerik = value; _imlec = _icerik.Length; SetNeedsDisplay(); }
    }

    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value; SetNeedsDisplay(); }
    }

    public event Action<string>? MesajGonderildi;

    public IstemKutusu()
    {
        CanFocus = true;
        Height = 3;
        KeyPress += TusIsle;
    }

    private void TusIsle(View.KeyEventEventArgs e)
    {
        var k = e.KeyEvent.Key;
        if (k == Key.Enter)
        {
            e.Handled = true;
            MesajGonderildi?.Invoke(_icerik);
            return;
        }
        if (k == Key.Backspace)
        {
            if (_imlec > 0)
            {
                _icerik = _icerik.Remove(--_imlec, 1);
                SetNeedsDisplay();
            }
            e.Handled = true;
            return;
        }
        var ch = (char)(k & Key.CharMask);
        if (ch >= 32)
        {
            _icerik = _icerik.Insert(_imlec++, ch.ToString());
            SetNeedsDisplay();
            e.Handled = true;
        }
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        if (genislik <= 0 || Bounds.Height <= 0)
            return;

        var kenar = HasFocus ? Color.BrightCyan : Color.DarkGray;
        surucu.SetAttribute(surucu.MakeAttribute(kenar, Color.Black));
        Move(0, 0);
        surucu.AddStr("┌" + new string('─', Math.Max(0, genislik - 2)) + "┐");
        Move(0, 2);
        surucu.AddStr("└" + new string('─', Math.Max(0, genislik - 2)) + "┘");

        string icerik = _icerik.Length > 0 || !HasFocus ? _icerik : _placeholder;
        var renk = _icerik.Length > 0 ? Color.White : Color.DarkGray;
        surucu.SetAttribute(surucu.MakeAttribute(renk, Color.Black));
        Move(1, 1);
        surucu.AddStr(Kirp(icerik, genislik - 2));
        if (HasFocus)
        {
            Move(1 + Math.Min(_imlec, genislik - 2), 1);
            surucu.SetAttribute(surucu.MakeAttribute(Color.Black, Color.BrightCyan));
            surucu.AddStr(_imlec < _icerik.Length ? _icerik[_imlec].ToString() : " ");
        }
    }

    private static string Kirp(string metin, int en) => metin.Length <= en ? metin : metin[..en];
}
