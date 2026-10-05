namespace Mizan.Presentation.Models;

/// <summary>
/// Ana sayfadaki "Kalan ödemeler" kartında tek bir bekleyen ödeme satırı. Kart "bu dönem daha ne ödeyeceğim?"
/// sorusunu taratır; satır bu yüzden yalnız ad, vade ve tutar taşır (S72-9).
/// </summary>
public sealed record DashboardRemainingItem
{
    /// <summary>Planlanan vade tarihi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Ödemenin adı.</summary>
    public required string Name { get; init; }

    /// <summary>Ödenecek tutar: kart satırında kartın bugünkü hâli, plandaki tahmin değil (I165).</summary>
    public required decimal Amount { get; init; }
}
