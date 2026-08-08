using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Tüm genişliğini kesintisiz "─" ile dolduran yatay ayraç. Terminal.Gui v1'de <c>Label</c> kullanmak
/// güvenilmez: <c>Label.AutoSize</c> varsayılan <c>true</c> olduğundan <c>Width = Dim.Fill()</c> yok
/// sayılır ve etiket boş metne göre 0 genişliğe çöker. Kendi <see cref="Redraw"/>'unu yapan bu view
/// (ChatView / PromptBox ile aynı desen) AutoSize ve layout-zamanlamasından bağımsızdır. Renk
/// dışarıdan <see cref="View.ColorScheme"/> ile verilir.
/// </summary>
public sealed class HLineView : View
{
    public HLineView()
    {
        CanFocus = false;
    }

    public override void Redraw(Rect sinirlar)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        int yukseklik = Bounds.Height;
        if (genislik <= 0 || yukseklik <= 0)
            return;

        surucu.SetAttribute(ColorScheme?.Normal ?? surucu.MakeAttribute(Color.DarkGray, Color.Black));
        var cizgi = new string('─', genislik);
        for (int satir = 0; satir < yukseklik; satir++)
        {
            Move(0, satir);
            surucu.AddStr(cizgi);
        }
    }
}

/// <summary>Bir sohbet mesajının rolü — render rengini/biçimini belirler.</summary>
public enum ChatRole
{
    Kullanici,   // tam genişlikte vurgulu bant + "> " ön eki
    Asistan,     // "●" renkli nokta + beyaz metin
    Dusunce,     // sönük "✱" + gri (düşünme)
    Sistem,      // sönük gri (araç blokları, komut çıktısı, hata, usage)
    Durum,       // sönük gri "Worked for Ns" (tur sonu kalıcı durum)
}

/// <summary>
/// Claude Code tarzı mesaj-başına renkli sohbet alanı. Terminal.Gui v1'in tek-renkli
/// <c>TextView</c>'i bandı/rol renklerini yapamadığı için kendi <see cref="Redraw"/>'ını yapan
/// özel view.
///
/// Yerleşim (Claude Code analizi): mesajlar **üstten** akar (word-wrap, role göre renk). Canlı
/// durum (<see cref="DurumAyarla"/>) içeriğin ardına değil, view'in **en altına sabit** çizilir
/// (input'un hemen üstü) — çalışırken spinner orada durur, bitince kaldırılır. Fare tekeri ve
/// PageUp/PageDown ile geçmişe kaydırılır. Tüm metot çağrıları UI iş parçacığından yapılmalıdır.
/// </summary>
public sealed class ChatView : View
{
    private sealed class Mesaj
    {
        public ChatRole Rol;
        public string Metin = "";
        public bool Bitti;   // true: tur bitti -> markdown render (bkz. AsistanRenderiniBitir)
    }

    private readonly List<Mesaj> _mesajlar = new();
    private string? _durum;          // dibe sabit canlı durum ("✶ <fiil>…"); null → gizli
    private int _kaydirma;           // dipten yukarı kaydırılan satır sayısı (0 = en altta)
    private int _satirSayisi;        // son render'daki içerik satırı sayısı (kaydırma sınırı)
    private int _icerikYuksekligi;   // son render'daki içerik yüksekliği (durum alanı hariç)

    public ChatView()
    {
        CanFocus = false;
    }

    // ---- İçerik mutasyonları (yeni içerik → dibe yapış) -------------------

    public void Ekle(ChatRole rol, string metin)
    {
        _mesajlar.Add(new Mesaj { Rol = rol, Metin = metin ?? "" });
        DibeYapis();
    }

    public void MesajBaslat(ChatRole rol)
    {
        _mesajlar.Add(new Mesaj { Rol = rol, Metin = "" });
        DibeYapis();
    }

    /// <summary>Son mesaj aynı rolde değilse yeni açar; aynı rolde ise ona ekler (streaming).</summary>
    public void EkleVeyaBaslat(ChatRole rol, string sonEk)
    {
        if (_mesajlar.Count == 0 || _mesajlar[^1].Rol != rol)
            _mesajlar.Add(new Mesaj { Rol = rol, Metin = "" });
        _mesajlar[^1].Metin += sonEk;
        DibeYapis();
    }

    public void Temizle()
    {
        _mesajlar.Clear();
        _durum = null;
        DibeYapis();
    }

    /// <summary>Bu turun bitiminde çağrılır: tamamlanmış asistan mesajlarını markdown render'a
    /// geçirir (streaming sırasında ham metin, tur bitince stilize — Python MarkdownRenderer
    /// davranışının portu). Zararsız/idempotent.</summary>
    public void AsistanRenderiniBitir()
    {
        foreach (var mesaj in _mesajlar)
            if (mesaj.Rol == ChatRole.Asistan)
                mesaj.Bitti = true;
        SetNeedsDisplay();
    }

