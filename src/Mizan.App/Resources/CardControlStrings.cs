#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// Kart kontrol ekranında (EK-V7) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/CardControlStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// </summary>
public static class CardControlStrings
{
    /// <summary>Sayfa başlığının üstündeki etiket.</summary>
    public const string Etiket_KrediKarti = "KREDİ KARTI";

    /// <summary>Banka, kesim günü ve son ödeme günü satırı: {0} banka, {1} kesim, {2} son ödeme.</summary>
    public const string Bicim_KartDongusu = "{0} · Kesim {1} · Son ödeme {2}";

    /// <summary>Sıradaki ödeme kartının etiketi.</summary>
    public const string Etiket_SiradakiOdeme = "SIRADAKİ ÖDEME";

    /// <summary>Sıradaki ödeme kesilmiş ekstreden değil tahminden geliyorsa.</summary>
    public const string Etiket_Tahmini = "tahmini";

    /// <summary>Sıradaki ödeme kesilmiş ekstreden geliyorsa.</summary>
    public const string Etiket_KesilmisEkstre = "kesilmiş ekstre";

    /// <summary>Ekstre ve asgari satırı: {0} ekstre tutarı, {1} asgari.</summary>
    public const string Bicim_EkstreAsgari = "Ekstre {0} · asgari {1}";

    /// <summary>Asgari ödeme seçeneği.</summary>
    public const string Aksiyon_Asgari = "Asgari";

    /// <summary>Tamamını ödeme seçeneği.</summary>
    public const string Aksiyon_Tamami = "Tamamı";

    /// <summary>Özel tutar seçeneği.</summary>
    public const string Aksiyon_Ozel = "Özel";

    /// <summary>Özel tutar giriş alanının yer tutucusu.</summary>
    public const string YerTutucu_OzelTutar = "Ödeyeceğin tutar";

    /// <summary>Kaydet düğmesi.</summary>
    public const string Aksiyon_Kaydet = "Kaydet";

    /// <summary>Kararın bedeli: {0} devreden tutar, {1} sonraki ekstreye binen faiz.</summary>
    public const string Bicim_DevirBedeli = "{0} sonraki ekstreye devreder, ~{1} faiz biner.";

    /// <summary>Ekstre giriş formunu açan düğme (ekstre yokken).</summary>
    public const string Aksiyon_EkstreyiGir = "Ekstreyi Gir";

    /// <summary>Kesilmiş ekstreyi düzeltme formunu açan düğme.</summary>
    public const string Aksiyon_EkstreyiDuzenle = "Ekstreyi Düzenle";

    /// <summary>Kartın önümüzdeki ödemesi yokken gösterilen cümle.</summary>
    public const string Cumle_OdemeYok = "Bu kartın önümüzdeki ödemesi yok. Banka ekstre kestiyse girebilirsin.";

    /// <summary>Giriş formu: ekstre tutarı etiketi.</summary>
    public const string Etiket_EkstreTutari = "EKSTRE TUTARI";

    /// <summary>Giriş formu: asgari ödeme etiketi.</summary>
    public const string Etiket_AsgariOdeme = "ASGARİ ÖDEME";

    /// <summary>Giriş formu: kesim tarihi etiketi.</summary>
    public const string Etiket_KesimTarihi = "KESİM TARİHİ";

    /// <summary>Giriş formu: son ödeme tarihi etiketi.</summary>
    public const string Etiket_SonOdemeTarihi = "SON ÖDEME TARİHİ";

    /// <summary>Tutar alanlarının yer tutucusu.</summary>
    public const string YerTutucu_Tutar = "0";

    /// <summary>Formu kaydetmeden kapatan düğme.</summary>
    public const string Aksiyon_Vazgec = "Vazgeç";

    /// <summary>Sonraki ödemeler listesinin etiketi.</summary>
    public const string Etiket_SonrakiOdemeler = "SONRAKİ ÖDEMELER";

    /// <summary>Sonraki ödemeler listesinin altındaki ipucu.</summary>
    public const string Cumle_VadeKarari = "Bir vadeye dokunup yalnız o ay için karar verebilirsin.";

    /// <summary>Kartın varsayılan ödeme şekli satırının başlığı.</summary>
    public const string Baslik_VarsayilanOdeme = "Varsayılan ödeme şekli";

    /// <summary>Limit satırı: {0} limit, {1} kullanılabilir limit.</summary>
    public const string Bicim_LimitSatiri = "Limit {0} · kullanılabilir {1}";

    /// <summary>Ödeme kuralı satırı: {0} ödeme tipi, {1} kaynağı.</summary>
    public const string Bicim_OdemeKurali = "{0} · {1}";

    /// <summary>"Her ekstrede sor" seçiliyken hesabın varsayımı: {0} ödeme tipi.</summary>
    public const string Bicim_Varsayim = "varsayım {0}";

    /// <summary>Ödeme tipi: asgari.</summary>
    public const string Etiket_OdemeAsgari = "Asgari";

    /// <summary>Ödeme tipi: tamamı.</summary>
    public const string Etiket_OdemeTamami = "Tamamı";

    /// <summary>Ödeme tipi: sabit tutar.</summary>
    public const string Etiket_OdemeSabit = "Sabit tutar";

    /// <summary>Ödeme tipi belirlenemediğinde.</summary>
    public const string Etiket_OdemeBelirsiz = "Belirlenmedi";

    /// <summary>Kartın varsayılan ödeme şekli: her ekstrede sor.</summary>
    public const string Etiket_HerEkstredeSor = "Her ekstrede sor";

    /// <summary>Ödeme tipi bu vade için ayrıca seçildi.</summary>
    public const string Etiket_KuralVadeyeOzel = "bu vade için";

    /// <summary>Ödeme tipi kartın varsayılanından geliyor.</summary>
    public const string Etiket_KuralKartinVarsayilani = "kartın varsayılanı";

    /// <summary>Karar yok; ödeme tipi hesabın varsayımı.</summary>
    public const string Etiket_KuralVarsayim = "varsayım";

    /// <summary>Profilde kart yokken gösterilen boş durum metni.</summary>
    public const string Bos_KartYok = "Henüz kredi kartın yok. Kartlar Finansal Yapı'dan eklenir.";

    /// <summary>Kart okunamadığında gösterilen hata metni.</summary>
    public const string Hata_KartYuklenemedi = "Kart bilgileri okunamadı.";
}
