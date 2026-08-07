using System.IO;
using System.Text.Json;

namespace Ares.Core;

public static partial class Config
{
    public static string OpenAIKey { get; set; } = "";
    public static string OpenAIURL { get; set; } = "";
    public static string OpenAIModel { get; set; } = "";
    public static string AnthropicKey { get; set; } = "";
    public static string AnthropicURL { get; set; } = "";
    public static string AnthropicModel { get; set; } = "";
    public static string DefaultProvider { get; set; } = "OpenAI";

    private static readonly object _kilit = new();
    private static readonly string _yol = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ares", "Api", "config.json");
    private static readonly JsonSerializerOptions _jsonOps = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
