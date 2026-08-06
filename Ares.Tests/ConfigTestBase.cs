using System;
using System.IO;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class ConfigTestBase : IDisposable
{
    protected static readonly string Yol = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ares", "Api", "config.json");
    private readonly string _yedek;

    protected ConfigTestBase()
    {
        if (File.Exists(Yol))
        {
            _yedek = Yol + ".test-backup";
            File.Copy(Yol, _yedek, true);
        }
        else
        {
            _yedek = "";
        }
    }

    public void Dispose()
    {
        if (_yedek == "")
        {
            if (File.Exists(Yol))
                File.Delete(Yol);
        }
        else
        {
            File.Copy(_yedek, Yol, true);
            File.Delete(_yedek);
        }
        TumunuBosalt();
    }

    protected static void TumunuBosalt()
    {
        Config.OpenAIKey = "";
        Config.OpenAIURL = "";
        Config.OpenAIModel = "";
        Config.AnthropicKey = "";
        Config.AnthropicURL = "";
        Config.AnthropicModel = "";
    }
}
