using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class UserSettingsValidatorTests
{
    [Fact]
    public void Validate_GecerliAyarlar_HataFirlatmaz()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(10),
            PeriodVariableExpenseAllowance = 20_000m,
            ProjectionOpeningBalance = 50_000m,
            ProjectionAnchorDate = new DateOnly(2026, 9, 10),
            CreditCardCarryInterestRate = 0.05m,
            DeficitFinancingInterestRate = 0.05m
        };

        var exception = Record.Exception(() => UserSettingsValidator.Validate(settings));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_AyarlarNullIse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => UserSettingsValidator.Validate(null!));
    }

    [Fact]
    public void Validate_DonemCapasiNullIse_ArgumentNullExceptionFirlatir()
    {
        var settings = new UserSettings
        {
            PeriodAnchor = null!
        };

        Assert.Throws<ArgumentNullException>(() => UserSettingsValidator.Validate(settings));
    }

    [Fact]
    public void Validate_YasamGideriNegatifIse_InvalidOperationExceptionFirlatir()
    {
        var settings = new UserSettings
        {
            PeriodVariableExpenseAllowance = -0.01m
        };

        var exception = Assert.Throws<InvalidOperationException>(() => UserSettingsValidator.Validate(settings));
        Assert.Equal("Dönem yaşam gideri negatif olamaz.", exception.Message);
    }

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    [InlineData(2.5)]
    public void Validate_KartAkdiFaizOraniGecersizIse_ArgumentOutOfRangeExceptionFirlatir(decimal invalidRate)
    {
        var settings = new UserSettings
        {
            CreditCardCarryInterestRate = invalidRate
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UserSettingsValidator.Validate(settings));
        Assert.StartsWith("Kredi kartı akdi faiz oranı %0 ile %100 arasında olmalıdır.", exception.Message);
    }

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(1.0001)]
    [InlineData(5.0)]
    public void Validate_FinansmanAcigiFaizOraniGecersizIse_ArgumentOutOfRangeExceptionFirlatir(decimal invalidRate)
    {
        var settings = new UserSettings
        {
            DeficitFinancingInterestRate = invalidRate
        };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UserSettingsValidator.Validate(settings));
        Assert.StartsWith("Finansman açığı faiz oranı %0 ile %100 arasında olmalıdır.", exception.Message);
    }

    [Theory]
    [InlineData(-100_000)]
    [InlineData(0)]
    [InlineData(250_000)]
    public void Validate_AcilisBakiyesiNegatifVeyaPozitifOlabilir_HataFirlatmaz(decimal balance)
    {
        var settings = new UserSettings
        {
            ProjectionOpeningBalance = balance
        };

        var exception = Record.Exception(() => UserSettingsValidator.Validate(settings));

        Assert.Null(exception);
    }
}
