using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Simülatör ve Finansal Yapı ekranlarının paylaştığı tekil senaryo seçenekleri kataloğu.
/// Kullanıcının What-If analizi için seçebileceği seçenekleri gruplar, açıklar ve motor türlerine çözümler.
/// </summary>
public static class SimulationScenarioCatalog
{
    /// <summary>Arayüzde bir grup altında gösterilebilecek azami kart sayısı.</summary>
    public const int MaxOptionsPerGroup = 3;

    /// <summary>Senaryo gruplarının arayüzdeki sıralı başlık listesi.</summary>
    public static IReadOnlyList<(ScenarioGroup Group, string Label)> Groups { get; } =
    [
        (ScenarioGroup.Spending, "Harcama"),
        (ScenarioGroup.Debt, "Borç / Kredi"),
        (ScenarioGroup.Income, "Gelir"),
        (ScenarioGroup.Setting, "Ayar")
    ];

    /// <summary>Nakit ödeme seçeneği.</summary>
    public static ScenarioOption CashPayment { get; } = new()
    {
        Key = "cash",
        Group = ScenarioGroup.Spending,
        Title = "Nakit ödeme",
        Summary = "Tutar seçtiğin gün hesabından düşer",
        Description = "Tutar, seçtiğin tarihte finansal durumundan düşer.",
        Types = [SimulationScenarioType.CashPurchase, SimulationScenarioType.FutureOneTimePayment],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Kartla harcama (tek çekim veya taksitli) seçeneği.</summary>
    public static ScenarioOption CardSpending { get; } = new()
    {
        Key = "card",
        Group = ScenarioGroup.Spending,
        Title = "Kartla harcama",
        Summary = "Taksit sayısı 1 ise tek çekim",
        Description = "Harcama, kartının ekstre kesim ve son ödeme tarihlerine göre hesaplanır; taksitler ilgili ekstrelere yansıtılır.",
        Types = [SimulationScenarioType.CreditCardSinglePayment, SimulationScenarioType.CreditCardInstallmentPurchase],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Düzenli tekrarlayan periyodik harcama seçeneği.</summary>
    public static ScenarioOption RecurringPayment { get; } = new()
    {
        Key = "recurring",
        Group = ScenarioGroup.Spending,
        Title = "Düzenli ödeme",
        Summary = "Her ay aynı tutar, belirlediğin ay sayısı kadar",
        Description = "Girilen tutar, belirtilen dönem sayısı boyunca aylık tekrarlanır.",
        Types = [SimulationScenarioType.RecurringPayment],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Finansman kredisi çekme seçeneği.</summary>
    public static ScenarioOption Financing { get; } = new()
    {
        Key = "financing",
        Group = ScenarioGroup.Debt,
        Title = "Kredi / finansman çek",
        Summary = "Para bugün gelir, geri ödeme taksitle çıkar",
        Description = "Kredi tutarı işlem tarihinde gelir olarak eklenir; toplam geri ödeme, ilk ödeme tarihinden başlayarak taksitlere bölünür.",
        Types = [SimulationScenarioType.FinancingLoan],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Taksitli elden/nakit borçlanma seçeneği.</summary>
    public static ScenarioOption CashDebt { get; } = new()
    {
        Key = "cash-debt",
        Group = ScenarioGroup.Debt,
        Title = "Taksitli nakit borç",
        Summary = "Borcu eşit ödemelere böl",
        Description = "Borç tutarı, seçtiğin ödeme sayısına kuruş farkı bırakmadan bölünür.",
        Types = [SimulationScenarioType.CashDebt],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Krediye erken kapama veya ara ödeme seçeneği.</summary>
    public static ScenarioOption LoanPrepayment { get; } = new()
    {
        Key = "loan-prepayment",
        Group = ScenarioGroup.Debt,
        Title = "Krediye erken ödeme",
        Summary = "Tamamen kapat ya da ara ödeme yap",
        Description = "Tamamen kapatırsan kalan anapara ve son taksitten bu yana işleyen faiz tek seferde ödenir; sonraki taksitler kalkar. Ara ödemede girdiğin tutar anaparadan düşer: vadeyi kısaltırsan taksit aynı kalır, taksiti azaltırsan kredi aynı tarihte biter. Taksit gününde ödersen işleyen faiz olmaz; tüketici kredisinde erken ödeme ücreti alınamaz.",
        Types = [SimulationScenarioType.LoanEarlyClosure, SimulationScenarioType.LoanPartialPrepayment],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Tek seferlik arızi gelir seçeneği.</summary>
    public static ScenarioOption OneTimeIncome { get; } = new()
    {
        Key = "income",
        Group = ScenarioGroup.Income,
        Title = "Tek seferlik gelir",
        Summary = "Prim, satış, iade gibi bir kerelik para",
        Description = "Gelir, seçtiğin tarihin dahil olduğu döneme eklenir.",
        Types = [SimulationScenarioType.FutureIncome],
        EntryHome = ScenarioEntryHome.SharedForm
    };

    /// <summary>Düzenli gelir değişikliği seçeneği.</summary>
    public static ScenarioOption IncomeChange { get; } = new()
    {
        Key = "income-change",
        Group = ScenarioGroup.Income,
        Title = "Gelir değişikliği",
        Summary = "Gelirin bir tarihten itibaren değişir",
        Description = "Yeni gelir, seçtiğin tarihten itibaren kullanılır.",
        Types = [SimulationScenarioType.IncomeChange],
        EntryHome = ScenarioEntryHome.IncomeForm
    };

    /// <summary>Kredi kartı ekstre ödeme şekli seçeneği.</summary>
    public static ScenarioOption CardPaymentMode { get; } = new()
    {
        Key = "card-payment-mode",
        Group = ScenarioGroup.Setting,
        Title = "Kart ödeme şekli",
        Summary = "Ekstreyi asgari ya da tamamen öde",
        Description = "Kartın ödeme şeklini değiştirir. Kart faizi ile finansman açığı faizi ters yönde hareket edebilir; Faiz Karşılaştırması ikisini ayrı gösterir.",
        Types = [SimulationScenarioType.CreditCardPaymentMode],
        EntryHome = ScenarioEntryHome.CardControl
    };

    /// <summary>Tüm senaryo seçeneklerinin sıralı listesi.</summary>
    public static IReadOnlyList<ScenarioOption> Options { get; } =
    [
        CashPayment, CardSpending, RecurringPayment, Financing, CashDebt,
        LoanPrepayment, OneTimeIncome, IncomeChange, CardPaymentMode
    ];

    /// <summary>Belirli bir gruba ait seçenekleri listeler.</summary>
    public static IReadOnlyList<ScenarioOption> OptionsIn(ScenarioGroup group, bool directEntryOnly = false) =>
        Options.Where(x => x.Group == group && (!directEntryOnly || x.EntryHome == ScenarioEntryHome.SharedForm)).ToArray();

    /// <summary>Belirtilen motor senaryo türüne karşılık gelen seçeneği döner.</summary>
    public static ScenarioOption For(SimulationScenarioType type) =>
        Options.Single(x => x.Types.Contains(type));

    /// <summary>Belirtilen motor senaryo türünün Finansal Yapı ortak formundan doğrudan girilebilir olup olmadığını döner.</summary>
    public static bool IsDirectEntry(SimulationScenarioType type) =>
        For(type).EntryHome == ScenarioEntryHome.SharedForm;

    /// <summary>Seçenek ve kullanıcı form girdilerinden kesin motor senaryo türünü çözümler.</summary>
    public static SimulationScenarioType Resolve(
        ScenarioOption option,
        int paymentCount,
        LoanPrepaymentMode? prepaymentMode,
        SimulationScenarioType? editingType = null)
    {
        if (option.Key == CardSpending.Key)
        {
            return paymentCount > 1
                ? SimulationScenarioType.CreditCardInstallmentPurchase
                : SimulationScenarioType.CreditCardSinglePayment;
        }

        if (option.Key == LoanPrepayment.Key)
        {
            return prepaymentMode is null or LoanPrepaymentMode.FullClosure
                ? SimulationScenarioType.LoanEarlyClosure
                : SimulationScenarioType.LoanPartialPrepayment;
        }

        if (option.Key == CashPayment.Key && editingType == SimulationScenarioType.FutureOneTimePayment)
        {
            return SimulationScenarioType.FutureOneTimePayment;
        }

        return option.DefaultType;
    }

    /// <summary>Koşul listesinde ve arayüzde senaryo türünü anlatan kısa Türkçe etiket.</summary>
    public static string TypeText(SimulationScenarioType type) =>
        type switch
        {
            SimulationScenarioType.CashPurchase => "Nakit ödeme",
            SimulationScenarioType.CreditCardSinglePayment => "Karttan tek çekim",
            SimulationScenarioType.CreditCardInstallmentPurchase => "Kart taksitli harcama",
            SimulationScenarioType.FinancingLoan => "Finansman / kredi",
            SimulationScenarioType.CashDebt => "Taksitli nakit borç",
            SimulationScenarioType.FutureOneTimePayment => "Tek seferlik ödeme",
            SimulationScenarioType.RecurringPayment => "Düzenli ödeme",
            SimulationScenarioType.FutureIncome => "Tek seferlik gelir",
            SimulationScenarioType.IncomeChange => "Gelir değişikliği",
            SimulationScenarioType.CreditCardPaymentMode => "Kart ödeme şekli",
            SimulationScenarioType.LoanEarlyClosure => "Kredi erken kapama",
            SimulationScenarioType.LoanPartialPrepayment => "Kredi ara ödeme",
            _ => "Koşul"
        };
}
