using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartının mevcut ekstre planı, vadeye özel planları, genel ödeme stratejisi
/// ve simülasyon yedek stratejisi arasındaki hiyerarşiye göre ekstre ödeme kararını belirleyen saf hesaplayıcı.
/// </summary>
public sealed class CreditCardPaymentDecisionResolver
{
    /// <summary>
    /// Ekstre için uygulanacak ödeme tutarını, karar kaynağını ve ödeme tipini belirler.
    /// </summary>
    public CreditCardPaymentDecision ResolvePayment(
        CreditCard card,
        DateOnly dueDate,
        decimal? statementBalance,
        decimal? minimumPayment,
        bool isActualStatement,
        bool useProjectionFallback)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (isActualStatement && card.CurrentStatementPaymentPlan is { } currentPlan)
        {
            return new CreditCardPaymentDecision(
                CalculateCurrentStatementPayment(currentPlan, statementBalance, minimumPayment),
                CreditCardPaymentResolution.CurrentStatementPlan,
                ToPaymentType(currentPlan.Mode));
        }

        var paymentOverride = card.PaymentPlans.SingleOrDefault(x => x.DueDate == dueDate);
        if (paymentOverride is not null)
        {
            return new CreditCardPaymentDecision(
                CalculatePayment(paymentOverride.PaymentType, paymentOverride.Amount, statementBalance, minimumPayment),
                CreditCardPaymentResolution.DueDateOverride,
                paymentOverride.PaymentType);
        }

        var strategyType = ToPaymentType(card.PaymentStrategy);
        if (strategyType is not null)
        {
            return new CreditCardPaymentDecision(
                CalculatePayment(strategyType.Value, card.FixedPaymentAmount, statementBalance, minimumPayment),
                CreditCardPaymentResolution.GeneralStrategy,
                strategyType);
        }

        var fallbackType = useProjectionFallback ? ToPaymentType(card.ProjectionFallbackStrategy) : null;
        if (fallbackType is not null)
        {
            return new CreditCardPaymentDecision(
                CalculatePayment(fallbackType.Value, card.ProjectionFallbackFixedAmount, statementBalance, minimumPayment),
                CreditCardPaymentResolution.ProjectionFallback,
                fallbackType);
        }

        return new CreditCardPaymentDecision(null, CreditCardPaymentResolution.Undetermined, null);
    }

    private static decimal? CalculateCurrentStatementPayment(
        CurrentStatementPaymentPlan plan,
        decimal? statementBalance,
        decimal? minimumPayment)
    {
        if (statementBalance is null || minimumPayment is null)
        {
            return null;
        }

        var requested = plan.Mode switch
        {
            CurrentStatementPaymentMode.Minimum => minimumPayment.Value,
            CurrentStatementPaymentMode.Full => statementBalance.Value,
            CurrentStatementPaymentMode.Custom =>
                plan.CustomAmount ?? throw new InvalidOperationException("Bu ekstre için özel ödeme tutarı gereklidir."),
            _ => throw new ArgumentOutOfRangeException(nameof(plan))
        };

        var rounded = MoneyRules.Round(requested);
        if (rounded < 0m || rounded > statementBalance.Value)
        {
            throw new InvalidOperationException("Bu ekstre için ödeme tutarı 0 ile ekstre tutarı arasında olmalıdır.");
        }

        return rounded;
    }

    private static decimal? CalculatePayment(
        CreditCardPaymentType paymentType,
        decimal? fixedAmount,
        decimal? statementBalance,
        decimal? minimumPayment)
    {
        if (statementBalance is null || minimumPayment is null)
        {
            return null;
        }

        var requested = paymentType switch
        {
            CreditCardPaymentType.Minimum => minimumPayment.Value,
            CreditCardPaymentType.FullStatement => statementBalance.Value,
            CreditCardPaymentType.FixedAmount => Math.Max(
                fixedAmount ?? throw new InvalidOperationException("Sabit kart ödeme tutarı gereklidir."),
                minimumPayment.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(paymentType))
        };

        return Math.Min(statementBalance.Value, Math.Max(0m, MoneyRules.Round(requested)));
    }

    private static CreditCardPaymentType? ToPaymentType(CreditCardPaymentStrategy strategy) => strategy switch
    {
        CreditCardPaymentStrategy.AskEachStatement => null,
        CreditCardPaymentStrategy.Minimum => CreditCardPaymentType.Minimum,
        CreditCardPaymentStrategy.FullStatement => CreditCardPaymentType.FullStatement,
        CreditCardPaymentStrategy.FixedAmount => CreditCardPaymentType.FixedAmount,
        _ => throw new ArgumentOutOfRangeException(nameof(strategy))
    };

    private static CreditCardPaymentType ToPaymentType(CurrentStatementPaymentMode mode) => mode switch
    {
        CurrentStatementPaymentMode.Minimum => CreditCardPaymentType.Minimum,
        CurrentStatementPaymentMode.Full => CreditCardPaymentType.FullStatement,
        CurrentStatementPaymentMode.Custom => CreditCardPaymentType.FixedAmount,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private static CreditCardPaymentType? ToPaymentType(ProjectionFallbackStrategy strategy) => strategy switch
    {
        ProjectionFallbackStrategy.None => null,
        ProjectionFallbackStrategy.Minimum => CreditCardPaymentType.Minimum,
        ProjectionFallbackStrategy.FullStatement => CreditCardPaymentType.FullStatement,
        ProjectionFallbackStrategy.FixedAmount => CreditCardPaymentType.FixedAmount,
        _ => throw new ArgumentOutOfRangeException(nameof(strategy))
    };
}
