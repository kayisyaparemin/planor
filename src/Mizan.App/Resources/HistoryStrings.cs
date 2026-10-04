#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Geçmiş ve geçmiş ayrıntısı ekranlarında (EK-V12, GS32) görünen metinlerin kataloğu.
/// Değerler <c>Resources/Strings/HistoryStrings.json</c> ile aynıdır.
/// </summary>
public static class HistoryStrings
{
    /// <summary>Geçmiş sayfası başlığı.</summary>
    public const string Baslik_Gecmis = "Geçmiş";

    /// <summary>Dönem ayrıntısı sayfası başlığı.</summary>
    public const string Baslik_GecmisAyrintisi = "Dönem Ayrıntısı";

    /// <summary>Son 3 dönem özeti kartı başlığı.</summary>
    public const string Etiket_SonDonemlerOzeti = "Son 3 dönem özeti";

    /// <summary>Planlanan net değişim etiketi.</summary>
    public const string Etiket_PlanlananNetDegisim = "Planlanan net değişim";

    /// <summary>Gerçekleşen net değişim etiketi.</summary>
    public const string Etiket_GerceklesenNetDegisim = "Gerçekleşen net değişim";

    /// <summary>Net fark etiketi.</summary>
    public const string Etiket_NetFark = "Net fark";

    /// <summary>Dönem sonu etiketi.</summary>
    public const string Etiket_DonemSonu = "DÖNEM SONU";

    /// <summary>Bakiye seyri grafik başlığı.</summary>
    public const string Etiket_BakiyeSeyri = "Bakiye Seyri";

    /// <summary>Farkın kaynağı kartı başlığı.</summary>
    public const string Etiket_FarkinKaynagi = "Farkın kaynağı";

    /// <summary>Yaşam gideri satırı başlığı.</summary>
    public const string Etiket_YasamGideri = "Yaşam gideri";

    /// <summary>Ödemeler satırı başlığı.</summary>
    public const string Etiket_Odemeler = "Ödemeler";

    /// <summary>KMH faizi satırı başlığı.</summary>
    public const string Etiket_KmhFaizi = "KMH faizi";

    /// <summary>Kasa düzeltmesi satırı başlığı.</summary>
    public const string Etiket_KasaDuzeltmesi = "Kasa düzeltmesi";

    /// <summary>Planın üzerinde durumu rozeti.</summary>
    public const string Etiket_PlaninUzerinde = "Planın üzerinde";

    /// <summary>Planın altında durumu rozeti.</summary>
    public const string Etiket_PlaninAltinda = "Planın altında";

    /// <summary>Planlandığı gibi durumu rozeti.</summary>
    public const string Etiket_PlanlandigiGibi = "Planlandığı gibi";

    /// <summary>Ödendi durumu.</summary>
    public const string Etiket_Odendi = "Ödendi";

    /// <summary>Farklı tutarla ödendi durumu.</summary>
    public const string Etiket_FarkliTutar = "Farklı tutar";

    /// <summary>Ödenmedi durumu.</summary>
    public const string Etiket_Odenmedi = "Ödenmedi";

    /// <summary>Ana sayfaya dön aksiyonu.</summary>
    public const string Aksiyon_AnaSayfayaDon = "Ana Sayfaya Dön";

    /// <summary>Tekrar dene butonu.</summary>
    public const string Aksiyon_TekrarDene = "Tekrar Dene";

    /// <summary>Geri dön butonu.</summary>
    public const string Aksiyon_GeriDon = "Geri Dön";

    /// <summary>Kapanmış dönem bulunamadığında gösterilen boş durum cümlesi.</summary>
    public const string Cumle_KapanmisDonemYok = "Henüz kapanmış bir finansal dönem bulunmuyor.";

    /// <summary>Dönem kaydı bulunamadığında gösterilen boş durum cümlesi.</summary>
    public const string Cumle_DonemKaydiBulunamadi = "Seçilen döneme ait gerçekleşme kaydı bulunamadı.";

    /// <summary>Geçmiş yüklenemediğinde hata metni.</summary>
    public const string Hata_GecmisYuklenemedi = "Geçmiş dönem bilgileri yüklenemedi.";

    /// <summary>Dönem ayrıntısı yüklenemediğinde hata metni.</summary>
    public const string Hata_DonemAyrintisiYuklenemedi = "Dönem ayrıntısı yüklenemedi.";

    /// <summary>Dönem aralığı biçimi; {0} başlama, {1} bitiş günü.</summary>
    public const string Bicim_DonemAraligi = "{0} – {1}";

    /// <summary>Plana göre fark ve plan tutarı biçimi; {0} fark, {1} planlanan.</summary>
    public const string Bicim_PlanaGoreFark = "Plana göre {0} · Plan {1}";

    /// <summary>Yaşam gideri satırı alt bilgisi; {0} planlanan, {1} harcanan.</summary>
    public const string Bicim_YasamPlaniHarcanan = "Planlanan {0} · harcanan {1}";

    /// <summary>Ödemeler satırı alt bilgisi; {0} toplam, {1} ödenen.</summary>
    public const string Bicim_OdemeDurumu = "{0} ödemenin {1}'si ödendi";

    /// <summary>Açılış bakiyesi biçimi; {0} tutar.</summary>
    public const string Bicim_AcilisBakiyesi = "Açılış {0}";

    /// <summary>Gizli ödeme satırları için yerinde açma biçimi; {0} gizli satır sayısı.</summary>
    public const string Bicim_DahaFazlaOdeme = "+{0} daha";
}
