#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Gelir formlarında (EK-V6d) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/IncomeFormStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// Alan adları kurulumun gelir adımıyla aynıdır: kullanıcı aynı alanı iki adla görmez (S67-2).
/// </summary>
public static class IncomeFormStrings
{
    /// <summary>Yeni düzenli gelir eklerken sayfa başlığı.</summary>
    public const string Baslik_YeniGelir = "Yeni Gelir";

    /// <summary>Var olan geliri düzenlerken sayfa başlığı.</summary>
    public const string Baslik_GeliriDuzenle = "Geliri Düzenle";

    /// <summary>Gelir adı alanının etiketi.</summary>
    public const string Etiket_GelirAdi = "GELİR ADI";

    /// <summary>Yeni gelirin aylık net tutarı alanının etiketi.</summary>
    public const string Etiket_AylikNetTutar = "AYLIK NET TUTAR";

    /// <summary>Gelirin her ay yattığı gün alanının etiketi.</summary>
    public const string Etiket_OdemeGunu = "ÖDEME GÜNÜ";

    /// <summary>Gelir adı alanının yer tutucusu.</summary>
    public const string YerTutucu_GelirAdi = "Örn. Kira geliri, emekli aylığı";

    /// <summary>Tutar alanının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Gün alanının yer tutucusu.</summary>
    public const string YerTutucu_Gun = "1–31";

    /// <summary>Geliri kaydeden buton.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Formdan çıkan buton.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Tarih metni biçimi: "{0} itibarıyla".</summary>
    public const string Bicim_Itibariyla = "{0} itibarıyla";

    /// <summary>Tutar değişikliği giriş bloğunu açan buton.</summary>
    public const string Aksiyon_TutarDegisikligiEkle = "Tutar değişikliği ekle";

    /// <summary>Yeni tutar girişinin etiketi.</summary>
    public const string Etiket_YeniTutar = "YENİ TUTAR";

    /// <summary>Yeni tutarın geçerlilik tarihi etiketi.</summary>
    public const string Etiket_GecerlilikTarihi = "GEÇERLİLİK TARİHİ";

    /// <summary>Tutar değişikliğini listeye ekleyen buton.</summary>
    public const string Aksiyon_Ekle = "Ekle";

    /// <summary>Düzenlenecek gelir okunamadığında gösterilen hata.</summary>
    public const string Hata_GelirYuklenemedi = "Gelir bilgileri okunamadı.";
}
