using System.Globalization;
using System.Text;
using Mizan.App.Resources;
using Mizan.Domain.Models;

namespace Mizan.App.Converters;

/// <summary>Kart ödeme tiplerinin ve kurallarının ekrandaki metni; iki kart converter'ı ortak kullanır.</summary>
internal static class KartOdemeMetni
{
    private static readonly CompositeFormat KuralBicimi = CompositeFormat.Parse(CardControlStrings.Bicim_OdemeKurali);
    private static readonly CompositeFormat VarsayimBicimi = CompositeFormat.Parse(CardControlStrings.Bicim_Varsayim);

    public static string Tip(CreditCardPaymentType? type) => type switch
    {
        CreditCardPaymentType.Minimum => CardControlStrings.Etiket_OdemeAsgari,
        CreditCardPaymentType.FullStatement => CardControlStrings.Etiket_OdemeTamami,
        CreditCardPaymentType.FixedAmount => CardControlStrings.Etiket_OdemeSabit,
        _ => CardControlStrings.Etiket_OdemeBelirsiz
    };

    /// <summary>"Asgari · bu vade için" biçiminde tip ve kaynağı birleştirir.</summary>
    public static string Kural(string tip, string kaynak) =>
        string.Format(CultureInfo.InvariantCulture, KuralBicimi, tip, kaynak);

    /// <summary>"Her ekstrede sor · varsayım Asgari"; varsayım yoksa yalnız "Her ekstrede sor".</summary>
    public static string HerEkstredeSor(ProjectionFallbackStrategy fallback)
    {
        CreditCardPaymentType? type = fallback switch
        {
            ProjectionFallbackStrategy.Minimum => CreditCardPaymentType.Minimum,
            ProjectionFallbackStrategy.FullStatement => CreditCardPaymentType.FullStatement,
            ProjectionFallbackStrategy.FixedAmount => CreditCardPaymentType.FixedAmount,
            _ => null
        };

        return type is null
            ? CardControlStrings.Etiket_HerEkstredeSor
            : Kural(CardControlStrings.Etiket_HerEkstredeSor, string.Format(CultureInfo.InvariantCulture, VarsayimBicimi, Tip(type)));
    }
}
