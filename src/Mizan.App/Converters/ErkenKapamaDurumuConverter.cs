using System.Globalization;
using System.Text;
using Mizan.App.Resources;
using Mizan.Application.Models;
using Mizan.Presentation.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Erken kapama satırının durumunu önerinin sonucuna göre yazar: "18 Eyl 2027 · 21.400 ₺ ile kapat",
/// "Kapama planlı · 18 Eyl 2027", "Kapatmak açık oluşturur" (EK-V8 S5). Eskide bu cümleleri ViewModel kuruyordu;
/// burada satır ham değer taşır, tarih ve tutar kültürüyle sunum kenarında biçimlenir.
/// </summary>
public sealed class ErkenKapamaDurumuConverter : IValueConverter
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly CompositeFormat KapamaOnerisi = CompositeFormat.Parse(FuturePeriodsStrings.Bicim_KapamaOnerisi);
    private static readonly CompositeFormat KapamaPlanli = CompositeFormat.Parse(FuturePeriodsStrings.Bicim_KapamaPlanli);

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is LoanPayoffRow row ? Durum(row) : string.Empty;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string Durum(LoanPayoffRow row) => row.Status switch
    {
        LoanPayoffAdviceStatus.Recommended => string.Format(Tr, KapamaOnerisi, Kisa(row.Date), ParaConverter.Bicimle(row.PayoffAmount)),
        LoanPayoffAdviceStatus.AlreadyClosing => string.Format(Tr, KapamaPlanli, Kisa(row.Date)),
        LoanPayoffAdviceStatus.NoSafeMonth => FuturePeriodsStrings.Etiket_KapatmakAcikOlusturur,
        LoanPayoffAdviceStatus.NotWorthIt => FuturePeriodsStrings.Etiket_KapatmakKazandirmiyor,
        LoanPayoffAdviceStatus.NeedsPrincipal => FuturePeriodsStrings.Etiket_AnaparaGerekli,
        _ => string.Empty
    };

    // Kapama yılı uzak olabilir; yıl yazılır, ay kısaltılır (konsept: "18 Eyl 2027").
    private static string? Kisa(DateOnly? date) => date?.ToString("d MMM yyyy", Tr);
}
