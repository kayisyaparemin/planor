namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının belirli bir andaki tüm aktif finansal sözleşmelerini, gelir akışlarını,
/// borç yükümlülüklerini ve temel yapılandırma parametrelerini tek bir çatı altında toplayan
/// bütüncül finansal plan modeli.
/// Nakit akış projeksiyonu, simülasyon senaryoları ve dönem mutabakat süreçleri için temel kaynaktır.
/// </summary>
public sealed record FinancialPlan
{
    /// <summary>Kullanıcının dönem çapası, serbest harcama havuzu ve faiz parametrelerini belirleyen temel ayarları.</summary>
    public UserSettings Settings { get; init; } = new();

    /// <summary>Kullanıcının tanımladığı düzenli gelir akışları (gelir, kira vb.).</summary>
    public IReadOnlyList<RecurringIncome> RecurringIncomes { get; init; } = [];

    /// <summary>Düzenli gelir akışlarına ait etkin tarihli tutar ve zam revizyon kayıtları.</summary>
    public IReadOnlyList<IncomeAmountHistory> IncomeHistories { get; init; } = [];

    /// <summary>Tek seferlik arızi gelirler (ikramiye, prim, iade vb.).</summary>
    public IReadOnlyList<AdHocIncome> AdHocIncomes { get; init; } = [];

    /// <summary>Kullanıcının banka ve finansman kredisi sözleşmeleri.</summary>
    public IReadOnlyList<Loan> Loans { get; init; } = [];

    /// <summary>Kredilere yapılmış veya planlanmış erken ödeme/itfa kayıtları.</summary>
    public IReadOnlyList<LoanPrepayment> LoanPrepayments { get; init; } = [];

    /// <summary>Kredi ve kart dışındaki vadeli borç, senet, taksit ve periyodik ödeme planları.</summary>
    public IReadOnlyList<TemporaryPaymentPlan> PaymentPlans { get; init; } = [];

    /// <summary>Kullanıcının kredi kartı sözleşmeleri, ekstre döngüleri ve harcamaları.</summary>
    public IReadOnlyList<CreditCard> CreditCards { get; init; } = [];

    /// <summary>Planlanmış tek seferlik büyük harcamalar.</summary>
    public IReadOnlyList<PlannedLargeExpense> PlannedLargeExpenses { get; init; } = [];

    /// <summary>
    /// Planın 12 dönemlik nakit akış projeksiyonu üretmeye hazır olup olmadığını belirler.
    /// Başlangıç çapa tarihinin ayarlanmış olmasını ve en az bir finansal başlangıç dayanağını
    /// (aktif düzenli gelir, tek seferlik arızi gelir veya sıfırdan farklı açılış bakiyesi) gerektirir.
    /// </summary>
    public bool CanBuildProjection =>
        Settings.ProjectionAnchorDate != default &&
        (RecurringIncomes.Any(x => x.IsActive) ||
         AdHocIncomes.Count > 0 ||
         Settings.ProjectionOpeningBalance != 0m);
}
