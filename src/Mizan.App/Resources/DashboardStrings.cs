#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Ana sayfada (EK-V3) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/DashboardStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// Ekranda kodun iç adı ("gözlem") geçmez; kullanıcı "bakiye" der (S72).
/// </summary>
public static class DashboardStrings
{
    /// <summary>Başlıktaki dönem aralığı; {0} ilk gün, {1} son gün.</summary>
    public const string Bicim_DonemAraligi = "{0} – {1}";

    /// <summary>Başlıktaki gün sayacı; {0} geçen gün, {1} dönemin gün sayısı.</summary>
    public const string Bicim_GunSayaci = "{0} / {1} gün";

    /// <summary>Dönem bitmiş ama kapanmamışken başlıktaki sayaç yerine.</summary>
    public const string Etiket_Bitti = "Bitti";

    /// <summary>Bakiye girildiyse hero rakamın etiketi.</summary>
    public const string Etiket_DonemSonuTahmini = "DÖNEM SONU TAHMİNİ";

    /// <summary>Bakiye girilmediyse hero rakamın etiketi: rakam planın dönem sonudur.</summary>
    public const string Etiket_PlanlananDonemSonu = "PLANLANAN DÖNEM SONU";

    /// <summary>Tahminin plandan farkı; {0} işaretli tutar.</summary>
    public const string Bicim_PlanaGore = "Plana göre {0}";

    /// <summary>Planın dönem sonu; {0} tutar.</summary>
    public const string Bicim_Plan = "Plan {0}";

    /// <summary>Plan / şu an tablosunun planlanan tutar sütunu (S87).</summary>
    public const string Etiket_Plan = "PLAN";

    /// <summary>Plan / şu an tablosunun bugünkü duruma göre tutar sütunu; simülatördeki "Şu an" ile aynı söz (GS28).</summary>
    public const string Etiket_SuAn = "ŞU AN";

    /// <summary>Plan / şu an tablosunda eksi bakiyenin faiz satırı.</summary>
    public const string Etiket_KmhFaizi = "KMH faizi";

    /// <summary>Halkanın ortasındaki tutarın etiketi.</summary>
    public const string Etiket_KalanYasamGideri = "Kalan yaşam gideri";

    /// <summary>Bakiye girilmediyse halkanın ortasında.</summary>
    public const string Etiket_BakiyeGirilmedi = "Bakiye girilmedi";

    /// <summary>Yaşam gideri için ayrılan tutarın satırı; bakiye girilmese de bilinir (S87-6).</summary>
    public const string Etiket_Planlanan = "Planlanan";

    /// <summary>Yaşam giderinden harcanan tutarın ve oranın satırı.</summary>
    public const string Etiket_Harcanan = "Harcanan";

    /// <summary>Harcanan satırının değeri; {0} tutar, {1} oran.</summary>
    public const string Bicim_TutarOran = "{0} · {1}";

    /// <summary>Dönemden geçen sürenin satırı.</summary>
    public const string Etiket_GecenSure = "Geçen süre";

    /// <summary>Harcama süreden hızlıysa; {0} puan.</summary>
    public const string Bicim_TempoOnde = "Harcama, geçen sürenin {0} puan önünde.";

    /// <summary>Harcama süreden yavaşsa; {0} puan.</summary>
    public const string Bicim_TempoGeride = "Harcama, geçen sürenin {0} puan gerisinde.";

    /// <summary>Harcama ile süre aynı hızdaysa.</summary>
    public const string Cumle_TempoAyniHiz = "Harcama, geçen süreyle aynı hızda.";

    /// <summary>Bakiye girilmediyse halka sayfasının tek cümlesi.</summary>
    public const string Cumle_TempoIlkBakiye = "Harcama temposu ilk bakiye girişiyle hesaplanır.";

    /// <summary>Bakiye kartının etiketi.</summary>
    public const string Etiket_BankadakiBakiye = "BANKADAKİ BAKİYE";

    /// <summary>Hiç bakiye girilmediyse tutarın yerinde.</summary>
    public const string Etiket_HenuzGirilmedi = "Henüz girilmedi";

    /// <summary>"Bakiye gir" sayfasını açan buton.</summary>
    public const string Aksiyon_BakiyeGir = "Bakiye Gir";

    /// <summary>Dönem bitmiş ama kapanmamışken bakiye kartının yerindeki etiket.</summary>
    public const string Etiket_DonemBitti = "DÖNEM BİTTİ";

    /// <summary>Biten dönemin kapanışını açan buton.</summary>
    public const string Aksiyon_DonemiKapat = "Dönemi Kapat";

    /// <summary>Kalan ödemeler kartının etiketi.</summary>
    public const string Etiket_KalanOdemeler = "KALAN ÖDEMELER";

    /// <summary>Kalan ödemeler kartının notu; {0} adet, {1} toplam tutar. Dönem ayrıntısının ödeme listesi de kullanır.</summary>
    public const string Bicim_OdemeAdediToplam = "{0} ödeme · {1}";

    /// <summary>Hatırlatıcıda ödemenin yapıldığını söyleyen buton.</summary>
    public const string Aksiyon_Odedim = "Ödedim";

    /// <summary>Hatırlatıcıyı erteleyen buton.</summary>
    public const string Aksiyon_Ertele = "Ertele";

    /// <summary>Açık dönem yokken boş hâlin cümlesi.</summary>
    public const string Cumle_AcikDonemYokRehber = "Yeni bir dönem planlamak için gelir ve giderlerini kurabilirsin.";

    /// <summary>Ana sayfa okunamadığında hata metni; hata hesaptan da gelebildiği için genel kalır.</summary>
    public const string Hata_DashboardYuklenemedi = "Ana sayfa verileri yüklenirken bir sorun oluştu.";
}
