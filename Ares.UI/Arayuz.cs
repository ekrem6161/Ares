using Ares.Core;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// TUI giriş noktası: config'i yükler, Terminal.Gui'yi başlatır, sohbet
/// ekranını kurar, ana döngüyü çalıştırır ve kapatır.
/// </summary>
public static class Arayuz
{
    public static void Calistir()
    {
        Config.Yukle();
        Application.Init();
        try
        {
            var ust = Application.Top;
            var ekran = new SohbetEkrani
            {
                X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            };
            ekran.CikisIstendi += () => Application.RequestStop();
            ust.Add(ekran);
            ekran.OdagiKutuyaVer();
            Application.Run();
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
