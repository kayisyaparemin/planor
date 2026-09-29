using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Ana sayfanın mevcut dönem verisi: dönem başında dondurulan planın (varsa son revizyonuyla) dediği
/// ile kullanıcının girdiği bakiyeden çıkan gidişat yan yana. "Elimdeki tutarla kalan ödemeleri yapınca
/// yaşam havuzumdan ne kalır, KMH faizi ne olur, dönem sonu nereye gider?" sorusunun cevabıdır.
/// Yalnız açık dönemi anlatır; gelecek dönemler 12 dönem ekranının işidir. Planlanan tutarlar kilitlidir
/// (I23): dönem içi harcama yalnız gidişat alanlarını değiştirir.
/// </summary>
public sealed record PeriodProgress
{
    /// <summary>Gidişatın dayandığı dondurulmuş dönem planının kimliği.</summary>
    public required Guid PeriodPlanSnapshotId { get; init; }

    /// <summary>Dönemin ilk günü (dahil). Plan her zaman bu gün dondurulur; ayrı bir dondurma tarihi yoktur (S32).</summary>
    public required DateOnly PeriodStart { get; init; }

    /// <summary>Dönemin bitiş günü (hariç), yani sonraki dönemin ilk günü.</summary>
    public required DateOnly PeriodEnd { get; init; }

    /// <summary>Gidişatın hesaplandığı gün; vadesi bu güne kadar gelen ödemeler yapılmış sayılır.</summary>
    public required DateOnly Today { get; init; }

    /// <summary>Dönem başından bugüne geçen gün; sıfır ile dönem uzunluğu arasına kenetlenir.</summary>
    public required int ElapsedDays { get; init; }

    /// <summary>Dönemin gün sayısı.</summary>
    public required int TotalDays { get; init; }

    /// <summary>Dönem içinde plana yapılan revizyon sayısı; planlanan tutarlar sonuncusundan gelir (I24).</summary>
    public required int RevisionCount { get; init; }

    /// <summary>Planlanan toplam gelir.</summary>
    public required decimal PlannedIncome { get; init; }

    /// <summary>Planlanan zorunlu ödemeler toplamı.</summary>
    public required decimal PlannedMandatoryPayments { get; init; }

    /// <summary>Planlanan dönem sonu bakiyesi.</summary>
    public required decimal PlannedEndingBalance { get; init; }

    /// <summary>
    /// Dönem için ayrılan yaşam gideri havuzu. Günlere bölünmez: 20.000 ayrıldı ve 15.000 harcandıysa
    /// 5.000 kalmıştır. Kart harcaması buraya girmez; o, kartın içinde ekstre tarihiyle yönetilir.
    /// </summary>
    public required decimal PlannedVariableExpenseAllowance { get; init; }

    /// <summary>Planlanan KMH (finansman açığı) faizi.</summary>
    public required decimal PlannedDeficitInterest { get; init; }

    /// <summary>Gözlenen bakiyeden geri çözülen yaşam harcaması; bakiye girilmediyse <c>null</c>.</summary>
    public required decimal? ObservedLivingSpend { get; init; }

    /// <summary>Havuzdan kalan, aşıldıysa sıfır; bakiye girilmediyse <c>null</c>.</summary>
    public required decimal? RemainingVariableExpenseAllowance { get; init; }

    /// <summary>Bu gidişatla dönem sonunda doğacak KMH faizi; bakiye girilmediyse <c>null</c>.</summary>
    public required decimal? ProjectedDeficitInterest { get; init; }

    /// <summary>Bu gidişatla dönem sonu bakiyesi; bakiye girilmediyse <c>null</c>.</summary>
    public required decimal? ProjectedEndingBalance { get; init; }

    /// <summary>
    /// Harcamanın süreye göre temposu; bakiye girilmediyse ya da yaşam havuzu 0 ise <c>null</c> (S70).
    /// Gözlem gününe göre hesaplanır; bugüne göre olan <see cref="ElapsedDays"/> ile karıştırılmaz.
    /// </summary>
    public required SpendingPace? Pace { get; init; }

    /// <summary>Planlanan kart ödemeleri ile kartların bugünkü hâlinin karşılaştırması.</summary>
    public required IReadOnlyList<PeriodCardComparison> Cards { get; init; }

    /// <summary>Kullanıcının dönem içi gözlem defteri; gözlenen bakiye ve gözlem günü buradan okunur (S34).</summary>
    public required PeriodObservation? Observation { get; init; }

    /// <summary>Dönemin gözlemleri, güne göre sıralı; grafikte her biri bir nokta işaretidir (S68-1). Gözlem yoksa boş.</summary>
    public required IReadOnlyList<PeriodObservation> Observations { get; init; }

    /// <summary>Ana sayfa grafiğinin çizdiği bakiye rotası: her gün bir nokta, son noktası dönem sonu (S71).</summary>
    public required PeriodBalancePath Path { get; init; }

    /// <summary>Henüz yapılmamış plan satırları, önce vadeye sonra ada göre sıralı.</summary>
    public required IReadOnlyList<PeriodPlanPaymentLine> RemainingLines { get; init; }

    /// <summary>Kalan satırların planlanan tutarları toplamı.</summary>
    public required decimal RemainingPlannedTotal { get; init; }

    /// <summary>Dönemin kapanış mutabakatına açılıp açılmadığı.</summary>
    public required bool IsClosable { get; init; }

    /// <summary>Kalan satırlardan hatırlatıcıda "Ertele" denenlerin kimlikleri.</summary>
    public required IReadOnlySet<Guid> SnoozedLineIds { get; init; }

    /// <summary>Havuz aşıldıysa aşım tutarı, aşılmadıysa <c>null</c>. Aşım dönem sonuna yansır.</summary>
    public decimal? LivingOverspend =>
        ObservedLivingSpend is { } spent && spent > PlannedVariableExpenseAllowance
            ? spent - PlannedVariableExpenseAllowance
            : null;

    /// <summary>Gidişatın planlanan dönem sonundan sapması (gidişat − plan); bakiye girilmediyse <c>null</c>.</summary>
    public decimal? EndingDeviation => ProjectedEndingBalance - PlannedEndingBalance;

    /// <summary>KMH faizinin plandan sapması (gidişat − plan); bakiye girilmediyse <c>null</c>.</summary>
    public decimal? DeficitInterestDeviation => ProjectedDeficitInterest - PlannedDeficitInterest;

    /// <summary>Kalan bir satırın hatırlatıcıda ertelenip ertelenmediği; ertelenen satır vadesi geçse de kalandır.</summary>
    public bool IsSnoozed(Guid lineId) => SnoozedLineIds.Contains(lineId);
}
