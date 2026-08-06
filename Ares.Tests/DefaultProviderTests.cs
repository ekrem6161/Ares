using System.IO;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class DefaultProviderTests : ConfigTestBase
{
    [Fact]
    public void DosyaYokken_VarsayilanOpenAI()
    {
        if (File.Exists(Yol))
            File.Delete(Yol);
        Assert.False(Config.Yukle());
        Assert.Equal("OpenAI", Config.DefaultProvider);
    }

    [Fact]
    public void Kaydet_Yukle_Roundtrip()
    {
        Config.DefaultProvider = "Anthropic";
        Config.Kaydet();
        Assert.True(Config.Yukle());
        Assert.Equal("Anthropic", Config.DefaultProvider);
    }

    [Fact]
    public void GecersizDeger_OpenAIaDoner()
    {
        File.WriteAllText(Yol, "{\"DefaultProvider\":\"bilinmeyen\"}");
        Assert.True(Config.Yukle());
        Assert.Equal("OpenAI", Config.DefaultProvider);
    }
}
