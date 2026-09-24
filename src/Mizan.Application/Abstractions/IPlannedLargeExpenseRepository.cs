using Mizan.Domain.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Gelecekte tek seferde gerçekleşmesi beklenen planlı büyük harcamaların
/// kalıcı veri deposuna erişimini ve yönetimini sağlayan dar port arayüzü.
/// </summary>
public interface IPlannedLargeExpenseRepository
{
    /// <summary>
    /// Kayıtlı tüm planlı büyük harcamaları listeler.
    /// </summary>
    Task<IReadOnlyList<PlannedLargeExpense>> GetPlannedLargeExpensesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir planlı büyük harcamayı kaydeder veya günceller.
    /// </summary>
    Task UpsertPlannedLargeExpenseAsync(PlannedLargeExpense expense, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirtilen planlı büyük harcamayı kalıcı olarak siler.
    /// </summary>
    Task DeletePlannedLargeExpenseAsync(Guid id, CancellationToken cancellationToken = default);
}
