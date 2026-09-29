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

    /// <summary>
    /// Kaydetmeden önizleme: verilen bakiyenin verilen güne gözlem olarak girilmesi hâlinde gidişatın nasıl görüneceğini
    /// hesaplar. Taslak gözlem defterin gözlemlerine eklenir (aynı günün gözlemi yerine geçer), hiçbir şey yazılmaz.
    /// </summary>
    /// <exception cref="InvalidOperationException">Açık dönem yoksa ya da gün gözlem alamıyorsa; kaydetmeyle aynı kural.</exception>
    Task<PeriodProgress> PreviewAsync(
        decimal balance,
        DateOnly observedOn,
        CancellationToken cancellationToken = default);
}
