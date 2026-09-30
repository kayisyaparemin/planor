using System.Globalization;
using System.Text;
using Mizan.App.Resources;

namespace Mizan.App.Converters;

/// <summary>
/// ViewModel'in sunduğu tempo puanını (harcanan − geçen süre, tam sayı; S72-4) tek cümleye çevirir:
/// "Harcama, geçen sürenin 4 puan önünde." Yönü işaret seçer, cümle hep pozitif sayı taşır; 0'da
/// "aynı hızda" denir. Tempo yoksa boş metin döner.
/// </summary>
public sealed class TempoCumlesiConverter : IValueConverter
{
    private static readonly CompositeFormat Onde = CompositeFormat.Parse(DashboardStrings.Bicim_TempoOnde);
    private static readonly CompositeFormat Geride = CompositeFormat.Parse(DashboardStrings.Bicim_TempoGeride);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            int points when points > 0 => string.Format(CultureInfo.InvariantCulture, Onde, points),
            int points when points < 0 => string.Format(CultureInfo.InvariantCulture, Geride, -points),
            int => DashboardStrings.Cumle_TempoAyniHiz,
            _ => string.Empty
        };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
