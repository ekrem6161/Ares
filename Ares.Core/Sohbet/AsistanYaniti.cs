using System.Text;

namespace Ares.Core.Sohbet;

/// <summary>
/// Streaming akışında biriken asistan yanıtı. <see cref="ParcaEkle"/> ile
/// parçalar biriktirilir; <see cref="Bitir"/> tam metni geçmişe yazar.
/// "[HATA]" ile başlayan yanıtlar geçmişe yazılmaz (context kirlenmesin).
/// </summary>
public sealed class AsistanYaniti
{
    private readonly Sohbet _sahip;
    private readonly StringBuilder _birikim = new();

    internal AsistanYaniti(Sohbet sahip)
    {
        _sahip = sahip;
    }

    public void ParcaEkle(string parca)
    {
        _birikim.Append(parca);
    }

    /// <summary>Yanıtı geçmişe yazar; hata parçasıyla başladıysa yazmaz. true = yazıldı.</summary>
    public bool Bitir()
    {
        var metin = _birikim.ToString();
        if (metin.Length == 0 || metin.StartsWith("[HATA]", System.StringComparison.Ordinal))
            return false;
        _sahip.AsistanMesajiEkle(metin);
        return true;
    }
}
