#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Ayarlar sayfasında (EK-V13, GS33) görünen metinlerin kataloğu.
/// Değerler <c>Resources/Strings/SettingsStrings.json</c> ile aynıdır.
/// </summary>
public static class SettingsStrings
{
    /// <summary>Sayfa başlığı.</summary>
    public const string Baslik_Ayarlar = "Ayarlar";

    /// <summary>Hata durumu başlığı.</summary>
    public const string Baslik_Hata = "Ayarlar Yüklenemedi";

    /// <summary>Dönem ve serbest yaşam havuzu bölüm başlığı.</summary>
    public const string Etiket_PlanlamaAyarlari = "DÖNEM VE YAŞAM HAVUZU";

    /// <summary>Dönem başlangıç günü giriş alanı etiketi.</summary>
    public const string Etiket_DonemGunu = "Dönem başlangıç günü";

    /// <summary>Dönem yaşam gideri serbest havuz bütçesi etiketi.</summary>
    public const string Etiket_YasamGideri = "Dönem yaşam gideri";

    /// <summary>Varsayılan faiz oranları bölüm başlığı.</summary>
    public const string Etiket_FaizOranlari = "FAİZ ORANLARI";

    /// <summary>Kredi kartı devreden borç faiz oranı etiketi.</summary>
    public const string Etiket_KartDevredenFaiz = "Kredi kartı faizi %";

    /// <summary>Finansman açığı (KMH) faiz oranı etiketi.</summary>
    public const string Etiket_FinansmanAcigiFaizi = "Finansman faizi %";

    /// <summary>Ödeme hatırlatıcı bildirim modu bölüm başlığı.</summary>
    public const string Etiket_Hatirlatici = "BİLDİRİM VE HATIRLATICI";

    /// <summary>Bildirim modu kapalı çipi etiketi.</summary>
    public const string Etiket_ModKapali = "Kapalı";

    /// <summary>Bildirim modu rahat çipi etiketi.</summary>
    public const string Etiket_ModRahat = "Rahat";

    /// <summary>Bildirim modu agresif çipi etiketi.</summary>
    public const string Etiket_ModAgresif = "Agresif";

    /// <summary>Veri ve yedekleme bölüm başlığı.</summary>
    public const string Etiket_Yedekleme = "VERİ VE YEDEKLEME";

    /// <summary>Uygulama hakkında bölüm başlığı.</summary>
    public const string Etiket_Hakkinda = "UYGULAMA HAKKINDA";

    /// <summary>Uygulama marka adı etiketi.</summary>
    public const string Etiket_UygulamaAdi = "Planör";

    /// <summary>Yedekleme yerel saklama açıklaması.</summary>
    public const string Cumle_YedekAciklamasi = "Veriler cihazınızda saklanır. Düzenli yerel yedek almanız önerilir.";

    /// <summary>Cihaz içi veri güvenliği bilgilendirme cümlesi.</summary>
    public const string Cumle_VeriGuvenligi = "Finansal verileriniz yalnızca bu cihazda şifreli olarak saklanır.";

    /// <summary>Değişiklikleri kaydet butonu etiketi.</summary>
    public const string Aksiyon_DegisiklikleriKaydet = "Değişiklikleri Kaydet";

    /// <summary>Şimdi yerel yedek al butonu etiketi.</summary>
    public const string Aksiyon_SimdiYedekle = "Şimdi Yedekle";

    /// <summary>Yedekleme klasör erişim izni talep etme butonu etiketi.</summary>
    public const string Aksiyon_ErisimIzniVer = "Erişim İzni Ver";

    /// <summary>Hata durumunda işlemi tekrar deneme butonu etiketi.</summary>
    public const string Aksiyon_TekrarDene = "Tekrar Dene";

    /// <summary>Ayarlar yüklenemediğinde hata metni.</summary>
    public const string Hata_AyarlarYuklenemedi = "Ayarlar yüklenirken bir sorun oluştu.";
}
