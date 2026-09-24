using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının ana ekranında (Dashboard) gösterilen aktif nakit akış dönemini,
/// ilk dönem öncesi açık yükümlülükleri, yaklaşan ödemeleri,
/// 12 dönem sonu tahmini nakit dengesini ve en sıkışık dönemi özetleyen veri sözleşmesi.
/// Kullanıcıya anlık nakit sağlığını ve yakın finansal riskleri tek bakışta sunmak için vardır.
/// </summary>
public sealed record DashboardSnapshot
{
    /// <summary>Şu an içinde bulunulan veya projeksiyonun ilk aktif nakit akış dönemi.</summary>
    public required CashFlowPeriodProjection CurrentPeriod { get; init; }

    /// <summary>İlk dönem başlangıcından önce kalan ancak vadesi henüz gelmemiş/açık yükümlülükler.</summary>
    public IReadOnlyList<ObligationItem> PreFirstPeriodObligations { get; init; } = [];

    /// <summary>Kullanıcının önündeki en yakın 5 zorunlu ödeme kalemi.</summary>
    public IReadOnlyList<ObligationItem> UpcomingPayments { get; init; } = [];

    /// <summary>12 dönemlik projeksiyon ufkunun sonundaki tahmini net nakit kapanış bakiyesi.</summary>
    public required decimal TwelvePeriodEndingBalance { get; init; }

    /// <summary>12 dönem içinde kapanış bakiyesi en düşük / likidite riski en yüksek olan dönem.</summary>
    public required CashFlowPeriodProjection TightestPeriod { get; init; }

    /// <summary>Projeksiyon ufkunda ödeme tutarı belirsiz olan herhangi bir kart ekstre döngüsü olup olmadığı.</summary>
    public bool HasUndeterminedCardPayments { get; init; }

    /// <summary>Projeksiyonun dayandığı referans çapa tarihi.</summary>
    public DateOnly ProjectionAnchorDate { get; init; }

    /// <summary>Projeksiyonun başladığı ilk açılış nakit bakiyesi.</summary>
    public decimal ProjectionOpeningBalance { get; init; }

    /// <summary>12 dönem boyunca oluşması öngörülen toplam kredi kartı carry faizi maliyeti.</summary>
    public decimal TwelvePeriodCreditCardInterest { get; init; }

    /// <summary>12 dönem boyunca eksi bakiyeye düşülmesi durumunda oluşacak toplam finansman açığı faizi.</summary>
    public decimal TwelvePeriodDeficitFinancingInterest { get; init; }

    /// <summary>12 dönem boyunca katlanılacak toplam faiz ve finansman maliyeti.</summary>
    public decimal TwelvePeriodTotalInterest { get; init; }
}
