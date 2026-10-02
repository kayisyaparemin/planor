using System.Globalization;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// 12 dönem karosunu dönemin başladığı ayla adlandırır: "Ekim 2026", "Kasım". Yıl yalnız ilk karoda ve yılın
/// ilk döneminde yazılır (GS26-1); her karoda tekrarlanırsa dar karoda tutarla yarışır ve yeni bir şey söylemez.
/// </summary>
public sealed class DonemAyiConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string AyVeYil = "MMMM yyyy";
    private const string YalnizAy = "MMMM";

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is FuturePeriodTile tile
            ? tile.PeriodStart.ToString(tile.ShowsYear ? AyVeYil : YalnizAy, Tr)
            : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
