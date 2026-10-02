#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Dönem ayrıntısı sayfasında (EK-V9) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/PeriodDetailStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// Eski ekranın gelir karşılama cümleleri yok: "dönem başına göre" satırı aynı cevabı sayıyla verir (GS27).
/// </summary>
public static class PeriodDetailStrings
{
    /// <summary>Hero rakamın etiketi: dönemin sonunda kalacak bakiye.</summary>
    public const string Etiket_DonemSonu = "DÖNEM SONU";

    /// <summary>Hero'nun altında dönem sonu ile dönem başı arasındaki fark.</summary>
    public const string Etiket_DonemBasinaGore = "Dönem başına göre";

    /// <summary>Akışın ilk satırı: dönemin açılış bakiyesi.</summary>
    public const string Etiket_DonemBasi = "Dönem başı";

    /// <summary>Akışta dönemin toplam geliri.</summary>
    public const string Etiket_Gelir = "Gelir";

    /// <summary>Akışta dönemin ödemeleri (zorunlu ödemeler ve büyük harcamalar).</summary>
    public const string Etiket_Odemeler = "Ödemeler";

    /// <summary>Akışta dönem için ayrılan yaşam gideri.</summary>
    public const string Etiket_YasamGideri = "Yaşam gideri";

    /// <summary>Akışta eksi bakiyenin faizi; yalnız doğduğu dönemde görünür.</summary>
    public const string Etiket_KmhFaizi = "KMH faizi";

    /// <summary>Ödeme listesinin etiketi.</summary>
    public const string Etiket_OdemeListesi = "ÖDEMELER";

    /// <summary>Kart ekstresinin tutarı belirlenemediğinde tutarın yerine (S75-4).</summary>
    public const string Etiket_Belirlenmedi = "Belirlenmedi";

    /// <summary>Kartın ödeme şekli seçilmediği için tutar tahmin; {0} vade.</summary>
    public const string Bicim_TahminiGun = "{0} · tahmini";

    /// <summary>Kart faizi listesinin etiketi.</summary>
    public const string Etiket_KartFaizi = "KART FAİZİ";

    /// <summary>Kart faizi listesinin notu; {0} dönemin kart faizi toplamı. Faiz borca eklenir, akışta yoktur (S75-5).</summary>
    public const string Bicim_KartFaiziToplam = "{0} · ekstre borcuna eklenir";

    /// <summary>Dönem artık 12 dönem zincirinde değilken boş hâlin metni (S75-2).</summary>
    public const string Bos_DonemBulunamadi = "Bu dönem artık 12 dönemin içinde değil.";

    /// <summary>Boş hâlin aksiyonu: dönemin açıldığı 12 Dönem sayfası.</summary>
    public const string Aksiyon_OnIkiDonemeDon = "12 Döneme Dön";

    /// <summary>Dönem okunamadığında ya da hesaplanamadığında hata metni.</summary>
    public const string Hata_DonemHesaplanamadi = "Dönem şu an hesaplanamadı.";
}
