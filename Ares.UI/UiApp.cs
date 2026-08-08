using System;
using System.IO;
using System.Threading.Tasks;
using Ares.Core;
using Ares.Core.Sohbet;
using Ares.UI.Bilesenler;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Ares TUI giriş noktası — aresv7'deki arayüzün görünüm portu (spinner hariç).
/// v6 Core (Router + SSE streaming) üzerine kurulur:
///   • üstte kimlik bloğu: logo + "v0.1" + provider · model · dizin
///   • ChatView: rol renkli sohbet (kullanıcı bant, asistan "●", sistem gri)
///   • altta ayraç + "> " input + ayraç + footer (sol kısayollar / sağ durum)
/// Spinner (SpinnerGorunumu) eski yerinde: en alt satırda, solda.
/// Slash komutları, markdown render ve Esc iptali sonraki görevler.
/// </summary>
public static class UiApp
{
    // LOGO: "ARES" box-drawing ASCII (kırmızı renk). 13 sütun geniş.
    private static readonly string[] AresLogo =
    {
        "╔═╗╦═╗╔═╗╔═╗",
        "╠═╣╠╦╝╠═ ╚═╗",
        "╩ ╩╩╚═╚═╝╚═╝",
    };

    private const string FooterIdle = "? shortcuts   /help  /clear  /exit   Ctrl+Q quit";

