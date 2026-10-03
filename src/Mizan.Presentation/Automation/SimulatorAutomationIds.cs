namespace Mizan.Presentation.Automation;

/// <summary>
/// Simülatör ve deneme formunun (V10) otomasyon kimlikleri. <see cref="AutomationIds"/> dosya sınırına dayandığı
/// için aynı ad alanında ayrı durur; kebab-case ve tekillik kuralları hepsine birlikte uygulanır.
/// </summary>
public static class SimulatorAutomationIds
{
    /// <summary>Simülatör sayfası kimliği.</summary>
    public const string PageSimulator = "page-simulator";

    /// <summary>Başlıktaki "Ekle": deneme türünü sorar.</summary>
    public const string BtnSimulatorAdd = "btn-simulator-add";

    /// <summary>Denemeler listesi.</summary>
    public const string ListSimulatorConditions = "list-simulator-conditions";

    /// <summary>Hata hâli: çalışma listesi okunamadı.</summary>
    public const string StateSimulatorError = "state-simulator-error";

    /// <summary>Sonuç kartı: en düşük dönem sonu, fark ve iki çizgili grafik.</summary>
    public const string CardSimulatorResult = "card-simulator-result";

    /// <summary>Hero rakam: denemeyle 12 dönemin en düşük dönem sonu.</summary>
    public const string LblSimulatorLowest = "lbl-simulator-lowest";

    /// <summary>Boş hâl: açık dönem ya da kurulabilir plan yok.</summary>
    public const string StateSimulatorResultEmpty = "state-simulator-result-empty";

    /// <summary>Hata hâli: sonuç hesaplanamadı.</summary>
    public const string StateSimulatorResultError = "state-simulator-result-error";

    /// <summary>12 dönem sonra karşılaştırma şeridi.</summary>
    public const string StripSimulatorTwelvePeriods = "strip-simulator-twelve-periods";

    /// <summary>12 dönem sonları karo ızgarası.</summary>
    public const string GridSimulatorPeriods = "grid-simulator-periods";

    /// <summary>Deneme formu sayfası kimliği.</summary>
    public const string PageSimulationCondition = "page-simulation-condition";

    /// <summary>Denemenin adı.</summary>
    public const string InputConditionName = "input-condition-name";

    /// <summary>Denemenin tutarı.</summary>
    public const string InputConditionAmount = "input-condition-amount";

    /// <summary>Denemenin tarihi.</summary>
    public const string PickerConditionDate = "picker-condition-date";

    /// <summary>Denemeyi çalışma listesine yazar.</summary>
    public const string BtnSaveCondition = "btn-save-condition";

    /// <summary>Formdan çıkar; değişiklik varsa onay sorar.</summary>
    public const string BtnCancelCondition = "btn-cancel-condition";
}
