using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class FinancialPlanTests
{
    [Fact]
    public void VarsayilanDegerler_BeklendigiGibiBaslatilir()
    {
        var plan = new FinancialPlan();

        Assert.NotNull(plan.Settings);
        Assert.Empty(plan.RecurringIncomes);
        Assert.Empty(plan.IncomeHistories);
        Assert.Empty(plan.AdHocIncomes);
        Assert.Empty(plan.Loans);
        Assert.Empty(plan.LoanPrepayments);
        Assert.Empty(plan.PaymentPlans);
        Assert.Empty(plan.CreditCards);
        Assert.Empty(plan.PlannedLargeExpenses);
        Assert.False(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_CapaTarihiDefaultIse_FalseDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { ProjectionAnchorDate = default },
            RecurringIncomes =
            [
                new RecurringIncome { Name = "Maaş", PaymentDay = 15, IsActive = true }
            ]
        };

        Assert.False(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_GelirYokIse_FalseDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { ProjectionAnchorDate = new DateOnly(2026, 10, 1) },
            RecurringIncomes = []
        };

        Assert.False(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_TumGelirlerPasifIse_FalseDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { ProjectionAnchorDate = new DateOnly(2026, 10, 1) },
            RecurringIncomes =
            [
                new RecurringIncome { Name = "Eski İş Geliri", PaymentDay = 1, IsActive = false }
            ]
        };

        Assert.False(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_CapaTarihiVeAktifGelirVarsa_TrueDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { ProjectionAnchorDate = new DateOnly(2026, 10, 1) },
            RecurringIncomes =
            [
                new RecurringIncome { Name = "Emekli Aylığı", PaymentDay = 1, IsActive = false },
                new RecurringIncome { Name = "Maaş", PaymentDay = 15, IsActive = true }
            ]
        };

        Assert.True(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_DuzenliGelirYokAmaTekSeferlikGelirVarsa_TrueDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings { ProjectionAnchorDate = new DateOnly(2026, 10, 1) },
            AdHocIncomes =
            [
                new AdHocIncome { ExactDate = new DateOnly(2026, 10, 15), Amount = 500_000m, Description = "Telif" }
            ]
        };

        Assert.True(plan.CanBuildProjection);
    }

    [Fact]
    public void CanBuildProjection_GelirYokAmaAcilisBakiyesiVarsa_TrueDondurur()
    {
        var plan = new FinancialPlan
        {
            Settings = new UserSettings
            {
                ProjectionAnchorDate = new DateOnly(2026, 10, 1),
                ProjectionOpeningBalance = 150_000m
            }
        };

        Assert.True(plan.CanBuildProjection);
    }

    [Fact]
    public void LoanPrepayment_ModelBaslatma_AlanlariDogruTutar()
    {
        var prepaymentId = Guid.NewGuid();
        var loanId = Guid.NewGuid();
        var date = new DateOnly(2026, 11, 20);

        var prepayment = new LoanPrepayment
        {
            Id = prepaymentId,
            LoanId = loanId,
            Date = date,
            Mode = LoanPrepaymentMode.ReduceTerm,
            PrincipalAmount = 50_000m
        };

        Assert.Equal(prepaymentId, prepayment.Id);
        Assert.Equal(loanId, prepayment.LoanId);
        Assert.Equal(date, prepayment.Date);
        Assert.Equal(LoanPrepaymentMode.ReduceTerm, prepayment.Mode);
        Assert.Equal(50_000m, prepayment.PrincipalAmount);
    }

    [Fact]
    public void LoanPrepayment_TamKapamaDurumunda_PrincipalAmountNullOlabilir()
    {
        var prepayment = new LoanPrepayment
        {
            LoanId = Guid.NewGuid(),
            Date = new DateOnly(2026, 12, 1),
            Mode = LoanPrepaymentMode.FullClosure,
            PrincipalAmount = null
        };

        Assert.Equal(LoanPrepaymentMode.FullClosure, prepayment.Mode);
        Assert.Null(prepayment.PrincipalAmount);
    }
}
