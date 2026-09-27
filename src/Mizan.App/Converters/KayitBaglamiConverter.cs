using System.Globalization;
using System.Text;
using Mizan.App.Resources;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Finansal Yapı satırının bağlam cümlesini kaydın türüne göre yazar: "Her ayın 15. günü",
/// "Sıradaki ödeme 5 Ekim", "12 taksit kaldı · sonraki 15 Ekim" (EK-V6 S2). Tek seferlik kayıtlar
/// uzak olabileceği için yılıyla yazılır.
/// </summary>
public sealed class KayitBaglamiConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly CompositeFormat HerAyGunu = CompositeFormat.Parse(FinancialStructureStrings.Bicim_HerAyGunu);
    private static readonly CompositeFormat TekSeferlik = CompositeFormat.Parse(FinancialStructureStrings.Bicim_TekSeferlik);
    private static readonly CompositeFormat SiradakiOdeme = CompositeFormat.Parse(FinancialStructureStrings.Bicim_SiradakiOdeme);
    private static readonly CompositeFormat KrediBaglami = CompositeFormat.Parse(FinancialStructureStrings.Bicim_KrediBaglami);
    private static readonly CompositeFormat PlanBaglami = CompositeFormat.Parse(FinancialStructureStrings.Bicim_PlanBaglami);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is FinancialRecordRow row ? Baglam(row) : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string Baglam(FinancialRecordRow row) => row.Kind switch
    {
        FinancialRecordKind.RecurringIncome => Bicimle(HerAyGunu, row.DayOfMonth),
        FinancialRecordKind.AdHocIncome or FinancialRecordKind.LargeExpense =>
            Bicimle(TekSeferlik, row.NextDate?.ToString("d MMMM yyyy", Tr)),
        FinancialRecordKind.CreditCard => row.NextDate is { } due
            ? Bicimle(SiradakiOdeme, Kisa(due))
            : FinancialStructureStrings.Etiket_OdemeYok,
        FinancialRecordKind.Loan => Bicimle(KrediBaglami, row.RemainingCount, row.NextDate is { } next ? Kisa(next) : null),
        FinancialRecordKind.PaymentPlan => row.NextDate is { } installment
            ? Bicimle(PlanBaglami, row.RemainingCount, Kisa(installment))
            : FinancialStructureStrings.Etiket_PlanOdemesiYok,
        _ => string.Empty
    };

    private static string Kisa(DateOnly date) => date.ToString("d MMMM", Tr);

    private static string Bicimle(CompositeFormat format, params object?[] args) => string.Format(Tr, format, args);
}
