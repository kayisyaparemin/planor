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
    public const string FlyoutItemFinancialStructure = "flyout-item-financial-structure";

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

    /// <summary>Ana sayfa kapsayıcı kimliği.</summary>
    public const string PageDashboard = "page-dashboard";

    /// <summary>Bugünkü nakit bakiye giriş alanı kimliği.</summary>
    public const string InputTodayBalance = "input-today-balance";

    /// <summary>Gözlemi kaydet butonu kimliği.</summary>
    public const string BtnSaveObservation = "btn-save-observation";

    /// <summary>Bu dönemi kapat butonu kimliği.</summary>
    public const string BtnClosePeriod = "btn-close-period";

    /// <summary>Ana sayfa ayarlar başlık butonu kimliği.</summary>
    public const string BtnDashboardSettings = "btn-dashboard-settings";

    /// <summary>Görsel merkez özet kartı kimliği.</summary>
    public const string CardVisualCenter = "card-visual-center";

    /// <summary>Bütçe oranı halka grafik göstergesi kimliği.</summary>
    public const string GaugeBudgetRatio = "gauge-budget-ratio";

    /// <summary>Dönem sonu tahmini metin etiketi kimliği.</summary>
    public const string LblProjectedEnding = "lbl-projected-ending";

    /// <summary>Kalan ödemeler listesi kimliği.</summary>
    public const string ListRemainingPayments = "list-remaining-payments";

    /// <summary>Ana sayfa öncelikli uyarı bilgi çubuğu kimliği.</summary>
    public const string BannerDashboardAlert = "banner-dashboard-alert";

    /// <summary>Ana sayfa uyarı aksiyon butonu kimliği.</summary>
    public const string BtnDashboardAlertAction = "btn-dashboard-alert-action";

    /// <summary>Kurulum sihirbazı sayfası kimliği.</summary>
    public const string PageOnboarding = "page-onboarding";

    /// <summary>Kurulum sonraki adım butonu kimliği.</summary>
    public const string BtnOnboardingNext = "btn-onboarding-next";

    /// <summary>Kurulum önceki adım butonu kimliği.</summary>
    public const string BtnOnboardingBack = "btn-onboarding-back";

    /// <summary>Kurulum adımı atla butonu kimliği.</summary>
    public const string BtnOnboardingSkip = "btn-onboarding-skip";

    /// <summary>Kurulumu tamamlayıp Planör'ü başlatma butonu kimliği.</summary>
    public const string BtnOnboardingStart = "btn-onboarding-start";

    /// <summary>Kurulumda gelir ekleme butonu kimliği.</summary>
    public const string BtnOnboardingAddIncome = "btn-onboarding-add-income";

    /// <summary>Kurulumda kart ekleme butonu kimliği.</summary>
    public const string BtnOnboardingAddCard = "btn-onboarding-add-card";

    /// <summary>Kurulumda kredi ekleme butonu kimliği.</summary>
    public const string BtnOnboardingAddLoan = "btn-onboarding-add-loan";

    /// <summary>Kurulumda harcama/ödeme ekleme butonu kimliği.</summary>
    public const string BtnOnboardingAddExpense = "btn-onboarding-add-expense";

    /// <summary>Kart kontrol sayfası kimliği.</summary>
    public const string PageCardControl = "page-card-control";

    /// <summary>Kart kontrol başlığındaki kart seçme ikonu kimliği.</summary>
    public const string BtnSelectCard = "btn-select-card";

    /// <summary>Sıradaki vadede ödenecek tutar (hero) kimliği.</summary>
    public const string LblNextPayment = "lbl-next-payment";

    /// <summary>Sıradaki ödeme: asgari seçeneği kimliği.</summary>
    public const string BtnPaymentModeMinimum = "btn-payment-mode-minimum";

    /// <summary>Sıradaki ödeme: tamamı seçeneği kimliği.</summary>
    public const string BtnPaymentModeFull = "btn-payment-mode-full";

    /// <summary>Sıradaki ödeme: özel tutar seçeneği kimliği.</summary>
    public const string BtnPaymentModeCustom = "btn-payment-mode-custom";

    /// <summary>Sıradaki ödeme: özel tutar giriş alanı kimliği.</summary>
    public const string InputPaymentCustomAmount = "input-payment-custom-amount";

    /// <summary>Sıradaki ödeme: özel tutarı kaydet butonu kimliği.</summary>
    public const string BtnSavePaymentCustomAmount = "btn-save-payment-custom-amount";

    /// <summary>Ekstre giriş / düzenleme formunu açan buton kimliği.</summary>
    public const string BtnStatementEntry = "btn-statement-entry";

    /// <summary>Sonraki ödemeler listesi kimliği.</summary>
    public const string ListUpcomingPayments = "list-upcoming-payments";

    /// <summary>Kartın varsayılan ödeme şekli satırı kimliği.</summary>
    public const string NavDefaultPayment = "nav-default-payment";

    /// <summary>Ekstre giriş formu: ekstre tutarı alanı kimliği.</summary>
    public const string InputStatementAmount = "input-statement-amount";

    /// <summary>Ekstre giriş formu: asgari ödeme alanı kimliği.</summary>
    public const string InputStatementMinimum = "input-statement-minimum";

    /// <summary>Ekstre giriş formu: kesim tarihi seçici kimliği.</summary>
    public const string PickerStatementDate = "picker-statement-date";

    /// <summary>Ekstre giriş formu: son ödeme tarihi seçici kimliği.</summary>
    public const string PickerStatementDueDate = "picker-statement-due-date";

    /// <summary>Ekstre giriş formu: kaydet butonu kimliği.</summary>
    public const string BtnSaveStatement = "btn-save-statement";

    /// <summary>Ekstre giriş formu: vazgeç butonu kimliği.</summary>
    public const string BtnCancelStatement = "btn-cancel-statement";

    /// <summary>Finansal yapı sayfası kimliği.</summary>
    public const string PageFinancialStructure = "page-financial-structure";

    /// <summary>Finansal yapı: gelirler listesi kimliği.</summary>
    public const string ListStructureIncomes = "list-structure-incomes";

    /// <summary>Finansal yapı: kartlar listesi kimliği.</summary>
    public const string ListStructureCards = "list-structure-cards";

    /// <summary>Finansal yapı: krediler listesi kimliği.</summary>
    public const string ListStructureLoans = "list-structure-loans";

    /// <summary>Finansal yapı: ödemeler listesi kimliği.</summary>
    public const string ListStructurePayments = "list-structure-payments";

    /// <summary>Finansal yapı: başlıktaki ekle aksiyonu kimliği.</summary>
    public const string BtnStructureAdd = "btn-structure-add";
}
