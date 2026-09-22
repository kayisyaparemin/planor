namespace Mizan.Domain.Models;

/// <summary>
/// Bir nakit akış döneminin kapanışı (settlement / checkpoint) anında, kapanan dönemin
/// kesinleşen gerçekleşmesini, güncellenen finansal araçları ve yeni dönemin dondurulmuş planını
/// veritabanına atomik bir işlem olarak kaydetmek üzere bir araya getiren taahhüt sözleşmesi.
/// Dondurulan tarihçe ile cüzdandaki finansal araçların gerçek durumu arasındaki veri bütünlüğünü
/// ve bakiye sürekliliğini tek bir elden garanti etmek için vardır.
/// </summary>
public sealed record PeriodSettlementCommit
{
    /// <summary>Dönem içinde varsa yapılmış olan dondurulmuş plan revizyonu.</summary>
    public PeriodPlanRevision? Revision { get; init; }

    /// <summary>Kapanan dönemin kesinleşmiş fiilî durum karnesi.</summary>
    public PeriodActual Actual { get; init; } = null!;

    /// <summary>Yeni dönemi başlatan güncel finansal durum görüntüsü.</summary>
    public FinancialSnapshot NewSnapshot { get; init; } = null!;

    /// <summary>Yeni dönem için dondurulan başlangıç plan taahhüdü.</summary>
    public PeriodPlanSnapshot NewPlan { get; init; } = null!;

    /// <summary>Dönem kapanışında güncellenen kullanıcı ayarları.</summary>
    public UserSettings UpdatedSettings { get; init; } = null!;

    /// <summary>Dönem içinde ödemeler veya erken kapamalar neticesinde güncellenen krediler.</summary>
    public IReadOnlyList<Loan> UpdatedLoans { get; init; } = [];

    /// <summary>Taksit ödemeleri neticesinde güncellenen vadeli/geçici borç planları.</summary>
    public IReadOnlyList<TemporaryPaymentPlan> UpdatedPaymentPlans { get; init; } = [];

    /// <summary>Ekstre döngüsü, ödeme ve devreden bakiyesi güncellenen kredi kartları.</summary>
    public IReadOnlyList<CreditCard> UpdatedCreditCards { get; init; } = [];

    /// <summary>Gerçekleştiği teyit edilen veya güncellenen planlı büyük harcamalar.</summary>
    public IReadOnlyList<PlannedLargeExpense> UpdatedLargeExpenses { get; init; } = [];

    /// <summary>Checkpoint anında tüketilen (ödenen) veya iptal edilen kredi erken ödeme kimlikleri.</summary>
    public IReadOnlyList<Guid> RemovedLoanPrepaymentIds { get; init; } = [];

    /// <summary>
    /// Taahhüt paketinin referans bütünlüğü, takvim sürekliliği ve devir bakiyesi kurallarına
    /// uygun olup olmadığını denetler.
    /// </summary>
    public bool IsConsistent =>
        Actual is not null &&
        NewSnapshot is not null &&
        NewPlan is not null &&
        UpdatedSettings is not null &&
        Actual.PeriodEnd == NewPlan.PeriodStart &&
        Actual.ConfirmedEndingBalance == NewSnapshot.ProjectionOpeningBalance &&
        Actual.ConfirmedEndingBalance == NewPlan.OpeningBalance &&
        NewPlan.FinancialSnapshotId == NewSnapshot.Id &&
        Actual.ResultFinancialSnapshotId == NewSnapshot.Id &&
        (Revision is null || Revision.PeriodPlanSnapshotId == Actual.PeriodPlanSnapshotId) &&
        !RemovedLoanPrepaymentIds.Any(id => id == Guid.Empty);
}
