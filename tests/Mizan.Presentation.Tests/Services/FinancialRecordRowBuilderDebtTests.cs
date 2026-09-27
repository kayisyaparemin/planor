using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;
using Xunit;
using static Mizan.Presentation.Tests.Services.FinancialStructureData;

namespace Mizan.Presentation.Tests.Services;

/// <summary>
/// Finansal Yapı satır üreticisinin kredi ve ödeme grupları: biten kayıtlar düşer, taksitsiz plan
/// silinebilsin diye tutarsız satır olur, sıra sıradaki tarihe göredir (EK-V6, S62-5).
/// </summary>
public sealed class FinancialRecordRowBuilderDebtTests
{
    private readonly FinancialRecordRowBuilder _builder = new(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));

    [Fact]
    public void Build_Kredi_TaksitKalanVeSonrakiTarihleGelir()
    {
        var loan = Loan("İhtiyaç", new DateOnly(2026, 10, 15));

        var row = Assert.Single(_builder.Build(new FinancialPlan { Loans = [loan] }).Loans);

        Assert.Equal(new FinancialRecordRow(loan.Id, FinancialRecordKind.Loan, "İhtiyaç", 7500m, new DateOnly(2026, 10, 15)) { RemainingCount = 12 }, row);
    }

    [Fact]
    public void Build_BitmisVeyaPasifKredi_Listelenmez()
    {
        var finished = Loan("Taşıt", new DateOnly(2026, 10, 1), remaining: 0);
        var inactive = Loan("Konut", new DateOnly(2026, 10, 1), active: false);

        Assert.Empty(_builder.Build(new FinancialPlan { Loans = [finished, inactive] }).Loans);
    }

    [Fact]
    public void Build_Krediler_SonrakiTariheGoreSiralanirAdiBossaBankaGorunur()
    {
        var november = Loan("Konut", new DateOnly(2026, 11, 1));
        var october = Loan("", new DateOnly(2026, 10, 15), bank: "Ziraat");

        var rows = _builder.Build(new FinancialPlan { Loans = [november, october] }).Loans;

        Assert.Equal(["Ziraat", "Konut"], rows.Select(r => r.Name));
    }

    [Fact]
    public void Build_OdemePlani_SiradakiOdenmemisTaksitiGosterir()
    {
        var plan = Plan("Okul", (new DateOnly(2026, 9, 20), 4000m, true), (new DateOnly(2026, 10, 20), 4200m, false),
            (new DateOnly(2026, 11, 20), 4200m, false));

        var row = Assert.Single(_builder.Build(new FinancialPlan { PaymentPlans = [plan] }).Payments);

        Assert.Equal(new FinancialRecordRow(plan.Id, FinancialRecordKind.PaymentPlan, "Okul", 4200m, new DateOnly(2026, 10, 20)) { RemainingCount = 2 }, row);
    }

    [Fact]
    public void Build_TamamlanmisPlan_Listelenmez()
    {
        var plan = Plan("Mobilya", (new DateOnly(2026, 8, 1), 3000m, true), (new DateOnly(2026, 9, 1), 3000m, true));

        Assert.Empty(_builder.Build(new FinancialPlan { PaymentPlans = [plan] }).Payments);
    }

    [Fact]
    public void Build_TaksitsizPlan_TutarsizSatirOlarakSondaDurur()
    {
        var empty = Plan("Buzdolabı");
        var expense = Expense("Tatil", 30000m, new DateOnly(2027, 7, 12));

        var rows = _builder.Build(new FinancialPlan { PaymentPlans = [empty], PlannedLargeExpenses = [expense] }).Payments;

        Assert.Equal([expense.Id, empty.Id], rows.Select(r => r.Id));
        Assert.Equal(new FinancialRecordRow(empty.Id, FinancialRecordKind.PaymentPlan, "Buzdolabı", null, null) { RemainingCount = 0 }, rows[1]);
    }

    [Fact]
    public void Build_BuyukHarcama_YalnizPlanlananlarListelenir()
    {
        var planned = Expense("Tatil", 30000m, new DateOnly(2027, 7, 12));
        var done = Expense("Telefon", 25000m, new DateOnly(2026, 9, 1), PlannedExpenseStatus.Completed);
        var cancelled = Expense("Araba", 900000m, new DateOnly(2027, 1, 1), PlannedExpenseStatus.Cancelled);

        var row = Assert.Single(_builder.Build(new FinancialPlan { PlannedLargeExpenses = [planned, done, cancelled] }).Payments);

        Assert.Equal(new FinancialRecordRow(planned.Id, FinancialRecordKind.LargeExpense, "Tatil", 30000m, new DateOnly(2027, 7, 12)), row);
    }

    [Fact]
    public void Build_Odemeler_PlanVeHarcamaBirlikteSiradakiTariheGoreSiralanir()
    {
        var expense = Expense("Tatil", 30000m, new DateOnly(2026, 12, 1));
        var plan = Plan("Okul", (new DateOnly(2026, 10, 20), 4200m, false));
        var early = Expense("Sigorta", 6000m, new DateOnly(2026, 10, 5));

        var rows = _builder.Build(new FinancialPlan { PaymentPlans = [plan], PlannedLargeExpenses = [expense, early] }).Payments;

        Assert.Equal(["Sigorta", "Okul", "Tatil"], rows.Select(r => r.Name));
    }
}
