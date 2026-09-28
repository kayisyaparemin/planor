using System.Globalization;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kredi, vadeli/taksitli borç planı ve planlı büyük harcama yükümlülüklerinin doğrulanmasını,
/// kalıcı depolara kaydedilmesini, silinmesini ve açık dönem plan revizyonu tetiklenmesini yöneten kullanım senaryosu servisi.
/// </summary>
public sealed class ObligationManagementService(
    ILoanRepository loanRepository,
    ITemporaryPaymentPlanRepository paymentPlanRepository,
    IPlannedLargeExpenseRepository largeExpenseRepository,
    LoanPayoffService loanPayoffService,
    IPlanChangeRecorder planChangeRecorder) : IObligationManagementService
{
    private const string LoanChangeTrigger = "Kredi planı değişti";
    private const string PaymentPlanChangeTrigger = "Planlı ödeme değişti";
    private const string LargeExpenseChangeTrigger = "Büyük ödeme planı değişti";
    private const string InvalidTermsMessage = "Kredi taksiti ve kalan taksit sayısı pozitif olmalıdır.";

    // Kültürden bağımsız sabit desen; ekran biçimi sunum kenarına aittir (S28).
    private const string MessageDateFormat = "dd.MM.yyyy";

    private readonly ILoanRepository _loanRepository =
        loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
    private readonly ITemporaryPaymentPlanRepository _paymentPlanRepository =
        paymentPlanRepository ?? throw new ArgumentNullException(nameof(paymentPlanRepository));
    private readonly IPlannedLargeExpenseRepository _largeExpenseRepository =
        largeExpenseRepository ?? throw new ArgumentNullException(nameof(largeExpenseRepository));
    private readonly LoanPayoffService _loanPayoffService =
        loanPayoffService ?? throw new ArgumentNullException(nameof(loanPayoffService));
    private readonly IPlanChangeRecorder _planChangeRecorder =
        planChangeRecorder ?? throw new ArgumentNullException(nameof(planChangeRecorder));

    /// <inheritdoc />
    public async Task SaveLoanAsync(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(prepayments);
        cancellationToken.ThrowIfCancellationRequested();

        if (loan.MonthlyPayment <= 0m || loan.RemainingInstallmentCount < 1)
        {
            throw new InvalidOperationException(InvalidTermsMessage);
        }

        CalendarRules.ValidateDay(loan.PaymentDay);
        var prepared = _loanPayoffService.PrepareForSave(loan);

        // Erken ödemeler kredinin kaydedilecek hâline göre yeniden doğrulanır; biri reddedilirse hiçbiri yazılmaz.
        var attached = prepayments.Select(p => p with { LoanId = prepared.Id }).ToArray();
        foreach (var prepayment in attached.OrderBy(p => p.Date))
        {
            if (_loanPayoffService.CheckPrepayment(prepared, attached, prepayment) is { } error)
            {
                var date = prepayment.Date.ToString(MessageDateFormat, CultureInfo.InvariantCulture);
                throw new InvalidOperationException($"{date} tarihli erken ödeme: {error}");
            }
        }

        await _loanRepository.UpsertLoanWithPrepaymentsAsync(prepared, attached, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LoanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public LoanPayoffOverview? PreviewLoan(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return IsSavable(loan) ? _loanPayoffService.Preview(loan) : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<PlannedLoanPrepayment> PreviewLoanPrepayments(Loan loan, IReadOnlyList<LoanPrepayment> prepayments)
    {
        ArgumentNullException.ThrowIfNull(loan);
        ArgumentNullException.ThrowIfNull(prepayments);
        return IsSavable(loan)
            ? _loanPayoffService.PreviewPrepayments(loan, prepayments)
            : prepayments.OrderBy(p => p.Date).Select(p => new PlannedLoanPrepayment(loan, p, null, true)).ToArray();
    }

    /// <inheritdoc />
    public string? ValidateLoanPrepayment(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, LoanPrepayment candidate)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return IsSavable(loan) ? _loanPayoffService.CheckPrepayment(loan, prepayments, candidate) : InvalidTermsMessage;
    }

    /// <inheritdoc />
    public async Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _loanRepository.DeleteLoanAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LoanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        if (plan.Installments.Count == 0 || plan.Installments.Any(x => x.Amount <= 0m))
        {
            throw new InvalidOperationException("Ödeme planında en az bir pozitif ödeme olmalıdır.");
        }

        var normalized = ObligationValidation.NormalizePaymentPlan(plan);
        await _paymentPlanRepository.UpsertPaymentPlanAsync(normalized, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(PaymentPlanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _paymentPlanRepository.DeletePaymentPlanAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(PaymentPlanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SavePlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);
        cancellationToken.ThrowIfCancellationRequested();

        if (expense.Amount <= 0m)
        {
            throw new InvalidOperationException("Planlı büyük ödeme tutarı 0'dan büyük olmalı.");
        }

        await _largeExpenseRepository.UpsertPlannedLargeExpenseAsync(expense, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LargeExpenseChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _largeExpenseRepository.DeletePlannedLargeExpenseAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LargeExpenseChangeTrigger, cancellationToken);
    }

    // SaveLoanAsync'in taksit ve gün kapısı; reddedeceği krediyi önizlemeler de çözmez.
    private static bool IsSavable(Loan loan) =>
        loan.MonthlyPayment > 0m && loan.RemainingInstallmentCount >= 1 && loan.PaymentDay is >= 1 and <= 31;
}
