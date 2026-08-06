using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ares.Core;

namespace Ares;

internal static class Program
{
    private static async Task Main()
    {
        var satirlar = File.ReadAllLines(@"C:\Users\Ekrem\Desktop\dih.txt");
        var deepseek = satirlar.First(s => s.TrimStart().StartsWith("sk-c1", StringComparison.Ordinal))
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        Config.OpenAIKey = deepseek;
        Config.OpenAIURL = "https://api.deepseek.com/chat/completions";
        Config.OpenAIModel = "deepseek-v4-pro";
        Config.AnthropicKey = satirlar[0].Trim();
        Config.AnthropicURL = "https://api.anthropic.com/v1/messages";
        Config.AnthropicModel = "claude-sonnet-5";
        Config.DefaultProvider = "OpenAI";
        Config.Kaydet();
        Config.Yukle();
        Console.WriteLine("--- OpenAI (DeepSeek) ---");
        Console.WriteLine(await Router.IstekGonder(ProviderTipi.OpenAI, new List<Mesaj> { new(RolTipi.User, "Merhaba, tek cümleyle kendini tanit.") }));
        Console.WriteLine("--- Anthropic ---");
        Console.WriteLine(await Router.IstekGonder(ProviderTipi.Anthropic, new List<Mesaj> { new(RolTipi.User, "Merhaba, tek cümleyle kendini tanit.") }));
    }
}
