namespace Mizan.Presentation.Navigation;

/// <summary>
/// Uygulama içi Shell rotalarını ve sayfa gezinme yollarını tek kaynakta toplayan sabitler.
/// </summary>
public static class Routes
{
    /// <summary>Ana sayfa kabuk rotası.</summary>
    public const string Dashboard = "//dashboard";

    /// <summary>12 Dönem projeksiyon sayfası rotası.</summary>
    public const string Projection = "//projection";

    /// <summary>What-If simülatörü rotası.</summary>
    public const string Simulation = "//simulation";

    /// <summary>Finansal yapı (yükümlülükler ve gelirler) sayfası rotası.</summary>
    public const string FinancialStructure = "//financial-structure";

    /// <summary>Geçmiş dönemler sayfası rotası.</summary>
    public const string History = "//history";

    /// <summary>Ayarlar sayfası rotası.</summary>
    public const string Settings = "//settings";

    /// <summary>Dönem ayrıntısı sayfası rotası; 12 Dönem karosundan <see cref="PeriodStartParameter"/> ile açılır (S75-1).</summary>
    public const string PeriodDetail = "period-detail";

    /// <summary>Dönem ayrıntısının hangi dönemi açacağını taşıyan sorgu parametresi: dönemin ilk günü (<c>DateOnly</c>).</summary>
    public const string PeriodStartParameter = "periodStart";

    /// <summary>Kredi kartı kontrol detay sayfası rotası.</summary>
    public const string CardControl = "card-control";

    /// <summary>Kart kontrol sayfasına hangi kartın açılacağını taşıyan sorgu parametresi.</summary>
    public const string CardIdParameter = "cardId";

    /// <summary>Kart ekleme / düzenleme formu rotası; <see cref="CardIdParameter"/> verilirse düzenleme.</summary>
    public const string CardForm = "card-form";

    /// <summary>Kredi ekleme / düzenleme formu rotası; <see cref="LoanIdParameter"/> verilirse düzenleme.</summary>
    public const string LoanForm = "loan-form";

    /// <summary>Kredi formunun hangi krediyi düzenleyeceğini taşıyan sorgu parametresi.</summary>
    public const string LoanIdParameter = "loanId";

    /// <summary>Düzenli gelir ekleme / düzenleme formu rotası; <see cref="IncomeIdParameter"/> verilirse düzenleme.</summary>
    public const string IncomeForm = "income-form";

    /// <summary>Gelir formunun hangi düzenli geliri düzenleyeceğini taşıyan sorgu parametresi.</summary>
    public const string IncomeIdParameter = "incomeId";

    /// <summary>Tek seferlik gelir ekleme / düzenleme formu rotası; <see cref="AdHocIncomeIdParameter"/> verilirse düzenleme.</summary>
    public const string AdHocIncomeForm = "ad-hoc-income-form";

    /// <summary>Tek seferlik gelir formunun hangi geliri düzenleyeceğini taşıyan sorgu parametresi.</summary>
    public const string AdHocIncomeIdParameter = "adHocIncomeId";

    /// <summary>Taksitli ödeme planı ekleme / düzenleme formu rotası; <see cref="PlanIdParameter"/> verilirse düzenleme.</summary>
    public const string PaymentPlanForm = "payment-plan-form";

    /// <summary>Ödeme planı formunun hangi planı düzenleyeceğini taşıyan sorgu parametresi.</summary>
    public const string PlanIdParameter = "planId";

    /// <summary>Planlanan büyük harcama ekleme / düzenleme formu rotası; <see cref="ExpenseIdParameter"/> verilirse düzenleme.</summary>
    public const string PlannedExpenseForm = "planned-expense-form";

    /// <summary>Planlanan harcama formunun hangi harcamayı düzenleyeceğini taşıyan sorgu parametresi.</summary>
    public const string ExpenseIdParameter = "expenseId";

    /// <summary>Finansal Yapı "Ekle"sinin açtığı kayıt türü seçicisi rotası (EK-V6f).</summary>
    public const string RecordEntryPicker = "record-entry-picker";

    /// <summary>Simülatör "Ekle"sinin açtığı deneme türü seçicisi rotası (EK-V10, S77 V10 notları i).</summary>
    public const string SimulationConditionPicker = "simulation-condition-picker";

    /// <summary>
    /// Rotayı açık sayfanın yerine açar: Shell'in "../" öneki önce açık sayfayı yığından düşürür. Seçici
    /// formla yer değiştirir ki formdan dönüş seçiciye değil listeye insin (S77-2).
    /// </summary>
    public static string ReplacingCurrent(string route) => "../" + route;

    /// <summary>
    /// Simülatörün deneme formu rotası: yeni deneme <see cref="ScenarioOptionParameter"/>, düzenleme
    /// <see cref="ConditionIdParameter"/> ile açılır (EK-V10).
    /// </summary>
    public const string SimulationCondition = "simulation-condition";

    /// <summary>Yeni denemenin türünü taşıyan sorgu parametresi: senaryo kataloğundaki seçeneğin anahtarı.</summary>
    public const string ScenarioOptionParameter = "scenarioOption";

    /// <summary>Düzenlenecek denemenin kimliğini taşıyan sorgu parametresi.</summary>
    public const string ConditionIdParameter = "conditionId";

    /// <summary>Kurulum sihirbazı rotası.</summary>
    public const string Onboarding = "onboarding";

    /// <summary>Profil seçimi rotası.</summary>
    public const string ProfileSelection = "profile-selection";

    /// <summary>Dönem mutabakatı ve kapanışı sihirbazı rotası.</summary>
    public const string PeriodSettlement = "period-settlement";

    /// <summary>"Bakiye gir" sayfası rotası: tutar, gözlem günü ve kaydetmeden önce önizleme (S72-8, sayfası V3b).</summary>
    public const string BalanceEntry = "balance-entry";
}
