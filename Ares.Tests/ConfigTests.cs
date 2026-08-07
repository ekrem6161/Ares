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
}
