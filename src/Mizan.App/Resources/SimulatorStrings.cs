#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Simülatör ve deneme formunda (EK-V10) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/SimulatorStrings.json</c> ile aynıdır.
/// </summary>
public static class SimulatorStrings
{
    /// <summary>Nakit ödeme denemesinin form başlığı.</summary>
    public const string Baslik_NakitOdeme = "Nakit Ödeme";

    /// <summary>Denemeler listesi kartının üst etiketi.</summary>
    public const string Etiket_Denemeler = "DENEMELER";

    /// <summary>Deneme satırında nakit ödemenin tür adı.</summary>
    public const string Etiket_NakitOdeme = "Nakit ödeme";

    /// <summary>Tarihi geçen denemenin satırında bağlamın yerine geçen işaret (S76-5).</summary>
    public const string Etiket_TarihiGecti = "Tarihi geçti";

    /// <summary>Deneme adı alanının etiketi.</summary>
    public const string Etiket_Ad = "AD";

    /// <summary>Tutar alanının etiketi.</summary>
    public const string Etiket_Tutar = "TUTAR";

    /// <summary>Tarih alanının etiketi.</summary>
    public const string Etiket_Tarih = "TARİH";

    /// <summary>Sonuç kartının hero etiketi: denemeyle 12 dönemin en düşük dönem sonu.</summary>
    public const string Etiket_EnDusukDonemSonu = "EN DÜŞÜK DÖNEM SONU";

    /// <summary>Hero'nun altındaki fark satırının etiketi: denemeyle, şu anki gidişatın aynı dönemine göre fark.</summary>
    public const string Etiket_SuAnkiGidisataGore = "Şu anki gidişata göre";

    /// <summary>ComparisonStrip'teki plan (şu anki gidişat) başlığı.</summary>
    public const string Etiket_SuAn = "ŞU AN";

    /// <summary>ComparisonStrip'teki senaryo (denemeyle) başlığı.</summary>
    public const string Etiket_Denemeyle = "DENEMEYLE";

    /// <summary>ComparisonStrip'teki fark başlığı.</summary>
    public const string Etiket_Fark = "FARK";

    /// <summary>Faiz farkı satırının etiketi.</summary>
    public const string Etiket_FaizFarki = "Faiz farkı";

    /// <summary>Hero rakamın altında hangi dönem olduğu; {0} dönemin ilk günü.</summary>
    public const string Bicim_EnDusukDonem = "{0} dönemi sonunda";

    /// <summary>Deneme altındaki şu anki bakiye/faiz metni; {0} tutar.</summary>
    public const string Bicim_SuAn = "Şu an {0}";

    /// <summary>Açık dönem ya da kurulabilir plan yokken sonuç kartının yerindeki metin (S76-1).</summary>
    public const string Bos_SimulatorYok = "Simülatör, gelir ya da bakiye bilgisi girildiğinde hesaplanır.";

    /// <summary>Boş hâlin aksiyonu: gelir ve bakiye bilgisinin girildiği yer.</summary>
    public const string Aksiyon_FinansalYapiyaGit = "Finansal Yapı'ya Git";

    /// <summary>Sonuç hesaplanamadığında (örn. denemedeki kart silinmiş) sonuç kartının yerindeki metin.</summary>
    public const string Hata_SimulasyonHesaplanamadi = "Sonuç şu an hesaplanamadı.";

    /// <summary>Deneme adı alanının yer tutucusu.</summary>
    public const string YerTutucu_Ad = "Örn. Telefon, tatil";

    /// <summary>Tutar alanının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Deneme satırının bağlamı: {0} tür, {1} tarih.</summary>
    public const string Bicim_DenemeBaglami = "{0} · {1}";

    /// <summary>Hiç deneme yokken listenin yerindeki cümle.</summary>
    public const string Cumle_DenemeYok = "Ekle ile bir harcamayı dene; gerçek kayıtların değişmez.";

    /// <summary>Başlıktaki deneme ekleme düğmesi.</summary>
    public const string Aksiyon_Ekle = "Ekle";

    /// <summary>Denemeyi listeye yazar.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Formdan çıkar.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Çalışma listesi okunamadığında gösterilen metin.</summary>
    public const string Hata_SimulatorAcilamadi = "Denemeler şu an okunamadı.";

    /// <summary>Düzenlenecek deneme okunamadığında gösterilen metin.</summary>
    public const string Hata_DenemeYuklenemedi = "Deneme şu an okunamadı.";
}
