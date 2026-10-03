using System.Globalization;
using Glif = Mizan.App.Icons.Icons;
using Mizan.App.Resources;
using Mizan.Application.Models;

namespace Mizan.App.Converters;

/// <summary>
/// Tür seçicinin görünen metnini ve ikonunu kurar (EK-V6f). ViewModel yalnız grup türünü ve seçenek
/// anahtarını sunar (kural 03); bölüm başlığı, karo başlığı, parametre <c>AltSatir</c> ise alt satır,
/// <c>Ikon</c> ise karonun ikonu burada <see cref="RecordEntryStrings"/> ve <see cref="Glif"/>'tan
/// bulunur. Parametre <c>Soru</c> ise sayfanın başlığıdır: grup yoksa "Ne eklemek istiyorsun?", kaydı
/// sorulan grupta "Hangi kart?" gibi ikinci seviyenin sorusu (S77-5). Simülatör (V10c) kendi grup ve
/// anahtarlarını buraya ekler.
/// </summary>
public sealed class SeciciMetniConverter : IValueConverter
{
    private const string AltSatirParametresi = "AltSatir";
    private const string IkonParametresi = "Ikon";
    private const string SoruParametresi = "Soru";

    private static readonly Dictionary<RecordEntryGroup, string> GrupAdlari = new()
    {
        [RecordEntryGroup.Income] = RecordEntryStrings.Etiket_GrupGelir,
        [RecordEntryGroup.Card] = RecordEntryStrings.Etiket_GrupKart,
        [RecordEntryGroup.Loan] = RecordEntryStrings.Etiket_GrupKredi,
        [RecordEntryGroup.Payment] = RecordEntryStrings.Etiket_GrupOdeme
    };

    private static readonly Dictionary<RecordEntryGroup, string> Sorular = new()
    {
        [RecordEntryGroup.Income] = RecordEntryStrings.Baslik_HangiGelir,
        [RecordEntryGroup.Card] = RecordEntryStrings.Baslik_HangiKart,
        [RecordEntryGroup.Loan] = RecordEntryStrings.Baslik_HangiKredi
    };

    private static readonly Dictionary<string, (string Baslik, string AltSatir, string Ikon)> Secenekler = new()
    {
        ["recurring-income"] = (RecordEntryStrings.Etiket_DuzenliGelir, RecordEntryStrings.Etiket_DuzenliGelirAlt, Glif.Repeat),
        ["income"] = (RecordEntryStrings.Etiket_TekSeferlikGelir, RecordEntryStrings.Etiket_TekSeferlikGelirAlt, Glif.AutoAwesome),
        ["income-change"] = (RecordEntryStrings.Etiket_GelirDegisikligi, RecordEntryStrings.Etiket_GelirDegisikligiAlt, Glif.TrendingUp),
        ["credit-card"] = (RecordEntryStrings.Etiket_KrediKarti, RecordEntryStrings.Etiket_KrediKartiAlt, Glif.CreditCard),
        ["card"] = (RecordEntryStrings.Etiket_KartlaHarcama, RecordEntryStrings.Etiket_KartlaHarcamaAlt, Glif.ShoppingBag),
        ["bank-loan"] = (RecordEntryStrings.Etiket_BankadakiKredi, RecordEntryStrings.Etiket_BankadakiKrediAlt, Glif.AccountBalance),
        ["loan-prepayment"] = (RecordEntryStrings.Etiket_ErkenOdeme, RecordEntryStrings.Etiket_ErkenOdemeAlt, Glif.FastForward),
        ["financing"] = (RecordEntryStrings.Etiket_KrediFinansman, RecordEntryStrings.Etiket_KrediFinansmanAlt, Glif.AccountBalance),
        ["cash"] = (RecordEntryStrings.Etiket_NakitOdeme, RecordEntryStrings.Etiket_NakitOdemeAlt, Glif.Payments),
        ["recurring"] = (RecordEntryStrings.Etiket_DuzenliOdeme, RecordEntryStrings.Etiket_DuzenliOdemeAlt, Glif.EventAvailable),
        ["cash-debt"] = (RecordEntryStrings.Etiket_TaksitliBorc, RecordEntryStrings.Etiket_TaksitliBorcAlt, Glif.PieChart),
        ["payment-plan"] = (RecordEntryStrings.Etiket_OdemePlani, RecordEntryStrings.Etiket_OdemePlaniAlt, Glif.BarChart),
        ["card-payment-mode"] = (RecordEntryStrings.Etiket_KartOdemeSekli, RecordEntryStrings.Etiket_KartOdemeSekliAlt, Glif.CreditCard)
    };

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value, parameter as string) switch
    {
        (RecordEntryGroup group, SoruParametresi) => Sorular.GetValueOrDefault(group, RecordEntryStrings.Baslik_NeEklemek),
        (null, SoruParametresi) => RecordEntryStrings.Baslik_NeEklemek,
        (RecordEntryGroup group, _) => GrupAdlari[group],
        (string key, AltSatirParametresi) when Secenekler.TryGetValue(key, out var secenek) => secenek.AltSatir,
        (string key, IkonParametresi) when Secenekler.TryGetValue(key, out var secenek) => secenek.Ikon,
        (string key, _) when Secenekler.TryGetValue(key, out var secenek) => secenek.Baslik,
        _ => string.Empty
    };

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
