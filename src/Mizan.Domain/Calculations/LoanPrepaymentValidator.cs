using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Bir krediye uygulanan erken kapama veya kısmi ara ödeme taahhütlerinin iş kurallarına,
/// kredi sözleşmesine ve mevcut diğer olaylara uygunluğunu denetleyen saf doğrulayıcı.
/// </summary>
public sealed class LoanPrepaymentValidator(
    LoanAmortizationCalculator amortizationCalculator,
    LoanPaymentScheduleBuilder scheduleBuilder)
{
    /// <summary>
    /// Yeni bir erken ödemenin anlamlı ve geçerli olup olmadığını diğer olaylarla birlikte denetler.
    /// </summary>
    /// <param name="loan">Erken ödemenin ait olduğu kredi sözleşmesi.</param>
    /// <param name="existing">Krediye daha önce kaydedilmiş diğer erken ödeme olayları.</param>
    /// <param name="prepayment">Doğrulanacak yeni erken ödeme taahhüdü.</param>
    /// <exception cref="InvalidOperationException">İş kuralı ihlalinde fırlatılır.</exception>
    public void Validate(Loan? loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(prepayment);

        if (loan is null || !loan.IsActive || loan.RemainingInstallmentCount < 1)
        {
            throw new InvalidOperationException("Erken ödeme için aktif bir kredi seçmelisin.");
        }

        if (amortizationCalculator.Analyze(loan).Amortization is not { } amortization)
        {
            throw new InvalidOperationException(
                "Bu kredinin erken ödeme hesabı için kalan anaparası ya da bankanın tarihli kapatma tutarı gerekli. " +
                "Finansal Yapı'dan krediyi düzenle.");
        }

        if (prepayment.Date < amortization.PreviousDueDate)
        {
            throw new InvalidOperationException(
                $"Erken ödeme tarihi kredinin son ödenen taksitinden ({amortization.PreviousDueDate:dd.MM.yyyy}) önce olamaz.");
        }

        CheckEarlierClosure(loan.Id, existing, prepayment);
        CheckRemainingInstallments(loan, existing, prepayment);

        if (prepayment.Mode != LoanPrepaymentMode.FullClosure)
        {
            ValidatePartialPrepayment(loan, existing, prepayment);
        }
    }

    private static void CheckEarlierClosure(Guid loanId, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        var earlierClosure = existing
            .Where(x => x.LoanId == loanId && x.Id != prepayment.Id && x.Mode == LoanPrepaymentMode.FullClosure)
            .OrderBy(x => x.Date)
            .FirstOrDefault();

        if (earlierClosure is not null && (earlierClosure.Date <= prepayment.Date || prepayment.Mode == LoanPrepaymentMode.FullClosure))
        {
            throw new InvalidOperationException($"Bu kredi {earlierClosure.Date:dd.MM.yyyy} tarihinde zaten kapatılıyor.");
        }
    }

    private void CheckRemainingInstallments(Loan loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        var earlier = existing
            .Where(x => x.LoanId == loan.Id && x.Id != prepayment.Id && x.Date <= prepayment.Date)
            .ToArray();

        var before = scheduleBuilder.Replay(loan, earlier);
        var lastDate = before.LastPaymentDate;
        if (lastDate is null || prepayment.Date >= lastDate)
        {
            throw new InvalidOperationException("Bu tarihte kredinin kapatılacak taksiti kalmıyor. Daha erken bir tarih seç.");
        }
    }

    private void ValidatePartialPrepayment(Loan loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        if (prepayment.PrincipalAmount is not decimal amount || amount <= 0m)
        {
            throw new InvalidOperationException("Ara ödeme tutarı 0'dan büyük olmalı.");
        }

        var earlier = existing
            .Where(x => x.LoanId == loan.Id && x.Id != prepayment.Id && x.Date <= prepayment.Date)
            .ToArray();

        var probe = scheduleBuilder.Replay(loan, [.. earlier, prepayment with { Mode = LoanPrepaymentMode.FullClosure }]);
        if (probe.PrincipalBeforeEvent.TryGetValue(prepayment.Id, out var principal) && amount >= principal)
        {
            throw new InvalidOperationException(
                $"Ara ödeme, o günkü kalan anaparadan ({principal:N0} TL) küçük olmalı. Tamamını ödemek için erken kapamayı seç.");
        }
    }
}
