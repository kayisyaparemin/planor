using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartı ekstre döngülerini, dönem içi bekleyen harcamaları, asgari ödeme tutarlarını
/// ve devreden bakiye üzerindeki carry (akdi) faizini simüle eden saf nakit akış projeksiyon motoru.
/// </summary>
public sealed class CreditCardStatementCalculator(
    CreditCardDateResolver dateResolver,
    CreditCardPaymentDecisionResolver paymentDecisionResolver)
{
    private readonly CreditCardDateResolver _dateResolver = dateResolver;
    private readonly CreditCardPaymentDecisionResolver _paymentDecisionResolver = paymentDecisionResolver;

    /// <summary>
    /// Varsayılan yardımcılarla yeni bir <see cref="CreditCardStatementCalculator"/> örneği oluşturur.
    /// </summary>
    public CreditCardStatementCalculator()
        : this(new CreditCardDateResolver(), new CreditCardPaymentDecisionResolver())
    {
    }

    /// <summary>
    /// Belirtilen referans tarih ve kesim gününe göre o tarihte veya sonrasındaki ilk hesap kesim tarihini belirler.
    /// </summary>
    public DateOnly ResolveStatementCloseOnOrAfter(DateOnly date, int statementClosingDay) =>
        _dateResolver.ResolveStatementCloseOnOrAfter(date, statementClosingDay);

    /// <summary>
    /// Kredi kartı için belirtilen sayıda ekstre döngüsünü simüle ederek her bir ekstrenin
    /// borç, ödeme, akdi faiz ve devreden bakiye projeksiyonunu üretir.
    /// </summary>
    public IReadOnlyList<CreditCardStatementProjection> Project(
        CreditCard card,
        int statementCount,
        bool useProjectionFallback = false,
        decimal carryInterestRate = 0.05m)
    {
        if (statementCount < 1)
        {
            return [];
        }

        CreditCardValidator.Validate(card);
        CreditCardValidator.ValidateInterestRate(carryInterestRate);

        var actualStatement = card.CurrentStatement;
        var closeDate = ResolveInitialCloseDate(card, actualStatement);
        var assignedCharges = PrepareAssignedCharges(card, actualStatement, closeDate);
        decimal? carried = actualStatement?.StatementAmount ?? card.CarriedBalance;
        var result = new List<CreditCardStatementProjection>(statementCount);

        for (var index = 0; index < statementCount; index++)
        {
            var isActual = actualStatement is not null && index == 0;
            var projection = CalculateStatement(
                card, actualStatement, closeDate, carried, assignedCharges,
                isActual, index == 0, useProjectionFallback, carryInterestRate);

            result.Add(projection);
            carried = projection.NextCarriedBalance;
            closeDate = _dateResolver.ResolveNextStatementCloseDate(card, closeDate, isActual);
        }

        return result;
    }

    private DateOnly ResolveInitialCloseDate(CreditCard card, CreditCardStatement? actualStatement) =>
        actualStatement?.StatementDate ??
        card.KnownNextStatementDate ??
        _dateResolver.ResolveStatementCloseOnOrAfter(card.BalanceAsOfDate, card.StatementClosingDay);

    private Dictionary<DateOnly, decimal> PrepareAssignedCharges(
        CreditCard card,
        CreditCardStatement? actualStatement,
        DateOnly firstClose) =>
        card.Charges
            .Where(x => actualStatement is null || x.PostingDate > actualStatement.StatementDate)
            .GroupBy(x => _dateResolver.ResolveChargeStatementClose(card, x.PostingDate, firstClose))
            .ToDictionary(x => x.Key, x => x.Sum(charge => charge.Amount));

    private CreditCardStatementProjection CalculateStatement(
        CreditCard card,
        CreditCardStatement? actualStatement,
        DateOnly closeDate,
        decimal? carried,
        Dictionary<DateOnly, decimal> assignedCharges,
        bool isActual,
        bool isFirstIndex,
        bool useProjectionFallback,
        decimal carryInterestRate)
    {
        var newCharges = isActual ? 0m : assignedCharges.GetValueOrDefault(closeDate);
        if (actualStatement is null && isFirstIndex)
        {
            newCharges += card.UnbilledSpending;
        }

        var carryInterest = !isActual && carried is > 0m ? MoneyRules.Round(carried.Value * carryInterestRate) : 0m;
        decimal? statementBalance = carried is null ? null : carried.Value + carryInterest + newCharges;
        decimal? minimumPayment = isActual
            ? actualStatement!.MinimumPaymentAmount
            : statementBalance is null ? null : MoneyRules.Round(statementBalance.Value * card.MinimumPaymentRate);

        var dueDate = _dateResolver.ResolvePaymentDueDate(card, closeDate, isActual);
        var decision = _paymentDecisionResolver.ResolvePayment(
            card, dueDate, statementBalance, minimumPayment, isActual, useProjectionFallback);

        decimal? carriedAfterPayment = statementBalance is null || decision.Payment is null
            ? null
            : Math.Max(0m, statementBalance.Value - decision.Payment.Value);

        return CreateProjection(closeDate, dueDate, carried, newCharges, statementBalance, minimumPayment,
            decision, carriedAfterPayment, carryInterest, carryInterestRate, isActual);
    }

    private static CreditCardStatementProjection CreateProjection(
        DateOnly closeDate, DateOnly dueDate, decimal? carried, decimal newCharges,
        decimal? statementBalance, decimal? minimumPayment, CreditCardPaymentDecision decision,
        decimal? carriedAfter, decimal carryInterest, decimal carryInterestRate, bool isActual) => new()
    {
        StatementCloseDate = closeDate, PaymentDueDate = dueDate, OpeningCarriedBalance = carried,
        NewCharges = newCharges, StatementBalance = statementBalance, MinimumPayment = minimumPayment,
        Payment = decision.Payment, CarriedAfterPayment = carriedAfter, CarryInterest = carryInterest,
        NextCarriedBalance = carriedAfter, AppliedInterestRate = carryInterestRate,
        PaymentResolution = decision.Resolution, AppliedPaymentType = decision.PaymentType,
        IsActualStatement = isActual
    };
}