    public static Task Calistir()
    {
        Config.Yukle();
        var gecmis = new Sohbet();
        string provider = Config.DefaultProvider == "Anthropic" ? "Anthropic" : "OpenAI";

        Application.Init();
        try
        {
            var ust = Application.Top;
            string seciliModel = provider == "Anthropic" ? Config.AnthropicModel : Config.OpenAIModel;
            var modelEtiketi = string.IsNullOrWhiteSpace(seciliModel) ? "(no model)" : seciliModel;

            // ---- Kimlik bloğu (üst) ----
            var logo = new Label(string.Join("\n", AresLogo))
            {
                X = 0, Y = 0, Width = 13, Height = AresLogo.Length,
                ColorScheme = Theme.RenkSemasi(Color.BrightRed, Color.Black),
            };
            var surum = new Label("v0.1")
            {
                X = 14, Y = 0, Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.BrightCyan, Color.Black),
            };
            var bilgi = new Label($"{provider} · {modelEtiketi}")
            {
                X = 14, Y = 1, Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.Gray, Color.Black),
            };
            var dizin = new Label(Kisalt(Directory.GetCurrentDirectory(), 60))
            {
                X = 14, Y = 2, Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.DarkGray, Color.Black),
            };

            // ---- Sohbet alanı (orta) ----
            // Alt blok 4 satır: üst ayraç + input + alt ayraç + footer.
            var sohbet = new ChatView
            {
                X = 0, Y = 4, Width = Dim.Fill(), Height = Dim.Fill(4),
            };

            // ---- Alt: input'u saran çizgiler + input + footer ----
            var ustAyrac = new HLineView
            {
                X = 0, Y = Pos.AnchorEnd(4), Width = Dim.Fill(), Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.DarkGray, Color.Black),
            };
            var isaret = new Label(">")
            {
                X = 0, Y = Pos.AnchorEnd(3), Width = 2, Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.BrightGreen, Color.Black),
            };
            var input = new PromptBox("", Application.Driver.MakeAttribute(Color.DarkGray, Color.Black))
            {
                X = 2, Y = Pos.AnchorEnd(3), Width = Dim.Fill(1), Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.White, Color.Black),
            };
            var altAyrac = new HLineView
            {
                X = 0, Y = Pos.AnchorEnd(2), Width = Dim.Fill(), Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.DarkGray, Color.Black),
            };
            var solFooter = new Label(FooterIdle)
            {
                X = 0, Y = Pos.AnchorEnd(1), Height = 1,
                ColorScheme = Theme.RenkSemasi(Color.DarkGray, Color.Black),
            };
            var sagFooter = new Label("ready")
            {
                X = Pos.AnchorEnd(7), Y = Pos.AnchorEnd(1), Width = 7, Height = 1,
                TextAlignment = TextAlignment.Right,
                ColorScheme = Theme.RenkSemasi(Color.BrightGreen, Color.Black),
            };
            var spinner = new SpinnerGorunumu
            {
                X = 1, Y = Pos.AnchorEnd(1),
            };

            bool mesgul = false;
            DateTime turBaslangici = DateTime.UtcNow;
            AsistanYaniti? yanit = null;

            // Tur sırasında input'u kilitle, spinner'ı çalıştır, footer durumunu yönet.
            void MesgulYap(bool mesgulMu)
            {
                mesgul = mesgulMu;
                input.ReadOnly = mesgulMu;
                if (mesgulMu)
                {
                    turBaslangici = DateTime.UtcNow;
                    spinner.Baslat();
                    solFooter.Text = "";
                    sagFooter.Text = "working";
                    sagFooter.ColorScheme = Theme.RenkSemasi(Color.BrightYellow, Color.Black);
                }
                else
                {
                    spinner.Durdur();
                    int saniye = Math.Max(0, (int)(DateTime.UtcNow - turBaslangici).TotalSeconds);
                    sohbet.Ekle(ChatRole.Durum, $"Worked for {saniye}s");
                    solFooter.Text = FooterIdle;
                    sagFooter.Text = "ready";
                    sagFooter.ColorScheme = Theme.RenkSemasi(Color.BrightGreen, Color.Black);
                }
                solFooter.SetNeedsDisplay();
                sagFooter.SetNeedsDisplay();
            }

            void Cik() => Application.RequestStop();

            input.KeyPress += e =>
            {
                switch (e.KeyEvent.Key)
                {
                    case Key.Enter:
                        e.Handled = true;
                        if (mesgul)
                            break;   // tur sürerken yeni mesajı yok say

                        var metin = (input.Text?.ToString() ?? string.Empty).Trim();
                        if (metin.Length == 0)
                            break;
                        input.Text = string.Empty;
                        input.SetNeedsDisplay();

                        // Slash komutları sonraki görevde gelecek; şimdilik bilinmeyen komut yanıtı.
                        if (metin.StartsWith("/"))
                        {
                            sohbet.Ekle(ChatRole.Sistem, $"Unknown command: {metin}. Type /help for available commands.");
                            break;
                        }

                        // Normal mesaj → agent turu (Router + SSE streaming).
                        sohbet.Ekle(ChatRole.Kullanici, metin);
                        gecmis.KullaniciMesajiEkle(metin);
                        yanit = gecmis.AsistanYanitiniBaslat();

                        MesgulYap(true);
                        AkisTuketicisi.Calistir(
                            Router.IstekGonder(gecmis.Mesajlar()),
                            parca =>
                            {
                                if (parca.StartsWith("[HATA]", StringComparison.Ordinal))
                                    sohbet.Ekle(ChatRole.Sistem, parca);
                                else
                                {
                                    yanit?.ParcaEkle(parca);
                                    sohbet.EkleVeyaBaslat(ChatRole.Asistan, parca);
                                }
                            },
                            () =>
                            {
                                yanit?.Bitir();
                                MesgulYap(false);
                            });
                        break;

                    case Key.Q | Key.CtrlMask:
                        Cik();
                        e.Handled = true;
                        break;
                }
            };

            // Yedek: odak nerede olursa olsun Ctrl+Q çıkış; PageUp/PageDown ile geçmişe kaydır.
            ust.KeyPress += e =>
            {
                switch (e.KeyEvent.Key)
                {
                    case Key.Q | Key.CtrlMask:
                        Cik();
                        e.Handled = true;
                        break;
                    case Key.PageUp:
                        sohbet.SayfaKaydir(1);
                        e.Handled = true;
                        break;
                    case Key.PageDown:
                        sohbet.SayfaKaydir(-1);
                        e.Handled = true;
                        break;
                }
            };

            ust.Add(logo, surum, bilgi, dizin, sohbet, ustAyrac, isaret, input, altAyrac, spinner, solFooter, sagFooter);
            input.SetFocus();

            sohbet.Ekle(ChatRole.Sistem, "Ares ready. Type a message and press Enter.");
            if (string.IsNullOrEmpty(Config.AnthropicKey) && string.IsNullOrEmpty(Config.OpenAIKey))
                sohbet.Ekle(ChatRole.Sistem, "No API key — you can test for free with a local model (OpenAI/LM Studio).");

            Application.Run();
        }
        finally
        {
            Application.Shutdown();
        }
        return Task.CompletedTask;
    }

    /// <summary>Uzun yolu sona doğru "…" ile kısaltır (header'da taşmasın).</summary>
    private static string Kisalt(string yol, int maksimum)
    {
        if (yol.Length <= maksimum)
            return yol;
        return "…" + yol.Substring(yol.Length - (maksimum - 1));
    }
}
