using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kalan bir ödeme satırının gidişatta sayılan tutarı: kart satırında kartın bugünkü hâli, diğerlerinde planlanan
/// tutar (I23). Dondurulan kart tahmini dönem içindeki ekstre ve harcamaları bilmez; dönem sonu tahmini, bakiye
/// rotası ve hatırlatıcı aynı tutarı kullanmazsa ekranlar birbirinden farklı rakam söyler. Üçü de kuralı buradan
/// alır (M8).
/// </summary>
public static class ProjectedPaymentAmount
{
    /// <summary>Satırın gidişatta sayılan tutarını döner; tutarı bilinmeyen satır 0 sayılır.</summary>
    /// <param name="line">Kalan ödeme satırı.</param>
    /// <param name="currentCardPayments">Kart kimliğine göre kartın bugünkü hâliyle bu dönemde vadesi gelen ödeme.</param>
    public static decimal Of(PeriodPlanPaymentLine line, IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        Known(line, currentCardPayments) ?? 0m;

    /// <summary>
    /// Satırın tutarını döner; tutar bilinmiyorsa <c>null</c>. Hatırlatıcı bilinmeyen tutarı 0 diye değil
    /// "henüz belli değil" diye söyler, bu yüzden boşluğu korur.
    /// </summary>
    /// <param name="line">Kalan ödeme satırı.</param>
    /// <param name="currentCardPayments">Kart kimliğine göre kartın bugünkü hâliyle bu dönemde vadesi gelen ödeme.</param>
    public static decimal? Known(PeriodPlanPaymentLine line, IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        line.SourceType == PlanPaymentSourceType.CreditCard &&
        currentCardPayments.TryGetValue(line.SourceEntityId, out var current)
            ? current
            : line.PlannedAmount;
}
