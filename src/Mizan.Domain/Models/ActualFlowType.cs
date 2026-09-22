namespace Mizan.Domain.Models;

/// <summary>
/// Dönem planında öngörülmeyen, dönem içinde arızi olarak ortaya çıkan nakit akışının yönü.
/// </summary>
public enum ActualFlowType
{
    /// <summary>Planda yer almayan arızi nakit çıkışı (plansız harcama/ödeme).</summary>
    UnplannedPayment = 0,

    /// <summary>Planda yer almayan arızi nakit girişi (plansız gelir/tahsilat).</summary>
    UnplannedIncome = 1
}
