#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kredi formunda (EK-V6c) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/LoanFormStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// </summary>
public static class LoanFormStrings
{
    /// <summary>Yeni kredi eklerken sayfa başlığı.</summary>
    public const string Baslik_YeniKredi = "Yeni Kredi";

    /// <summary>Var olan krediyi düzenlerken sayfa başlığı.</summary>
    public const string Baslik_KrediyiDuzenle = "Krediyi Düzenle";

    /// <summary>Kredi adı alanının etiketi.</summary>
    public const string Etiket_KrediAdi = "KREDİ ADI";

    /// <summary>Banka alanının etiketi.</summary>
    public const string Etiket_Banka = "BANKA";

    /// <summary>Aylık taksit alanının etiketi.</summary>
    public const string Etiket_AylikTaksit = "AYLIK TAKSİT";

    /// <summary>Kalan taksit sayısı alanının etiketi.</summary>
    public const string Etiket_KalanTaksit = "KALAN TAKSİT";

    /// <summary>Sonraki taksit tarihi alanının etiketi; ödeme günü bu tarihten çözülür (S64-2).</summary>
    public const string Etiket_SonrakiTaksit = "SONRAKİ TAKSİT";

    /// <summary>Kredi türü seçicisinin etiketi.</summary>
    public const string Etiket_KrediTuru = "KREDİ TÜRÜ";

    /// <summary>Kredi türü seçeneği: ihtiyaç ve taşıt kredisi (tüketici kredisi).</summary>
    public const string Etiket_TurIhtiyac = "İhtiyaç / taşıt";

    /// <summary>Kredi türü seçeneği: sabit faizli konut kredisi (erken ödeme ücreti alınabilir).</summary>
    public const string Etiket_TurKonutSabit = "Konut, sabit";

    /// <summary>Kredi türü seçeneği: değişken faizli konut kredisi.</summary>
    public const string Etiket_TurKonutDegisken = "Konut, değişken";

    /// <summary>Kredi adı alanının yer tutucusu.</summary>
    public const string YerTutucu_KrediAdi = "Örn. İhtiyaç kredisi";

    /// <summary>Banka alanının yer tutucusu.</summary>
    public const string YerTutucu_Banka = "Örn. Ziraat Bankası";

    /// <summary>Tutar alanlarının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Kalan taksit sayısı alanının yer tutucusu.</summary>
    public const string YerTutucu_TaksitSayisi = "Örn. 24";

    /// <summary>Faiz kartında kalan anapara alanının etiketi.</summary>
    public const string Etiket_KalanAnapara = "KALAN ANAPARA";

    /// <summary>Faiz kartında bankanın bugünkü kapatma tutarı alanının etiketi; girilirse otoritedir (I25).</summary>
    public const string Etiket_KapatmaTutari = "BANKANIN KAPATMA TUTARI";

    /// <summary>Faiz kartı: bugün kapatmanın toplam bedeli satırı.</summary>
    public const string Etiket_BugunKapatirsan = "Bugün kapatırsan";

    /// <summary>Faiz kartı: bedelin içindeki yasal erken ödeme ücreti satırı (sabit faizli konut).</summary>
    public const string Etiket_ErkenOdemeUcreti = "Erken ödeme ücreti";

    /// <summary>Faiz kartı: kapatınca ödenmeyecek faiz satırı.</summary>
    public const string Etiket_KurtulacaginFaiz = "Kurtulacağın faiz";

    /// <summary>Faiz kartı: çözülen aylık faiz satırı.</summary>
    public const string Etiket_AylikFaiz = "Aylık faiz (vergi dahil)";

    /// <summary>Faiz kartındaki iki tutar alanının yer tutucusu.</summary>
    public const string YerTutucu_IstegeBagli = "İsteğe bağlı";

    /// <summary>Planlı erken ödemeler listesinin üst etiketi (S64-6).</summary>
    public const string Etiket_ErkenOdemeler = "ERKEN ÖDEMELER";

    /// <summary>Erken ödeme girişi: şekil seçicisinin etiketi; tutarın sorulup sorulmayacağını belirler.</summary>
    public const string Etiket_OdemeSekli = "ÖDEME ŞEKLİ";

    /// <summary>Erken ödeme girişi: tarih seçicisinin etiketi.</summary>
    public const string Etiket_OdemeTarihi = "ÖDEME TARİHİ";

    /// <summary>Erken ödeme girişi: ara ödemede anaparadan düşecek tutar alanının etiketi.</summary>
    public const string Etiket_AnaparadanDusecek = "ANAPARADAN DÜŞECEK";

    /// <summary>Erken ödeme şekli: kalan borcun tamamı ödenir.</summary>
    public const string Etiket_TamamenKapatma = "Tamamen kapatma";

    /// <summary>Erken ödeme şekli: ara ödeme, taksit aynı kalır, vade kısalır.</summary>
    public const string Etiket_VadeKisalir = "Ara ödeme · vade kısalır";

    /// <summary>Erken ödeme şekli: ara ödeme, vade aynı kalır, taksit düşer.</summary>
    public const string Etiket_TaksitDuser = "Ara ödeme · taksit düşer";

    /// <summary>Faiz kartı: geçerli tutar girilmemişken gösterilen cümle.</summary>
    public const string Cumle_FaizIcinTutarGir = "Faizi görmek için kalan anaparayı ya da bankanın kapatma tutarını gir.";

    /// <summary>Faiz kartı: girilen tutarla faiz çözülemediğinde (kayıt reddedecekken) gösterilen cümle.</summary>
    public const string Cumle_TutarUyusmuyor = "Bu tutar taksitlerle uyuşmuyor; tutarı ve taksit bilgilerini kontrol et.";

    /// <summary>Krediyi kaydedip listeye dönen aksiyon.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Kaydetmeden listeye dönen aksiyon; değişiklik varsa onay sorulur.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Erken ödeme giriş bloğunu açan aksiyon.</summary>
    public const string Aksiyon_ErkenOdemePlanla = "Erken ödeme planla";

    /// <summary>Girilen erken ödemeyi listeye ekleyen aksiyon; kayıt Kaydet'e kadar bekler.</summary>
    public const string Aksiyon_Ekle = "Ekle";

    /// <summary>Düzenlenecek kredi okunamadığında gösterilen hata metni.</summary>
    public const string Hata_KrediYuklenemedi = "Kredi bilgileri okunamadı.";
}
