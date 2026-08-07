using OaiParser = Ares.Core.OpenAI.CevapParser;
using AntParser = Ares.Core.Anthropic.CevapParser;
using Xunit;

namespace Ares.Tests;

public class CevapParserTests
{
    // ---- OpenAI ----

    [Fact]
    public void OpenAI_AkisParcasi_MetinParcasiDoner()
    {
        var json = "{\"choices\":[{\"delta\":{\"content\":\"Mer\"}}]}";
        Assert.Equal("Mer", OaiParser.AkisParcasiCikar(json));
    }

    [Fact]
    public void OpenAI_AkisParcasi_ContentYoksaBos()
    {
        var json = "{\"choices\":[{\"delta\":{\"role\":\"assistant\"}}]}";
        Assert.Equal("", OaiParser.AkisParcasiCikar(json));
    }

    [Fact]
    public void OpenAI_AkisParcasi_ChoicesBossaBos()
    {
        Assert.Equal("", OaiParser.AkisParcasiCikar("{\"choices\":[]}"));
    }

    [Fact]
    public void OpenAI_TekParca_MetinDoner()
    {
        var json = "{\"choices\":[{\"message\":{\"content\":\"Merhaba\"}}]}";
        Assert.Equal("Merhaba", OaiParser.TekParcaCikar(json));
    }

    // ---- Anthropic ----

    [Fact]
    public void Anthropic_AkisParcasi_TextDeltaDoner()
    {
        var json = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Mer\"}}";
        Assert.Equal("Mer", AntParser.AkisParcasiCikar(json));
    }

    [Fact]
    public void Anthropic_AkisParcasi_MessageDelta_Bos()
    {
        Assert.Equal("", AntParser.AkisParcasiCikar("{\"type\":\"message_delta\"}"));
    }

    [Fact]
    public void Anthropic_AkisParcasi_ThinkingDelta_Bos()
    {
        var json = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"thinking_delta\",\"thinking\":\"x\"}}";
        Assert.Equal("", AntParser.AkisParcasiCikar(json));
    }
}
