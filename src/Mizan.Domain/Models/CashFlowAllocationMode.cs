namespace Mizan.Domain.Models;

/// <summary>
/// Dönem nakit akışı kullanım düzeninde harcamanın hangi döneme tahsis edileceğini belirleyen mod.
/// </summary>
public enum CashFlowAllocationMode
{
    /// <summary>Gelir gününden önceki harcama gelen yeni dönemin bütçesinden karşılanır.</summary>
    UpcomingPeriod = 0,
    /// <summary>Gelir gününden önceki harcama önceki dönemin bakiyesinden karşılanır.</summary>
    PreviousPeriod = 1
}
