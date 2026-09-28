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
    /// <see cref="Validate"/>'in kurallarını hata fırlatmadan uygular: erken ödeme kabul edilecekse
    /// null, edilmeyecekse kullanıcıya gösterilecek mesajı döner. Kredi formu bir erken ödemeyi
    /// eklemeden ve kaydetmeden önce aynı kuralı sorar (S64-14).
    /// </summary>
    public string? Check(Loan? loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(prepayment);

        if (loan is null || !loan.IsActive || loan.RemainingInstallmentCount < 1)
        {
            return "Erken ödeme için aktif bir kredi seçmelisin.";
        }

        if (amortizationCalculator.Analyze(loan).Amortization is not { } amortization)
        {
            return "Bu kredinin erken ödeme hesabı için kalan anaparası ya da bankanın tarihli kapatma tutarı gerekli. " +
                   "Finansal Yapı'dan krediyi düzenle.";
        }

        if (prepayment.Date < amortization.PreviousDueDate)
        {
            return $"Erken ödeme tarihi kredinin son ödenen taksitinden ({amortization.PreviousDueDate:dd.MM.yyyy}) önce olamaz.";
        }

        return EarlierClosureError(loan.Id, existing, prepayment)
               ?? RemainingInstallmentsError(loan, existing, prepayment)
               ?? (prepayment.Mode == LoanPrepaymentMode.FullClosure ? null : PartialPrepaymentError(loan, existing, prepayment));
    }

    /// <summary>
    /// Yeni bir erken ödemenin anlamlı ve geçerli olup olmadığını diğer olaylarla birlikte denetler.
    /// </summary>
    /// <param name="loan">Erken ödemenin ait olduğu kredi sözleşmesi.</param>
    /// <param name="existing">Krediye daha önce kaydedilmiş diğer erken ödeme olayları.</param>
    /// <param name="prepayment">Doğrulanacak yeni erken ödeme taahhüdü.</param>
    /// <exception cref="InvalidOperationException">İş kuralı ihlalinde fırlatılır.</exception>
    public void Validate(Loan? loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        if (Check(loan, existing, prepayment) is { } error)
        {
            throw new InvalidOperationException(error);
        }
    }

    private static string? EarlierClosureError(Guid loanId, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        var earlierClosure = existing
            .Where(x => x.LoanId == loanId && x.Id != prepayment.Id && x.Mode == LoanPrepaymentMode.FullClosure)
            .OrderBy(x => x.Date)
            .FirstOrDefault();

        return earlierClosure is not null && (earlierClosure.Date <= prepayment.Date || prepayment.Mode == LoanPrepaymentMode.FullClosure)
            ? $"Bu kredi {earlierClosure.Date:dd.MM.yyyy} tarihinde zaten kapatılıyor."
            : null;
    }

    private string? RemainingInstallmentsError(Loan loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        var earlier = existing
            .Where(x => x.LoanId == loan.Id && x.Id != prepayment.Id && x.Date <= prepayment.Date)
            .ToArray();

        var lastDate = scheduleBuilder.Replay(loan, earlier).LastPaymentDate;
        return lastDate is null || prepayment.Date >= lastDate
            ? "Bu tarihte kredinin kapatılacak taksiti kalmıyor. Daha erken bir tarih seç."
            : null;
    }

    private string? PartialPrepaymentError(Loan loan, IReadOnlyList<LoanPrepayment> existing, LoanPrepayment prepayment)
    {
        if (prepayment.PrincipalAmount is not decimal amount || amount <= 0m)
        {
            return "Ara ödeme tutarı 0'dan büyük olmalı.";
        }

        var earlier = existing
            .Where(x => x.LoanId == loan.Id && x.Id != prepayment.Id && x.Date <= prepayment.Date)
            .ToArray();

        var probe = scheduleBuilder.Replay(loan, [.. earlier, prepayment with { Mode = LoanPrepaymentMode.FullClosure }]);
        return probe.PrincipalBeforeEvent.TryGetValue(prepayment.Id, out var principal) && amount >= principal
            ? $"Ara ödeme, o günkü kalan anaparadan ({principal:N0} TL) küçük olmalı. Tamamını ödemek için erken kapamayı seç."
            : null;
    }
}
