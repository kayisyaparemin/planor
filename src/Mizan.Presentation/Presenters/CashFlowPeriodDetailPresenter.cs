using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Presenters;

/// <summary>
/// Bir nakit akış döneminin projeksiyon verilerini Dönem Ayrıntısı (V9) ekranının ihtiyaç duyduğu
/// zengin metrik, ödeme satırları ve senaryo karşılaştırma modellerine dönüştüren sunum yardımcısı.
/// </summary>
public sealed class CashFlowPeriodDetailPresenter
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Verilen nakit akış dönemi projeksiyonunu detay sunum paketine dönüştürür.
    /// </summary>
    public CashFlowPeriodDetailData Build(
        CashFlowPeriodProjection scenario,
        CashFlowPeriodProjection? baseline = null,
        bool isSimulationScenario = false)
    {
        if (baseline is not null && baseline.Period != scenario.Period)
        {
            throw new InvalidOperationException("Mevcut Plan ile Yeni Plan aynı döneme ait olmalıdır.");
        }

        var periodNeed = scenario.MandatoryOutflow + scenario.VariableExpenseAllowance +
                         scenario.PlannedLargeCashExpenses + scenario.DeficitFinancingInterest;
        var incomeCoverage = scenario.TotalIncome - periodNeed;

        return new CashFlowPeriodDetailData
        {
            PeriodStart = scenario.PeriodStart,
            PeriodTitle = $"{scenario.PeriodStart.ToString("dd MMMM yyyy", TurkishCulture)} Dönemi",
            PeriodWindowText = $"{scenario.PeriodStart.ToString("dd MMMM", TurkishCulture)} – {scenario.PeriodEnd.ToString("dd MMMM", TurkishCulture)}",
            IsSimulationScenario = isSimulationScenario,
            OpeningBalanceSummary = Summary("DÖNEM BAŞI", scenario.OpeningBalance, scenario.OpeningBalance < 0m ? DetailSemanticType.Deficit : DetailSemanticType.Projection),
            PeriodNeedSummary = Summary("BU DÖNEM GEREKEN", periodNeed, DetailSemanticType.Mandatory),
            IncomeCoverageSummary = Summary(incomeCoverage >= 0m ? "GELİRLERDEN KALAN" : "GELİRLERİN KARŞILAMADIĞI", Math.Abs(incomeCoverage), incomeCoverage >= 0m ? DetailSemanticType.Surplus : DetailSemanticType.Deficit),
            IncomeCoverageMessage = IncomeCoverageMessage(scenario, incomeCoverage),
            NeedBreakdownRows = BuildNeedBreakdown(scenario, periodNeed),
            IncomeSummary = Summary(isSimulationScenario ? "DÖNEM GELİRLERİ" : "GELİR", scenario.TotalIncome, DetailSemanticType.Income),
            MandatorySummary = Summary("ZORUNLU", scenario.MandatoryOutflow, DetailSemanticType.Mandatory),
            NetSurplusSummary = Summary("DÖNEM NETİ", scenario.EstimatedSurplus, scenario.EstimatedSurplus < 0m ? DetailSemanticType.Deficit : DetailSemanticType.Surplus),
            EndingSummary = Summary("DÖNEM SONU", scenario.EndingBalance, scenario.EndingBalance < 0m ? DetailSemanticType.Deficit : DetailSemanticType.Projection),
            CashFlow = BuildFlow(scenario),
            MandatoryRows = BuildMandatoryRows(scenario),
            InterestRows = BuildInterestRows(scenario),
            CardInterestRows = BuildCardInterestRows(scenario),
            PaymentRows = BuildPaymentRows(scenario, !isSimulationScenario),
            ComparisonRows = baseline is null || isSimulationScenario ? [] : BuildComparisonRows(baseline, scenario),
            Deficit = scenario.HasCarryOverDeficit
                ? new DetailDeficitCallout(scenario.CarryOverDeficit, scenario.DeficitCoveredThisPeriod, scenario.RemainingCarryOverDeficit, scenario.RecoveredCarryOverDeficit)
                : null
        };
    }

    private static DetailMetric Summary(string label, decimal amount, DetailSemanticType semantic) =>
        new(label, amount, semantic) { DecimalPlaces = 0 };

    private static List<DetailMetric> BuildFlow(CashFlowPeriodProjection row)
    {
        var result = new List<DetailMetric>
        {
            new("Gelir", row.TotalIncome, DetailSemanticType.Income) { ShowPositiveSign = true },
            new("Zorunlu ödemeler", -row.MandatoryOutflow, DetailSemanticType.Mandatory),
            new("Zorunlular sonrası", row.AvailableAfterMandatory, DetailSemanticType.Projection) { IsTotal = true }
        };

        if (row.OpeningBalance > 0m)
        {
            result.Add(new DetailMetric("Dönem başı durumu", row.OpeningBalance, DetailSemanticType.Surplus) { ShowPositiveSign = true });
        }
        else if (row.HasCarryOverDeficit)
        {
            result.Add(new DetailMetric("Devreden açık", -row.CarryOverDeficit, DetailSemanticType.Deficit));
            result.Add(new DetailMetric("Açık kapatıldıktan sonra kalan", row.AvailableAfterCarryOverDeficit, DetailSemanticType.Projection));
        }

        AddIfNonZero(result, "Tahmini yaşam gideri", -row.VariableExpenseAllowance, DetailSemanticType.Expense);
        AddIfNonZero(result, "Planlı büyük ödeme", -row.PlannedLargeCashExpenses, DetailSemanticType.Expense);
        AddIfNonZero(result, "Faiz yükü", -row.DeficitFinancingInterest, DetailSemanticType.Interest);

        result.Add(new DetailMetric("Dönem sonu tahmini durum", row.EndingBalance, row.EndingBalance < 0m ? DetailSemanticType.Deficit : DetailSemanticType.Surplus) { IsTotal = true });
        return result;
    }

    private static List<DetailMetric> BuildMandatoryRows(CashFlowPeriodProjection row)
    {
        var rows = new List<DetailMetric>();
        AddIfNonZero(rows, "Krediler", row.LoanPayments, DetailSemanticType.Mandatory);
        AddIfNonZero(rows, "Kredi Kartları", row.CreditCardPayments, DetailSemanticType.Mandatory);
        AddIfNonZero(rows, "Geçici Ödeme Planları", row.TemporaryPayments, DetailSemanticType.Mandatory);
        AddIfNonZero(rows, "Taksit / Finansman", row.InstallmentPayments, DetailSemanticType.Mandatory);
        AddIfNonZero(rows, "Diğer Planlı Ödemeler", row.OtherScheduledPayments, DetailSemanticType.Mandatory);
        return rows;
    }

    private static List<DetailMetric> BuildInterestRows(CashFlowPeriodProjection row)
    {
        if (row.TotalInterestGenerated == 0m)
        {
            return [];
        }

        var result = new List<DetailMetric>();
        AddIfNonZero(result, "Devreden kart borcu faizi", row.CardInterestGenerated, DetailSemanticType.Interest);
        AddIfNonZero(result, "Finansman açığı faiz yükü", row.DeficitFinancingInterest, DetailSemanticType.Interest);
        result.Add(new DetailMetric("Toplam", row.TotalInterestGenerated, DetailSemanticType.Interest) { IsTotal = true });
        return result;
    }

    private static List<DetailMetric> BuildCardInterestRows(CashFlowPeriodProjection row) =>
        row.CardPaymentStatuses
            .Where(x => x.CarryInterest > 0m)
            .Select(x => new DetailMetric(x.CardName, x.CarryInterest, DetailSemanticType.Interest))
            .ToList();

    private static List<DetailMetric> BuildNeedBreakdown(CashFlowPeriodProjection row, decimal totalNeed)
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
        rows.Add(new DetailMetric("Toplam", totalNeed, DetailSemanticType.Mandatory) { IsTotal = true });
        return rows;
    }

    private static List<DetailPaymentRow> BuildPaymentRows(CashFlowPeriodProjection row, bool allowCardChange)
    {
        var result = row.MandatoryItems.Select(item => ToPaymentRow(item, row.CardPaymentStatuses, row.PeriodStart, allowCardChange)).ToList();
        result.AddRange(row.LargeExpenseItems.Select(exp => new DetailPaymentRow(exp.ExactDate, exp.Name, "Planlı Büyük Ödeme", exp.Amount, DetailSemanticType.Expense) { PeriodDate = row.PeriodStart, Note = exp.Note }));
        result.AddRange(row.CardPaymentStatuses.Where(x => x.Payment is null).Select(card => new DetailPaymentRow(card.PaymentDueDate, card.CardName, "Kredi Kartı", null, DetailSemanticType.Mandatory) { PeriodDate = card.AssignedPeriodDate, IsUndetermined = true, Note = "Gerçek ödeme planı henüz belirlenmedi." }));
        return result.OrderBy(x => x.Date).ThenBy(x => x.Name).ToList();
    }

    private static DetailPaymentRow ToPaymentRow(ObligationItem item, IReadOnlyList<CreditCardPaymentProjectionStatus> cards, DateOnly periodStart, bool allowCardChange)
    {
        var status = item.Type == ObligationType.CreditCard ? cards.FirstOrDefault(x => x.CardId == item.PaymentId && x.PaymentDueDate == item.DueDate) : null;
        return new DetailPaymentRow(item.DueDate, item.Name, Category(item.Type), item.Amount, DetailSemanticType.Mandatory)
        {
            PeriodDate = periodStart,
            IsEstimated = item.IsEstimate,
            Note = item.Detail,
            CreditCardId = item.Type == ObligationType.CreditCard && item.PaymentId != Guid.Empty ? item.PaymentId : null,
            CardPaymentType = status?.PaymentType,
            CanChangeCardPaymentMode = allowCardChange && status?.PaymentType is CreditCardPaymentType.Minimum or CreditCardPaymentType.FullStatement
        };
    }

    private static List<DetailComparisonRow> BuildComparisonRows(CashFlowPeriodProjection b, CashFlowPeriodProjection s) =>
    [
        new("Zorunlu", s.MandatoryOutflow - b.MandatoryOutflow),
        new("Dönem neti", s.EstimatedSurplus - b.EstimatedSurplus, s.EstimatedSurplus < b.EstimatedSurplus),
        new("Faiz yükü", s.TotalInterestGenerated - b.TotalInterestGenerated),
        new("Dönem sonu durumu", s.EndingBalance - b.EndingBalance, s.EndingBalance < b.EndingBalance)
    ];

    private static string IncomeCoverageMessage(CashFlowPeriodProjection row, decimal coverage) =>
        coverage < 0m && row.EndingBalance >= 0m
            ? $"Bu ay dönem gelirlerin ihtiyacın {Money(Math.Abs(coverage))} altında kalıyor. Fark dönem başındaki finansal durumundan karşılanıyor."
            : row.EndingBalance < 0m
                ? $"Bu dönem sonunda yaklaşık {Money(Math.Abs(row.EndingBalance))} finansman açığı oluşuyor."
                : coverage > 0m
                    ? $"Dönem gelirlerinden {Money(coverage)} kalıyor."
                    : "Dönem gelirleri bu ayki toplam ihtiyacı tam karşılıyor.";

    private static void AddIfNonZero(List<DetailMetric> rows, string label, decimal amount, DetailSemanticType semantic = DetailSemanticType.Mandatory)
    {
        if (amount != 0m)
        {
            rows.Add(new DetailMetric(label, amount, semantic));
        }
    }

    private static string Category(ObligationType type) => type switch
    {
        ObligationType.Loan => "Kredi",
        ObligationType.CreditCard => "Kredi Kartı",
        ObligationType.TemporaryPayment => "Geçici Plan",
        ObligationType.InstallmentPayment => "Taksit / Finansman",
        ObligationType.PlannedLargeExpense => "Planlı Büyük Ödeme",
        _ => "Planlı Ödeme"
    };

    private static string Money(decimal value) => $"{value.ToString("N2", TurkishCulture)} TL";
}
