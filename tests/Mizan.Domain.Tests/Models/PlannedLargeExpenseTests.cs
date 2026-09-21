using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PlannedLargeExpenseTests
{
    [Fact]
    public void VarsayilanDegerler_BeklendigiGibiBaslatilir()
    {
        var expense = new PlannedLargeExpense();

        Assert.NotEqual(Guid.Empty, expense.Id);
        Assert.Equal(string.Empty, expense.Name);
        Assert.Equal(0m, expense.Amount);
        Assert.Equal(default, expense.ExactDate);
        Assert.Equal(string.Empty, expense.Note);
        Assert.Equal(PlannedExpenseStatus.Planned, expense.Status);
        Assert.True(expense.IsActive);
    }

    [Theory]
    [InlineData(PlannedExpenseStatus.Planned, true)]
    [InlineData(PlannedExpenseStatus.Completed, false)]
    [InlineData(PlannedExpenseStatus.Cancelled, false)]
    public void IsActive_YalnizcaPlannedDurumundaTrue_DigerlerindeFalse(PlannedExpenseStatus status, bool expectedIsActive)
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Yaz Tatili",
            Amount = 45_000m,
            ExactDate = new DateOnly(2026, 7, 15),
            Status = status
        };

        Assert.Equal(expectedIsActive, expense.IsActive);
    }

    [Fact]
    public void WithKopyalama_DurumDegistiginde_IsActiveGuncellenir()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Buzdolabı",
            Amount = 30_000m,
            ExactDate = new DateOnly(2026, 8, 1),
            Status = PlannedExpenseStatus.Planned
        };

        Assert.True(expense.IsActive);

        var completed = expense with { Status = PlannedExpenseStatus.Completed };

        Assert.False(completed.IsActive);
        Assert.Equal(expense.Id, completed.Id);
        Assert.Equal(expense.Name, completed.Name);
        Assert.Equal(expense.Amount, completed.Amount);
    }
}
