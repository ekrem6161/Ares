using System.Text.RegularExpressions;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Asistan mesajları için hafif markdown render — sadece <see cref="ChatView"/>'in özel
/// <c>Redraw</c> mimarisine (satır-içi karma renk, <see cref="ChatView.SatirCizimi.Parcalar"/>) hizmet
/// eder. Python referansı (Trashcode/aresv1/ui/markdown/markdownrenderer.py) Rich'in <c>Markdown</c>
/// sınıfına sarılıyordu; Terminal.Gui'de hazır eşdeğeri yok, bu yüzden davranış (stream sırasında
/// ham metin, tur bitince stilize render) burada kendi hafif parser'ımızla yeniden inşa edildi.
/// Kapsam: **bold**, `inline code`, ```fenced code```, #/##/### başlık, -/* madde listesi,
/// 1. numaralı liste, &gt; alıntı, --- yatay çizgi. Tablo/link kapsam dışı (v1).
/// Bağımlılık disiplini (plan.md): 3rd-party markdown kütüphanesi YOK, sadece BCL (Regex).
/// Dosya boyutu kuralı (plan.md SABİT KURAL): ChatView.cs'i 400 satırın altında tutmak için bu
/// sorumluluk ayrı dosyaya bölündü.
/// </summary>
public static class MarkdownInline
{
    /// <summary>Markdown render renk paleti — tek struct, uzun parametre listesi yerine.</summary>
    public readonly record struct MdPalette(
        Terminal.Gui.Attribute Taban,
        Terminal.Gui.Attribute Kalin,
        Terminal.Gui.Attribute Kod,
        Terminal.Gui.Attribute Soluk,
        Terminal.Gui.Attribute Gri);

    private readonly record struct MantikSatiri(
        List<(string Metin, Terminal.Gui.Attribute Renk)> Parcalar, bool AyracMi);

