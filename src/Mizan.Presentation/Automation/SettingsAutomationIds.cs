namespace Mizan.Presentation.Automation;

/// <summary>
/// Ayarlar sayfası (EK-V13, GS33) UI otomasyon kimlikleri.
/// </summary>
public static class SettingsAutomationIds
{
    /// <summary>Ayarlar sayfası ana kapsayıcı kimliği.</summary>
    public const string Page = "page-settings";

    /// <summary>Dönem çapa günü giriş alanı kimliği.</summary>
    public const string InputAnchorDay = "input-settings-anchor-day";

    /// <summary>Dönemsel yaşam gideri havuzu giriş alanı kimliği.</summary>
    public const string InputAllowance = "input-settings-allowance";

    /// <summary>Kredi kartı devreden borç faizi giriş alanı kimliği.</summary>
    public const string InputCardInterestRate = "input-settings-card-rate";

    /// <summary>Finansman açığı faizi giriş alanı kimliği.</summary>
    public const string InputDeficitInterestRate = "input-settings-deficit-rate";

    /// <summary>Bildirim modu: Kapalı butonu kimliği.</summary>
    public const string BtnReminderDisabled = "btn-settings-reminder-disabled";

    /// <summary>Bildirim modu: Rahat butonu kimliği.</summary>
    public const string BtnReminderRelaxed = "btn-settings-reminder-relaxed";

    /// <summary>Bildirim modu: Agresif butonu kimliği.</summary>
    public const string BtnReminderAggressive = "btn-settings-reminder-aggressive";

    /// <summary>Şimdi yedekle butonu kimliği.</summary>
    public const string BtnBackUpNow = "btn-settings-backup-now";

    /// <summary>Yedek klasörü erişim izni butonu kimliği.</summary>
    public const string BtnRequestBackupAccess = "btn-settings-backup-access";

    /// <summary>Değişiklikleri kaydet birincil butonu kimliği.</summary>
    public const string BtnSave = "btn-settings-save";

    /// <summary>Son yedekleme durumu metni etiketi kimliği.</summary>
    public const string LblLastBackup = "lbl-settings-last-backup";

    /// <summary>Yükleniyor durumu alanı kimliği.</summary>
    public const string StateLoading = "state-loading-settings";

    /// <summary>Hata durumu alanı kimliği.</summary>
    public const string StateError = "state-error-settings";
}
