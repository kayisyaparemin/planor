namespace Mizan.Application.Models;

/// <summary>
/// Finansal Yapı ekranında seçilen kayıt seçeneğinin hangi form bileşeni üzerinden girileceğini belirten tür.
/// </summary>
public enum RecordEntryForm
{
    /// <summary>Simülatörün paylaşılan ortak formu; kayıt simülasyon mekanizmasıyla aynı sözleşmeyle yazılır.</summary>
    SharedForm,

    /// <summary>Düzenli gelir akışını tanımlayan gelir formu.</summary>
    Income,

    /// <summary>Devam eden banka kredisi sözleşmesi formu.</summary>
    Loan,

    /// <summary>Kredi kartı tanım ve borç formu.</summary>
    CreditCard,

    /// <summary>Değişken vadeli ve taksitli borç planı formu.</summary>
    PaymentPlan
}
