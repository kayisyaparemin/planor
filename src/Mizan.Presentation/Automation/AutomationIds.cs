namespace Mizan.Presentation.Automation;

/// <summary>
/// UI otomasyonu ve E2E testlerinin ekrandaki ve kabuktaki öğeleri bulmasını sağlayan tekil kimlikler.
/// </summary>
public static class AutomationIds
{
    /// <summary>Flyout menüsü profil adı etiketi kimliği.</summary>
    public const string FlyoutProfileName = "lbl-flyout-profile-name";

    /// <summary>Flyout menüsü profil başlığı etiketi kimliği.</summary>
    public const string FlyoutProfileTitle = "lbl-flyout-profile-title";

    /// <summary>Flyout menüsü profil değiştirme menü öğesi kimliği.</summary>
    public const string FlyoutItemSwitchProfile = "flyout-item-switch-profile";

    /// <summary>Flyout menüsü ana sayfa öğesi kimliği.</summary>
    public const string FlyoutItemDashboard = "flyout-item-dashboard";

    /// <summary>Flyout menüsü 12 dönem öğesi kimliği.</summary>
    public const string FlyoutItemProjection = "flyout-item-projection";

    /// <summary>Flyout menüsü simülatör öğesi kimliği.</summary>
    public const string FlyoutItemSimulation = "flyout-item-simulation";

    /// <summary>Flyout menüsü finansal yapı öğesi kimliği.</summary>
    public const string FlyoutItemCommitments = "flyout-item-commitments";

    /// <summary>Flyout menüsü geçmiş öğesi kimliği.</summary>
    public const string FlyoutItemHistory = "flyout-item-history";

    /// <summary>Flyout menüsü ayarlar öğesi kimliği.</summary>
    public const string FlyoutItemSettings = "flyout-item-settings";

    /// <summary>Profil seçimi sayfası kimliği.</summary>
    public const string PageProfileSelection = "page-profile-selection";

    /// <summary>Yeni profil oluşturma butonu kimliği.</summary>
    public const string BtnNewProfile = "btn-new-profile";

    /// <summary>Yedekten profil ekleme butonu kimliği.</summary>
    public const string BtnAddFromBackup = "btn-add-from-backup";

    /// <summary>İlk kurulum temiz başlangıç butonu kimliği.</summary>
    public const string BtnCleanStart = "btn-clean-start";

    /// <summary>İlk kurulum yedekten geri yükleme butonu kimliği.</summary>
    public const string BtnRestore = "btn-restore-backup";

    /// <summary>Profil kartı işlem seçenekleri menü butonu kimliği.</summary>
    public const string BtnProfileOptions = "btn-profile-options";

    /// <summary>Kayıtlı profiller liste kapsayıcısı kimliği.</summary>
    public const string ListProfiles = "list-profiles";

    /// <summary>Profil bulunamadı boş durum alanı kimliği.</summary>
    public const string StateEmpty = "state-empty-profiles";

    /// <summary>Profil yükleme iskelet alanı kimliği.</summary>
    public const string StateLoading = "state-loading-profiles";

    /// <summary>Hatırlatıcı kartı kapsayıcı kimliği.</summary>
    public const string CardReminder = "card-reminder";

    /// <summary>Hatırlatıcı ödedim aksiyon butonu kimliği.</summary>
    public const string BtnReminderPay = "btn-reminder-pay";

    /// <summary>Hatırlatıcı ertele aksiyon butonu kimliği.</summary>
    public const string BtnReminderSnooze = "btn-reminder-snooze";
}
