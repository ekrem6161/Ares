using System;
using System.Threading.Tasks;
using Ares.Core;

namespace Ares;

internal static class Program
{
    private static async Task Main()
    {
        Config.OpenAIKey = "lm-studio";
        Config.OpenAIURL = "http://192.168.1.105:1234/v1/chat/completions";
        Config.OpenAIModel = "ornith";
        Config.Kaydet();
        Config.Yukle();
        var cevap = await Router.IstekGonder("Merhaba, tek cümleyle kendini tanit.");
        Console.WriteLine(cevap);
    }
}
