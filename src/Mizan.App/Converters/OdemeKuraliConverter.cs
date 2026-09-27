using System.Globalization;
using Mizan.App.Resources;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Sonraki ödeme satırının kuralını yazar: "Asgari · bu vade için", "Tamamı · kartın varsayılanı",
/// "Asgari · varsayım" (EK-V7 S3). Karar yoksa "Belirlenmedi".
/// </summary>
public sealed class OdemeKuraliConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not UpcomingPaymentItem item)
        {
            return string.Empty;
        }

        var source = item.Resolution switch
        {
            CreditCardPaymentResolution.DueDateOverride => CardControlStrings.Etiket_KuralVadeyeOzel,
            CreditCardPaymentResolution.GeneralStrategy => CardControlStrings.Etiket_KuralKartinVarsayilani,
            CreditCardPaymentResolution.ProjectionFallback => CardControlStrings.Etiket_KuralVarsayim,
            _ => null
        };

        var type = KartOdemeMetni.Tip(item.AppliedPaymentType);
        return source is null ? type : KartOdemeMetni.Kural(type, source);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
