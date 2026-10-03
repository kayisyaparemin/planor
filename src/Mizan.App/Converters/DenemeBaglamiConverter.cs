using System.Globalization;
using System.Text;
using Mizan.App.Resources;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Simülatördeki deneme satırının bağlam cümlesini yazar: "Nakit ödeme · 15 Kas 2026" (EK-V10 S3). Tarihi
/// geçen denemede bağlamın yerine "Tarihi geçti" durur (S76-5): kullanıcı denemenin neden hesaba girmediğini
/// satırın kendisinde görür. Deneme aylar sonrasına kurulabildiği için tarih yılıyla yazılır.
/// </summary>
public sealed class DenemeBaglamiConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly CompositeFormat Baglam = CompositeFormat.Parse(SimulatorStrings.Bicim_DenemeBaglami);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is SimulationConditionRow row ? Yaz(row) : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string Yaz(SimulationConditionRow row) => row.Issue switch
    {
        SimulationConditionIssue.DatePassed => SimulatorStrings.Etiket_TarihiGecti,
        _ => string.Format(Tr, Baglam, TurAdi(row.Type), row.Date.ToString("d MMM yyyy", Tr))
    };

    // Nakit ödeme seçeneği iki motor türünü kapsar; ikisi de kullanıcıya aynı adla görünür (S76-7).
    private static string TurAdi(SimulationScenarioType type) => type switch
    {
        SimulationScenarioType.CashPurchase or SimulationScenarioType.FutureOneTimePayment => SimulatorStrings.Etiket_NakitOdeme,
        _ => SimulationScenarioCatalog.TypeText(type)
    };
}
