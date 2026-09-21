using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardPaymentDecisionResolverTests
{
    private readonly CreditCardPaymentDecisionResolver _resolver = new();

    [Fact]
    public void ResolvePayment_KesilmisEkstrePlaniVarsa_CurrentStatementPlanOlarakCozumler()
    {
        var card = new CreditCard
        {
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 15_000m,
                MinimumPaymentAmount = 6_000m
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum
            }
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 9, 7),
            statementBalance: 15_000m,
            minimumPayment: 6_000m,
            isActualStatement: true,
            useProjectionFallback: false);

        Assert.Equal(6_000m, decision.Payment);
        Assert.Equal(CreditCardPaymentResolution.CurrentStatementPlan, decision.Resolution);
        Assert.Equal(CreditCardPaymentType.Minimum, decision.PaymentType);
    }

    [Fact]
    public void ResolvePayment_KesilmisEkstreTamOdeme_EkstreBakiyesiniTamOder()
    {
        var card = new CreditCard
        {
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 15_000m,
                MinimumPaymentAmount = 6_000m
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Full
            }
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 9, 7),
            statementBalance: 15_000m,
            minimumPayment: 6_000m,
            isActualStatement: true,
            useProjectionFallback: false);

        Assert.Equal(15_000m, decision.Payment);
        Assert.Equal(CreditCardPaymentType.FullStatement, decision.PaymentType);
    }

    [Fact]
    public void ResolvePayment_KesilmisEkstreOzelTutar_GirilenTutariOder()
    {
        var card = new CreditCard
        {
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 15_000m,
                MinimumPaymentAmount = 6_000m
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Custom,
                CustomAmount = 10_000m
            }
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 9, 7),
            statementBalance: 15_000m,
            minimumPayment: 6_000m,
            isActualStatement: true,
            useProjectionFallback: false);

        Assert.Equal(10_000m, decision.Payment);
        Assert.Equal(CreditCardPaymentType.FixedAmount, decision.PaymentType);
    }

    [Fact]
    public void ResolvePayment_VadeyeOzelPlanVarsa_GenelStratejiyiEzer()
    {
        var dueDate = new DateOnly(2026, 10, 5);
        var card = new CreditCard
        {
            PaymentStrategy = CreditCardPaymentStrategy.Minimum,
            PaymentPlans =
            [
                new CreditCardPaymentPlan
                {
                    DueDate = dueDate,
                    PaymentType = CreditCardPaymentType.FixedAmount,
                    Amount = 50_000m
                }
            ]
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: dueDate,
            statementBalance: 90_000m,
            minimumPayment: 36_000m,
            isActualStatement: false,
            useProjectionFallback: false);

        Assert.Equal(50_000m, decision.Payment);
        Assert.Equal(CreditCardPaymentResolution.DueDateOverride, decision.Resolution);
        Assert.Equal(CreditCardPaymentType.FixedAmount, decision.PaymentType);
    }

    [Fact]
    public void ResolvePayment_GenelStratejiSabitTutar_AsgarininAltinaDusmez()
    {
        var card = new CreditCard
        {
            PaymentStrategy = CreditCardPaymentStrategy.FixedAmount,
            FixedPaymentAmount = 20_000m
        };

        // Asgari 38.300 iken sabit ödeme 20.000 ise asgariye yükselir
        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 10, 5),
            statementBalance: 95_750m,
            minimumPayment: 38_300m,
            isActualStatement: false,
            useProjectionFallback: false);

        Assert.Equal(38_300m, decision.Payment);
        Assert.Equal(CreditCardPaymentResolution.GeneralStrategy, decision.Resolution);
    }

    [Fact]
    public void ResolvePayment_GenelStratejiSabitTutar_EkstreBorcunuAsamaz()
    {
        var card = new CreditCard
        {
            PaymentStrategy = CreditCardPaymentStrategy.FixedAmount,
            FixedPaymentAmount = 50_000m
        };

        // Ekstre borcu 15.000 iken sabit ödeme 50.000 ise borç kadar (15.000) öder
        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 10, 5),
            statementBalance: 15_000m,
            minimumPayment: 6_000m,
            isActualStatement: false,
            useProjectionFallback: false);

        Assert.Equal(15_000m, decision.Payment);
    }

    [Fact]
    public void ResolvePayment_AskEachStatementVeFallbackAciksa_YedekStratejiyiUygular()
    {
        var card = new CreditCard
        {
            PaymentStrategy = CreditCardPaymentStrategy.AskEachStatement,
            ProjectionFallbackStrategy = ProjectionFallbackStrategy.Minimum
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 10, 5),
            statementBalance: 100_000m,
            minimumPayment: 40_000m,
            isActualStatement: false,
            useProjectionFallback: true);

        Assert.Equal(40_000m, decision.Payment);
        Assert.Equal(CreditCardPaymentResolution.ProjectionFallback, decision.Resolution);
        Assert.Equal(CreditCardPaymentType.Minimum, decision.PaymentType);
    }

    [Fact]
    public void ResolvePayment_AskEachStatementVeFallbackYoksa_UndeterminedDoner()
    {
        var card = new CreditCard
        {
            PaymentStrategy = CreditCardPaymentStrategy.AskEachStatement,
            ProjectionFallbackStrategy = ProjectionFallbackStrategy.None
        };

        var decision = _resolver.ResolvePayment(
            card,
            dueDate: new DateOnly(2026, 10, 5),
            statementBalance: 100_000m,
            minimumPayment: 40_000m,
            isActualStatement: false,
            useProjectionFallback: true);

        Assert.Null(decision.Payment);
        Assert.Equal(CreditCardPaymentResolution.Undetermined, decision.Resolution);
        Assert.Null(decision.PaymentType);
    }
}
