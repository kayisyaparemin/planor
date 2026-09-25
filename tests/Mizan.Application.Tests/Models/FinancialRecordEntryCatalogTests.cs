using Mizan.Application.Models;

namespace Mizan.Application.Tests.Models;

/// <summary>
/// Finansal Yapı kayıt giriş kataloğu davranış testleri.
/// Grupların seçenek sayılarını, doğrudan giriş senaryolarının kataloğa entegrasyonunu,
/// tüm formların erişilebilirliğini ve sıralama kurallarını doğrular.
/// </summary>
public sealed class FinancialRecordEntryCatalogTests
{
    [Fact]
    public void TumGruplar_BirIleUcArasiSecenekIcerir_VeHerSeceneginGrubuVardir()
    {
        foreach (var (group, label) in FinancialRecordEntryCatalog.Groups)
        {
            Assert.False(string.IsNullOrWhiteSpace(label));
            var inGroup = FinancialRecordEntryCatalog.OptionsIn(group);
            Assert.InRange(inGroup.Count, 1, FinancialRecordEntryCatalog.MaxOptionsPerGroup);
        }

        var totalCount = FinancialRecordEntryCatalog.Groups
            .Sum(g => FinancialRecordEntryCatalog.OptionsIn(g.Group).Count);

        Assert.Equal(FinancialRecordEntryCatalog.Options.Count, totalCount);

        var uniqueKeys = FinancialRecordEntryCatalog.Options.Select(x => x.Key).Distinct().Count();
        Assert.Equal(FinancialRecordEntryCatalog.Options.Count, uniqueKeys);
    }

    [Fact]
    public void TumDogrudanGirisSenaryolari_OrtakFormIle_TamBirKezSunulur()
    {
        var directScenarios = SimulationScenarioCatalog.Options
            .Where(x => x.EntryHome == ScenarioEntryHome.SharedForm)
            .ToArray();

        foreach (var scenario in directScenarios)
        {
            var option = Assert.Single(
                FinancialRecordEntryCatalog.Options,
                x => x.Scenario == scenario);

            Assert.Equal(RecordEntryForm.SharedForm, option.Form);
            Assert.Equal(scenario.Title, option.Title);
            Assert.Equal(scenario.Summary, option.Summary);
            Assert.Equal(scenario.Group.ToString(), option.Group.ToString());
        }

        Assert.All(
            FinancialRecordEntryCatalog.Options.Where(x => x.Form == RecordEntryForm.SharedForm),
            opt => Assert.Contains(opt.Scenario, directScenarios));
    }

    [Fact]
    public void TumKayitFormlari_Erisilebilirdir()
    {
        foreach (var form in Enum.GetValues<RecordEntryForm>())
        {
            Assert.Contains(FinancialRecordEntryCatalog.Options, x => x.Form == form);
        }

        Assert.All(
            FinancialRecordEntryCatalog.Options.Where(x => x.Form != RecordEntryForm.SharedForm),
            opt => Assert.Null(opt.Scenario));
    }

    [Fact]
    public void Gruplar_VeSecenekler_Siralidir_VeVarsayilanNakitOdemedir()
    {
        Assert.Equal(
            ["Harcama", "Borç / Kredi", "Gelir", "Hesap"],
            FinancialRecordEntryCatalog.Groups.Select(x => x.Label));

        Assert.Equal(
            ["Tek seferlik gelir", "Gelir değişikliği"],
            FinancialRecordEntryCatalog.OptionsIn(RecordEntryGroup.Income).Select(x => x.Title));

        Assert.Equal(
            [RecordEntryForm.CreditCard, RecordEntryForm.Loan, RecordEntryForm.PaymentPlan],
            FinancialRecordEntryCatalog.OptionsIn(RecordEntryGroup.Account).Select(x => x.Form));

        Assert.Same(
            SimulationScenarioCatalog.CashPayment,
            FinancialRecordEntryCatalog.Default.Scenario);

        Assert.Same(
            FinancialRecordEntryCatalog.CreditCard,
            FinancialRecordEntryCatalog.For("credit-card"));
    }
}
