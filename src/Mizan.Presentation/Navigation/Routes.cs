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

    /// <summary>Dönem ayrıntısı detay sayfası rotası.</summary>
    public const string PeriodDetail = "period-detail";

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

    /// <summary>Kurulum sihirbazı rotası.</summary>
    public const string Onboarding = "onboarding";

    /// <summary>Profil seçimi rotası.</summary>
    public const string ProfileSelection = "profile-selection";

    /// <summary>Dönem mutabakatı ve kapanışı sihirbazı rotası.</summary>
    public const string PeriodSettlement = "period-settlement";
}
