namespace Mizan.Presentation.Automation;

/// <summary>
/// Finansal Yapı'dan açılan kayıt formlarının (kart, kredi, gelir, ödeme — V6b–V6e) otomasyon
/// kimlikleri. <see cref="AutomationIds"/> ile aynı ad alanında ve aynı kurallarla durur; dosya
/// sınırı (200 satır) yüzünden ayrıdır, kimlikler iki sınıf arasında da tekildir.
/// </summary>
public static class RecordFormAutomationIds
{
    /// <summary>Kart formu sayfası kimliği.</summary>
    public const string PageCardForm = "page-card-form";

    /// <summary>Kart formu: kart adı girişi kimliği.</summary>
    public const string InputCardName = "input-card-name";

    /// <summary>Kart formu: banka girişi kimliği.</summary>
    public const string InputCardBank = "input-card-bank";

    /// <summary>Kart formu: limit girişi kimliği.</summary>
    public const string InputCardLimit = "input-card-limit";

    /// <summary>Kart formu: güncel borç girişi kimliği.</summary>
    public const string InputCardDebt = "input-card-debt";

    /// <summary>Kart formu: kesim günü girişi kimliği.</summary>
    public const string InputCardClosingDay = "input-card-closing-day";

    /// <summary>Kart formu: son ödeme günü girişi kimliği.</summary>
    public const string InputCardDueDay = "input-card-due-day";

    /// <summary>Kart formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveCard = "btn-save-card";

    /// <summary>Kart formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelCard = "btn-cancel-card";
}
