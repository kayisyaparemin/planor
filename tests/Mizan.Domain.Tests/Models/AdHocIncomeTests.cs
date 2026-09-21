using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class AdHocIncomeTests
{
    [Fact]
    public void Constructor_OzellikleriDogruAtar()
    {
        var id = Guid.NewGuid();
        var date = new DateOnly(2026, 9, 25);
        var income = new AdHocIncome
        {
            Id = id,
            Amount = 25_000m,
            ExactDate = date,
            Description = "Yıl Sonu Primi"
        };

        Assert.Equal(id, income.Id);
        Assert.Equal(25_000m, income.Amount);
        Assert.Equal(date, income.ExactDate);
        Assert.Equal("Yıl Sonu Primi", income.Description);
    }
}
