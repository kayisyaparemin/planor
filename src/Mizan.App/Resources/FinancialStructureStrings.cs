#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Finansal Yapı ekranında (EK-V6) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/FinancialStructureStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// </summary>
public static class FinancialStructureStrings
{
    /// <summary>Gelirler grubunun etiketi.</summary>
    public const string Etiket_Gelirler = "GELİRLER";

    /// <summary>Kartlar grubunun etiketi.</summary>
    public const string Etiket_Kartlar = "KARTLAR";

    /// <summary>Krediler grubunun etiketi.</summary>
    public const string Etiket_Krediler = "KREDİLER";

    /// <summary>Ödeme planları ve büyük harcamalar grubunun etiketi.</summary>
    public const string Etiket_Odemeler = "ÖDEMELER";

    /// <summary>Düzenli gelirin bağlamı: {0} ayın günü ("15'i / 3'ü" ek uyumuna düşmemek için sıra sayısı).</summary>
    public const string Bicim_HerAyGunu = "Her ayın {0}. günü";

    /// <summary>Tek seferlik gelir ya da büyük harcamanın bağlamı: {0} tarih.</summary>
    public const string Bicim_TekSeferlik = "Tek seferlik · {0}";

    /// <summary>Kartın bağlamı: {0} sıradaki ödemenin vadesi.</summary>
    public const string Bicim_SiradakiOdeme = "Sıradaki ödeme {0}";

    /// <summary>Önümüzdeki ödemesi olmayan kartın bağlamı.</summary>
    public const string Etiket_OdemeYok = "Önümüzdeki ödeme yok";

    /// <summary>Kredinin bağlamı: {0} kalan taksit, {1} sonraki ödeme tarihi.</summary>
    public const string Bicim_KrediBaglami = "{0} taksit kaldı · sonraki {1}";

    /// <summary>Ödeme planının bağlamı: {0} kalan ödeme, {1} sıradaki tarih.</summary>
    public const string Bicim_PlanBaglami = "{0} ödeme kaldı · sonraki {1}";

    /// <summary>Hiç taksiti olmayan (tutarsız) ödeme planının bağlamı.</summary>
    public const string Etiket_PlanOdemesiYok = "Ödemesi yok";

    /// <summary>Grubun gizli satır sayısı: {0} gizli satır.</summary>
    public const string Bicim_FazlaKayit = "+{0} daha";

    /// <summary>Hiç kayıt yokken görünen boş durum metni.</summary>
    public const string Bos_KayitYok = "Henüz gelir, kart, kredi ya da ödeme kaydın yok.";

    /// <summary>Okuma hatasında görünen metin.</summary>
    public const string Hata_YapiYuklenemedi = "Finansal yapı okunamadı.";

    /// <summary>Başlıktaki aksiyon: eklenecek kayıt türünü sorar (S63-1).</summary>
    public const string Aksiyon_Ekle = "Ekle";
}
