#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Dönem kapanışı özet sayfasında (EK-V11, GS31, S78) görünen metinlerin kataloğu.
/// Değerler <c>Resources/Strings/PeriodSettlementStrings.json</c> ile aynıdır.
/// </summary>
public static class PeriodSettlementStrings
{
    /// <summary>Sayfa başlığı.</summary>
    public const string Baslik_DonemiKapat = "Dönemi Kapat";

    /// <summary>Dönem sonu gerçekleşen tutar kartı üst etiketi.</summary>
    public const string Etiket_DonemSonu = "DÖNEM SONU";

    /// <summary>Farkın kaynağı kartı başlığı.</summary>
    public const string Etiket_FarkinKaynagi = "Farkın kaynağı";

    /// <summary>Yaşam gideri satırı başlığı.</summary>
    public const string Etiket_YasamGideri = "Yaşam gideri";

    /// <summary>Ödemeler satırı başlığı.</summary>
    public const string Etiket_Odemeler = "Ödemeler";

    /// <summary>KMH faizi satırı başlığı.</summary>
    public const string Etiket_KmhFaizi = "KMH faizi";

    /// <summary>Kapanış bakiyesi satırı başlığı.</summary>
    public const string Etiket_KapanisBakiyesi = "Kapanış bakiyesi";

    /// <summary>Kapanış bakiyesini değiştirme butonu.</summary>
    public const string Aksiyon_Degistir = "Değiştir";

    /// <summary>Dönemi kapatma birincil butonu.</summary>
    public const string Aksiyon_DonemiKapat = "Dönemi Kapat";

    /// <summary>Boş durumdan ana sayfaya dönme butonu.</summary>
    public const string Aksiyon_AnaSayfayaDon = "Ana Sayfaya Dön";

    /// <summary>Dönemin geçmişe taşınacağını belirten alt bilgilendirme cümlesi.</summary>
    public const string Cumle_DonemGecmiseTasinir = "Dönem Geçmiş dönemler'e taşınır; yeni dönem başlar.";

    /// <summary>Kapatılacak dönem yokken boş durum cümlesi.</summary>
    public const string Cumle_KapatilacakDonemYok = "Kapatılmaya hazır bir dönem bulunmuyor.";

    /// <summary>Kapanış yüklenemediğinde hata metni.</summary>
    public const string Hata_DonemKapanisiYuklenemedi = "Dönem kapanışı bilgileri yüklenemedi.";

    /// <summary>Dönem aralığı biçimi; {0} başlama, {1} bitiş günü.</summary>
    public const string Bicim_DonemAraligi = "{0} – {1}";

    /// <summary>Yaşam gideri satırı alt bilgisi; {0} planlanan, {1} harcanan.</summary>
    public const string Bicim_YasamPlaniHarcanan = "Planlanan {0} · harcanan {1}";

    /// <summary>Ödemeler satırı alt bilgisi; {0} toplam, {1} ödenen.</summary>
    public const string Bicim_OdemeDurumu = "{0} ödemenin {1}'si ödendi";

    /// <summary>Kapanış bakiyesi satırı; {0} bakiye, {1} tarih.</summary>
    public const string Bicim_KapanisBakiyeTarihi = "Kapanış bakiyesi {0} · {1}";

    /// <summary>Plana göre fark; {0} fark tutarı, {1} planlanan bakiye.</summary>
    public const string Bicim_PlanaGoreFark = "Plana göre {0} · Plan {1}";

    /// <summary>Yeni dönemin başlama tarihi bilgisi; {0} yeni dönemin başlama tarihi.</summary>
    public const string Bicim_YeniDonemBaslangici = "Dönem Geçmiş dönemler'e taşınır; yeni dönem {0}'de başlar.";

    /// <summary>Bakiye değiştirme diyaloğu başlığı.</summary>
    public const string Diyalog_BakiyeDegistirBaslik = "Kapanış Bakiyesi";

    /// <summary>Bakiye değiştirme diyaloğu mesajı.</summary>
    public const string Diyalog_BakiyeDegistirMesaj = "Dönem sonundaki kesinleşen banka bakiyesini girin:";
}
