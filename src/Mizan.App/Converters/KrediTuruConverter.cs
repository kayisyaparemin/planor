using System.Globalization;
using Mizan.App.Resources;
using Mizan.Domain.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Kredi türü seçicisinin görünen adlarını kurar (EK-V6c). ViewModel ham <see cref="LoanKind"/> sunar;
/// seçenek listesi de seçili değer de bu çeviriciden geçer, seçilen ad geri türe çevrilir.
/// </summary>
public sealed class KrediTuruConverter : IValueConverter
{
    private static readonly Dictionary<LoanKind, string> Adlar = new()
    {
        [LoanKind.Consumer] = LoanFormStrings.Etiket_TurIhtiyac,
        [LoanKind.HousingFixed] = LoanFormStrings.Etiket_TurKonutSabit,
        [LoanKind.HousingVariable] = LoanFormStrings.Etiket_TurKonutDegisken
    };

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        LoanKind kind => Adlar[kind],
        IEnumerable<LoanKind> kinds => kinds.Select(k => Adlar[k]).ToList(),
        _ => Binding.DoNothing
    };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Seçici kaynak değişirken seçimi geçici olarak boşaltır; boş seçim türü değiştirmez.
        foreach (var (kind, ad) in Adlar)
        {
            if (ad == value as string)
            {
                return kind;
            }
        }

        return Binding.DoNothing;
    }
}
