#pragma warning disable CA1707 // GK5 kuralı önekleri zorunlu kılar.

namespace Mizan.App.Resources;

/// <summary>
/// "12 Dönem" sayfasında (EK-V8) görünen metinlerin kataloğu. Değerler
/// <c>Resources/Strings/FuturePeriodsStrings.json</c> ile aynıdır; GK5 sınırları JSON'dan denetlenir.
/// Ekranda kodun iç adı ("projeksiyon", "zincir") geçmez; kullanıcı dönem ve dönem sonu der.
/// </summary>
public static class FuturePeriodsStrings
{
    /// <summary>Hero rakamın etiketi: 12 dönemin en düşük dönem sonu.</summary>
    public const string Etiket_EnDusukDonemSonu = "EN DÜŞÜK DÖNEM SONU";

    /// <summary>Hero rakamın altında hangi dönem olduğu; {0} dönemin ilk günü.</summary>
    public const string Bicim_EnDusukDonem = "{0} dönemi sonunda";

    /// <summary>Gidişat kartında son dönemin sonu.</summary>
    public const string Etiket_OnIkiDonemSonra = "12 dönem sonra";

    /// <summary>Gidişat kartında 12 dönemin toplam faizi (kart + KMH).</summary>
    public const string Etiket_OnIkiDonemdeFaiz = "12 dönemde faiz";

    /// <summary>Dönem sonları ızgarasının etiketi.</summary>
    public const string Etiket_DonemSonlari = "DÖNEM SONLARI";

    /// <summary>Açık dönem ya da kurulabilir plan yokken boş hâlin metni (S74-2).</summary>
    public const string Bos_OnIkiDonemYok = "12 dönem, gelir ya da bakiye bilgisi girildiğinde hesaplanır.";

    /// <summary>Boş hâlin aksiyonu: gelir ve bakiye bilgisinin girildiği yer.</summary>
    public const string Aksiyon_FinansalYapiyaGit = "Finansal Yapı'ya Git";

    /// <summary>12 dönem okunamadığında ya da hesaplanamadığında hata metni.</summary>
    public const string Hata_OnIkiDonemHesaplanamadi = "12 dönem şu an hesaplanamadı.";
}
