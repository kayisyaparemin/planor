namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// SQLite veritabanının tablo adları, tarih ve sayı biçimlendirme kuralları ile dosya adlarını
/// merkezi olarak barındıran tanımlayıcı sınıf. Şema sürümü burada değil, göç listesinde durur
/// (<see cref="SchemaMigrations.CurrentVersion"/>, S69).
/// </summary>
public static class DatabaseConstants
{
    /// <summary>
    /// Kültürden bağımsız ISO 8601 tarih biçim deseni.
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Kültürden bağımsız ISO 8601 zaman damgası (UTC) biçim deseni.
    /// </summary>
    public const string DateTimeOffsetFormat = "O";

    /// <summary>
    /// Varsayılan finansal planlama ve faiz oranı (yıllık/aylık %5 referans başlangıç değeri).
    /// </summary>
    public const decimal DefaultPlanningInterestRate = 0.05m;

    /// <summary>Düzenli gelir akışları ana tablosu.</summary>
    public const string TableRecurringIncomes = "recurring_incomes";

    /// <summary>Düzenli gelir akışlarının zaman içindeki etkin tutar ve zam geçmişi tablosu.</summary>
    public const string TableIncomeAmountHistories = "income_amount_histories";

    /// <summary>Münferit tek seferlik arızi gelirler tablosu.</summary>
    public const string TableAdHocIncomes = "ad_hoc_incomes";

    /// <summary>Banka kredileri sözleşme tablosu.</summary>
    public const string TableLoans = "loans";

    /// <summary>Kredilere ait planlanmış ara ödeme ve erken kapama taahhütleri tablosu.</summary>
    public const string TableLoanPrepayments = "loan_prepayments";

    /// <summary>Kredi ve kart dışındaki vadeli/geçici borç planları tablosu.</summary>
    public const string TablePaymentPlans = "payment_plans";

    /// <summary>Geçici borç planlarına ait vadeli taksit kayıtları tablosu.</summary>
    public const string TablePaymentInstallments = "payment_installments";

    /// <summary>Kredi kartı ana tanımları ve limit parametreleri tablosu.</summary>
    public const string TableCreditCards = "credit_cards";

    /// <summary>Kredi kartına ait bekleyen veya taksitli harcamalar tablosu.</summary>
    public const string TableCardInstallments = "card_installments";

    /// <summary>Banka tarafından kesilmiş kredi kartı ekstreleri tablosu.</summary>
    public const string TableCreditCardStatements = "credit_card_statements";

    /// <summary>Kredi kartı özel son ödeme günü planları tablosu.</summary>
    public const string TableCreditCardPaymentPlans = "credit_card_payment_plans";

    /// <summary>Kredi kartı etkin tarihli ödeme stratejisi tercihleri tablosu.</summary>
    public const string TableCreditCardPaymentPreferences = "credit_card_payment_preferences";

    /// <summary>Planlanan tek seferlik büyük harcamalar tablosu.</summary>
    public const string TablePlannedLargeExpenses = "planned_large_expenses";

    /// <summary>Kullanıcı finansal planlama ayarları tablosu.</summary>
    public const string TableSettings = "settings";

    /// <summary>Finansal durum ve takvim döngüsü anlık görüntüleri tablosu.</summary>
    public const string TableFinancialSnapshots = "financial_snapshots";

    /// <summary>Dönem başında dondurulan nakit akış taahhüt planları tablosu.</summary>
    public const string TablePeriodPlanSnapshots = "period_plan_snapshots";

    /// <summary>Dondurulan dönem planına ait kilitlenmiş ödeme satırları tablosu.</summary>
    public const string TablePeriodPlanPaymentLines = "period_plan_payment_lines";

    /// <summary>Dondurulan dönem planına ait tarihli gelir satırları tablosu.</summary>
    public const string TablePeriodPlanIncomeLines = "period_plan_income_lines";

    /// <summary>Açık dönem süresince yapılan plan revizyonları üst tablosu.</summary>
    public const string TablePeriodPlanRevisions = "period_plan_revisions";

    /// <summary>Plan revizyonuna ait revize ödeme satırları tablosu.</summary>
    public const string TablePeriodPlanRevisionPaymentLines = "period_plan_revision_payment_lines";

    /// <summary>Plan revizyonuna ait revize gelir satırları tablosu.</summary>
    public const string TablePeriodPlanRevisionIncomeLines = "period_plan_revision_income_lines";

    /// <summary>Kapanan nakit akış döneminin kesinleşmiş fiilî durum ve karne tablosu.</summary>
    public const string TablePeriodActuals = "period_actuals";

    /// <summary>Kapanan dönemin fiilî ödeme gerçekleşmeleri tablosu.</summary>
    public const string TableActualPayments = "actual_payments";

    /// <summary>Kapanan dönemin plansız fiilî nakit akışları tablosu.</summary>
    public const string TableActualFlows = "actual_flows";

    /// <summary>Fiilî serbest yaşam harcamasının kategori dökümü tablosu.</summary>
    public const string TableActualLivingBreakdowns = "actual_living_breakdowns";

    /// <summary>Açık dönemin canlı ara nakit bakiyesi ve harcama gözlem defteri tablosu.</summary>
    public const string TablePeriodObservations = "period_observations";

    /// <summary>Kullanıcının açık dönemin ödeme satırlarına koyduğu ödeme işaretleri tablosu (v2).</summary>
    public const string TablePeriodPaymentMarks = "period_payment_marks";

    /// <summary>Kullanıcının simülatörde oluşturduğu varsayımsal senaryo taslakları tablosu.</summary>
    public const string TableSimulationDrafts = "simulation_drafts";

    /// <summary>Simülasyon taslağı içerisindeki tekil senaryo koşulları tablosu.</summary>
    public const string TableSimulationDraftConditions = "simulation_draft_conditions";

    /// <summary>Ödeme günü hatırlatıcı bildirimlerine verilen kullanıcı yanıtları defteri tablosu.</summary>
    public const string TablePaymentReminderResponses = "payment_reminder_responses";

    /// <summary>Uygulama SQLite veritabanı dosya adı.</summary>
    public const string DatabaseFileName = "mizan.db3";

    /// <summary>Profillerin disk üzerinde toplandığı klasör adı.</summary>
    public const string ProfilesDirectoryName = "profiles";

    /// <summary>Profil meta bilgilerinin saklandığı dosya adı.</summary>
    public const string MetadataFileName = "profile.json";
}

