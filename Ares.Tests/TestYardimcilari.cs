using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ares.Tests;

/// <summary>Streaming akışlarını tek metinde birleştiren ortak test yardımcısı.</summary>
internal static class TestYardimcilari
{
    public static async Task<string> AkisiTopla(IAsyncEnumerable<string> akis)
    {
        var bir = new StringBuilder();
        await foreach (var parca in akis)
            bir.Append(parca);
        return bir.ToString();
    }
}
