namespace Mizan.Presentation.Models;

/// <summary>
/// Ana sayfadaki "Kalan ödemeler" kartında tek bir bekleyen ödeme satırı. Kart "bu dönem daha ne ödeyeceğim?"
/// sorusunu taratır; satır bu yüzden yalnız ad, vade ve tutar gösterir (S72-9). Satıra dokunup "Ödedim" demek
/// için hatırlatıcının ödeme anahtarını da taşır (S88).
/// </summary>
public sealed record DashboardRemainingItem
{
    /// <summary>Hatırlatıcı cevabının bağlandığı ödeme anahtarı; ekranda görünmez (S88, I22).</summary>
    public required string DueKey { get; init; }

    /// <summary>Planlanan vade tarihi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Ödemenin adı.</summary>
    public required string Name { get; init; }

    /// <summary>Ödenecek tutar: kart satırında kartın bugünkü hâli, plandaki tahmin değil (I165).</summary>
    public required decimal Amount { get; init; }
}
