using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Simülasyon senaryosundan üretilen tüm finansal varlıkları tek bir atomik işlemde
/// kalıcı depolara aktarmak üzere bir araya getiren kayıt paketi.
/// </summary>
public sealed record SimulationPersistenceBatch
{
    /// <summary>Planlanan büyük harcama kayıtları.</summary>
    public IReadOnlyList<PlannedLargeExpense> LargeExpenses { get; init; } = [];

    /// <summary>Geçici ve taksitli borç ödeme planları.</summary>
    public IReadOnlyList<TemporaryPaymentPlan> PaymentPlans { get; init; } = [];

    /// <summary>Güncellenen kredi kartı varlıkları.</summary>
    public IReadOnlyList<CreditCard> CreditCards { get; init; } = [];

    /// <summary>Krediye ait erken/ara ödeme kayıtları.</summary>
    public IReadOnlyList<LoanPrepayment> LoanPrepayments { get; init; } = [];

    /// <summary>Tek seferlik arızi gelir kayıtları.</summary>
    public IReadOnlyList<AdHocIncome> AdHocIncomes { get; init; } = [];

    /// <summary>Düzenli gelir akışına ait tutar revizyon kayıtları.</summary>
    public IReadOnlyList<IncomeAmountHistory> IncomeHistories { get; init; } = [];
}
