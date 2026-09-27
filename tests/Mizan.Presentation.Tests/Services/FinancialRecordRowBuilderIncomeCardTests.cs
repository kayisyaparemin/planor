using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;
using static Mizan.Presentation.Tests.Services.FinancialStructureData;

namespace Mizan.Presentation.Tests.Services;

/// <summary>
/// Finansal Yapı satır üreticisinin gelir ve kart grupları: yalnız plana giren kayıtlar, bugün
/// geçerli gelir tutarı ve kart kontrolün hero'suyla aynı sıradaki ödeme (EK-V6, S62-4, S62-5).
/// </summary>
public sealed class FinancialRecordRowBuilderIncomeCardTests
{
    private readonly FinancialRecordRowBuilder _builder = new(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));

    [Fact]
    public void Build_BosPlanda_HicSatirYok()
    {
        Assert.True(_builder.Build(new FinancialPlan()).IsEmpty);
    }

    [Fact]
    public void Build_DuzenliGelir_BugunGecerliTutarVeAyinGunuyleGelir()
    {
        var (income, histories) = Income("Gelir", 15, true,
            (40000m, new DateOnly(2026, 1, 1)), (45000m, new DateOnly(2026, 9, 1)), (50000m, new DateOnly(2027, 1, 1)));

        var rows = _builder.Build(new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories }).Incomes;

        var row = Assert.Single(rows);
        Assert.Equal(new FinancialRecordRow(income.Id, FinancialRecordKind.RecurringIncome, "Gelir", 45000m, null) { DayOfMonth = 15 }, row);
    }

    [Fact]
    public void Build_YalnizIleriTarihliTutarVarsa_IlkIleriTutarGosterilir()
    {
        var (income, histories) = Income("Kira", 5, true, (32000m, new DateOnly(2027, 1, 1)), (30000m, new DateOnly(2026, 10, 1)));

        var row = Assert.Single(_builder.Build(new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories }).Incomes);

        Assert.Equal(30000m, row.Amount);
    }

    [Fact]
    public void Build_TutariOlmayanGelir_TutarsizSatirOlur()
    {
        var (income, _) = Income("Ek iş", 20);

        var row = Assert.Single(_builder.Build(new FinancialPlan { RecurringIncomes = [income] }).Incomes);

        Assert.Null(row.Amount);
        Assert.False(row.HasAmount);
    }

    [Fact]
    public void Build_PasifGelir_Listelenmez()
    {
        var (income, histories) = Income("Eski iş", 10, false, (20000m, new DateOnly(2026, 1, 1)));

        Assert.Empty(_builder.Build(new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories }).Incomes);
    }

    [Fact]
    public void Build_TekSeferlikGelir_YalnizBugunVeSonrasiListelenir()
    {
        var past = AdHoc("İade", 900m, Today.AddDays(-1));
        var today = AdHoc("Prim", 5000m, Today);

        var row = Assert.Single(_builder.Build(new FinancialPlan { AdHocIncomes = [past, today] }).Incomes);

        Assert.Equal(new FinancialRecordRow(today.Id, FinancialRecordKind.AdHocIncome, "Prim", 5000m, Today), row);
    }

    [Fact]
    public void Build_Gelirler_DuzenliOnceGuneGoreSonraTekSeferlikTariheGore()
    {
        var (late, lateHistory) = Income("Kira", 20, true, (10000m, new DateOnly(2026, 1, 1)));
        var (early, earlyHistory) = Income("Gelir", 5, true, (45000m, new DateOnly(2026, 1, 1)));
        var december = AdHoc("İkramiye", 20000m, new DateOnly(2026, 12, 20));
        var october = AdHoc("Prim", 5000m, new DateOnly(2026, 10, 10));
        var plan = new FinancialPlan
        {
            RecurringIncomes = [late, early], IncomeHistories = [.. lateHistory, .. earlyHistory], AdHocIncomes = [december, october]
        };

        var names = _builder.Build(plan).Incomes.Select(r => r.Name);

        Assert.Equal(["Gelir", "Kira", "Prim", "İkramiye"], names);
    }

    [Fact]
    public void Build_KartTutari_KartKontrolunSiradakiOdemesidir()
    {
        var card = Card("Axess", carried: 4000m);
        var projections = new CreditCardStatementCalculator().Project(card, 8, true, 0.05m);
        var next = projections[NextPaymentViewModel.FindPaymentIndex(projections)];

        var row = Assert.Single(_builder.Build(new FinancialPlan { CreditCards = [card] }).Cards);

        Assert.Equal(new FinancialRecordRow(card.Id, FinancialRecordKind.CreditCard, "Axess", next.Payment, next.PaymentDueDate), row);
        Assert.True(row.HasAmount);
    }

    [Fact]
    public void Build_OdemesiOlmayanKart_TutarVeTarihsizSondaDurur()
    {
        var idle = Card("Bonus");
        var busy = Card("Axess", carried: 4000m);

        var rows = _builder.Build(new FinancialPlan { CreditCards = [idle, busy] }).Cards;

        Assert.Equal([busy.Id, idle.Id], rows.Select(r => r.Id));
        Assert.Null(rows[1].Amount);
        Assert.Null(rows[1].NextDate);
    }

    [Fact]
    public void Build_PasifKart_Listelenmez()
    {
        Assert.Empty(_builder.Build(new FinancialPlan { CreditCards = [Card("Eski", carried: 1000m, active: false)] }).Cards);
    }

    [Fact]
    public void Build_AdiBosKart_BankaAdiylaGorunur()
    {
        var row = Assert.Single(_builder.Build(new FinancialPlan { CreditCards = [Card("  ", bank: "Yapı Kredi")] }).Cards);

        Assert.Equal("Yapı Kredi", row.Name);
    }
}
