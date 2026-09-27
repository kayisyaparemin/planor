namespace Mizan.Presentation.Models;

/// <summary>
/// Finansal Yapı listesindeki bir satırın hangi kayıt türünden geldiğini söyler; satırın bağlam
/// metni, dokununca açılan seçenekler ve silme portu bu türe göre seçilir (EK-V6, S62).
/// </summary>
public enum FinancialRecordKind
{
    /// <summary>Düzenli gelir akışı.</summary>
    RecurringIncome,

    /// <summary>Tek seferlik gelir.</summary>
    AdHocIncome,

    /// <summary>Kredi kartı.</summary>
    CreditCard,

    /// <summary>Banka kredisi.</summary>
    Loan,

    /// <summary>Taksitli ödeme planı.</summary>
    PaymentPlan,

    /// <summary>Planlı büyük harcama.</summary>
    LargeExpense
}
