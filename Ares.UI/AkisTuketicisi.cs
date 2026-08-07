using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Ares.UI;

/// <summary>
/// Streaming akışını ana döngüyü bloklamadan tüketir: akış arka planda
/// okunur, her parça <see cref="Application.MainLoop.Invoke"/> ile UI
/// iş parçacığına taşınır. Terminal.Gui thread-safe değildir; UI'a
/// yalnızca ana döngüden dokunulur (bkz. docs/mainloop.md).
/// UI thread'inde senkron bekletme (.Result) YASAK — ekran donar,
/// tüm yazı birden basılır, streaming anlamsızlaşır.
/// </summary>
public static class AkisTuketicisi
{
    public static Task Calistir(IAsyncEnumerable<string> akis, Action<string> parcaIslendi, Action bitti)
    {
        return Task.Run(async () =>
        {
            try
            {
                await foreach (var parca in akis)
                    Application.MainLoop.Invoke(() => parcaIslendi(parca));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[AkisTuketicisi] {e.Message}");
                Application.MainLoop.Invoke(() => parcaIslendi("[HATA] UI akışı başarısız: " + e.Message));
            }
            finally
            {
                Application.MainLoop.Invoke(bitti);
            }
        });
    }
}
