namespace Mizan.Presentation.Automation;

/// <summary>
/// Dönem ayrıntısı sayfasının (V9) otomasyon kimlikleri. <see cref="AutomationIds"/> dosya sınırına dayandığı için
/// aynı ad alanında ayrı durur; kebab-case ve tekillik kuralları hepsine birlikte uygulanır.
/// </summary>
public static class PeriodDetailAutomationIds
{
    /// <summary>Dönem ayrıntısı sayfası kimliği.</summary>
    public const string PagePeriodDetail = "page-period-detail";

    /// <summary>Akış kartı: dönem sonu, dönem başına göre fark ve akışın satırları.</summary>
    public const string CardPeriodFlow = "card-period-flow";

    /// <summary>Hero rakam: dönemin sonu.</summary>
    public const string LblPeriodEnding = "lbl-period-ending";

    /// <summary>Ödeme listesi; kart satırı Kart Kontrol'ü açar.</summary>
    public const string ListPeriodPayments = "list-period-payments";

    /// <summary>Kart faizi listesi.</summary>
    public const string ListPeriodCardInterest = "list-period-card-interest";

    /// <summary>Boş hâl: dönem 12 dönemin içinde değil.</summary>
    public const string StatePeriodDetailEmpty = "state-period-detail-empty";

    /// <summary>Hata hâli: dönem hesaplanamadı.</summary>
    public const string StatePeriodDetailError = "state-period-detail-error";
}
