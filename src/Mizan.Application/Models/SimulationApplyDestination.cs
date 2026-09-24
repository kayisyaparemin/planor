namespace Mizan.Application.Models;

/// <summary>
/// Bir simülasyon senaryo koşulunun canlı finansal planda hangi varlık alanına aktarıldığını belirten hedef türü.
/// </summary>
public enum SimulationApplyDestination
{
    /// <summary>Planlanan büyük harcama, taksitli borç veya kredi erken kapama/ara ödeme kayıtları.</summary>
    Payments,

    /// <summary>Kredi kartı tek çekim, taksitli harcama veya ekstre ödeme tercihi kayıtları.</summary>
    CreditCard,

    /// <summary>Münferit tek seferlik arızi gelir kayıtları.</summary>
    Income,

    /// <summary>Düzenli gelir akışına ait tutar ve zam revizyon geçmişi kayıtları.</summary>
    IncomeHistory
}
