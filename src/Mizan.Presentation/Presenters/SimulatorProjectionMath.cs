using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Presenters;

/// <summary>
/// Simülasyon projeksiyonunda tek bir dönemin toplam nakit ihtiyacını ve ihtiyaç kırılımını hesaplayan saf sunum yardımcısı.
/// </summary>
public static class SimulatorProjectionMath
{
    /// <summary>
    /// Bir dönemin zorunlu giderler, yaşam gideri havuzu, büyük harcamalar ve faiz yükünden oluşan toplam nakit ihtiyacını hesaplar.
    /// </summary>
    public static decimal PeriodNeed(CashFlowPeriodProjection row) =>
        row.MandatoryOutflow +
        row.VariableExpenseAllowance +
        row.PlannedLargeCashExpenses +
        row.DeficitFinancingInterest;

    /// <summary>
    /// Dönemin toplam ihtiyacını kalem kalem metrik listesi olarak derler.
    /// </summary>
    public static List<DetailMetric> BuildNeedBreakdown(CashFlowPeriodProjection row)
    {
        var rows = new List<DetailMetric>();
        AddIfNonZero(rows, "Krediler", row.LoanPayments);
        AddIfNonZero(rows, "Kredi kartları", row.CreditCardPayments);
        AddIfNonZero(rows, "Geçici ödemeler", row.TemporaryPayments);
        AddIfNonZero(rows, "Taksit / finansman", row.InstallmentPayments);
        AddIfNonZero(rows, "Planlı nakit ödemeler", row.PlannedLargeCashExpenses, DetailSemanticType.Expense);
        AddIfNonZero(rows, "Diğer planlı ödemeler", row.OtherScheduledPayments);
        AddIfNonZero(rows, "Tahmini yaşam gideri", row.VariableExpenseAllowance, DetailSemanticType.Expense);
        AddIfNonZero(rows, "Finansman açığı faizi", row.DeficitFinancingInterest, DetailSemanticType.Interest);
        rows.Add(new DetailMetric("Toplam", PeriodNeed(row), DetailSemanticType.Mandatory) { IsTotal = true });
        return rows;
    }

    private static void AddIfNonZero(List<DetailMetric> rows, string label, decimal amount, DetailSemanticType semantic = DetailSemanticType.Mandatory)
    {
        if (amount != 0m)
        {
            rows.Add(new DetailMetric(label, amount, semantic));
        }
    }
}
