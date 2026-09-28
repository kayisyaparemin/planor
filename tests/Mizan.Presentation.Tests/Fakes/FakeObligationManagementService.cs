using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Kredi, ödeme planı ve büyük harcama yazma portunun sahtesi: kaydedilen kredileri ve erken
/// ödemelerini, silinen kimlikleri, önizlenen taslakları ve doğrulanan adayları tutar; istenirse
/// kaydetmede ya da silmede hata fırlatır. Önizlemelerin ve doğrulamanın cevabı testten verilir.
/// </summary>
internal sealed class FakeObligationManagementService : IObligationManagementService
{
    public List<Loan> SavedLoans { get; } = [];

    /// <summary>Her kayıtta krediyle birlikte verilen erken ödemeler, sırayla.</summary>
    public List<IReadOnlyList<LoanPrepayment>> SavedPrepayments { get; } = [];

    public List<Guid> DeletedLoanIds { get; } = [];
    public List<Guid> DeletedPaymentPlanIds { get; } = [];
    public List<Guid> DeletedLargeExpenseIds { get; } = [];
    public Exception? SaveException { get; set; }
    public Exception? DeleteException { get; set; }

    /// <summary>Önizlemeye verilen taslaklar, sırayla.</summary>
    public List<Loan> PreviewedLoans { get; } = [];

    /// <summary>Önizlemenin cevabı; varsayılan "kayıt reddeder" (null).</summary>
    public Func<Loan, LoanPayoffOverview?> Preview { get; set; } = _ => null;

    /// <summary>Erken ödeme önizlemesine verilen taslaklar, sırayla.</summary>
    public List<Loan> PrepaymentPreviewLoans { get; } = [];

    /// <summary>Erken ödeme tutarı; varsayılan "hesaplanamaz" (null).</summary>
    public Func<Loan, LoanPrepayment, decimal?> PrepaymentAmount { get; set; } = (_, _) => null;

    /// <summary>Doğrulamaya verilen (taslak, aday) çiftleri, sırayla.</summary>
    public List<(Loan Loan, LoanPrepayment Candidate)> ValidatedCandidates { get; } = [];

    /// <summary>Doğrulamanın cevabı; varsayılan "kabul" (null).</summary>
    public Func<LoanPrepayment, string?> Validate { get; set; } = _ => null;

    public LoanPayoffOverview? PreviewLoan(Loan loan)
    {
        PreviewedLoans.Add(loan);
        return Preview(loan);
    }

    public IReadOnlyList<PlannedLoanPrepayment> PreviewLoanPrepayments(Loan loan, IReadOnlyList<LoanPrepayment> prepayments)
    {
        PrepaymentPreviewLoans.Add(loan);
        return prepayments
            .OrderBy(p => p.Date)
            .Select(p => new PlannedLoanPrepayment(loan, p, PrepaymentAmount(loan, p), false))
            .ToArray();
    }

    public string? ValidateLoanPrepayment(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, LoanPrepayment candidate)
    {
        ValidatedCandidates.Add((loan, candidate));
        return Validate(candidate);
    }

    public Task SaveLoanAsync(Loan loan, IReadOnlyList<LoanPrepayment> prepayments, CancellationToken cancellationToken = default)
    {
        if (SaveException is not null)
        {
            return Task.FromException(SaveException);
        }

        SavedLoans.Add(loan);
        SavedPrepayments.Add(prepayments);
        return Task.CompletedTask;
    }

    public Task DeleteLoanAsync(Guid id, CancellationToken cancellationToken = default) => Delete(DeletedLoanIds, id);

    public Task SavePaymentPlanAsync(TemporaryPaymentPlan plan, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeletePaymentPlanAsync(Guid id, CancellationToken cancellationToken = default) => Delete(DeletedPaymentPlanIds, id);

    public Task SavePlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default) =>
        Delete(DeletedLargeExpenseIds, id);

    private Task Delete(List<Guid> deleted, Guid id)
    {
        if (DeleteException is not null)
        {
            return Task.FromException(DeleteException);
        }

        deleted.Add(id);
        return Task.CompletedTask;
    }
}
