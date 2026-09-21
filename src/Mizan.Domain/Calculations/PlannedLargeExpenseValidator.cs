using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Planlanan büyük harcama modelinin iş kurallarına ve veri bütünlüğüne uygunluğunu doğrulayan saf denetleyici.
/// </summary>
public static class PlannedLargeExpenseValidator
{
    /// <summary>
    /// Harcama adı, tutarı, tarihi ve durumunun geçerliliğini doğrular.
    /// </summary>
    /// <param name="expense">Doğrulanacak planlanan harcama.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expense"/> null ise fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">Harcama alanları iş kurallarına aykırı ise fırlatılır.</exception>
    public static void Validate(PlannedLargeExpense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);

        if (string.IsNullOrWhiteSpace(expense.Name))
        {
            throw new InvalidOperationException("Planlanan harcama adı boş olamaz.");
        }

        if (expense.Amount <= 0m)
        {
            throw new InvalidOperationException("Planlanan harcama tutarı sıfırdan büyük olmalıdır.");
        }

        if (expense.ExactDate == default)
        {
            throw new InvalidOperationException("Harcama tarihi geçersiz.");
        }

        if (!Enum.IsDefined(expense.Status))
        {
            throw new InvalidOperationException("Harcama durumu geçersiz.");
        }
    }
}
