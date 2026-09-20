namespace Mizan.Domain.Models;

/// <summary>
/// Kredi ve kredi kartı haricindeki geçici veya taksitli ödeme planlarının türü.
/// </summary>
public enum PaymentPlanKind
{
    /// <summary>Geçici veya kısa vadeli borç planı.</summary>
    Temporary = 0,
    /// <summary>Düzenli senetli veya taksitli borç.</summary>
    Installment = 1,
    /// <summary>Tekrarlayan periyodik ödeme planı.</summary>
    Recurring = 2,
    /// <summary>Planlanmış diğer vadeli ödemeler.</summary>
    OtherScheduled = 3
}
