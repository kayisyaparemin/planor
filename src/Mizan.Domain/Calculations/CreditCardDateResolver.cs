using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartı hesap kesim döngülerini, son ödeme tarihlerini ve harcamaların
/// hangi ekstreye faturalanacağını takvim ve banka kurallarına göre çözümleyen saf hesaplayıcı.
/// </summary>
public sealed class CreditCardDateResolver
{
    /// <summary>
    /// Belirtilen referans tarih ve kesim gününe göre o tarihte veya sonrasındaki ilk hesap kesim tarihini belirler.
    /// </summary>
    public DateOnly ResolveStatementCloseOnOrAfter(DateOnly date, int statementClosingDay)
    {
        CalendarRules.ValidateDay(statementClosingDay);
        var closeDate = CalendarRules.ResolveDay(date.Year, date.Month, statementClosingDay);
        return closeDate >= date
            ? closeDate
            : CalendarRules.AddMonthsKeepingDay(closeDate, 1, statementClosingDay);
    }

    /// <summary>
    /// Harcamanın yapıldığı tarih ve ilk projeksiyon kesim tarihine göre harcamanın hangi ekstreye dahil olacağını belirler.
    /// </summary>
    public DateOnly ResolveChargeStatementClose(
        DateOnly postingDate,
        DateOnly firstProjectionClose,
        int statementClosingDay)
    {
        var closeDate = ResolveStatementCloseOnOrAfter(postingDate, statementClosingDay);
        return closeDate < firstProjectionClose ? firstProjectionClose : closeDate;
    }

    /// <summary>
    /// Kartın güncel kesilmiş ekstresi veya bilinen sonraki kesim tarihlerini dikkate alarak
    /// harcamanın ait olduğu ekstre kesim tarihini belirler.
    /// </summary>
    public DateOnly ResolveChargeStatementClose(
        CreditCard card,
        DateOnly postingDate,
        DateOnly firstProjectionClose)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (card.CurrentStatement is { NextStatementDate: { } nextStatementDate } currentStatement &&
            postingDate > currentStatement.StatementDate &&
            postingDate <= nextStatementDate)
        {
            return nextStatementDate;
        }

        if (card.CurrentStatement is null &&
            card.KnownNextStatementDate is { } knownNextStatementDate &&
            postingDate <= knownNextStatementDate)
        {
            return knownNextStatementDate;
        }

        return ResolveChargeStatementClose(postingDate, firstProjectionClose, card.StatementClosingDay);
    }

    /// <summary>
    /// Belirtilen hesap kesim tarihi ve son ödeme gününe göre son ödeme tarihini hesaplar.
    /// </summary>
    public DateOnly ResolvePaymentDueDate(DateOnly statementCloseDate, int paymentDueDay)
    {
        CalendarRules.ValidateDay(paymentDueDay);
        var sameMonth = CalendarRules.ResolveDay(statementCloseDate.Year, statementCloseDate.Month, paymentDueDay);
        return sameMonth > statementCloseDate
            ? sameMonth
            : CalendarRules.AddMonthsKeepingDay(sameMonth, 1, paymentDueDay);
    }

    /// <summary>
    /// Mevcut ekstre tarihinden bir sonraki döngünün hesap kesim tarihini türetir.
    /// </summary>
    public DateOnly ResolveNextStatementDate(
        DateOnly actualStatementDate,
        int statementClosingDay)
    {
        CalendarRules.ValidateDay(statementClosingDay);
        return CalendarRules.AddMonthsKeepingDay(actualStatementDate, 1, statementClosingDay);
    }

    /// <summary>
    /// Bir sonraki ekstre kesim tarihine göre sonraki son ödeme tarihini hesaplar.
    /// </summary>
    public DateOnly ResolveNextDueDate(
        DateOnly nextStatementDate,
        int paymentDueDay) =>
        ResolvePaymentDueDate(nextStatementDate, paymentDueDay);

    /// <summary>
    /// Kartın güncel durumuna veya simülasyon döngüsüne göre ilgili ekstrenin son ödeme tarihini belirler.
    /// </summary>
    public DateOnly ResolvePaymentDueDate(
        CreditCard card,
        DateOnly statementCloseDate,
        bool isCurrentActualStatement)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (card.CurrentStatement is { } statement)
        {
            if (isCurrentActualStatement)
            {
                return statement.DueDate;
            }

            if (statement.NextStatementDate == statementCloseDate &&
                statement.NextDueDate is { } nextDueDate)
            {
                return nextDueDate;
            }
        }
        else if (card.KnownNextStatementDate == statementCloseDate &&
                 card.KnownNextDueDate is { } knownDueDate)
        {
            return knownDueDate;
        }

        return ResolvePaymentDueDate(statementCloseDate, card.PaymentDueDay);
    }

    /// <summary>
    /// Kartın güncel durumuna göre bir sonraki döngünün hesap kesim tarihini belirler.
    /// </summary>
    public DateOnly ResolveNextStatementCloseDate(
        CreditCard card,
        DateOnly closeDate,
        bool wasCurrentActualStatement)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (wasCurrentActualStatement &&
            card.CurrentStatement?.NextStatementDate is { } nextStatementDate)
        {
            return nextStatementDate;
        }

        return ResolveNextStatementDate(closeDate, card.StatementClosingDay);
    }
}
