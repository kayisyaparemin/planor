namespace Mizan.Presentation.Automation;

/// <summary>
/// "12 Dönem" sayfasının (V8) otomasyon kimlikleri. <see cref="AutomationIds"/> dosya sınırına dayandığı için
/// aynı ad alanında ayrı durur; kebab-case ve tekillik kuralları hepsine birlikte uygulanır.
/// </summary>
public static class FuturePeriodsAutomationIds
{
    /// <summary>12 dönem sayfası kimliği.</summary>
    public const string PageFuturePeriods = "page-future-periods";

    /// <summary>Gidişat kartı: en düşük dönem sonu, grafik, 12 dönem sonrası ve faiz.</summary>
    public const string CardFutureTrend = "card-future-trend";

    /// <summary>Hero rakam: 12 dönemin en düşük dönem sonu.</summary>
    public const string LblLowestEnding = "lbl-lowest-ending";

    /// <summary>Dönem sonları ızgarası.</summary>
    public const string GridFuturePeriods = "grid-future-periods";

    /// <summary>Boş hâl: açık dönem ya da kurulabilir plan yok.</summary>
    public const string StateFuturePeriodsEmpty = "state-future-periods-empty";

    /// <summary>Hata hâli: 12 dönem hesaplanamadı.</summary>
    public const string StateFuturePeriodsError = "state-future-periods-error";
}
