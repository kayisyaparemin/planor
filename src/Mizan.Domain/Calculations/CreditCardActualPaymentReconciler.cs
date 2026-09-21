using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Dönem mutabakatı ve kapanışında kredi kartına yapılan fiili ödemeyi ekstre borcundan düşerek
/// kartın devreden bakiyesini, harcama havuzunu ve sonraki döngü başlangıç durumunu güncelleyen saf hesaplayıcı.
/// Gelecek projeksiyonunu kendisi hesaplamaz (T2 düğümü çözümü); mutabakatı yapılacak dönemin
/// hazır ekstre projeksiyonunu parametre olarak alarak faiz işletilmeden anaparanın yeni döneme devretmesini sağlar.
/// </summary>
public sealed class CreditCardActualPaymentReconciler
{
    /// <summary>
    /// Kredi kartına yapılan fiili ödemeyi belirtilen ekstre projeksiyonuna uygulayarak
    /// güncellenmiş yeni kart durumunu üretir.
    /// </summary>
    /// <param name="card">Mutabakatı yapılacak güncel kredi kartı sözleşmesi.</param>
    /// <param name="statement">Ödemenin yapıldığı döneme ait hazır ekstre projeksiyonu.</param>
    /// <param name="actualPayment">Kullanıcı tarafından karta fiilen ödenen tutar.</param>
    /// <returns>Ödeme sonrası güncellenmiş yeni kredi kartı sözleşmesi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Fiili ödeme tutarı negatif olduğunda fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">Ekstre projeksiyonunda borç tutarı bulunmadığında fırlatılır.</exception>
    public CreditCard Apply(
        CreditCard card,
        CreditCardStatementProjection statement,
        decimal actualPayment)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(statement);

        if (actualPayment < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(actualPayment), "Gerçekleşen ödeme tutarı negatif olamaz.");
        }

        var statementBalance = statement.StatementBalance
            ?? throw new InvalidOperationException("Kart ekstresi için borç tutarı hesaplanamadı.");

        var remainingPrincipal = Math.Max(0m, statementBalance - actualPayment);

        var remainingCharges = card.Charges
            .Where(charge => charge.PostingDate > statement.StatementCloseDate)
            .ToArray();

        var remainingPaymentPlans = card.PaymentPlans
            .Where(plan => plan.DueDate != statement.PaymentDueDate)
            .ToArray();

        return card with
        {
            CarriedBalance = remainingPrincipal,
            UnbilledSpending = 0m,
            BalanceAsOfDate = statement.StatementCloseDate.AddDays(1),
            Charges = remainingCharges,
            PaymentPlans = remainingPaymentPlans,
            CurrentStatement = null,
            CurrentStatementPaymentPlan = null,
            KnownNextStatementDate = card.CurrentStatement?.NextStatementDate,
            KnownNextDueDate = card.CurrentStatement?.NextDueDate
        };
    }

    /// <summary>
    /// Önceden hesaplanmış ekstre projeksiyonları arasından son ödeme tarihine göre ilgili ekstreyi bulur
    /// ve fiili ödemeyi karta uygulayarak güncellenmiş yeni kart durumunu üretir.
    /// </summary>
    /// <param name="card">Mutabakatı yapılacak güncel kredi kartı sözleşmesi.</param>
    /// <param name="statementProjections">Önceden simüle edilmiş ekstre projeksiyonları listesi.</param>
    /// <param name="paymentDueDate">Ödemenin ait olduğu ekstrenin son ödeme tarihi.</param>
    /// <param name="actualPayment">Kullanıcı tarafından karta fiilen ödenen tutar.</param>
    /// <returns>Ödeme sonrası güncellenmiş yeni kredi kartı sözleşmesi.</returns>
    /// <exception cref="InvalidOperationException">Belirtilen vade tarihine uygun ekstre projeksiyonu bulunamadığında fırlatılır.</exception>
    public CreditCard Apply(
        CreditCard card,
        IEnumerable<CreditCardStatementProjection> statementProjections,
        DateOnly paymentDueDate,
        decimal actualPayment)
    {
        ArgumentNullException.ThrowIfNull(statementProjections);

        var matchingStatement = statementProjections
            .SingleOrDefault(x => x.PaymentDueDate == paymentDueDate)
            ?? throw new InvalidOperationException("Kart ödemesinin bağlı olduğu ekstre bulunamadı.");

        return Apply(card, matchingStatement, actualPayment);
    }
}
