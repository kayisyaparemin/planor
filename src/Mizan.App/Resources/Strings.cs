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

    /// <summary>Dönem sonu tahmini etiket metni.</summary>
    public const string Etiket_DonemSonuTahmini = "DÖNEM SONU TAHMİNİ";

    /// <summary>Kalan bütçe etiket metni.</summary>
    public const string Etiket_KalanButce = "KALAN BÜTÇE";

    /// <summary>Harcanan etiket metni.</summary>
    public const string Etiket_Harcanan = "HARCANAN";

    /// <summary>Kalan ödemeler etiket metni.</summary>
    public const string Etiket_KalanOdemeler = "KALAN ÖDEMELER";

    /// <summary>Dönem kontrol başlığı.</summary>
    public const string Baslik_GozlemKontrolu = "Dönem Ortası Kontrolü";

    /// <summary>Gözlem kaydet butonu.</summary>
    public const string Aksiyon_GozlemiKaydet = "Gözlemi Kaydet";

    /// <summary>Güncel bakiye giriş etiketi.</summary>
    public const string Etiket_GuncelBakiye = "GÜNCEL BAKİYE";

    /// <summary>Gözlem giriş alanı yer tutucusu.</summary>
    public const string YerTutucu_Bakiye = "0 ₺";

    /// <summary>Gözlem açıklama ipucu cümlesi.</summary>
    public const string Cumle_GozlemIpucu = "Girilen bakiye dönem projeksiyonunu anında günceller.";

    /// <summary>Dönemi kapat butonu.</summary>
    public const string Aksiyon_DonemiKapat = "Dönemi Kapat";

    /// <summary>Dönem kapatmaya hazır bildirim metni.</summary>
    public const string Cumle_DonemKapatBildirimi = "Dönem süresi tamamlandı. Kapanış bakiyesini onaylayıp kapatabilirsin.";

    /// <summary>Gelecek dönemler gezinme başlığı.</summary>
    public const string Baslik_NavGelecekDonemler = "Gelecek Dönemler";

    /// <summary>Gelecek dönemler özet alt yazısı.</summary>
    public const string Etiket_GelecekDonemOzet = "12 dönemlik nakit akışı";

    /// <summary>Gelir planı gezinme başlığı.</summary>
    public const string Baslik_NavGelirPlani = "Gelir Planı";

    /// <summary>Gelir planı özet alt yazısı.</summary>
    public const string Etiket_GelirPlaniOzet = "Maaş ve yan gelirler";

    /// <summary>Ödeme planları gezinme başlığı.</summary>
    public const string Baslik_NavOdemePlanlari = "Ödeme Planları";

    /// <summary>Ödeme planları özet alt yazısı.</summary>
    public const string Etiket_OdemePlanOzet = "Kredi ve kart ödemeleri";

    /// <summary>Simülatör gezinme başlığı.</summary>
    public const string Baslik_NavSimulator = "Simülatör";

    /// <summary>Simülatör özet alt yazısı.</summary>
    public const string Etiket_SimulatorOzet = "Karar senaryoları";

    /// <summary>Geçmiş dönemler özet alt yazısı.</summary>
    public const string Etiket_GecmisOzet = "Kapanan dönem kayıtları";

    /// <summary>Açık dönem bulunmadığı durum metni.</summary>
    public const string Durum_AcikDonemYok = "Henüz açık bir nakit akış dönemi yok.";

    /// <summary>Açık dönem yok rehber cümlesi.</summary>
    public const string Cumle_AcikDonemYokRehber = "Yeni bir dönem planlamak için gelir ve giderlerini kurabilirsin.";

    /// <summary>Dashboard verileri yüklenemedi hatası.</summary>
    public const string Hata_DashboardYuklenemedi = "Ana sayfa verileri yüklenirken bir sorun oluştu.";

    /// <summary>Hatırlatıcı ödedim aksiyonu.</summary>
    public const string Aksiyon_Odedim = "Ödedim";

    /// <summary>Hatırlatıcı ertele aksiyonu.</summary>
    public const string Aksiyon_Ertele = "Ertele";

    /// <summary>Dönem adı şablonu; {0} dönem başlangıç tarihi.</summary>
    public const string Bicim_DonemAdi = "{0} Dönemi";

    /// <summary>Dönem gün sayacı şablonu; {0} geçen gün, {1} toplam gün, {2} dönem sonu tarihi.</summary>
    public const string Bicim_DonemGunSayaci = "{0}/{1} gün · {2}";

    /// <summary>Ana sayfada gösterilmeyen kalan ödeme sayısı şablonu.</summary>
    public const string Bicim_FazlaOdeme = "+{0} ödeme daha";

    /// <summary>Toplam tutar şablonu; {0} biçimlenmiş tutar.</summary>
    public const string Bicim_ToplamTutar = "Toplam: {0}";
}
