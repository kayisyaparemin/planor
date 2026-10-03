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

    /// <summary>Deneme türü seçici sayfası kimliği.</summary>
    public const string PageSimulationConditionPicker = "page-simulation-condition-picker";

    /// <summary>Deneme türü gruplarının karolarını taşıyan alanın kimliği.</summary>
    public const string PickerSimulationConditionType = "picker-simulation-condition-type";

    /// <summary>"Hangi kart?" ikinci seviyesinde adayları taşıyan listenin kimliği.</summary>
    public const string ListSimulationConditionChoices = "list-simulation-condition-choices";

    /// <summary>Deneme formu sayfası kimliği.</summary>
    public const string PageSimulationCondition = "page-simulation-condition";

    /// <summary>Denemenin adı.</summary>
    public const string InputConditionName = "input-condition-name";

    /// <summary>Denemenin tutarı.</summary>
    public const string InputConditionAmount = "input-condition-amount";

    /// <summary>Denemenin tarihi.</summary>
    public const string PickerConditionDate = "picker-condition-date";

    /// <summary>Hedef kart etiketi / salt okunur adı.</summary>
    public const string LblConditionCard = "lbl-condition-card";

    /// <summary>Hedef kredi etiketi / salt okunur adı.</summary>
    public const string LblConditionLoan = "lbl-condition-loan";

    /// <summary>Hedef düzenli gelir etiketi / salt okunur adı.</summary>
    public const string LblConditionIncome = "lbl-condition-income";

    /// <summary>Kart ödeme şekli seçicisi (Tamamı / Asgari).</summary>
    public const string PickerConditionPaymentMode = "picker-condition-payment-mode";

    /// <summary>Kart ödeme kapsamı seçicisi (Tek ekstre / Tüm ekstreler).</summary>
    public const string PickerConditionPaymentScope = "picker-condition-payment-scope";

    /// <summary>Krediye erken ödeme şekli seçicisi (Tamamen kapat / Vadeyi kısalt / Taksiti azalt).</summary>
    public const string PickerConditionPrepaymentMode = "picker-condition-prepayment-mode";

    /// <summary>Toplam geri ödeme tutarı girişi.</summary>
    public const string InputConditionTotalRepayment = "input-condition-total-repayment";

    /// <summary>İlk ödeme tarihi seçicisi.</summary>
    public const string PickerConditionFirstPaymentDate = "picker-condition-first-payment-date";

    /// <summary>Taksit veya ödeme sayısı girişi.</summary>
    public const string InputConditionPaymentCount = "input-condition-payment-count";

    /// <summary>Denemeyi çalışma listesine yazar.</summary>
    public const string BtnSaveCondition = "btn-save-condition";

    /// <summary>Formdan çıkar; değişiklik varsa onay sorar.</summary>
    public const string BtnCancelCondition = "btn-cancel-condition";
}
