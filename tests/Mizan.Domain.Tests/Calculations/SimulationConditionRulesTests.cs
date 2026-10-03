using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Simülatör denemesinin tarihi ne zaman geçmiş sayılır (S76-3, 5): bugün geçerli, dün geçmiş.
/// </summary>
public sealed class SimulationConditionRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    [Fact]
    public void IsDatePassed_TarihBugunse_GecmemisSayilir()
    {
        var request = Request(Today);

        var passed = SimulationConditionRules.IsDatePassed(request, Today);

        Assert.False(passed);
    }

    [Fact]
    public void IsDatePassed_TarihDunse_GecmisSayilir()
    {
        var request = Request(Today.AddDays(-1));

        var passed = SimulationConditionRules.IsDatePassed(request, Today);

        Assert.True(passed);
    }

    [Fact]
    public void IsDatePassed_TarihYarinsa_GecmemisSayilir()
    {
        var request = Request(Today.AddDays(1));

        var passed = SimulationConditionRules.IsDatePassed(request, Today);

        Assert.False(passed);
    }

    private static SimulationRequest Request(DateOnly date) =>
        new(SimulationScenarioType.CashPurchase, "Telefon", 30_000m, date);
}
