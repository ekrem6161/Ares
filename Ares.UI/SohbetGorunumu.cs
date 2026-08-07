using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace Ares.UI;

public enum MesajRol { Kullanici, Asistan, Hata }

/// <summary>
/// Sohbet mesaj listesi: kelime sarmalama, tekerlek/PgUp/PgDn/End ile kaydırma
/// ve role göre renklendirme yapan özel görünüm. Yeni mesajda en alta kayar.
/// </summary>
public sealed class SohbetGorunumu : View
{
    private readonly List<(MesajRol Rol, string Satir)> _satirlar = new();
    private int _kaydirma;

    public SohbetGorunumu()
    {
        CanFocus = true;
    }

    public void Ekle(MesajRol rol, string metin)
    {
        foreach (var satir in Sar(metin))
            _satirlar.Add((rol, satir));
        _kaydirma = 0;
        SetNeedsDisplay();
    }

    public void SonunaEkle(string parca)
    {
        if (_satirlar.Count == 0)
        {
            Ekle(MesajRol.Asistan, parca);
            return;
        }
        int son = _satirlar.Count - 1;
        var (rol, satir) = _satirlar[son];
        _satirlar.RemoveAt(son);
        foreach (var s in Sar(satir + parca))
            _satirlar.Add((rol, s));
        _kaydirma = 0;
        SetNeedsDisplay();
    }

    public void Temizle()
    {
        _satirlar.Clear();
        _kaydirma = 0;
        SetNeedsDisplay();
    }

    public override bool MouseEvent(MouseEvent mouseEvent)
    {
        if (mouseEvent.Flags.HasFlag(MouseFlags.WheeledUp))
        {
            Kaydir(1);
            return true;
        }
        if (mouseEvent.Flags.HasFlag(MouseFlags.WheeledDown))
        {
            Kaydir(-1);
            return true;
        }
        return base.MouseEvent(mouseEvent);
    }

    public override bool ProcessKey(KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.End:
                Kaydir(int.MinValue);
                return true;
            case Key.PageUp:
                Kaydir(Math.Max(1, Bounds.Height));
                return true;
            case Key.PageDown:
                Kaydir(-Math.Max(1, Bounds.Height));
                return true;
        }
        return base.ProcessKey(keyEvent);
    }

    private void Kaydir(int yon)
    {
        int maksimum = Math.Max(0, _satirlar.Count - Math.Max(1, Bounds.Height));
        if (yon == int.MinValue)
            _kaydirma = 0;
        else
            _kaydirma = Math.Clamp(_kaydirma + yon, 0, maksimum);
        SetNeedsDisplay();
    }

    private IEnumerable<string> Sar(string metin)
    {
        int genislik = Math.Max(1, Bounds.Width);
        if (metin.Length <= genislik)
        {
            yield return metin;
            yield break;
        }
        while (metin.Length > genislik)
        {
            int kesim = metin.LastIndexOf(' ', genislik - 1);
            if (kesim <= 0)
                kesim = genislik - 1;
            yield return metin.Substring(0, kesim);
            metin = metin.Substring(kesim).TrimStart();
        }
        yield return metin;
    }

    public override void Redraw(Rect bounds)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        int yukseklik = Bounds.Height;
        if (genislik <= 0 || yukseklik <= 0)
            return;

        int gorunur = Math.Min(yukseklik, _satirlar.Count);
        int bas = Math.Max(0, _satirlar.Count - gorunur - _kaydirma);
        for (int i = 0; i < yukseklik; i++)
        {
            int idx = bas + i;
            if (idx >= _satirlar.Count)
            {
                surucu.SetAttribute(surucu.MakeAttribute(Color.DarkGray, Color.Black));
                Move(0, i);
                surucu.AddStr(new string(' ', genislik));
                continue;
            }
            var (rol, satir) = _satirlar[idx];
            var renk = rol switch
            {
                MesajRol.Kullanici => Color.BrightCyan,
                MesajRol.Hata => Color.BrightRed,
                _ => Color.White,
            };
            surucu.SetAttribute(surucu.MakeAttribute(renk, Color.Black));
            Move(0, i);
            surucu.AddStr(Kirp(satir, genislik));
        }
    }

    private static string Kirp(string metin, int en) => metin.Length <= en ? metin : metin[..en];
}
