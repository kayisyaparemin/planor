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

    /// <summary>Krediyi kaydedip listeye dönen aksiyon.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Kaydetmeden listeye dönen aksiyon; değişiklik varsa onay sorulur.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Düzenlenecek kredi okunamadığında gösterilen hata metni.</summary>
    public const string Hata_KrediYuklenemedi = "Kredi bilgileri okunamadı.";
}
