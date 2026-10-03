using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Bugün ile açık dönemin bitişi arasına düşen denemenin etkisi (S76-2, V10b notları b): zincir açık dönemden sonra
/// başladığı için bu etki zincirin kendisinde görünmez, açılışa eklenir. Etki akış farkıdır, plandaki açılışa
/// bağlı değildir; KMH faizi ana sayfadaki gerçek dönem sonu üzerinden işler.
/// </summary>
public sealed class SimulationOpenPeriodEffectTests
{
    private static readonly DateOnly OpenStart = new(2026, 9, 10);
    private static readonly DateOnly OpenEnd = new(2026, 10, 10);
    private static readonly DateOnly Today = new(2026, 9, 30);
    private const decimal Income = 50_000m;
    private const decimal LivingAllowance = 30_000m;
    private const decimal DeficitRate = 0.05m;

    private readonly SimulationCalculator _calculator = CreateCalculator();

    [Fact]
    public void Etki_BugunTarihliNakitOdeme_AcikDonemSonunuTutarKadarDusurur()
    {
        var plan = Plan(openingBalance: 0m);

        var effect = _calculator.CalculateOpenPeriodEffect(plan, Today, [Cash(30_000m, Today)], OpenStart, 50_000m);

        Assert.Equal(-30_000m, effect);
    }

    [Fact]
    public void Etki_DenemeAcikDonemdenSonraDusuyorsa_TamSifirdir()
    {
        var plan = Plan(openingBalance: 0m);

        var effect = _calculator.CalculateOpenPeriodEffect(plan, Today, [Cash(30_000m, new DateOnly(2026, 11, 15))], OpenStart, 50_000m);

        Assert.Equal(0m, effect);
    }

    // Faiz öncesi 10.000 + (−30.000) = −20.000; %5 faiz 1.000. Plan açılışından işletilseydi başka çıkardı.
    [Fact]
    public void Etki_DenemeDonemSonunuEksiyeCekiyorsa_FaizGercekRakamdanIsletilir()
    {
        var plan = Plan(openingBalance: 0m);

        var effect = _calculator.CalculateOpenPeriodEffect(plan, Today, [Cash(30_000m, Today)], OpenStart, 10_000m);

        Assert.Equal(-31_000m, effect);
    }

    // Baz zaten −5.000: faizi 250; deneme sonrası −35.000: faizi 1.750. Etki yalnız ek faizi de içerir.
    [Fact]
    public void Etki_DonemSonuZatenEksiyse_YalnizEkFaiziEkler()
    {
        var plan = Plan(openingBalance: 0m);

        var effect = _calculator.CalculateOpenPeriodEffect(plan, Today, [Cash(30_000m, Today)], OpenStart, -5_000m);

        Assert.Equal(-31_500m, effect);
    }

    [Fact]
    public void Etki_PlanninAcilisindanBagimsizdir()
    {
        var small = _calculator.CalculateOpenPeriodEffect(Plan(0m), Today, [Cash(30_000m, Today)], OpenStart, 20_000m);
        var large = _calculator.CalculateOpenPeriodEffect(Plan(5_000_000m), Today, [Cash(30_000m, Today)], OpenStart, 20_000m);

        Assert.Equal(small, large);
        Assert.Equal(-30_000m - 500m, small);
    }

    [Fact]
    public void Etki_IkiDeneme_AkisFarklariToplanir()
    {
        var plan = Plan(openingBalance: 0m);
        var requests = new[] { Cash(10_000m, Today), Cash(5_000m, Today.AddDays(3)) };

        var effect = _calculator.CalculateOpenPeriodEffect(plan, Today, requests, OpenStart, 50_000m);

        Assert.Equal(-15_000m, effect);
    }

    [Fact]
    public void Hesapla_AcilisKaymasi_SenaryoZincirininAcilisinaEklenirBazDegismez()
    {
        var plan = Plan(openingBalance: 41_723m, anchor: OpenEnd);
        var request = Cash(1_000m, new DateOnly(2026, 12, 15));

        var shifted = _calculator.Calculate(plan, Today, [request], 12, OpenEnd, scenarioOpeningShift: -30_000m);
        var plain = _calculator.Calculate(plan, Today, [request], 12, OpenEnd);

        Assert.Equal(plain.Baseline.Select(x => x.EndingBalance), shifted.Baseline.Select(x => x.EndingBalance));
        Assert.Equal(41_723m, shifted.Baseline[0].OpeningBalance);
        Assert.Equal(41_723m - 30_000m, shifted.Scenario[0].OpeningBalance);
        Assert.Equal(plain.Scenario[0].EndingBalance - 30_000m, shifted.Scenario[0].EndingBalance);
    }

    private static SimulationRequest Cash(decimal amount, DateOnly date) =>
        new(SimulationScenarioType.CashPurchase, "Deneme", amount, date);

    // Çapa 10: açık dönem 10 Eylül – 10 Ekim. Çapa ve açılış sınama için değiştirilebilir.
    private static FinancialPlan Plan(decimal openingBalance, DateOnly? anchor = null)
    {
        var streamId = Guid.NewGuid();
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(10),
                ProjectionAnchorDate = anchor ?? OpenStart,
                ProjectionOpeningBalance = openingBalance,
                PeriodVariableExpenseAllowance = LivingAllowance,
                DeficitFinancingInterestRate = DeficitRate
            },
            RecurringIncomes = [new RecurringIncome { Id = streamId, Name = "Gelir", PaymentDay = 10, IsActive = true }],
            IncomeHistories =
            [
                new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = Income, EffectiveDate = new DateOnly(2026, 1, 1) }
            ]
        };
    }

    private static SimulationCalculator CreateCalculator()
    {
        var loanSchedule = new LoanScheduleCalculator();
        var loanAmortization = new LoanAmortizationCalculator(loanSchedule);
        var loanScheduleBuilder = new LoanPaymentScheduleBuilder(loanSchedule, loanAmortization);
        var projection = new FinancialProjectionCalculator(
            new CashFlowPeriodCalculator(),
            new IncomeProjectionCalculator(new IncomeResolver()),
            new CreditCardStatementCalculator(),
            new MandatoryPaymentCalculator(loanScheduleBuilder, new ScheduledPaymentCalculator()),
            new PeriodObligationGrouper());
        var planBuilder = new ScenarioPlanBuilder(
            new InstallmentScheduleCalculator(), new LoanPrepaymentValidator(loanAmortization, loanScheduleBuilder));
        return new SimulationCalculator(projection, planBuilder, loanScheduleBuilder);
    }
}