    /// <summary>Dibe sabit canlı durum satırını ayarlar (boş/null → gizle). Kaydırmayı sıfırlamaz.</summary>
    public void DurumAyarla(string? metin)
    {
        _durum = string.IsNullOrEmpty(metin) ? null : metin;
        SetNeedsDisplay();
    }

    // ---- Kaydırma ---------------------------------------------------------

    private void DibeYapis()
    {
        _kaydirma = 0;
        SetNeedsDisplay();
    }

    /// <summary>Pozitif = yukarı (eskiye), negatif = aşağı (yeniye) kaydırır.</summary>
    public void Kaydir(int delta)
    {
        _kaydirma += delta;
        int maksimum = Math.Max(0, _satirSayisi - _icerikYuksekligi);
        if (_kaydirma > maksimum) _kaydirma = maksimum;
        if (_kaydirma < 0) _kaydirma = 0;
        SetNeedsDisplay();
    }

    public void SayfaKaydir(int yon) => Kaydir(yon * Math.Max(1, _icerikYuksekligi - 2));

    public override bool OnMouseEvent(MouseEvent me)
    {
        if (me.Flags.HasFlag(MouseFlags.WheeledUp)) { Kaydir(3); return true; }
        if (me.Flags.HasFlag(MouseFlags.WheeledDown)) { Kaydir(-3); return true; }
        return base.OnMouseEvent(me);
    }

    // ---- Render -----------------------------------------------------------

    /// <summary>Tek bir ekran satırı. <c>internal</c> — <see cref="MarkdownInline"/> da bu tipte
    /// satır üretir (markdown render, ayrı dosyada).</summary>
    internal sealed class SatirCizimi
    {
        public string Metin = "";
        public Terminal.Gui.Attribute Renk;
        public bool Bant;                  // tüm satırı bant rengiyle doldur (kullanıcı)
        public bool AsistanBasi;           // ilk satırdaki "●" noktası ayrı renklensin
        public Terminal.Gui.Attribute NoktaRengi;
        public List<(string Metin, Terminal.Gui.Attribute Renk)>? Parcalar;  // doluysa Metin/Renk yerine kullanılır (markdown: satır-içi karma renk)
    }

    public override void Redraw(Rect sinirlar)
    {
        var surucu = Application.Driver;
        int genislik = Bounds.Width;
        int yukseklik = Bounds.Height;
        if (genislik <= 0 || yukseklik <= 0)
            return;

        var beyaz     = surucu.MakeAttribute(Color.White,          Color.Black);
        var griKoyu   = surucu.MakeAttribute(Color.DarkGray,       Color.Black);
        var griSoluk  = surucu.MakeAttribute(Color.Gray,           Color.Black);   // durum (sönük, mor değil)
        var kalin     = surucu.MakeAttribute(Color.BrightYellow,   Color.Black);   // markdown **bold** / başlık
        var kodRengi  = surucu.MakeAttribute(Color.BrightCyan,     Color.Black);   // markdown `code` / kod bloğu
        var nokta     = surucu.MakeAttribute(Color.BrightRed,      Color.Black);
        var kullanici = surucu.MakeAttribute(Color.White,          Color.DarkGray);
        var bos       = surucu.MakeAttribute(Color.White,          Color.Black);
        var palet = new MarkdownInline.MdPalette(beyaz, kalin, kodRengi, griSoluk, griKoyu);

        // İçerik satırları (mesajlar, üstten akar).
        var satirlar = new List<SatirCizimi>();
        for (int i = 0; i < _mesajlar.Count; i++)
        {
            if (i > 0)
                satirlar.Add(new SatirCizimi { Metin = "", Renk = bos });

            var mesaj = _mesajlar[i];
            switch (mesaj.Rol)
            {
                case ChatRole.Kullanici:
                    Sarmala(satirlar, mesaj.Metin, genislik, "> ", "  ", kullanici, bant: true);
                    break;
                case ChatRole.Asistan:
                    if (mesaj.Bitti)
                        MarkdownInline.MarkdownEkle(satirlar, mesaj.Metin, genislik, "● ", "  ", palet, asistanBasi: true, noktaRengi: nokta);
                    else
                        Sarmala(satirlar, mesaj.Metin, genislik, "● ", "  ", beyaz, bant: false, asistanBasi: true, noktaRengi: nokta);
                    break;
                case ChatRole.Dusunce:
                    Sarmala(satirlar, mesaj.Metin, genislik, "✱ ", "  ", griKoyu, bant: false);
                    break;
                case ChatRole.Durum:
                    Sarmala(satirlar, mesaj.Metin, genislik, "", "  ", griSoluk, bant: false);
                    break;
                default: // Sistem
                    Sarmala(satirlar, mesaj.Metin, genislik, "", "", griKoyu, bant: false);
                    break;
            }
        }

        // Canlı durum aktifse en altta 2 satır (boşluk + durum) ayır.
        int ayrilan = _durum != null ? 2 : 0;
        int icerikYuksekligi = Math.Max(1, yukseklik - ayrilan);

        _satirSayisi = satirlar.Count;
        _icerikYuksekligi = icerikYuksekligi;
        int maksimumKaydirma = Math.Max(0, satirlar.Count - icerikYuksekligi);
        if (_kaydirma > maksimumKaydirma) _kaydirma = maksimumKaydirma;
        if (_kaydirma < 0) _kaydirma = 0;

        // Tüm alanı temizle (siyah).
        surucu.SetAttribute(bos);
        for (int y = 0; y < yukseklik; y++)
        {
            Move(0, y);
            surucu.AddStr(new string(' ', genislik));
        }

        // İçerik penceresi: dipten _kaydirma kadar yukarı, son icerikYuksekligi satır.
        int ilk = Math.Max(0, satirlar.Count - icerikYuksekligi - _kaydirma);
        int son = Math.Min(satirlar.Count, ilk + icerikYuksekligi);
        for (int i = ilk; i < son; i++)
        {
            int y = i - ilk;
            SatiriCiz(surucu, satirlar[i], y, genislik);
        }

        // Dibe sabit canlı durum (input'un hemen üstü).
        if (_durum != null)
        {
            SatiriCiz(surucu, new SatirCizimi { Metin = _durum, Renk = griSoluk }, yukseklik - 1, genislik);
        }

        // Yukarı kaydırıldıysa sağ üstte gösterge.
        if (_kaydirma > 0 && genislik >= 8)
        {
            const string gosterge = "↑ more";
            surucu.SetAttribute(griSoluk);
            Move(genislik - gosterge.Length, 0);
            surucu.AddStr(gosterge);
        }
    }

