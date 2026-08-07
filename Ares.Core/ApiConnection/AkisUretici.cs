namespace Ares.Core;

/// <summary>
/// Streaming akışları için ortak yardımcılar.
/// <see cref="Guvenli"/> beklenmeyen hataları "[HATA] ..." parçasına çevirir;
/// <see cref="TekParca"/> tek parçalık akış üretir (doğrulama hataları için).
/// </summary>
internal static class AkisUretici
{
    public static async IAsyncEnumerable<string> Guvenli(IAsyncEnumerable<string> icAkis, string kaynak)
    {
        await using var sayac = icAkis.GetAsyncEnumerator();
        while (true)
        {
            var hata = "";
            bool devam;
            try
            {
                devam = await sayac.MoveNextAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[{kaynak}.Connection] {e.Message}");
                hata = "[HATA] " + kaynak + " isteği başarısız: " + e.Message;
                devam = false;
            }
            if (!devam)
            {
                if (hata.Length > 0)
                    yield return hata;
                yield break;
            }
            yield return sayac.Current;
        }
    }

    public static IAsyncEnumerable<string> TekParca(string mesaj) => TekParcaUret(mesaj);

    private static async IAsyncEnumerable<string> TekParcaUret(string mesaj)
    {
        await Task.Yield();
        yield return mesaj;
    }
}
