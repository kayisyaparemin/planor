using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Models;

/// <summary>
/// Simülasyon senaryo kataloğu davranış testleri.
/// Her senaryo motor türünün katalogda tek bir kullanıcı kartına karşılık geldiğini,
/// grup sınırlarını, doğrudan giriş ayrımını ve motor türü çözümlemesini doğrular.
/// </summary>
public sealed class SimulationScenarioCatalogTests
{
    [Fact]
    public void TumSenaryoTurleri_KatalogdaTamBirSecenegeAittir()
    {
        foreach (var type in Enum.GetValues<SimulationScenarioType>())
        {
            var matching = SimulationScenarioCatalog.Options
                .Where(o => o.Types.Contains(type))
                .ToArray();

            Assert.Single(matching);
            Assert.Same(matching[0], SimulationScenarioCatalog.For(type));
        }
    }

    [Fact]
    public void HicbirGrup_UctenFazlaSecenekIceremez_VeToplamlarEslesir()
    {
        foreach (var (group, label) in SimulationScenarioCatalog.Groups)
        {
            Assert.False(string.IsNullOrWhiteSpace(label));
            var count = SimulationScenarioCatalog.OptionsIn(group).Count;
            Assert.InRange(count, 1, SimulationScenarioCatalog.MaxOptionsPerGroup);
        }

        var totalByGroups = SimulationScenarioCatalog.Groups
            .Sum(g => SimulationScenarioCatalog.OptionsIn(g.Group).Count);

        Assert.Equal(SimulationScenarioCatalog.Options.Count, totalByGroups);
    }

    [Fact]
    public void DogrudanGirisFiltresi_YalnizcaOrtakFormEvineSahipTurleriVerir()
    {
        var directEntries = SimulationScenarioCatalog.Options
            .Where(x => x.EntryHome == ScenarioEntryHome.SharedForm)
            .SelectMany(x => x.Types)
            .ToHashSet();

        foreach (var type in Enum.GetValues<SimulationScenarioType>())
        {
            var isDirect = SimulationScenarioCatalog.IsDirectEntry(type);
            Assert.Equal(directEntries.Contains(type), isDirect);
        }

        Assert.False(SimulationScenarioCatalog.IsDirectEntry(SimulationScenarioType.IncomeChange));
        Assert.False(SimulationScenarioCatalog.IsDirectEntry(SimulationScenarioType.CreditCardPaymentMode));
        Assert.True(SimulationScenarioCatalog.IsDirectEntry(SimulationScenarioType.CashPurchase));
        Assert.True(SimulationScenarioCatalog.IsDirectEntry(SimulationScenarioType.FinancingLoan));
    }

    [Theory]
    [InlineData(1, SimulationScenarioType.CreditCardSinglePayment)]
    [InlineData(0, SimulationScenarioType.CreditCardSinglePayment)]
    [InlineData(2, SimulationScenarioType.CreditCardInstallmentPurchase)]
    [InlineData(12, SimulationScenarioType.CreditCardInstallmentPurchase)]
    public void KartHarcamasi_TaksitSayisinaGoreCozumlenir(
        int paymentCount,
        SimulationScenarioType expectedType)
    {
        var resolved = SimulationScenarioCatalog.Resolve(
            SimulationScenarioCatalog.CardSpending,
            paymentCount,
            prepaymentMode: null);

        Assert.Equal(expectedType, resolved);
    }

    [Theory]
    [InlineData(LoanPrepaymentMode.FullClosure, SimulationScenarioType.LoanEarlyClosure)]
    [InlineData(LoanPrepaymentMode.ReduceTerm, SimulationScenarioType.LoanPartialPrepayment)]
    [InlineData(LoanPrepaymentMode.ReduceInstallment, SimulationScenarioType.LoanPartialPrepayment)]
    [InlineData(null, SimulationScenarioType.LoanEarlyClosure)]
    public void KrediErkenOdeme_ModaGoreCozumlenir(
        LoanPrepaymentMode? mode,
        SimulationScenarioType expectedType)
    {
        var resolved = SimulationScenarioCatalog.Resolve(
            SimulationScenarioCatalog.LoanPrepayment,
            paymentCount: 1,
            prepaymentMode: mode);

        Assert.Equal(expectedType, resolved);
    }

    [Fact]
    public void NakitOdeme_EskiGelecekOdemeTuruyleDuzenleniyorsa_TurunuKorur()
    {
        var normal = SimulationScenarioCatalog.Resolve(
            SimulationScenarioCatalog.CashPayment,
            paymentCount: 1,
            prepaymentMode: null);

        var preserving = SimulationScenarioCatalog.Resolve(
            SimulationScenarioCatalog.CashPayment,
            paymentCount: 1,
            prepaymentMode: null,
            editingType: SimulationScenarioType.FutureOneTimePayment);

        Assert.Equal(SimulationScenarioType.CashPurchase, normal);
        Assert.Equal(SimulationScenarioType.FutureOneTimePayment, preserving);
    }

    [Fact]
    public void TumTurlerin_KendineOzguTurkceEtiketiVardir()
    {
        var types = Enum.GetValues<SimulationScenarioType>();
        var labels = types.Select(SimulationScenarioCatalog.TypeText).ToArray();

        Assert.All(labels, label =>
        {
            Assert.False(string.IsNullOrWhiteSpace(label));
            Assert.NotEqual("Koşul", label);
        });

        Assert.Equal(labels.Length, labels.Distinct().Count());
        Assert.Equal("Gelir değişikliği", SimulationScenarioCatalog.TypeText(SimulationScenarioType.IncomeChange));
    }
}
