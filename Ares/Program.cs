using System;
using System.IO;
using System.Threading.Tasks;
using Ares.Core;

namespace Ares;

internal static class Program
{
    private static async Task Main()
    {
        var satirlar = File.ReadAllLines(@"C:\Users\Ekrem\Desktop\dih.txt");
        Config.AnthropicKey = satirlar[0].Trim();
        Config.AnthropicURL = "https://api.anthropic.com/v1/messages";
        Config.AnthropicModel = "claude-sonnet-5";
        Config.DefaultProvider = "Anthropic";
        Config.Kaydet();
        Config.Yukle();
        var cevap = await Router.IstekGonder("Merhaba, tek cümleyle kendini tanit.");
        Console.WriteLine(cevap);
    }
}
