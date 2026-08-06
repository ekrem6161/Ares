using System.Security.Cryptography;
using System.Text;

namespace Ares.Core;

internal static class Crypto
{
    private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("ares-api-key");

    public static string Encrypt(string duzMetin)
    {
        if (string.IsNullOrWhiteSpace(duzMetin))
            throw new ArgumentException("Şifrelenecek metin boş olamaz", nameof(duzMetin));
        var duz = Encoding.UTF8.GetBytes(duzMetin);
        try
        {
            var sifreli = ProtectedData.Protect(duz, _entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(sifreli);
        }
        catch (CryptographicException e)
        {
            Console.Error.WriteLine($"[Crypto.Encrypt] {e.Message}");
            throw;
        }
        finally
        {
            Array.Clear(duz, 0, duz.Length);
        }
    }

    public static string Decrypt(string sifreliMetin)
    {
        if (string.IsNullOrWhiteSpace(sifreliMetin))
            throw new ArgumentException("Çözülecek metin boş olamaz", nameof(sifreliMetin));
        byte[]? duz = null;
        try
        {
            var sifreli = Convert.FromBase64String(sifreliMetin);
            duz = ProtectedData.Unprotect(sifreli, _entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(duz);
        }
        catch (FormatException e)
        {
            Console.Error.WriteLine($"[Crypto.Decrypt] {e.Message}");
            throw;
        }
        catch (CryptographicException e)
        {
            Console.Error.WriteLine($"[Crypto.Decrypt] {e.Message}");
            throw;
        }
        finally
        {
            if (duz is not null)
                Array.Clear(duz, 0, duz.Length);
        }
    }
}
