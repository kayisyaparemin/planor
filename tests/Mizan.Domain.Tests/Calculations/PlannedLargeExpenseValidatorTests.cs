using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class PlannedLargeExpenseValidatorTests
{
    [Fact]
    public void Validate_GecerliHarcama_HataFirlatmaz()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Yaz Tatili",
            Amount = 50_000m,
            ExactDate = new DateOnly(2026, 7, 20),
            Note = "Otel ve yol",
            Status = PlannedExpenseStatus.Planned
        };

        var exception = Record.Exception(() => PlannedLargeExpenseValidator.Validate(expense));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_HarcamaNullIse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => PlannedLargeExpenseValidator.Validate(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_HarcamaAdiBosVeyaWhitespaceIse_InvalidOperationExceptionFirlatir(string? invalidName)
    {
        var expense = new PlannedLargeExpense
        {
            Name = invalidName!,
            Amount = 10_000m,
            ExactDate = new DateOnly(2026, 8, 15)
        };

        var exception = Assert.Throws<InvalidOperationException>(() => PlannedLargeExpenseValidator.Validate(expense));
        Assert.Equal("Planlanan harcama adı boş olamaz.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5_000)]
    public void Validate_HarcamaTutariSifirVeyaNegatifIse_InvalidOperationExceptionFirlatir(decimal invalidAmount)
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Buzdolabı",
            Amount = invalidAmount,
            ExactDate = new DateOnly(2026, 8, 15)
        };

        var exception = Assert.Throws<InvalidOperationException>(() => PlannedLargeExpenseValidator.Validate(expense));
        Assert.Equal("Planlanan harcama tutarı sıfırdan büyük olmalıdır.", exception.Message);
    }

    [Fact]
    public void Validate_HarcamaTarihiGecersizIse_InvalidOperationExceptionFirlatir()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Buzdolabı",
            Amount = 25_000m,
            ExactDate = default
        };

        var exception = Assert.Throws<InvalidOperationException>(() => PlannedLargeExpenseValidator.Validate(expense));
        Assert.Equal("Harcama tarihi geçersiz.", exception.Message);
    }

    [Fact]
    public void Validate_HarcamaDurumuTanimsizIse_InvalidOperationExceptionFirlatir()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Buzdolabı",
            Amount = 25_000m,
            ExactDate = new DateOnly(2026, 8, 15),
            Status = (PlannedExpenseStatus)999
        };

        var exception = Assert.Throws<InvalidOperationException>(() => PlannedLargeExpenseValidator.Validate(expense));
        Assert.Equal("Harcama durumu geçersiz.", exception.Message);
    }
}