    private static readonly Regex BaslikDeseni = new(@"^(#{1,6})\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex MaddeDeseni = new(@"^(\s*)[-*]\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex NumaraliDesen = new(@"^(\s*)(\d+\.)\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex AlintiDeseni = new(@"^\s*>\s?(.*)$", RegexOptions.Compiled);

    /// <summary>Bir asistan mesajını markdown olarak ayrıştırıp sarmalayıp <paramref name="cikti"/>'ya
    /// ekler. Dış prefix mantığı <see cref="ChatView"/>'in <c>Sarmala</c>'sı ile aynıdır: <paramref
    /// name="onEk"/> ("● ") sadece mesajın ilk satırına, sonrakilere <paramref name="devamGirintisi"/>
    /// ("  ") eklenir.</summary>
    internal static void MarkdownEkle(
        List<ChatView.SatirCizimi> cikti, string metin, int genislik,
        string onEk, string devamGirintisi, MdPalette palet,
        bool asistanBasi, Terminal.Gui.Attribute noktaRengi)
    {
        if (genislik < 1) genislik = 1;
        bool baslikKondu = false;   // dot gerçek içerikli ilk satıra kondu mu

        foreach (var mantik in BloklariAyristir(metin, palet))
        {
            if (mantik.AyracMi)
            {
                cikti.Add(new ChatView.SatirCizimi
                {
                    Parcalar = new List<(string, Terminal.Gui.Attribute)> { (new string('─', genislik), palet.Soluk) },
                });
                continue;   // baslikKondu'ya dokunma — hr asla dot taşımaz
            }

            MantikSatiriniSar(cikti, mantik.Parcalar, genislik, onEk, devamGirintisi, palet.Taban,
                asistanBasi, noktaRengi, ref baslikKondu);
        }
    }

    /// <summary>Tek mantıksal satırı (zaten satır-içi renklenmiş "parçalar") kelime-kelime sarar; her
    /// kelimenin rengi korunur (<see cref="ChatView.SatirCizimi.Parcalar"/>).</summary>
    private static void MantikSatiriniSar(
        List<ChatView.SatirCizimi> cikti, List<(string Metin, Terminal.Gui.Attribute Renk)> parcalar,
        int genislik, string onEk, string devamGirintisi, Terminal.Gui.Attribute girintiRengi,
        bool asistanBasi, Terminal.Gui.Attribute noktaRengi, ref bool baslikKondu)
    {
        var simgeler = new List<(string Kelime, Terminal.Gui.Attribute Renk)>();
        foreach (var (parcaMetin, parcaRenk) in parcalar)
            foreach (var kelime in parcaMetin.Split(' '))
                if (kelime.Length > 0)
                    simgeler.Add((kelime, parcaRenk));
        if (simgeler.Count == 0)
            simgeler.Add(("", parcalar.Count > 0 ? parcalar[0].Renk : girintiRengi));

        bool yerelBaslik = baslikKondu; // ref lambda'da kullanılamaz -> yerel kopya
        bool satirinIlkSarmasi = true;
        var guncelParcalar = new List<(string, Terminal.Gui.Attribute)>();
        int guncelUzunluk = 0;
        int kullanilabilir = Math.Max(1, genislik - (yerelBaslik ? devamGirintisi.Length : onEk.Length));

        void Bosalt()
        {
            bool noktaKoy = !yerelBaslik && satirinIlkSarmasi && guncelUzunluk > 0;
            string girinti = noktaKoy ? onEk : devamGirintisi;
            var parcalarYeni = new List<(string, Terminal.Gui.Attribute)>();
            if (girinti.Length > 0)
                parcalarYeni.Add((girinti, girintiRengi));
            parcalarYeni.AddRange(guncelParcalar);
            cikti.Add(new ChatView.SatirCizimi
            {
                Parcalar = parcalarYeni,
                AsistanBasi = asistanBasi && noktaKoy,
                NoktaRengi = noktaRengi,
            });
            if (noktaKoy) yerelBaslik = true;
            satirinIlkSarmasi = false;
            guncelParcalar = new List<(string, Terminal.Gui.Attribute)>();
            guncelUzunluk = 0;
            kullanilabilir = Math.Max(1, genislik - devamGirintisi.Length);
        }

        foreach (var (kelime, renk) in simgeler)
        {
            if (guncelUzunluk == 0)
            {
                guncelParcalar.Add((kelime, renk));
                guncelUzunluk = kelime.Length;
            }
            else if (guncelUzunluk + 1 + kelime.Length <= kullanilabilir)
            {
                guncelParcalar.Add((" " + kelime, renk));
                guncelUzunluk += 1 + kelime.Length;
            }
            else
            {
                Bosalt();
                guncelParcalar.Add((kelime, renk));
                guncelUzunluk = kelime.Length;
            }
        }
        Bosalt();
        baslikKondu = yerelBaslik; // ref parametreye geri yaz
    }

    /// <summary>Ham metni blok-seviyesi markdown'a göre mantıksal satırlara böler (başlık, liste,
    /// alıntı, kod bloğu, yatay çizgi, düz paragraf). Her satır <see cref="SatirIciAyristir"/> ile
    /// satır-içi (bold/code) ayrıştırılır — kod bloğu içi hariç (literal kalır).</summary>
    private static List<MantikSatiri> BloklariAyristir(string tamMetin, MdPalette palet)
    {
        var sonuc = new List<MantikSatiri>();
        bool kodIci = false;

        foreach (var ham in (tamMetin ?? "").Replace("\r", "").Split('\n'))
        {
            var temiz = ham.TrimStart();

            if (temiz.StartsWith("```"))
            {
                kodIci = !kodIci;
                continue;   // çevreleme satırı görünmez
            }

            if (kodIci)
            {
                int girintiUzunlugu = ham.Length - ham.TrimStart(' ').Length;
                var parcalar = new List<(string, Terminal.Gui.Attribute)>();
                if (girintiUzunlugu > 0)
                    parcalar.Add((ham.Substring(0, girintiUzunlugu), palet.Kod));
                var kalan = ham.Substring(girintiUzunlugu);
                parcalar.Add((kalan, palet.Kod));
                sonuc.Add(new MantikSatiri(parcalar, false));
                continue;
            }

            if (temiz.Length == 0)
            {
                sonuc.Add(new MantikSatiri(new List<(string, Terminal.Gui.Attribute)> { ("", palet.Taban) }, false));
                continue;
            }

            if (YatayAyracMi(temiz))
            {
                sonuc.Add(new MantikSatiri(new(), true));
                continue;
            }

            var baslik = BaslikDeseni.Match(temiz);
            if (baslik.Success)
            {
                sonuc.Add(new MantikSatiri(
                    new List<(string, Terminal.Gui.Attribute)> { (baslik.Groups[2].Value, palet.Kalin) }, false));
                continue;
            }

            var madde = MaddeDeseni.Match(ham);
            if (madde.Success)
            {
                var parcalar = new List<(string, Terminal.Gui.Attribute)> { (madde.Groups[1].Value + "• ", palet.Gri) };
                parcalar.AddRange(SatirIciAyristir(madde.Groups[2].Value, palet));
                sonuc.Add(new MantikSatiri(parcalar, false));
                continue;
            }

            var numarali = NumaraliDesen.Match(ham);
            if (numarali.Success)
            {
                var parcalar = new List<(string, Terminal.Gui.Attribute)>
                {
                    (numarali.Groups[1].Value + numarali.Groups[2].Value + " ", palet.Gri),
                };
                parcalar.AddRange(SatirIciAyristir(numarali.Groups[3].Value, palet));
                sonuc.Add(new MantikSatiri(parcalar, false));
                continue;
            }

            var alinti = AlintiDeseni.Match(ham);
            if (alinti.Success)
            {
                var parcalar = new List<(string, Terminal.Gui.Attribute)> { ("▏ ", palet.Gri) };
                parcalar.AddRange(SatirIciAyristir(alinti.Groups[1].Value, palet));
                sonuc.Add(new MantikSatiri(parcalar, false));
                continue;
            }

            sonuc.Add(new MantikSatiri(SatirIciAyristir(ham, palet), false));
        }

        return sonuc;
    }

    private static bool YatayAyracMi(string temiz)
    {
        if (temiz.Length < 3) return false;
        char karakter = temiz[0];
        if (karakter != '-' && karakter != '*' && karakter != '_') return false;
        foreach (var ch in temiz)
            if (ch != karakter) return false;
        return true;
    }

    /// <summary>Tek satır içindeki <c>**bold**</c> ve <c>`code`</c> işaretlerini renkli parçalara
    /// böler. Kapanmayan işaret literal metin olarak kalır (asla patlamaz).</summary>
    private static List<(string Metin, Terminal.Gui.Attribute Renk)> SatirIciAyristir(string metin, MdPalette palet)
    {
        var parcalar = new List<(string, Terminal.Gui.Attribute)>();
        int konum = 0;
        while (konum < metin.Length)
        {
            int sonrakiKod = metin.IndexOf('`', konum);
            int sonrakiKalin = metin.IndexOf("**", konum, StringComparison.Ordinal);
            bool kodMu = sonrakiKod >= 0 && (sonrakiKalin < 0 || sonrakiKod < sonrakiKalin);
            int sonraki = kodMu ? sonrakiKod : sonrakiKalin;

            if (sonraki < 0)
            {
                parcalar.Add((metin.Substring(konum), palet.Taban));
                break;
            }

            if (sonraki > konum)
                parcalar.Add((metin.Substring(konum, sonraki - konum), palet.Taban));

            if (kodMu)
            {
                int kapanis = metin.IndexOf('`', sonraki + 1);
                if (kapanis < 0) { parcalar.Add((metin.Substring(sonraki), palet.Taban)); break; }
                parcalar.Add((metin.Substring(sonraki + 1, kapanis - sonraki - 1), palet.Kod));
                konum = kapanis + 1;
            }
            else
            {
                int kapanis = metin.IndexOf("**", sonraki + 2, StringComparison.Ordinal);
                if (kapanis < 0) { parcalar.Add((metin.Substring(sonraki), palet.Taban)); break; }
                parcalar.Add((metin.Substring(sonraki + 2, kapanis - sonraki - 2), palet.Kalin));
                konum = kapanis + 2;
            }
        }
        return parcalar;
    }
}
