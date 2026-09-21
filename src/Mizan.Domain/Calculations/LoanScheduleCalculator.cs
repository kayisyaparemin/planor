using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Bir kredinin kalan taksit sayısına ve ödeme gününe göre gelecekteki tüm ödeme tarihlerini
/// takvim kurallarına (<see cref="CalendarRules"/>) uygun olarak türeten saf ödeme takvimi hesaplayıcısı.
/// </summary>
public sealed class LoanScheduleCalculator
{
    /// <summary>
    /// Verilen kredinin <see cref="Loan.NextPaymentDate"/> tarihinden başlayarak,
    /// kalan taksit adedi kadar ödeme vadesini üretir.
    /// </summary>
    /// <param name="loan">Ödeme takvimi çıkarılacak kredi sözleşmesi.</param>
    /// <returns>Kronolojik taksit ödeme tarihleri listesi.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loan"/> null olduğunda fırlatılır.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Ödeme günü veya taksit tutarı geçersiz olduğunda fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">Sonraki ödeme tarihi tanımlanmamışsa fırlatılır.</exception>
    public IReadOnlyList<DateOnly> GetPaymentDates(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);
        CalendarRules.ValidateDay(loan.PaymentDay);

        if (loan.MonthlyPayment <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(loan), "Kredi taksiti sıfırdan büyük olmalıdır.");
        }

        if (loan.NextPaymentDate == default)
        {
            throw new InvalidOperationException("Kredinin ilk veya sonraki ödeme tarihi gereklidir.");
        }

        if (loan.RemainingInstallmentCount < 1)
        {
            return [];
        }

        return Enumerable.Range(0, loan.RemainingInstallmentCount)
            .Select(index => index == 0
                ? loan.NextPaymentDate
                : CalendarRules.AddMonthsKeepingDay(
                    loan.NextPaymentDate,
                    index,
                    loan.PaymentDay))
            .ToArray();
    }
}
