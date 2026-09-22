namespace Mizan.Domain.Models;

/// <summary>
/// Dönem planında yer alan bir ödeme taahhüdünün dönem kapanışındaki fiilî gerçekleşme durumu.
/// </summary>
public enum ActualPaymentStatus
{
    /// <summary>Ödeme taahhüt edilen tutarla tam olarak yapıldı.</summary>
    Paid = 0,

    /// <summary>Ödeme yapıldı ancak taahhüt edilenden farklı bir tutarla gerçekleşti.</summary>
    DifferentAmount = 1,

    /// <summary>Ödeme dönem içinde hiç yapılmadı / ertelendi.</summary>
    Unpaid = 2
}
