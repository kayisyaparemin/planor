using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartı sözleşmesi, ekstre ve ödeme planı parametrelerinin iş kurallarına ve
/// takvim kısıtlarına uygunluğunu doğrulayan saf denetleyici.
/// </summary>
public static class CreditCardValidator
{
    /// <summary>
    /// Kredi kartı bileşenlerini, tutarlarını, gün sınırlarını ve ekstre tutarlılığını doğrular.
    /// </summary>
    /// <param name="card">Doğrulanacak kredi kartı.</param>
    /// <exception cref="ArgumentNullException"><paramref name="card"/> null ise fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">İş kuralı ihlali durumunda fırlatılır.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Oran veya gün aralık dışı ise fırlatılır.</exception>
    public static void Validate(CreditCard card)
    {
        ArgumentNullException.ThrowIfNull(card);

        ValidateBasics(card);
        ValidateStrategies(card);
        ValidatePaymentPlans(card.PaymentPlans);

        if (card.CurrentStatement is { } statement)
        {
            ValidateStatement(card, statement);
        }
        else if (card.CurrentStatementPaymentPlan is not null)
        {
            throw new InvalidOperationException("Kesilmiş ekstre planı için önce ekstre bilgisi gereklidir.");
        }

        if (card.KnownNextDueDate is { } knownDue &&
            (card.KnownNextStatementDate is not { } knownClose || knownDue <= knownClose))
        {
            throw new InvalidOperationException(
                "Bilinen bir sonraki son ödeme tarihi bilinen kesim tarihinden sonra olmalıdır.");
        }
    }

    /// <summary>
    /// Kredi kartı devreden borç faiz oranının geçerli aralıkta (0 ile 1) olduğunu doğrular.
    /// </summary>
    /// <param name="rate">Doğrulanacak faiz oranı.</param>
    /// <exception cref="ArgumentOutOfRangeException">Oran 0 ile 1 arasında değilse fırlatılır.</exception>
    public static void ValidateInterestRate(decimal rate)
    {
        if (rate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rate),
                "Kart devreden borç faiz oranı 0 ile 1 arasında olmalıdır.");
        }
    }

    private static void ValidateBasics(CreditCard card)
    {
        if (card.BalanceAsOfDate == default)
        {
            throw new InvalidOperationException("Kart bakiye tarihi gereklidir.");
        }

        CalendarRules.ValidateDay(card.StatementClosingDay);
        CalendarRules.ValidateDay(card.PaymentDueDay);

        if (card.MinimumPaymentRate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(card),
                "Asgari ödeme oranı 0 ile 1 arasında olmalıdır.");
        }

        if (card.Limit < 0m || card.CarriedBalance < 0m || card.UnbilledSpending < 0m || card.Charges.Any(x => x.Amount < 0m))
        {
            throw new InvalidOperationException("Kart limit ve borç bileşenleri negatif olamaz.");
        }
    }

    private static void ValidateStrategies(CreditCard card)
    {
        if (card.PaymentStrategy == CreditCardPaymentStrategy.FixedAmount &&
            card.FixedPaymentAmount is null or <= 0m)
        {
            throw new InvalidOperationException("Sabit ödeme tercihi için 0'dan büyük bir tutar gereklidir.");
        }

        if (card.ProjectionFallbackStrategy == ProjectionFallbackStrategy.FixedAmount &&
            card.ProjectionFallbackFixedAmount is null or <= 0m)
        {
            throw new InvalidOperationException(
                "Gelecek hesaplamalarda sabit tutar kullanmak için 0'dan büyük bir tutar gereklidir.");
        }
    }

    private static void ValidatePaymentPlans(IReadOnlyList<CreditCardPaymentPlan> plans)
    {
        if (plans.Any(x => x.PaymentType == CreditCardPaymentType.FixedAmount && x.Amount is null or <= 0m))
        {
            throw new InvalidOperationException("Özel kart ödemesi için 0'dan büyük bir tutar gereklidir.");
        }

        if (plans.GroupBy(x => x.DueDate).Any(x => x.Count() > 1))
        {
            throw new InvalidOperationException("Aynı son ödeme tarihi için yalnızca bir özel kart planı olabilir.");
        }
    }

    private static void ValidateStatement(CreditCard card, CreditCardStatement statement)
    {
        if (statement.StatementDate == default || statement.DueDate == default)
        {
            throw new InvalidOperationException("Kesilmiş ekstre için kesim ve son ödeme tarihi gereklidir.");
        }

        if (statement.StatementAmount < 0m ||
            statement.MinimumPaymentAmount < 0m ||
            statement.MinimumPaymentAmount > statement.StatementAmount)
        {
            throw new InvalidOperationException("Kesilmiş ekstre tutarı ve asgari ödeme geçersiz.");
        }

        if (statement.NextStatementDate is { } nextStatementDate && nextStatementDate <= statement.StatementDate)
        {
            throw new InvalidOperationException("Bir sonraki kesim tarihi mevcut ekstre tarihinden sonra olmalıdır.");
        }

        if (statement.NextDueDate is { } nextDueDate && nextDueDate <= statement.DueDate)
        {
            throw new InvalidOperationException("Bir sonraki son ödeme tarihi mevcut son ödeme tarihinden sonra olmalıdır.");
        }

        if (card.CurrentStatementPaymentPlan is { Mode: CurrentStatementPaymentMode.Custom } customPlan &&
            (customPlan.CustomAmount is null or < 0m || customPlan.CustomAmount > statement.StatementAmount))
        {
            throw new InvalidOperationException("Bu ekstre için özel ödeme tutarı 0 ile ekstre tutarı arasında olmalıdır.");
        }
    }
}
