using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Krediler, kredi erken ödemeleri, vadeli borç planları ve kart ödemelerini tek bir yükümlülük listesinde
/// birleştiren ve dönem bazlı zorunlu ödeme özetini hesaplayan saf Domain motoru.
/// </summary>
public sealed class MandatoryPaymentCalculator(
    LoanPaymentScheduleBuilder loanScheduleBuilder,
    ScheduledPaymentCalculator scheduledPaymentCalculator)
{
    private readonly LoanPaymentScheduleBuilder _loanScheduleBuilder =
        loanScheduleBuilder ?? throw new ArgumentNullException(nameof(loanScheduleBuilder));

    private readonly ScheduledPaymentCalculator _scheduledPaymentCalculator =
        scheduledPaymentCalculator ?? throw new ArgumentNullException(nameof(scheduledPaymentCalculator));

    /// <summary>
    /// Krediler, vadeli planlar ve kart ödemelerini erken ödeme olayları olmaksızın birleştirir.
    /// </summary>
    public IReadOnlyList<ObligationItem> BuildObligations(
        IEnumerable<Loan> loans,
        IEnumerable<TemporaryPaymentPlan> plans,
        IEnumerable<ObligationItem> creditCardPayments) =>
        BuildObligations(loans, [], plans, creditCardPayments);

    /// <summary>
    /// Krediler, erken ödeme olayları, vadeli planlar ve kart ödemelerini kronolojik sırada birleştirir.
    /// </summary>
    public IReadOnlyList<ObligationItem> BuildObligations(
        IEnumerable<Loan> loans,
        IEnumerable<LoanPrepayment> prepayments,
        IEnumerable<TemporaryPaymentPlan> plans,
        IEnumerable<ObligationItem> creditCardPayments)
    {
        ArgumentNullException.ThrowIfNull(loans);
        ArgumentNullException.ThrowIfNull(prepayments);
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(creditCardPayments);

        var items = new List<ObligationItem>();
        var events = prepayments.ToArray();

        foreach (var loan in loans.Where(x => x.IsActive))
        {
            AppendLoanObligations(loan, events, items);
        }

        items.AddRange(_scheduledPaymentCalculator.GetItems(plans));
        items.AddRange(creditCardPayments);

        return items.OrderBy(x => x.DueDate).ThenBy(x => x.Name).ToArray();
    }

    /// <summary>
    /// Verilen yükümlülük kalemlerini kategorilerine göre toplayarak zorunlu ödeme özetini oluşturur.
    /// Büyük harcama kalemleri zorunlu ödeme toplamına dahil edilmez.
    /// </summary>
    public MandatoryPaymentSummary Summarize(IEnumerable<ObligationItem> assignedItems)
    {
        ArgumentNullException.ThrowIfNull(assignedItems);

        var ordered = assignedItems
            .Where(x => x.Type != ObligationType.PlannedLargeExpense)
            .OrderBy(x => x.DueDate)
            .ThenBy(x => x.Name)
            .ToArray();

        decimal Sum(ObligationType type) =>
            ordered.Where(x => x.Type == type).Sum(x => x.Amount);

        var loanPayments = Sum(ObligationType.Loan);
        var cardPayments = Sum(ObligationType.CreditCard);
        var temporaryPayments = Sum(ObligationType.TemporaryPayment);
        var installmentPayments = Sum(ObligationType.InstallmentPayment);
        var otherPayments = Sum(ObligationType.OtherScheduledPayment);
        var total = loanPayments + cardPayments + temporaryPayments + installmentPayments + otherPayments;

        return new MandatoryPaymentSummary
        {
            Items = ordered,
            LoanPayments = loanPayments,
            CreditCardPayments = cardPayments,
            TemporaryPayments = temporaryPayments,
            InstallmentPayments = installmentPayments,
            OtherScheduledPayments = otherPayments,
            Total = total
        };
    }

    private void AppendLoanObligations(Loan loan, LoanPrepayment[] events, List<ObligationItem> items)
    {
        var name = $"{loan.Bank} {loan.Name}".Trim();
        var replay = _loanScheduleBuilder.Replay(loan, events);

        foreach (var payment in replay.Payments)
        {
            var isInstallment = payment.Kind == LoanPaymentKind.Installment;
            var paymentName = payment.Kind switch
            {
                LoanPaymentKind.EarlyClosure => $"{name} · erken kapama",
                LoanPaymentKind.PartialPrepayment => $"{name} · ara ödeme",
                _ => name
            };

            items.Add(new ObligationItem(paymentName, ObligationType.Loan, payment.Date, payment.Amount)
            {
                IsFinalPayment = payment.IsFinal,
                Detail = isInstallment ? string.Empty : "Erken ödeme: anapara + işleyen faiz",
                PaymentId = payment.SourceId
            });
        }
    }
}
