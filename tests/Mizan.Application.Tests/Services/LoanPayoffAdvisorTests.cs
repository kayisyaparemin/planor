using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// <see cref="LoanPayoffAdvisor"/> için test kalkanı. Kural (I26): önerilen gün, hiçbir dönemde
/// açığı baz çizgiden büyütmeyen ve net kazancı pozitif olan en erken taksit günüdür.
/// Plan: dönem çapası ayın 1'i, ilk dönem [01.10.2026, 01.11.2026); gelir her ayın 1'inde;
/// kredi taksitleri her ayın 15'inde, ilki 15.10.2026.
/// </summary>
public sealed class LoanPayoffAdvisorTests
{
    private static readonly DateOnly AsOf = new(2026, 10, 1);
    private static readonly DateOnly FirstInstallment = new(2026, 10, 15);

    private static readonly LoanScheduleCalculator ScheduleCalculator = new();
    private static readonly LoanAmortizationCalculator AmortizationCalculator = new(ScheduleCalculator);
    private static readonly LoanPaymentScheduleBuilder ScheduleBuilder = new(ScheduleCalculator, AmortizationCalculator);
    private static readonly ScenarioPlanBuilder PlanBuilder =
        new(new InstallmentScheduleCalculator(), new LoanPrepaymentValidator(AmortizationCalculator, ScheduleBuilder));
    private static readonly FinancialProjectionCalculator Projection = CreateProjectionCalculator();
    private static readonly LoanPayoffAdvisor Advisor = new(Projection, PlanBuilder, AmortizationCalculator, ScheduleBuilder);

