using System;
using System.IO;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class ConfigTests
{
    [Fact]
    public void VarsayilanDegerlerBos()
    {
        Assert.Equal("", Config.OpenAIKey);
        Assert.Equal("", Config.OpenAIURL);
        Assert.Equal("", Config.OpenAIModel);
        Assert.Equal("", Config.AnthropicKey);
        Assert.Equal("", Config.AnthropicURL);
        Assert.Equal("", Config.AnthropicModel);
    }

    [Fact]
    public void EkranaYaz_TumDegerleriBastirir()
    {
        var cikti = new StringWriter();
        var eski = Console.Out;
        try
        {
            Console.SetOut(cikti);
            Config.EkranaYaz();
        }
        finally
        {
            Console.SetOut(eski);
        }

        var satirlar = cikti.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(6, satirlar.Length);
        Assert.Contains(satirlar, s => s.StartsWith("OpenAIKey:", StringComparison.Ordinal));
        Assert.Contains(satirlar, s => s.StartsWith("OpenAIURL:", StringComparison.Ordinal));
        Assert.Contains(satirlar, s => s.StartsWith("OpenAIModel:", StringComparison.Ordinal));
        Assert.Contains(satirlar, s => s.StartsWith("AnthropicKey:", StringComparison.Ordinal));
        Assert.Contains(satirlar, s => s.StartsWith("AnthropicURL:", StringComparison.Ordinal));
        Assert.Contains(satirlar, s => s.StartsWith("AnthropicModel:", StringComparison.Ordinal));
    }
}
