using System.IO;
using System.Text.Json;

namespace Ares.Core;

public static partial class Config
{
    public static string OpenAIKey { get; internal set; } = "";
    public static string OpenAIURL { get; internal set; } = "";
    public static string OpenAIModel { get; internal set; } = "";
    public static string AnthropicKey { get; internal set; } = "";
    public static string AnthropicURL { get; internal set; } = "";
    public static string AnthropicModel { get; internal set; } = "";

    private static readonly object _kilit = new();
    private static readonly string _yol = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ares", "Api", "config.json");
    private static readonly JsonSerializerOptions _jsonOps = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void EkranaYaz()
    {
        Console.WriteLine($"OpenAIKey: {OpenAIKey}");
        Console.WriteLine($"OpenAIURL: {OpenAIURL}");
        Console.WriteLine($"OpenAIModel: {OpenAIModel}");
        Console.WriteLine($"AnthropicKey: {AnthropicKey}");
        Console.WriteLine($"AnthropicURL: {AnthropicURL}");
        Console.WriteLine($"AnthropicModel: {AnthropicModel}");
    }
}
