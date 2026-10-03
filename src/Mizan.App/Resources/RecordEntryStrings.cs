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

    /// <summary>"Kartla harcama"da birden fazla kart varken ikinci seviyenin başlığı.</summary>
    public const string Baslik_HangiKart = "Hangi kart?";

    /// <summary>"Krediye erken ödeme"de birden fazla kredi varken ikinci seviyenin başlığı.</summary>
    public const string Baslik_HangiKredi = "Hangi kredi?";

    /// <summary>"Gelir değişikliği"nde birden fazla düzenli gelir varken ikinci seviyenin başlığı.</summary>
    public const string Baslik_HangiGelir = "Hangi gelir?";

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

    /// <summary>Gelir değişikliği seçeneği (var olan düzenli gelirin formu).</summary>
    public const string Etiket_GelirDegisikligi = "Gelir değişikliği";

    /// <summary>Gelir değişikliğinin alt satırı.</summary>
    public const string Etiket_GelirDegisikligiAlt = "Zam ya da yeni tutar";

    /// <summary>Kartla harcama seçeneği (var olan kartın formu).</summary>
    public const string Etiket_KartlaHarcama = "Kartla harcama";

    /// <summary>Kartla harcamanın alt satırı.</summary>
    public const string Etiket_KartlaHarcamaAlt = "Tek çekim ya da taksit";

    /// <summary>Krediye erken ödeme seçeneği (var olan kredinin formu).</summary>
    public const string Etiket_ErkenOdeme = "Krediye erken ödeme";

    /// <summary>Krediye erken ödemenin alt satırı.</summary>
    public const string Etiket_ErkenOdemeAlt = "Kapat ya da ara ödeme";

    /// <summary>Kart ödeme şekli seçeneği (simülatör).</summary>
    public const string Etiket_KartOdemeSekli = "Kart ödeme şekli";

    /// <summary>Kart ödeme şeklinin alt satırı.</summary>
    public const string Etiket_KartOdemeSekliAlt = "Asgari ya da tamamı";

    /// <summary>Kredi finansman çekme seçeneği (simülatör).</summary>
    public const string Etiket_KrediFinansman = "Kredi / finansman çek";

    /// <summary>Kredi finansman çekmenin alt satırı.</summary>
    public const string Etiket_KrediFinansmanAlt = "Taksitli geri ödeme";
}
