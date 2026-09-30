#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// "Bakiye gir" sayfasında (EK-V3 sayfa 2) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/BalanceEntryStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// Eyebrow ana sayfanın bakiye kartıyla aynı sözdür; "güncel" denmez, bakiye geçmiş bir güne de girilebilir (GS25).
/// </summary>
public static class BalanceEntryStrings
{
    /// <summary>Sayfa başlığı; ana sayfadaki düğmeyle aynı söz.</summary>
    public const string Baslik_BakiyeGir = "Bakiye Gir";

    /// <summary>Tutar alanının etiketi.</summary>
    public const string Etiket_BankadakiBakiye = "BANKADAKİ BAKİYE";

    /// <summary>Boş tutar alanının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Tutar alanının yanındaki birim.</summary>
    public const string Etiket_Tl = "₺";

    /// <summary>Önceki girişin tutarı ve günü; {0} tutar, {1} gün.</summary>
    public const string Bicim_SonGiris = "Son giriş {0} · {1}";

    /// <summary>Seçili gün bugünse tarih seçicinin yanında.</summary>
    public const string Etiket_Bugun = "Bugün";

    /// <summary>Önizleme kartının etiketi: rakam kaydedilmemiş girişle hesaplanır.</summary>
    public const string Etiket_BuGirisleDonemSonu = "BU GİRİŞLE DÖNEM SONU";

    /// <summary>Önizlemenin plandan farkı; {0} işaretli tutar.</summary>
    public const string Bicim_PlanaGore = "Plana göre {0}";

    /// <summary>Bu girişten önceki dönem sonu tahmini; {0} tutar.</summary>
    public const string Bicim_OncekiTahmin = "Önceki tahmin {0}";

    /// <summary>Bakiyeyi seçilen güne yazar.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Dönem bitmiş ama kapanmamışken bakiye yerine (S68-4).</summary>
    public const string Cumle_OnceKapanis = "Dönem bitti. Bakiye girmeden önce dönemi kapat.";

    /// <summary>Biten dönemin kapanışını açar (V11).</summary>
    public const string Aksiyon_DonemiKapat = "Dönemi Kapat";

    /// <summary>Açık dönem okunamadığında.</summary>
    public const string Hata_BakiyeGirisYuklenemedi = "Bakiye bilgileri yüklenirken bir sorun oluştu.";
}
