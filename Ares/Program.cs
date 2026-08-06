using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ares.Core;

namespace Ares;

internal static class Program
{
    private static async Task Main()
    {
        var yuklendi = Config.Yukle();
        Console.WriteLine($"Config.Yukle() sonucu: {yuklendi}");
        Console.WriteLine("--- OpenAI (config.json'dan yuklenen degerlerle) ---");
        Console.WriteLine(await Router.IstekGonder(ProviderTipi.OpenAI, new List<Mesaj> { new(RolTipi.User, "Hangi model oldugunu tek kelimeyle soyle.") }));
        Console.WriteLine("--- Anthropic (config.json'dan yuklenen degerlerle) ---");
        Console.WriteLine(await Router.IstekGonder(ProviderTipi.Anthropic, new List<Mesaj> { new(RolTipi.User, "Hangi model oldugunu tek kelimeyle soyle.") }));
    }
}
