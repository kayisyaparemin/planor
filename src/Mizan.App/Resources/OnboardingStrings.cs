#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kurulum sihirbazı ekranının (OnboardingPage) başlık, etiket, aksiyon ve rehber metinlerini içeren dize kataloğu.
/// </summary>
public static class OnboardingStrings
{
    /// <summary>Dönem adımı başlığı.</summary>
    public const string Baslik_KurulumDonem = "Dönemini Ayarlayalım";

    /// <summary>Gelir adımı başlığı.</summary>
    public const string Baslik_KurulumGelir = "Düzenli Gelirlerin";

    /// <summary>Kart adımı başlığı.</summary>
    public const string Baslik_KurulumKart = "Kredi Kartların";

    /// <summary>Kredi adımı başlığı.</summary>
    public const string Baslik_KurulumKredi = "Kredilerin";

    /// <summary>Harcama adımı başlığı.</summary>
    public const string Baslik_KurulumHarcama = "Yaklaşan Ödemelerin";

    /// <summary>Yaşam gideri adımı başlığı.</summary>
    public const string Baslik_KurulumYasam = "Yaşam Giderin";

    /// <summary>Mevcut bakiye adımı başlığı.</summary>
    public const string Baslik_KurulumBakiye = "Mevcut Nakit Paran";

    /// <summary>Kurulum özeti adımı başlığı.</summary>
    public const string Baslik_KurulumOzet = "İlk Planın Hazır";

    /// <summary>Dönem günü etiketi.</summary>
    public const string Etiket_DonemGunu = "DÖNEM GÜNÜ";

    /// <summary>Gelir adı etiketi.</summary>
    public const string Etiket_GelirAdi = "GELİR ADI";

    /// <summary>Aylık net tutar etiketi.</summary>
    public const string Etiket_AylikNetTutar = "AYLIK NET TUTAR";

    /// <summary>Ödeme günü etiketi.</summary>
    public const string Etiket_OdemeGunu = "ÖDEME GÜNÜ";

    /// <summary>Başlangıç tarihi etiketi.</summary>
    public const string Etiket_GecerlilikTarihi = "BAŞLANGIÇ TARİHİ";

    /// <summary>Kart adı etiketi.</summary>
    public const string Etiket_KartAdi = "KART ADI";

    /// <summary>Banka adı etiketi.</summary>
    public const string Etiket_Banka = "BANKA";

    /// <summary>Kart limiti etiketi.</summary>
    public const string Etiket_Limit = "LİMİT";

    /// <summary>Hesap kesim günü etiketi.</summary>
    public const string Etiket_KesimGunu = "KESİM GÜNÜ";

    /// <summary>Son ödeme günü etiketi.</summary>
    public const string Etiket_SonOdemeGunu = "SON ÖDEME GÜNÜ";

    /// <summary>Kart güncel borcu etiketi.</summary>
    public const string Etiket_GuncelBorc = "GÜNCEL BORÇ";

    /// <summary>Güncel ekstre borcu etiketi.</summary>
    public const string Etiket_GuncelEkstreBorcu = "GÜNCEL EKSTRE BORCU";

    /// <summary>Asgari ödeme tutarı etiketi.</summary>
    public const string Etiket_AsgariOdeme = "ASGARİ ÖDEME";

    /// <summary>Kredi adı etiketi.</summary>
    public const string Etiket_KrediAdi = "KREDİ ADI";

    /// <summary>Aylık kredi taksiti etiketi.</summary>
    public const string Etiket_AylikTaksit = "AYLIK TAKSİT";

    /// <summary>Kredi taksit günü etiketi.</summary>
    public const string Etiket_TaksitGunu = "TAKSİT GÜNÜ";

    /// <summary>Kalan taksit sayısı etiketi.</summary>
    public const string Etiket_KalanTaksitSayisi = "KALAN TAKSİT";

    /// <summary>Kalan anapara etiketi.</summary>
    public const string Etiket_KalanAnapara = "KALAN ANAPARA";

    /// <summary>Ödeme adı etiketi.</summary>
    public const string Etiket_OdemeAdi = "ÖDEME ADI";

    /// <summary>Ödeme tutarı etiketi.</summary>
    public const string Etiket_OdemeTutari = "TUTAR";

    /// <summary>Ödeme vade tarihi etiketi.</summary>
    public const string Etiket_OdemeTarihi = "VADE TARİHİ";

    /// <summary>Taksit adedi etiketi.</summary>
    public const string Etiket_TaksitSayisi = "TAKSİT SAYISI";

    /// <summary>Yaşam gideri havuzu etiketi.</summary>
    public const string Etiket_YasamGideri = "SERBEST YAŞAM HAVUZU";

    /// <summary>Mevcut bakiye etiketi.</summary>
    public const string Etiket_MevcutBakiye = "MEVCUT BAKİYE";

    /// <summary>Kurulum özeti başlık etiketi.</summary>
    public const string Etiket_KurulumOzeti = "KURULUM ÖZETİ";

    /// <summary>Adımı atlama aksiyonu.</summary>
    public const string Aksiyon_Atla = "Atla";

    /// <summary>Önceki adıma dönme aksiyonu.</summary>
    public const string Aksiyon_Geri = "Geri";

    /// <summary>Sonraki adıma geçme aksiyonu.</summary>
    public const string Aksiyon_Devam = "Devam";

    /// <summary>Gelir ekleme aksiyonu.</summary>
    public const string Aksiyon_GelirEkle = "Gelir Ekle";

    /// <summary>Kart ekleme aksiyonu.</summary>
    public const string Aksiyon_KartEkle = "Kart Ekle";

    /// <summary>Kredi ekleme aksiyonu.</summary>
    public const string Aksiyon_KrediEkle = "Kredi Ekle";

    /// <summary>Ödeme ekleme aksiyonu.</summary>
    public const string Aksiyon_OdemeEkle = "Ödeme Ekle";

    /// <summary>Kayıt silme aksiyonu.</summary>
    public const string Aksiyon_Sil = "Sil";

    /// <summary>Planör'ü başlatma aksiyonu.</summary>
    public const string Aksiyon_PlanoruBaslat = "Planör'ü Başlat";

    /// <summary>Kurulum adımı yönlendirme cümlesi.</summary>
    public const string Cumle_AdimAciklama = "Bilgilerini girerek ilk nakit akış dönemini planlayabilirsin.";

    /// <summary>Dönem günü seçim rehber cümlesi.</summary>
    public const string Cumle_DonemGunuRehber = "Maaşını veya ana gelirini aldığın günü seç.";

    /// <summary>Kurulum tamamlama bilgilendirme notu.</summary>
    public const string Cumle_KurulumNotu = "Kaydettiğin tüm bilgileri daha sonra Finansal Yapı'dan düzenleyebilirsin.";

    /// <summary>Kurulum kaydetme hatası.</summary>
    public const string Hata_KurulumKaydedilemedi = "Kurulum verileri kaydedilirken bir sorun oluştu.";
}
