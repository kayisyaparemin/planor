using System.Globalization;
using Mizan.App.Resources;
using Mizan.Domain.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Simülatör kart ödeme şekli ve kapsamı seçicilerinin görünen metinlerini kurar (V10c).
/// </summary>
public sealed class KartOdemeSecenegiConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        CreditCardPaymentType.FullStatement => SimulatorStrings.Secenek_TamaminiOde,
        CreditCardPaymentType.Minimum => SimulatorStrings.Secenek_AsgariOde,
        IEnumerable<CreditCardPaymentType> list => list.Select(x => Convert(x, targetType, parameter, culture)).ToList(),
        true when parameter as string == "Kapsam" || value is bool => SimulatorStrings.Secenek_TumEkstreler,
        false when parameter as string == "Kapsam" || value is bool => SimulatorStrings.Secenek_YalnizcaBuEkstre,
        IEnumerable<bool> list => list.Select(x => x ? SimulatorStrings.Secenek_TumEkstreler : SimulatorStrings.Secenek_YalnizcaBuEkstre).ToList(),
        _ => Binding.DoNothing
    };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        string s when s == SimulatorStrings.Secenek_TamaminiOde => CreditCardPaymentType.FullStatement,
        string s when s == SimulatorStrings.Secenek_AsgariOde => CreditCardPaymentType.Minimum,
        string s when s == SimulatorStrings.Secenek_TumEkstreler => true,
        string s when s == SimulatorStrings.Secenek_YalnizcaBuEkstre => false,
        _ => Binding.DoNothing
    };
}
