using System.IO;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class ConfigKaydetTests : ConfigTestBase
{
    [Fact]
    public void KaydetDuzMetinKeyYazmaz()
    {
        TumunuBosalt();
        Config.OpenAIKey = "gizli-anahtar-123";
        Config.AnthropicKey = "gizli-anahtar-456";

        Config.Kaydet();

        var metin = File.ReadAllText(Yol);
        Assert.DoesNotContain("gizli-anahtar-123", metin);
        Assert.DoesNotContain("gizli-anahtar-456", metin);
    }

    [Fact]
    public void KaydetSonraYukleDegerlerGeriDoner()
    {
        TumunuBosalt();
        Config.OpenAIKey = "gercek-key";
        Config.OpenAIURL = "http://localhost:1234/v1";
        Config.OpenAIModel = "model-x";
        Config.AnthropicKey = "ant-key";
        Config.AnthropicURL = "https://api.anthropic.com";
        Config.AnthropicModel = "claude-3";

        Config.Kaydet();

        TumunuBosalt();
        var sonuc = Config.Yukle();

        Assert.True(sonuc);
        Assert.Equal("gercek-key", Config.OpenAIKey);
        Assert.Equal("http://localhost:1234/v1", Config.OpenAIURL);
        Assert.Equal("model-x", Config.OpenAIModel);
        Assert.Equal("ant-key", Config.AnthropicKey);
        Assert.Equal("https://api.anthropic.com", Config.AnthropicURL);
        Assert.Equal("claude-3", Config.AnthropicModel);
    }

    [Fact]
    public void BosKeylerKaydedilirkenSifrelenmez()
    {
        TumunuBosalt();
        Config.OpenAIURL = "url-var";

        Config.Kaydet();

        var metin = File.ReadAllText(Yol);
        Assert.Contains("\"OpenAIKey\": \"\"", metin);
        Assert.Contains("\"AnthropicKey\": \"\"", metin);
    }
}
