namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir nakit akış dönemine [Start, End) vadesi düşen borç ve harcama yükümlülüklerini
/// doğal dönemsellik ilkesine göre bir arada tutan sözleşme.
/// Dönem bütçesinin zorunlu çıkışlarını ve büyük harcamalarını döneme özel olarak sunar.
/// </summary>
public sealed record PeriodObligationGroup(
    CashFlowPeriod Period,
    IReadOnlyList<ObligationItem> Items)
{
    /// <summary>Gruptaki tüm yükümlülüklerin nominal toplam tutarı.</summary>
    public decimal TotalAmount => Items.Sum(x => x.Amount);

    /// <summary>Bu dönemde ödenmesi gereken zorunlu borç yükümlülükleri (büyük harcamalar hariç).</summary>
    public IEnumerable<ObligationItem> MandatoryItems =>
        Items.Where(x => x.Type != ObligationType.PlannedLargeExpense);

    /// <summary>Bu dönemde planlanan tek seferlik büyük nakit harcama kalemleri.</summary>
    public IEnumerable<ObligationItem> LargeExpenseItems =>
        Items.Where(x => x.Type == ObligationType.PlannedLargeExpense);
}
