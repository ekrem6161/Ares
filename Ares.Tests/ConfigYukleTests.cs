using System.IO;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class ConfigYukleTests : ConfigTestBase
{
    [Fact]
    public void DosyaYoksaYukleCrashEtmezVarsayilanaDoner()
    {
        TumunuBosalt();
        if (File.Exists(Yol))
            File.Delete(Yol);

        var sonuc = Config.Yukle();

        Assert.False(sonuc);
        Assert.Equal("", Config.OpenAIKey);
        Assert.Equal("", Config.OpenAIURL);
        Assert.Equal("", Config.OpenAIModel);
        Assert.Equal("", Config.AnthropicKey);
        Assert.Equal("", Config.AnthropicURL);
        Assert.Equal("", Config.AnthropicModel);
    }

    [Fact]
    public void BozukJsonYukleCrashEtmez()
    {
        TumunuBosalt();
        Directory.CreateDirectory(Path.GetDirectoryName(Yol)!);
        File.WriteAllText(Yol, "{{{bozuk-json");

        var sonuc = Config.Yukle();

        Assert.False(sonuc);
        Assert.Equal("", Config.OpenAIKey);
    }

    [Fact]
    public void BozukSifreliDegerYukleCrashEtmez()
    {
        TumunuBosalt();
        Directory.CreateDirectory(Path.GetDirectoryName(Yol)!);
        File.WriteAllText(Yol, "{\"OpenAIKey\": \"%%%bozuk-base64%%%\"}");

        var sonuc = Config.Yukle();

        Assert.True(sonuc);
        Assert.Equal("", Config.OpenAIKey);
    }
}
