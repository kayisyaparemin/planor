using System.Globalization;
using Mizan.App.Resources;
using Mizan.Domain.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Erken ödeme şeklinin görünen adını kurar (EK-V6c). ViewModel ham <see cref="LoanPrepaymentMode"/>
/// sunar; listedeki satır, şekil seçicisinin seçenekleri ve seçili değer aynı adları bu çeviriciden
/// alır, seçilen ad geri şekle çevrilir.
/// </summary>
public sealed class ErkenOdemeTuruConverter : IValueConverter
{
    private static readonly Dictionary<LoanPrepaymentMode, string> Adlar = new()
    {
        [LoanPrepaymentMode.FullClosure] = LoanFormStrings.Etiket_TamamenKapatma,
        [LoanPrepaymentMode.ReduceTerm] = LoanFormStrings.Etiket_VadeKisalir,
        [LoanPrepaymentMode.ReduceInstallment] = LoanFormStrings.Etiket_TaksitDuser
    };

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        LoanPrepaymentMode mode => Adlar[mode],
        IEnumerable<LoanPrepaymentMode> modes => modes.Select(m => Adlar[m]).ToList(),
        _ => Binding.DoNothing
    };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Seçici kaynak değişirken seçimi geçici olarak boşaltır; boş seçim şekli değiştirmez.
        foreach (var (mode, ad) in Adlar)
        {
            if (ad == value as string)
            {
                return mode;
            }
        }

        return Binding.DoNothing;
    }
}
