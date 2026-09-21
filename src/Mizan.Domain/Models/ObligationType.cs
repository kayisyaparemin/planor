namespace Mizan.Domain.Models;

/// <summary>
/// Finansal nakit akışında yer alan zorunlu ve planlı yükümlülüklerin türünü belirten sınıflandırma.
/// </summary>
public enum ObligationType
{
    /// <summary>Banka ve finansman kredisi taksitleri veya erken ödemeleri.</summary>
    Loan,

    /// <summary>Kredi kartı dönem ekstresi asgari veya planlanan ödemesi.</summary>
    CreditCard,

    /// <summary>Geçici ve tek seferlik vadeli borç ödemesi.</summary>
    TemporaryPayment,

    /// <summary>Elden veya senetli taksitli ödeme planı taksiti.</summary>
    InstallmentPayment,

    /// <summary>Düzenli veya diğer periyodik planlı ödeme.</summary>
    OtherScheduledPayment,

    /// <summary>Planlanan tek seferlik büyük nakit harcaması.</summary>
    PlannedLargeExpense
}
