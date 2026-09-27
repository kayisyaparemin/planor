using System.Globalization;
using System.Text;
using Mizan.App.Resources;

namespace Mizan.App.Converters;

/// <summary>
/// Gruptaki gizli satır sayısını taşma metnine çevirir ("+2 daha"); gizli satır yoksa boş metin
/// döner, <c>ListCard</c> da taşma satırını gizler (EK-V6, GS21).
/// </summary>
public sealed class FazlaKayitConverter : IValueConverter
{
    private static readonly CompositeFormat Bicim = CompositeFormat.Parse(FinancialStructureStrings.Bicim_FazlaKayit);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int hidden && hidden > 0 ? string.Format(CultureInfo.InvariantCulture, Bicim, hidden) : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
