using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// TUI giriş noktası: Terminal.Gui'yi başlatır, hoş geldin ekranını kurar,
/// ana döngüyü çalıştırır ve kapatır. Ekran değişimleri ileride burada yönetilir.
/// </summary>
public static class Arayuz
{
    public static Task Calistir()
    {
        Application.Init();
        try
        {
            var ust = Application.Top;
            var ekran = new HosGeldinEkrani
            {
                X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            };
            ekran.CikisIstendi += () => Application.RequestStop();
            ust.Add(ekran);
            ekran.SetFocus();
            Application.Run();
        }
        finally
        {
            Application.Shutdown();
        }
        return Task.CompletedTask;
    }
}
