using System.Text.Json;

namespace Ares.Core;

public static partial class Config
{
    private static string Oku(Dictionary<string, string> sozluk, string anahtar, bool sifreli)
    {
        if (!sozluk.TryGetValue(anahtar, out var deger) || string.IsNullOrWhiteSpace(deger))
            return "";
        if (!sifreli)
            return deger;
        try
        {
            return Crypto.Decrypt(deger);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Config] {anahtar}: {e.Message}");
            return "";
        }
    }

    public static bool Yukle()
    {
        lock (_kilit)
        {
            if (!File.Exists(_yol))
                return false;
            try
            {
                var sozluk = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(_yol), _jsonOps);
                if (sozluk is null)
                    return false;
                OpenAIKey = Oku(sozluk, "OpenAIKey", true);
                OpenAIURL = Oku(sozluk, "OpenAIURL", false);
                OpenAIModel = Oku(sozluk, "OpenAIModel", false);
                AnthropicKey = Oku(sozluk, "AnthropicKey", true);
                AnthropicURL = Oku(sozluk, "AnthropicURL", false);
                AnthropicModel = Oku(sozluk, "AnthropicModel", false);
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[Config.Yukle] {e.Message}");
                return false;
            }
        }
    }
}
