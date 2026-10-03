using System.Globalization;
using Mizan.App.Resources;
using Mizan.Domain.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Deneme formu başlığını senaryo türünden çözen çevirici (EK-V10, S76-7).
/// </summary>
public sealed class DenemeBasligiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        SimulationScenarioType.CashPurchase or SimulationScenarioType.FutureOneTimePayment => SimulatorStrings.Baslik_NakitOdeme,
        SimulationScenarioType.CreditCardSinglePayment or SimulationScenarioType.CreditCardInstallmentPurchase => SimulatorStrings.Baslik_KartlaHarcama,
        SimulationScenarioType.RecurringPayment => SimulatorStrings.Baslik_DuzenliOdeme,
        SimulationScenarioType.CreditCardPaymentMode => SimulatorStrings.Baslik_KartOdemeSekli,
        "cash" => SimulatorStrings.Baslik_NakitOdeme,
        "card" => SimulatorStrings.Baslik_KartlaHarcama,
        "recurring" => SimulatorStrings.Baslik_DuzenliOdeme,
        "card-payment-mode" => SimulatorStrings.Baslik_KartOdemeSekli,
        _ => SimulatorStrings.Baslik_NakitOdeme
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
