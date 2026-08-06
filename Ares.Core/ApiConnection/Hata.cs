namespace Ares.Core;

internal static class Hata
{
    public static string Doner(string kaynak, string sebep)
    {
        Console.Error.WriteLine($"[{kaynak}] {sebep}");
        return $"[HATA] {kaynak} {sebep}.";
    }
}
