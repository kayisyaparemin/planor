namespace Mizan.Application.Models;

/// <summary>
/// Bir nakit akış döneminin kapatılması (settlement) anında kullanıcının sunduğu tüm gerçekleşme
/// bildirimlerini (borç ödemeleri, fiilî yaşam harcaması, faiz, plansız akışlar ve teyit edilen bakiye)
/// bir arada taşıyan girdi taslağıdır.
/// </summary>
public sealed record PeriodSettlementDraft
{
    /// <summary>Kapatılacak olan dondurulmuş dönem planının tekil kimliği.</summary>
    public required Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Dönemdeki planlanan ödeme satırlarına ait fiilî gerçekleşme bildirimleri.</summary>
    public IReadOnlyList<ActualPaymentDraft> Payments { get; init; } = [];

    /// <summary>Dönem içinde fiilen gerçekleşen serbest yaşam harcaması tutarı.</summary>
    public decimal ActualLivingSpend { get; init; }

    /// <summary>Dönem içinde fiilen yansıyan faiz/komisyon maliyetleri tutarı.</summary>
    public decimal ActualInterest { get; init; }

    /// <summary>Dönem içinde gerçekleşen plansız ek gelir ve harcama akışları.</summary>
    public IReadOnlyList<ActualFlowDraft> Flows { get; init; } = [];

    /// <summary>Serbest yaşam harcamasının isteğe bağlı girilen kategori dökümleri.</summary>
    public IReadOnlyList<LivingBreakdownDraft> LivingBreakdown { get; init; } = [];

    /// <summary>Kullanıcının cüzdanında/hesabında teyit ettiği kesin kapanış bakiyesi; boş bırakılırsa türetilen bakiye esas alınır.</summary>
    public decimal? ConfirmedEndingBalance { get; init; }

    /// <summary>Dönem kapanışına dair kullanıcının eklediği serbest açıklama notu.</summary>
    public string ActualNote { get; init; } = string.Empty;
}
