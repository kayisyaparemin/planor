namespace Mizan.Application.Models;

/// <summary>
/// Kayıt türü seçicide seçilen seçeneğin hangi formda girileceği. Seçici kullanıcının niyetini sunar,
/// form planda neyin saklanacağını belirler; aynı forma birden fazla seçenek gidebilir ("Düzenli ödeme"
/// ve "Taksitli nakit borç" ödeme planı formunu açar, S77-3). Rotayı Presentation bilir; Application
/// yalnız hangi form olduğunu söyler.
/// </summary>
public enum RecordEntryForm
{
    /// <summary>Planlı büyük harcama formu: ad, tutar, tarih.</summary>
    PlannedExpense,

    /// <summary>Ödeme planı formu: taksit tutarı, sayısı ve ilk vadesiyle seri taksit.</summary>
    PaymentPlan,

    /// <summary>Kredi kartının tanım formu.</summary>
    CreditCard,

    /// <summary>Bankada devam eden kredinin formu.</summary>
    Loan,

    /// <summary>Düzenli gelirin formu.</summary>
    RecurringIncome,

    /// <summary>Tek seferlik gelirin formu.</summary>
    AdHocIncome
}
