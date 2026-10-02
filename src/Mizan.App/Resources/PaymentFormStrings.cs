#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Ödeme formlarında (EK-V6e: taksitli ödeme planı ve planlı büyük harcama) görünen metinlerin
/// kataloğu. Değerler <c>Resources/Strings/PaymentFormStrings.json</c> ile aynıdır.
/// </summary>
public static class PaymentFormStrings
{
    /// <summary>Yeni ödeme planı eklerken sayfa başlığı.</summary>
    public const string Baslik_YeniOdemePlani = "Yeni Ödeme Planı";

    /// <summary>Var olan ödeme planını düzenlerken sayfa başlığı.</summary>
    public const string Baslik_OdemePlaniniDuzenle = "Ödeme Planını Düzenle";

    /// <summary>Yeni planlı büyük harcama eklerken sayfa başlığı.</summary>
    public const string Baslik_PlanliBuyukHarcama = "Planlı Büyük Harcama";

    /// <summary>Var olan planlı harcamayı düzenlerken sayfa başlığı.</summary>
    public const string Baslik_HarcamayiDuzenle = "Harcamayı Düzenle";

    /// <summary>Ödeme planı adı alanının etiketi.</summary>
    public const string Etiket_PlanAdi = "PLAN ADI";

    /// <summary>Planlanan büyük harcama adı alanının etiketi.</summary>
    public const string Etiket_HarcamaAdi = "HARCAMA ADI";

    /// <summary>Taksitler listesi kartının üst etiketi.</summary>
    public const string Etiket_Taksitler = "TAKSİTLER";

    /// <summary>Taksit tutarı alanının etiketi.</summary>
    public const string Etiket_TaksitTutari = "TAKSİT TUTARI";

    /// <summary>Taksit sayısı alanının etiketi.</summary>
    public const string Etiket_TaksitSayisi = "TAKSİT SAYISI";

    /// <summary>İlk vade tarihi seçicisinin etiketi.</summary>
    public const string Etiket_IlkVadeTarihi = "İLK VADE TARİHİ";

    /// <summary>Tutar alanının etiketi.</summary>
    public const string Etiket_Tutar = "TUTAR";

    /// <summary>Tarih alanının etiketi.</summary>
    public const string Etiket_Tarih = "TARİH";

    /// <summary>Plan adı alanının yer tutucusu.</summary>
    public const string YerTutucu_PlanAdi = "Örn. Okul taksiti, senetli mobilya";

    /// <summary>Harcama adı alanının yer tutucusu.</summary>
    public const string YerTutucu_HarcamaAdi = "Örn. Yaz tatili, beyaz eşya";

    /// <summary>Tutar alanının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Taksit sayısı alanının yer tutucusu.</summary>
    public const string YerTutucu_TaksitSayisi = "1";

    /// <summary>Taksit giriş bloğunu açan buton metni.</summary>
    public const string Aksiyon_TaksitEkle = "Taksit ekle";

    /// <summary>Taksiti listeye ekleyen buton metni.</summary>
    public const string Aksiyon_Ekle = "Ekle";

    /// <summary>Kaydet butonu metni.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Vazgeç butonu metni.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Ödeme planı okunamadığında gösterilen hata metni.</summary>
    public const string Hata_OdemePlaniYuklenemedi = "Ödeme planı okunamadı.";

    /// <summary>Planlanan büyük harcama okunamadığında gösterilen hata metni.</summary>
    public const string Hata_HarcamaYuklenemedi = "Planlanan harcama okunamadı.";
}
