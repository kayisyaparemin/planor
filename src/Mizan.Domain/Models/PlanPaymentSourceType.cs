namespace Mizan.Domain.Models;

/// <summary>
/// Dondurulan dönem planı ödeme satırının (PeriodPlanPaymentLine) hangi finansal
/// enstrümandan veya yükümlülük kaynağından doğduğunu belirten tür sözleşmesi.
/// Dönem kapanışı mutabakatında ve gözlemlerde satırın hangi sözleşmeyle eşleşeceğini
/// sınıflandırmak için vardır.
/// </summary>
public enum PlanPaymentSourceType
{
    /// <summary>Banka kredisi taksiti.</summary>
    Loan = 0,

    /// <summary>Kredi kartı ekstre veya dönem içi harcama ödemesi.</summary>
    CreditCard = 1,

    /// <summary>Geçici veya vadeli senetli borç ödemesi.</summary>
    TemporaryPayment = 2,

    /// <summary>Taksitli harcama veya borç ödemesi.</summary>
    InstallmentPayment = 3,

    /// <summary>Diğer planlanmış takvimli ödeme.</summary>
    OtherScheduledPayment = 4,

    /// <summary>Planlanan tek seferlik büyük nakit harcaması.</summary>
    PlannedLargeExpense = 5
}
