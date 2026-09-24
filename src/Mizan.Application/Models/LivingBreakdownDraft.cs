namespace Mizan.Application.Models;

/// <summary>
/// Fiilî serbest yaşam harcamasının kullanıcı tarafından girilen kategori bazlı döküm kalemi girdisidir.
/// </summary>
public sealed record LivingBreakdownDraft
{
    /// <summary>Harcama kategorisi adı (Örn. Market, Yakıt, Yeme-İçme).</summary>
    public required string Category { get; init; }

    /// <summary>Bu kategoride harcanan pozitif para tutarı.</summary>
    public required decimal Amount { get; init; }
}
