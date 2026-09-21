using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class UserSettingsTests
{
    [Fact]
    public void VarsayilanDegerler_BeklendigiGibiBaslatilir()
    {
        var settings = new UserSettings();

        Assert.Equal(10, settings.PeriodAnchor.DayOfMonth);
        Assert.Equal(0m, settings.PeriodVariableExpenseAllowance);
        Assert.Equal(0m, settings.ProjectionOpeningBalance);
        Assert.Equal(default, settings.ProjectionAnchorDate);
        Assert.Equal(0.05m, settings.CreditCardCarryInterestRate);
        Assert.Equal(0.05m, settings.DeficitFinancingInterestRate);
    }

    [Fact]
    public void WithKopyalama_OzelDegerlerle_DogruAtanir()
    {
        var anchor = new PeriodAnchor(15);
        var anchorDate = new DateOnly(2026, 9, 15);
        var settings = new UserSettings
        {
            PeriodAnchor = anchor,
            PeriodVariableExpenseAllowance = 25_000m,
            ProjectionOpeningBalance = 75_000m,
            ProjectionAnchorDate = anchorDate,
            CreditCardCarryInterestRate = 0.0425m,
            DeficitFinancingInterestRate = 0.0475m
        };

        Assert.Equal(15, settings.PeriodAnchor.DayOfMonth);
        Assert.Equal(25_000m, settings.PeriodVariableExpenseAllowance);
        Assert.Equal(75_000m, settings.ProjectionOpeningBalance);
        Assert.Equal(anchorDate, settings.ProjectionAnchorDate);
        Assert.Equal(0.0425m, settings.CreditCardCarryInterestRate);
        Assert.Equal(0.0475m, settings.DeficitFinancingInterestRate);
    }
}
