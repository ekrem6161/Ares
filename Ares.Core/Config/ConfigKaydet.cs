using System.Text.Json;

namespace Ares.Core;

public static partial class Config
{
    public static void Kaydet()
    {
        lock (_kilit)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_yol)!);
                var sozluk = new Dictionary<string, string>
                {
                    ["OpenAIKey"] = string.IsNullOrWhiteSpace(OpenAIKey) ? "" : Crypto.Encrypt(OpenAIKey),
                    ["OpenAIURL"] = OpenAIURL,
                    ["OpenAIModel"] = OpenAIModel,
                    ["AnthropicKey"] = string.IsNullOrWhiteSpace(AnthropicKey) ? "" : Crypto.Encrypt(AnthropicKey),
                    ["AnthropicURL"] = AnthropicURL,
                    ["AnthropicModel"] = AnthropicModel,
                };
                var gecici = _yol + ".tmp";
                File.WriteAllText(gecici, JsonSerializer.Serialize(sozluk, _jsonOps));
                File.Move(gecici, _yol, true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[Config.Kaydet] {e.Message}");
                throw;
            }
        }
    }
}
