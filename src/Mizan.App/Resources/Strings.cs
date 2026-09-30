#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kullanıcıya görünen tüm metinlerin tek merkezden yönetimini sağlayan statik dize kataloğu.
/// </summary>
public static class Strings
{
    /// <summary>Ana sayfa başlığı.</summary>
    public const string Baslik_AnaSayfa = "Ana Sayfa";

    /// <summary>12 Dönem projeksiyon başlığı.</summary>
    public const string Baslik_GelecekDonemler = "12 Dönem";

    /// <summary>Simülatör başlığı.</summary>
    public const string Baslik_Simulator = "Simülatör";

    /// <summary>Finansal yapı başlığı.</summary>
    public const string Baslik_FinansalYapi = "Finansal Yapı";

    /// <summary>Geçmiş başlığı.</summary>
    public const string Baslik_Gecmis = "Geçmiş";

    /// <summary>Ayarlar başlığı.</summary>
    public const string Baslik_Ayarlar = "Ayarlar";

    /// <summary>Profil başlık etiketi.</summary>
    public const string Etiket_Profil = "PROFİL";

    /// <summary>Profil değiştirme aksiyon butonu.</summary>
    public const string Aksiyon_ProfilDegistir = "Profil Değiştir";

    /// <summary>Profil seçimi başlığı.</summary>
    public const string Baslik_ProfilSecimi = "Profil Seçimi";

    /// <summary>Profil seçimi eyebrow etiketi.</summary>
    public const string Etiket_Profiller = "PROFİLLER";

    /// <summary>Mevcut kayıtlı profiller etiketi.</summary>
    public const string Etiket_MevcutProfiller = "KAYITLI PROFİLLER";

    /// <summary>Profil veri izolasyonu açıklama cümlesi.</summary>
    public const string Cumle_ProfilSecimNotu = "Her profil kendi finansal veritabanında izole tutulur.";

    /// <summary>Yeni profil ekleme butonu.</summary>
    public const string Aksiyon_YeniProfil = "Yeni Profil";

    /// <summary>Yedekten profil ekleme butonu.</summary>
    public const string Aksiyon_YedektenEkle = "Yedekten Ekle";

    /// <summary>Profil düzenleme butonu.</summary>
    public const string Aksiyon_Duzenle = "Düzenle";

    /// <summary>Planör karşılama başlığı.</summary>
    public const string Baslik_HosGeldiniz = "Planör'e Hoş Geldin";

    /// <summary>Başlangıç eyebrow etiketi.</summary>
    public const string Etiket_Planor = "BAŞLANGIÇ";

    /// <summary>İlk kurulum kartı başlığı.</summary>
    public const string Baslik_IlkKurulum = "İlk Profilini Kur";

    /// <summary>İlk kurulum rehber cümlesi.</summary>
    public const string Cumle_IlkKurulum = "Temiz bir başlangıç yapabilir veya mevcut bir yedeği geri yükleyebilirsin.";

    /// <summary>Temiz başla aksiyon butonu.</summary>
    public const string Aksiyon_TemizBasla = "Temiz Başla";

    /// <summary>Yedekten geri yükle aksiyon butonu.</summary>
    public const string Aksiyon_YedektenGeriYukle = "Yedekten Geri Yükle";

    /// <summary>Profil yükleme hatası durum metni.</summary>
    public const string Hata_ProfillerOkunamadi = "Profiller yüklenirken bir sorun oluştu.";

    /// <summary>Tekrar dene butonu.</summary>
    public const string Aksiyon_TekrarDene = "Tekrar Dene";

    /// <summary>Dönem durumu etiket metni.</summary>
    public const string Etiket_DonemDurumu = "DÖNEM DURUMU";

    /// <summary>Dönem kontrol başlığı.</summary>
    public const string Baslik_GozlemKontrolu = "Dönem Ortası Kontrolü";

    /// <summary>Dönem kapatmaya hazır bildirim metni.</summary>
    public const string Cumle_DonemKapatBildirimi = "Dönem süresi tamamlandı. Kapanış bakiyesini onaylayıp kapatabilirsin.";

    /// <summary>Gelir planı gezinme başlığı.</summary>
    public const string Baslik_NavGelirPlani = "Gelir Planı";

    /// <summary>Gelir planı özet alt yazısı.</summary>
    public const string Etiket_GelirPlaniOzet = "Maaş ve yan gelirler";

    /// <summary>Ödeme planları gezinme başlığı.</summary>
    public const string Baslik_NavOdemePlanlari = "Ödeme Planları";

    /// <summary>Açık dönem bulunmadığı durum metni.</summary>
    public const string Durum_AcikDonemYok = "Henüz açık bir nakit akış dönemi yok.";
}
