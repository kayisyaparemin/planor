using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Açık nakit akış döneminin gidişat verisini (<see cref="PeriodProgress"/>) sağlayan
/// kullanım senaryosu okuma portudur.
/// </summary>
public interface IPeriodProgressService
{
    /// <summary>
    /// Açık dönemin gidişatını getirir; henüz plan dondurulmamış veya açık dönem yoksa <c>null</c> döner.
    /// </summary>
    Task<PeriodProgress?> GetAsync(CancellationToken cancellationToken = default);
}
