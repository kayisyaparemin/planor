namespace Mizan.Presentation.Automation;

/// <summary>
/// Dönem kapanışı sayfası (EK-V11) otomasyon kimlikleri.
/// </summary>
public static class PeriodSettlementAutomationIds
{
    /// <summary>Dönem kapanışı sayfası kimliği.</summary>
    public const string Page = "page-period-settlement";

    /// <summary>Dönem sonu özet kartı kimliği.</summary>
    public const string CardSummary = "card-period-summary";

    /// <summary>Farkın kaynağı kartı kimliği.</summary>
    public const string CardDifferences = "card-differences";

    /// <summary>Kapanış bakiyesi değiştirme butonu kimliği.</summary>
    public const string BtnChangeBalance = "btn-change-balance";

    /// <summary>Dönemi kapat birincil butonu kimliği.</summary>
    public const string BtnClosePeriod = "btn-confirm-close-period";

    /// <summary>Kapanışı erteleme (kapatma) butonu kimliği.</summary>
    public const string BtnDismiss = "btn-dismiss-settlement";

    /// <summary>Dönem sonu kapanış tutarı etiketi kimliği.</summary>
    public const string LblEndingBalance = "lbl-ending-balance";

    /// <summary>Plana göre fark etiketi kimliği.</summary>
    public const string LblEndingDeviation = "lbl-ending-deviation";

    /// <summary>Yaşam gideri satırı kimliği.</summary>
    public const string RowLivingSpend = "row-living-spend";

    /// <summary>Ödemeler satırı kimliği.</summary>
    public const string RowPayments = "row-payments";

    /// <summary>KMH faizi satırı kimliği.</summary>
    public const string RowDeficitInterest = "row-deficit-interest";

    /// <summary>Boş durum alanı kimliği.</summary>
    public const string StateEmpty = "state-empty-settlement";

    /// <summary>Hata durum alanı kimliği.</summary>
    public const string StateError = "state-error-settlement";

    /// <summary>Yükleniyor durum alanı kimliği.</summary>
    public const string StateLoading = "state-loading-settlement";
}
