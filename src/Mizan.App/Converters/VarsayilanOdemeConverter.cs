using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Kartın varsayılan ödeme şeklini yazar: "Tamamı", "Asgari" ya da
/// "Her ekstrede sor · varsayım Asgari" (EK-V7 S4).
/// </summary>
public sealed class VarsayilanOdemeConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CardPaymentDefault rule)
        {
            return string.Empty;
        }

        return rule.Strategy switch
        {
            CreditCardPaymentStrategy.Minimum => KartOdemeMetni.Tip(CreditCardPaymentType.Minimum),
            CreditCardPaymentStrategy.FullStatement => KartOdemeMetni.Tip(CreditCardPaymentType.FullStatement),
            CreditCardPaymentStrategy.FixedAmount => KartOdemeMetni.Tip(CreditCardPaymentType.FixedAmount),
            _ => KartOdemeMetni.HerEkstredeSor(rule.Fallback)
        };
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
