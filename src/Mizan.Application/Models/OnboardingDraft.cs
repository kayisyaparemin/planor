using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kurulum sihirbazında (onboarding) kullanıcının girdiği başlangıç ayarlarını,
/// gelir akışlarını, kredilerini, kartlarını ve planlı ödemelerini bir arada taşıyan taslak sözleşmesidir.
/// </summary>
public sealed record OnboardingDraft
{
    /// <summary>Kullanıcının dönem çapası, serbest yaşam havuzu ve açılış bakiyesi ayarları.</summary>
    public required UserSettings Settings { get; init; }

    /// <summary>Tanımlanan düzenli gelir akışları.</summary>
    public IReadOnlyList<RecurringIncome> RecurringIncomes { get; init; } = [];

    /// <summary>Düzenli gelir akışlarına ait başlangıç tutar kayıtları.</summary>
    public IReadOnlyList<IncomeAmountHistory> IncomeAmountHistories { get; init; } = [];

    /// <summary>Tanımlanan kredi kartları.</summary>
    public IReadOnlyList<CreditCard> CreditCards { get; init; } = [];

    /// <summary>Tanımlanan krediler.</summary>
    public IReadOnlyList<Loan> Loans { get; init; } = [];

    /// <summary>Tanımlanan vadeli veya taksitli borç planları.</summary>
    public IReadOnlyList<TemporaryPaymentPlan> PaymentPlans { get; init; } = [];

    /// <summary>Tanımlanan planlı büyük harcamalar.</summary>
    public IReadOnlyList<PlannedLargeExpense> PlannedLargeExpenses { get; init; } = [];
}
