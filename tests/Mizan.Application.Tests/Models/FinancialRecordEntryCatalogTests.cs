using Mizan.Application.Models;

namespace Mizan.Application.Tests.Models;

/// <summary>
/// Kayıt türü seçicinin kataloğu: Finansal Yapı listesinin dört grubu sırasıyla, her grupta 1–4 seçenek,
/// her seçenek tek bir forma gider ve anahtarlar simülatörün ortak türleriyle aynıdır (S77-3, S77-6).
/// </summary>
public sealed class FinancialRecordEntryCatalogTests
{
    [Fact]
    public void Gruplar_FinansalYapiListesininSirasiylaDortGruptur()
    {
        var groups = FinancialRecordEntryCatalog.Groups;

        Assert.Equal([RecordEntryGroup.Income, RecordEntryGroup.Card, RecordEntryGroup.Loan, RecordEntryGroup.Payment], groups);
    }

    [Fact]
    public void HerGrup_BirIleDortArasiSecenekTasir_VeSeceneklerGrupSirasiyladir()
    {
        var counts = FinancialRecordEntryCatalog.Groups.Select(g => FinancialRecordEntryCatalog.OptionsIn(g).Count).ToArray();

        Assert.All(counts, count => Assert.InRange(count, 1, FinancialRecordEntryCatalog.MaxOptionsPerGroup));
        Assert.Equal(FinancialRecordEntryCatalog.Options.Count, counts.Sum());
        Assert.Equal(
            FinancialRecordEntryCatalog.Groups.SelectMany(FinancialRecordEntryCatalog.OptionsIn),
            FinancialRecordEntryCatalog.Options);
    }

    [Fact]
    public void Secenekler_PlanorFormlarinaBaglanir()
    {
        var forms = FinancialRecordEntryCatalog.Options.ToDictionary(x => x.Key, x => x.Form);

        Assert.Equal(new Dictionary<string, RecordEntryForm>
        {
            ["recurring-income"] = RecordEntryForm.RecurringIncome,
            ["income"] = RecordEntryForm.AdHocIncome,
            ["credit-card"] = RecordEntryForm.CreditCard,
            ["bank-loan"] = RecordEntryForm.Loan,
            ["cash"] = RecordEntryForm.PlannedExpense,
            ["recurring"] = RecordEntryForm.PaymentPlan,
            ["cash-debt"] = RecordEntryForm.PaymentPlan,
            ["payment-plan"] = RecordEntryForm.PaymentPlan
        }, forms);
    }

    [Fact]
    public void OrtakTurler_SimulatorunAnahtariniTasir()
    {
        var shared = new[] { "cash", "recurring", "cash-debt", "income" };

        var simulatorKeys = SimulationScenarioCatalog.Options.Select(x => x.Key);

        Assert.All(shared, key => Assert.Contains(key, simulatorKeys));
    }

    [Fact]
    public void For_AnahtarSecenegiBulur()
    {
        var payment = FinancialRecordEntryCatalog.OptionsIn(RecordEntryGroup.Payment)[0];

        var found = FinancialRecordEntryCatalog.For(payment.Key);

        Assert.Same(payment, found);
    }
}
