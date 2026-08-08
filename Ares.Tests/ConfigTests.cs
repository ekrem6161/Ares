using System;
using Ares.Core;
using Xunit;

namespace Ares.Tests;

public class ConfigTests
{
    [Fact]
    public void VarsayilanDegerlerBos()
    {
        if (Environment.GetEnvironmentVariable("ARES_CANLI") == "1")
            return;
        Assert.Equal("", Config.OpenAIKey);
        Assert.Equal("", Config.OpenAIURL);
        Assert.Equal("", Config.OpenAIModel);
        Assert.Equal("", Config.AnthropicKey);
        Assert.Equal("", Config.AnthropicURL);
        Assert.Equal("", Config.AnthropicModel);
    }
}
