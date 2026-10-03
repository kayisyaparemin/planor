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

    /// <summary>Kartla harcama denemesinin form başlığı.</summary>
    public const string Baslik_KartlaHarcama = "Kartla Harcama";

    /// <summary>Düzenli ödeme denemesinin form başlığı.</summary>
    public const string Baslik_DuzenliOdeme = "Düzenli Ödeme";

    /// <summary>Kart ödeme şekli denemesinin form başlığı.</summary>
    public const string Baslik_KartOdemeSekli = "Kart Ödeme Şekli";

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

    /// <summary>Hedef kart etiketi.</summary>
    public const string Etiket_Kart = "KART";

    /// <summary>Ödeme şekli etiketi.</summary>
    public const string Etiket_OdemeSekli = "ÖDEME ŞEKLİ";

    /// <summary>Kapsam etiketi.</summary>
    public const string Etiket_Kapsam = "KAPSAM";

    /// <summary>Taksit sayısı etiketi.</summary>
    public const string Etiket_TaksitSayisi = "TAKSİT SAYISI";

    /// <summary>Ödeme sayısı etiketi.</summary>
    public const string Etiket_OdemeSayisi = "ÖDEME SAYISI";

    /// <summary>Hangi ekstreden itibaren geçerli olacağı tarihi etiketi.</summary>
    public const string Etiket_HangiEkstredenItibaren = "HANGİ EKSTREDEN İTİBAREN";

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

    /// <summary>Taksit sayısı yer tutucusu.</summary>
    public const string YerTutucu_TaksitSayisi = "Boş bırakırsan tek çekim";

    /// <summary>Ödeme sayısı yer tutucusu.</summary>
    public const string YerTutucu_OdemeSayisi = "Örn. 12";

    /// <summary>Tamamını öde seçeneği.</summary>
    public const string Secenek_TamaminiOde = "Tamamını öde";

    /// <summary>Asgari öde seçeneği.</summary>
    public const string Secenek_AsgariOde = "Asgari öde";

    /// <summary>Yalnızca bu ekstre seçeneği.</summary>
    public const string Secenek_YalnizcaBuEkstre = "Yalnızca bu ekstre";

    /// <summary>Bundan sonraki tüm ekstreler seçeneği.</summary>
    public const string Secenek_TumEkstreler = "Bundan sonraki tüm ekstreler";

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

    /// <summary>Kredi çekme formunun başlığı.</summary>
    public const string Baslik_KrediCekme = "Kredi Çekme";

    /// <summary>Taksitli nakit borç formunun başlığı.</summary>
    public const string Baslik_TaksitliNakitBorc = "Taksitli Nakit Borç";

    /// <summary>Krediye erken ödeme formunun başlığı.</summary>
    public const string Baslik_KrediyeErkenOdeme = "Krediye Erken Ödeme";

    /// <summary>Kredi adı alanı etiketi.</summary>
    public const string Etiket_Kredi = "KREDİ";

    /// <summary>Toplam geri ödeme alanı etiketi.</summary>
    public const string Etiket_ToplamGeriOdeme = "TOPLAM GERİ ÖDEME";

    /// <summary>İlk ödeme tarihi alanı etiketi.</summary>
    public const string Etiket_IlkOdemeTarihi = "İLK ÖDEME TARİHİ";

    /// <summary>Kredi faizi kazancı metrik satırı etiketi.</summary>
    public const string Etiket_KrediFaizKazanci = "Kredi faizi kazancı";

    /// <summary>Kredinin maliyeti metrik satırı etiketi.</summary>
    public const string Etiket_KredininMaliyeti = "Kredinin maliyeti";

    /// <summary>Tamamen kapat erken ödeme seçeneği.</summary>
    public const string Secenek_TamamenKapat = "Tamamen kapat";

    /// <summary>Vadeyi kısalt erken ödeme seçeneği.</summary>
    public const string Secenek_VadeyiKisalt = "Vadeyi kısalt (ara ödeme)";

    /// <summary>Taksiti azalt erken ödeme seçeneği.</summary>
    public const string Secenek_TaksitiAzalt = "Taksiti azalt (ara ödeme)";

    /// <summary>Toplam geri ödeme alanı yer tutucusu.</summary>
    public const string YerTutucu_ToplamGeriOdeme = "Örn. 65.000";

    /// <summary>Geçersiz toplam geri ödeme hata mesajı.</summary>
    public const string Hata_GecersizToplamGeriOdeme = "Toplam geri ödeme ana tutardan düşük olamaz.";

    /// <summary>Geçersiz ilk ödeme tarihi hata mesajı.</summary>
    public const string Hata_IlkOdemeTarihiGecersiz = "İlk ödeme tarihi başlangıç tarihinden önce olamaz.";

    /// <summary>Kayıtlı kredi bulunamadığında uyarı başlığı.</summary>
    public const string Hata_KrediBulunamadi = "Kayıtlı kredi bulunamadı";

    /// <summary>Kayıtlı kredi bulunamadığında uyarı mesajı.</summary>
    public const string Mesaj_KrediBulunamadi = "Krediye erken ödeme denemek için önce Finansal Yapı'dan bir kredi eklemelisin.";

    /// <summary>Tek seferlik gelir denemesinin form başlığı.</summary>
    public const string Baslik_TekSeferlikGelir = "Tek Seferlik Gelir";

    /// <summary>Gelir değişikliği denemesinin form başlığı.</summary>
    public const string Baslik_GelirDegisikligi = "Gelir Değişikliği";

    /// <summary>Hedef düzenli gelir etiketi.</summary>
    public const string Etiket_Gelir = "GELİR";

    /// <summary>Kayıtlı düzenli gelir bulunamadığında uyarı başlığı.</summary>
    public const string Hata_GelirBulunamadi = "Kayıtlı düzenli gelir bulunamadı";

    /// <summary>Kayıtlı düzenli gelir bulunamadığında uyarı mesajı.</summary>
    public const string Mesaj_GelirBulunamadi = "Gelir değişikliği denemek için önce Finansal Yapı'dan bir düzenli gelir eklemelisin.";
}
