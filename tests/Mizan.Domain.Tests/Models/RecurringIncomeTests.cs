using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class RecurringIncomeTests
{
    [Fact]
    public void Constructor_VarsayilanDegerleriDogruBaslatir()
    {
        var income = new RecurringIncome
        {
            Name = "Aylık Gelir",
            PaymentDay = 15
        };

        Assert.NotEqual(Guid.Empty, income.Id);
        Assert.Equal("Aylık Gelir", income.Name);
        Assert.Equal(15, income.PaymentDay);
        Assert.True(income.IsActive);
    }
}
