using Mizan.Application.Abstractions;
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
    public async Task SaveLoanAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loan);
        cancellationToken.ThrowIfCancellationRequested();

        if (loan.MonthlyPayment <= 0m || loan.RemainingInstallmentCount < 1)
        {
            throw new InvalidOperationException("Kredi taksiti ve kalan taksit sayısı pozitif olmalıdır.");
        }

        CalendarRules.ValidateDay(loan.PaymentDay);
        var prepared = _loanPayoffService.PrepareForSave(loan);

        await _loanRepository.UpsertLoanAsync(prepared, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LoanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _loanRepository.DeleteLoanAsync(id, cancellationToken);
        await _planChangeRecorder.RecordChangeAsync(LoanChangeTrigger, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteLoanPrepaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _loanRepository.DeleteLoanPrepaymentAsync(id, cancellationToken);
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
}
