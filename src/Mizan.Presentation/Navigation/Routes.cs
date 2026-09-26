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
    public const string Commitments = "//commitments";

    /// <summary>Geçmiş dönemler sayfası rotası.</summary>
    public const string History = "//history";

    /// <summary>Ayarlar sayfası rotası.</summary>
    public const string Settings = "//settings";

    /// <summary>Dönem ayrıntısı detay sayfası rotası.</summary>
    public const string PeriodDetail = "period-detail";

    /// <summary>Kredi kartı kontrol detay sayfası rotası.</summary>
    public const string CardControl = "card-control";

    /// <summary>Kurulum sihirbazı rotası.</summary>
    public const string Onboarding = "onboarding";

    /// <summary>Profil seçimi rotası.</summary>
    public const string ProfileSelection = "profile-selection";

    /// <summary>Dönem mutabakatı ve kapanışı sihirbazı rotası.</summary>
    public const string PeriodSettlement = "period-settlement";
}
