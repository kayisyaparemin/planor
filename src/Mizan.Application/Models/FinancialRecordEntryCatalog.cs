namespace Mizan.Application.Models;

/// <summary>
/// Finansal Yapı ekranında yeni finansal kayıt girişi için sunulan seçenekler kataloğu.
/// Seçenekleri grup çiplerine göre filtreler ve simülatör ile ortak form seçeneklerini birleştirir.
/// </summary>
public static class FinancialRecordEntryCatalog
{
    /// <summary>Bir grup altında sunulabilecek azami seçenek sayısı.</summary>
    public const int MaxOptionsPerGroup = 3;

    /// <summary>Kayıt giriş gruplarının etiketli listesi.</summary>
    public static IReadOnlyList<(RecordEntryGroup Group, string Label)> Groups { get; } =
    [
        (RecordEntryGroup.Spending, "Harcama"),
        (RecordEntryGroup.Debt, "Borç / Kredi"),
        (RecordEntryGroup.Income, "Gelir"),
        (RecordEntryGroup.Account, "Hesap")
    ];

    /// <summary>Düzenli gelir tanımı veya tutar değişikliği seçeneği.</summary>
    public static RecordEntryOption Income { get; } = new()
    {
        Key = "income-change",
        Group = RecordEntryGroup.Income,
        Title = "Gelir değişikliği",
        Summary = "Düzenli gelirin ya da yeni tutarı",
        Description = "Düzenli gelirini ya da bir tarihten itibaren değişen tutarını ekle.",
        Form = RecordEntryForm.Income
    };

    /// <summary>Kredi kartı hesabı ekleme seçeneği.</summary>
    public static RecordEntryOption CreditCard { get; } = new()
    {
        Key = "credit-card",
        Group = RecordEntryGroup.Account,
        Title = "Kredi kartı",
        Summary = "Limit, borç ve ekstre günleri",
        Description = "Kartın limiti, borcu ve ekstre günleri. Ödeme kararlarını kaydettikten sonra kart ekranından verirsin.",
        Form = RecordEntryForm.CreditCard
    };

    /// <summary>Banka kredisi hesabı ekleme seçeneği.</summary>
    public static RecordEntryOption Loan { get; } = new()
    {
        Key = "bank-loan",
        Group = RecordEntryGroup.Account,
        Title = "Bankadaki kredi",
        Summary = "Devam eden kredinin kalan taksitleri",
        Description = "Bankada zaten devam eden bir kredinin taksitleri. Yeni kredi çekmeyi denemek için Borç / Kredi grubunu kullan.",
        Form = RecordEntryForm.Loan
    };

    /// <summary>Değişken ödeme planı hesabı ekleme seçeneği.</summary>
    public static RecordEntryOption PaymentPlan { get; } = new()
    {
        Key = "payment-plan",
        Group = RecordEntryGroup.Account,
        Title = "Değişken ödeme planı",
        Summary = "Tutarı ya da tarihi aydan aya değişen ödemeler",
        Description = "Tutarı ya da tarihi aydan aya değişen ödemeler; her ödemeyi tarihiyle ekle. Her ay aynı tutarsa Harcama → Düzenli ödeme daha kısa.",
        Form = RecordEntryForm.PaymentPlan
    };

    /// <summary>Tüm kayıt giriş seçeneklerinin listesi.</summary>
    public static IReadOnlyList<RecordEntryOption> Options { get; } = Build();

    /// <summary>Açılışta seçili gelen varsayılan seçenek (Nakit ödeme).</summary>
    public static RecordEntryOption Default => Options[0];

    /// <summary>Belirli bir gruba ait kayıt giriş seçeneklerini filtreler.</summary>
    public static IReadOnlyList<RecordEntryOption> OptionsIn(RecordEntryGroup group) =>
        Options.Where(x => x.Group == group).ToArray();

    /// <summary>Anahtarına göre kayıt giriş seçeneğini döner.</summary>
    public static RecordEntryOption For(string key) =>
        Options.Single(x => x.Key == key);

    private static RecordEntryOption[] Build()
    {
        var shared = SimulationScenarioCatalog.Options
            .Where(x => x.EntryHome == ScenarioEntryHome.SharedForm)
            .Select(option => new RecordEntryOption
            {
                Key = option.Key,
                Group = option.Group switch
                {
                    ScenarioGroup.Spending => RecordEntryGroup.Spending,
                    ScenarioGroup.Debt => RecordEntryGroup.Debt,
                    ScenarioGroup.Income => RecordEntryGroup.Income,
                    _ => throw new InvalidOperationException($"{option.Title} ortak formdan girilemez.")
                },
                Title = option.Title,
                Summary = option.Summary,
                Description = option.Description,
                Form = RecordEntryForm.SharedForm,
                Scenario = option
            });

        return shared
            .Append(Income)
            .Append(CreditCard)
            .Append(Loan)
            .Append(PaymentPlan)
            .OrderBy(x => x.Group)
            .ToArray();
    }
}
