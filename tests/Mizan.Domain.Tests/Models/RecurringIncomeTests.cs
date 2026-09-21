using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class RecurringIncomeTests
{
    [Fact]
    public void Constructor_VarsayilanDegerleriDogruBaslatir()
    {
        var income = new RecurringIncome
        {
            Name = "Maaş Geliri",
            PaymentDay = 15
        };

        Assert.NotEqual(Guid.Empty, income.Id);
        Assert.Equal("Maaş Geliri", income.Name);
        Assert.Equal(15, income.PaymentDay);
        Assert.True(income.IsActive);
    }
}
