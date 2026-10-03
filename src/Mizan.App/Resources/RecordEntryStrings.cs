#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kayıt türü seçicide (EK-V6f) görünen metinlerin kataloğu: sayfa başlığı, grup adları, seçeneklerin
/// başlıkları ve ≤ 24 harflik alt satırları. Değerler <c>Resources/Strings/RecordEntryStrings.json</c> ile
/// aynıdır; GK5 sınırları JSON'dan denetlenir. Simülatör (V10c) aynı anahtarlı türlerde bu metni paylaşır.
/// </summary>
public static class RecordEntryStrings
{
    /// <summary>Seçici sayfasının başlığı.</summary>
    public const string Baslik_NeEklemek = "Ne eklemek istiyorsun?";

    /// <summary>Gelir bölümünün başlığı; Finansal Yapı'daki "Gelirler" grubuna düşer.</summary>
    public const string Etiket_GrupGelir = "GELİR";

    /// <summary>Kart bölümünün başlığı; "Kartlar" grubuna düşer.</summary>
    public const string Etiket_GrupKart = "KART";

    /// <summary>Kredi bölümünün başlığı; "Krediler" grubuna düşer.</summary>
    public const string Etiket_GrupKredi = "KREDİ";

    /// <summary>Ödeme bölümünün başlığı; "Ödemeler" grubuna düşer.</summary>
    public const string Etiket_GrupOdeme = "ÖDEME";

    /// <summary>Nakit ödeme seçeneği (planlı büyük harcama formu).</summary>
    public const string Etiket_NakitOdeme = "Nakit ödeme";

    /// <summary>Nakit ödemenin alt satırı.</summary>
    public const string Etiket_NakitOdemeAlt = "Seçtiğin gün düşer";

    /// <summary>Düzenli ödeme seçeneği (ödeme planı formu).</summary>
    public const string Etiket_DuzenliOdeme = "Düzenli ödeme";

    /// <summary>Düzenli ödemenin alt satırı.</summary>
    public const string Etiket_DuzenliOdemeAlt = "Her ay aynı tutar";

    /// <summary>Taksitli borç seçeneği (ödeme planı formu).</summary>
    public const string Etiket_TaksitliBorc = "Taksitli borç";

    /// <summary>Taksitli borcun alt satırı.</summary>
    public const string Etiket_TaksitliBorcAlt = "Borcu taksitle öde";

    /// <summary>Düzenli gelir seçeneği.</summary>
    public const string Etiket_DuzenliGelir = "Düzenli gelir";

    /// <summary>Düzenli gelirin alt satırı.</summary>
    public const string Etiket_DuzenliGelirAlt = "Her ay aynı gün yatar";

    /// <summary>Tek seferlik gelir seçeneği.</summary>
    public const string Etiket_TekSeferlikGelir = "Tek seferlik gelir";

    /// <summary>Tek seferlik gelirin alt satırı.</summary>
    public const string Etiket_TekSeferlikGelirAlt = "Prim, satış, iade";

    /// <summary>Kredi kartı seçeneği.</summary>
    public const string Etiket_KrediKarti = "Kredi kartı";

    /// <summary>Kredi kartının alt satırı.</summary>
    public const string Etiket_KrediKartiAlt = "Limit ve ekstre günleri";

    /// <summary>Bankadaki kredi seçeneği.</summary>
    public const string Etiket_BankadakiKredi = "Bankadaki kredi";

    /// <summary>Bankadaki kredinin alt satırı.</summary>
    public const string Etiket_BankadakiKrediAlt = "Devam eden taksitler";

    /// <summary>Ödeme planı seçeneği.</summary>
    public const string Etiket_OdemePlani = "Ödeme planı";

    /// <summary>Ödeme planının alt satırı.</summary>
    public const string Etiket_OdemePlaniAlt = "Tutarı aydan aya değişir";
}
