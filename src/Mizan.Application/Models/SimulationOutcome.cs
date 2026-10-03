using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Simülatörün sonuç kartının ihtiyacı olan iki zincir: şu anki gidişatın 12 dönemi ve açık ve geçerli deneme varsa
/// denemeli 12 dönem (S76-1, 6). İkisi de 12 Dönem ekranının zinciriyle aynı yerden başlar; deneme yokken şu anki
/// gidişat 12 Dönem'in rakamlarıyla kuruşu kuruşuna aynıdır. Domain'in karşılaştırma sonucunu ekrana taşımaz: ekran
/// yalnız bu iki listeyi okur, kıyası kendisi kurar.
/// </summary>
/// <param name="Baseline">Şu anki gidişatın 12 dönemi.</param>
/// <param name="Scenario">Denemeli 12 dönem; açık ve geçerli deneme yoksa <c>null</c>.</param>
/// <param name="LoanInterestSaving">Erken ödemenin sağladığı toplam kredi faizi kazancı (S76-9).</param>
/// <param name="FinancingCost">Çekilen kredinin toplam maliyeti (S76-9).</param>
public sealed record SimulationOutcome(
    IReadOnlyList<CashFlowPeriodProjection> Baseline,
    IReadOnlyList<CashFlowPeriodProjection>? Scenario,
    decimal? LoanInterestSaving = null,
    decimal? FinancingCost = null);