    private void SatiriCiz(ConsoleDriver surucu, SatirCizimi satir, int y, int genislik)
    {
        if (satir.Bant)
        {
            surucu.SetAttribute(satir.Renk);
            Move(0, y);
            surucu.AddStr(new string(' ', genislik));
        }

        if (satir.Parcalar != null)
        {
            // Markdown render: satır-içi karma renk — her parça kendi rengiyle ardı ardına çizilir.
            int x = 0;
            foreach (var (parcaMetin, parcaRenk) in satir.Parcalar)
            {
                if (x >= genislik) break;
                string kirpilmis = Kirp(parcaMetin, genislik - x);
                surucu.SetAttribute(parcaRenk);
                Move(x, y);
                surucu.AddStr(kirpilmis);
                x += kirpilmis.Length;
            }
        }
        else
        {
            surucu.SetAttribute(satir.Renk);
            Move(0, y);
            surucu.AddStr(Kirp(satir.Metin, genislik));
        }

        if (satir.AsistanBasi && (satir.Metin.Length > 0 || satir.Parcalar is { Count: > 0 }))
        {
            surucu.SetAttribute(satir.NoktaRengi);
            Move(0, y);
            surucu.AddStr("●");
        }
    }

    /// <summary>Bir mesajı sarmalayıp (word-wrap) satırlara böler. İlk satıra <paramref name="onEk"/>,
    /// devam satırlarına <paramref name="devamGirintisi"/> eklenir. Açık satır sonları (\n) korunur.</summary>
    private static void Sarmala(
        List<SatirCizimi> cikti, string metin, int genislik, string onEk, string devamGirintisi,
        Terminal.Gui.Attribute renk, bool bant, bool asistanBasi = false,
        Terminal.Gui.Attribute noktaRengi = default)
    {
        if (genislik < 1) genislik = 1;
        bool baslikKondu = false;   // gerçek içerikli ilk satıra nokta kondu mu

        foreach (var sertSatir in (metin ?? "").Replace("\r", "").Split('\n'))
        {
            var kelimeler = sertSatir.Split(' ');
            var guncel = "";
            int kullanilabilir = Math.Max(1, genislik - onEk.Length);

            void Bosalt()
            {
                bool noktaKoy = !baslikKondu && guncel.Length > 0;
                string girinti = noktaKoy ? onEk : devamGirintisi;
                cikti.Add(new SatirCizimi
                {
                    Metin = girinti + guncel,
                    Renk = renk,
                    Bant = bant,
                    AsistanBasi = asistanBasi && noktaKoy,
                    NoktaRengi = noktaRengi,
                });
                if (noktaKoy) baslikKondu = true;
                guncel = "";
                kullanilabilir = Math.Max(1, genislik - devamGirintisi.Length);
            }

            foreach (var kelime in kelimeler)
            {
                if (guncel.Length == 0)
                    guncel = kelime;
                else if (guncel.Length + 1 + kelime.Length <= kullanilabilir)
                    guncel += " " + kelime;
                else
                {
                    Bosalt();
                    guncel = kelime;
                }
            }
            Bosalt();
        }

        // Mesaj tamamen boşsa (ör. streaming henüz başlamadıysa) yine de ilk satıra nokta koy.
        if (!baslikKondu && asistanBasi && cikti.Count > 0)
        {
            cikti[0].AsistanBasi = true;
            cikti[0].NoktaRengi = noktaRengi;
            cikti[0].Metin = onEk + cikti[0].Metin.TrimStart();
        }
    }

    private static string Kirp(string metin, int en) => metin.Length <= en ? metin : metin.Substring(0, en);
}
