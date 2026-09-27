#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kart formunda (EK-V6b) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/CardFormStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// </summary>
public static class CardFormStrings
{
    /// <summary>Yeni kart eklerken sayfa başlığı.</summary>
    public const string Baslik_YeniKart = "Yeni Kart";

    /// <summary>Var olan kartı düzenlerken sayfa başlığı.</summary>
    public const string Baslik_KartiDuzenle = "Kartı Düzenle";

    /// <summary>Kart adı alanının etiketi.</summary>
    public const string Etiket_KartAdi = "KART ADI";

    /// <summary>Banka alanının etiketi.</summary>
    public const string Etiket_Banka = "BANKA";

    /// <summary>Limit alanının etiketi.</summary>
    public const string Etiket_Limit = "LİMİT";

    /// <summary>Güncel borç alanının etiketi; yalnız kesilmiş ekstresi olmayan kartta görünür.</summary>
    public const string Etiket_GuncelBorc = "GÜNCEL BORÇ";

    /// <summary>Kesim günü alanının etiketi.</summary>
    public const string Etiket_KesimGunu = "KESİM GÜNÜ";

    /// <summary>Son ödeme günü alanının etiketi.</summary>
    public const string Etiket_SonOdemeGunu = "SON ÖDEME GÜNÜ";

    /// <summary>Kart adı alanının yer tutucusu.</summary>
    public const string YerTutucu_KartAdi = "Örn. Bonus, Maximum";

    /// <summary>Banka alanının yer tutucusu.</summary>
    public const string YerTutucu_Banka = "Örn. Garanti BBVA";

    /// <summary>Tutar alanlarının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Gün alanlarının yer tutucusu.</summary>
    public const string YerTutucu_Gun = "1–31";

    /// <summary>Kartı kaydeden aksiyon.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Formdan kaydetmeden çıkan aksiyon.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Düzenlenecek kart okunamadığında görünen metin.</summary>
    public const string Hata_KartYuklenemedi = "Kart bilgileri okunamadı.";
}