    [Fact]
    public void Advise_BolNakitle_IlkTaksitGunuOnerilir()
    {
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [ConsumerLoan()]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.Recommended, advice.Status);
        Assert.Equal(FirstInstallment, advice.Date);
        Assert.True(advice.PayoffAmount > 0m);
        Assert.True(advice.InterestSaving > 0m);
        Assert.True(advice.NetGain > 0m);
        Assert.Null(advice.BestDate);
    }

    /// <summary>
    /// Ayda 20.000 TL artan bakiye, kapatma bedelini ancak 8. dönemde (15.05.2027) karşılıyor;
    /// daha erken her gün bir dönemde açık doğuruyor.
    /// </summary>
    [Fact]
    public void Advise_OnerilenGun_KuraliSaglayanEnErkenTaksitGunudur()
    {
        var loan = ConsumerLoan();
        var plan = PlanWith(openingBalance: 0m, monthlyIncome: 30_000m, [loan]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.Recommended, advice.Status);
        Assert.Equal(new DateOnly(2027, 5, 15), advice.Date);
        Assert.True(PassesRule(plan, loan, advice.Date!.Value), "Önerilen gün kuralı sağlamalı.");
        Assert.False(PassesRule(plan, loan, new DateOnly(2027, 4, 15)), "Bir önceki taksit günü kuralı sağlamamalı.");
    }

    /// <summary>
    /// 6502 md. 37: sabit faizli konut kredisinde kalan vade 36 aya inince erken ödeme ücreti
    /// %2'den %1'e düşer. İlk gün de kazançlı ama bir ay beklemek daha kârlı.
    /// </summary>
    [Fact]
    public void Advise_EnKarliGunEnErkendenFarkliysa_IkisiniDeVerir()
    {
        var housing = ConsumerLoan() with
        {
            Kind = LoanKind.HousingFixed, RemainingInstallmentCount = 38, RemainingDebt = 326_500m
        };
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [housing]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(FirstInstallment, advice.Date);
        Assert.Equal(new DateOnly(2026, 11, 15), advice.BestDate);
        Assert.True(advice.BestNetGain > advice.NetGain);
    }

    /// <summary>
    /// Dipsiz açıkta kapatma parası hep açıktan gelir, bu yüzden hiçbir gün önerilmez. Ufkun
    /// sonuna yakın günler yine de "kazançlı" görünür, çünkü ufuk sonrası açık faizi sayılmaz
    /// (kasıtlı sadeleştirme); sonuç bu yüzden <see cref="LoanPayoffAdviceStatus.NoSafeMonth"/>.
    /// </summary>
    [Fact]
    public void Advise_DerinAcikta_HicbirGunOnerilmez()
    {
        var plan = PlanWith(openingBalance: -2_000_000m, monthlyIncome: 30_000m, [ConsumerLoan()]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.NoSafeMonth, advice.Status);
        Assert.Null(advice.Date);
    }

    /// <summary>
    /// Aylık ~%0,05 faizli sabit faizli konut kredisi: kalan faizin tamamı, 6502 md. 37'nin %1
    /// erken ödeme ücretinden az. Bol nakitle bile hiçbir gün kapatmak kazandırmaz.
    /// </summary>
    [Fact]
    public void Advise_KomisyonKalanFaizdenBuyukse_KapatmakKazandirmaz()
    {
        var lowRateHousing = ConsumerLoan() with { Kind = LoanKind.HousingFixed, RemainingDebt = 238_500m };
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [lowRateHousing]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.NotWorthIt, advice.Status);
    }

    /// <summary>Açık faizi çok düşükken kapatmak kazançlı, ama kapatma parası açığı derinleştiriyor.</summary>
    [Fact]
    public void Advise_KazancliAmaHerGunAcigiBuyutuyorsa_GuvenliAyYok()
    {
        var plan = PlanWith(openingBalance: -100_000m, monthlyIncome: 12_000m, [ConsumerLoan()], deficitRate: 0.001m);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.NoSafeMonth, advice.Status);
    }

    [Fact]
    public void Advise_AnaparasizKredi_AnaparaIster()
    {
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [ConsumerLoan() with { RemainingDebt = null }]);

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.NeedsPrincipal, advice.Status);
    }

    [Fact]
    public void Advise_ZatenKapamasiPlanliKredi_YenidenOnerilmezKapamaGunuDoner()
    {
        var loan = ConsumerLoan();
        var closing = new LoanPrepayment { LoanId = loan.Id, Date = new DateOnly(2027, 1, 15), Mode = LoanPrepaymentMode.FullClosure };
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [loan]) with { LoanPrepayments = [closing] };

        var advice = Assert.Single(Advisor.Advise(plan, AsOf));

        Assert.Equal(LoanPayoffAdviceStatus.AlreadyClosing, advice.Status);
        Assert.Equal(closing.Date, advice.Date);
    }

    /// <summary>Son taksitin günü aday değildir: o gün kapatacak bir şey kalmaz.</summary>
    [Fact]
    public void Advise_TekTaksitiKalmisVePasifKrediler_ListeyeGirmez()
    {
        var regular = ConsumerLoan();
        var lastInstallment = ConsumerLoan() with { RemainingInstallmentCount = 1, RemainingDebt = 9_900m };
        var inactive = ConsumerLoan() with { IsActive = false };
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [regular, lastInstallment, inactive]);

        var advice = Advisor.Advise(plan, AsOf);

        Assert.Equal([regular.Id], advice.Select(x => x.LoanId));
    }

    [Fact]
    public void Advise_Deneme_BazPlaninErkenOdemeleriniDegistirmez()
    {
        var loan = ConsumerLoan();
        var partial = new LoanPrepayment
        {
            LoanId = loan.Id, Date = new DateOnly(2027, 2, 20), Mode = LoanPrepaymentMode.ReduceTerm, PrincipalAmount = 10_000m
        };
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [loan]) with { LoanPrepayments = [partial] };

        Advisor.Advise(plan, AsOf);

        Assert.Equal([partial], plan.LoanPrepayments);
    }

    [Fact]
    public void Advise_IptalIstenmisse_OperationCanceledExceptionFirlatir()
    {
        var plan = PlanWith(openingBalance: 5_000_000m, monthlyIncome: 30_000m, [ConsumerLoan()]);

        Assert.Throws<OperationCanceledException>(() =>
            Advisor.Advise(plan, AsOf, cancellationToken: new CancellationToken(canceled: true)));
    }

    [Fact]
    public void Yapici_EksikBagimlilik_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffAdvisor(null!, PlanBuilder, AmortizationCalculator, ScheduleBuilder));
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffAdvisor(Projection, null!, AmortizationCalculator, ScheduleBuilder));
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffAdvisor(Projection, PlanBuilder, null!, ScheduleBuilder));
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffAdvisor(Projection, PlanBuilder, AmortizationCalculator, null!));
    }

    /// <summary>
    /// Kuralın bağımsız uygulaması: danışmanın kodunu değil tanımı ölçer. Simülasyon motoruyla
    /// kapamalı/kapamasız projeksiyonu kurar; açık büyümüyor mu ve net kazanç pozitif mi, bakar.
    /// </summary>
    private static bool PassesRule(FinancialPlan plan, Loan loan, DateOnly date)
    {
        var request = new SimulationRequest(SimulationScenarioType.LoanEarlyClosure, "Kapat", 0m, date) { LoanId = loan.Id };
        var simulation = new SimulationCalculator(Projection, PlanBuilder, ScheduleBuilder);
        var result = simulation.Calculate(plan, AsOf, request);

        var noNewDeficit = result.Rows.All(row => Deficit(row.Scenario) <= Deficit(row.Baseline) + 0.01m);
        var baselineTotal = ScheduleBuilder.Replay(loan, plan.LoanPrepayments).Total;
        var scenarioTotal = ScheduleBuilder.Replay(loan, PlanBuilder.Build(plan, request).LoanPrepayments).Total;
        var ids = new HashSet<Guid> { loan.Id, request.ScenarioId };
        var beyondHorizonSaving = (baselineTotal - LoanPaymentsIn(result.Baseline, ids)) -
                                  (scenarioTotal - LoanPaymentsIn(result.Scenario, ids));
        var netGain = result.Scenario[^1].EndingBalance - result.Baseline[^1].EndingBalance + beyondHorizonSaving;
        return noNewDeficit && netGain > 0m;
    }

    private static decimal Deficit(CashFlowPeriodProjection period) => Math.Max(0m, -period.EndingBalance);

    private static decimal LoanPaymentsIn(IReadOnlyList<CashFlowPeriodProjection> periods, HashSet<Guid> ids) =>
        periods.SelectMany(x => x.MandatoryItems)
            .Where(x => x.Type == ObligationType.Loan && ids.Contains(x.PaymentId))
            .Sum(x => x.Amount);

    /// <summary>24 × 10.000 TL kalan taksit, 200.000 TL anapara (aylık ~%1,5).</summary>
    private static Loan ConsumerLoan() => new()
    {
        Name = "İhtiyaç",
        Bank = "Test Bankası",
        MonthlyPayment = 10_000m,
        PaymentDay = 15,
        NextPaymentDate = FirstInstallment,
        RemainingInstallmentCount = 24,
        RemainingDebt = 200_000m
    };

    private static FinancialPlan PlanWith(
        decimal openingBalance, decimal monthlyIncome, IReadOnlyList<Loan> loans, decimal deficitRate = 0.05m)
    {
        var income = new RecurringIncome { Name = "Gelir", PaymentDay = 1 };
        return new FinancialPlan
        {
            Settings = new UserSettings
            {
                PeriodAnchor = new PeriodAnchor(1),
                ProjectionAnchorDate = AsOf,
                ProjectionOpeningBalance = openingBalance,
                DeficitFinancingInterestRate = deficitRate
            },
            RecurringIncomes = [income],
            IncomeHistories = [new IncomeAmountHistory { RecurringIncomeId = income.Id, Amount = monthlyIncome, EffectiveDate = AsOf }],
            Loans = loans
        };
    }

    private static FinancialProjectionCalculator CreateProjectionCalculator() => new(
        new CashFlowPeriodCalculator(),
        new IncomeProjectionCalculator(new IncomeResolver()),
        new CreditCardStatementCalculator(),
        new MandatoryPaymentCalculator(ScheduleBuilder, new ScheduledPaymentCalculator()),
        new PeriodObligationGrouper());
}
