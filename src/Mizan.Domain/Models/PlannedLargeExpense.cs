namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının belirli bir tarihte yapmayı planladığı tek seferlik büyük nakit çıkışını temsil eder.
/// Nakit akış projeksiyonunda ilgili dönemin harcamalarına dahil edilerek likidite açığını önceden görmeyi sağlar.
/// </summary>
public sealed record PlannedLargeExpense
{
    /// <summary>Harcamanın benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Harcamanın adı veya başlığı (örn. Yaz Tatili, Beyaz Eşya).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Planlanan harcama tutarı.</summary>
    public decimal Amount { get; init; }

    /// <summary>Harcamanın yapılmasının planlandığı kesin tarih.</summary>
    public DateOnly ExactDate { get; init; }

    /// <summary>Harcamaya ilişkin kullanıcı notu veya açıklama.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Harcamanın yaşam döngüsü durumu.</summary>
    public PlannedExpenseStatus Status { get; init; } = PlannedExpenseStatus.Planned;

    /// <summary>
    /// Harcamanın aktif olarak plan aşamasında olup olmadığını belirler.
    /// Yalnızca <see cref="PlannedExpenseStatus.Planned"/> durumundaki harcamalar nakit projeksiyonunu etkiler.
    /// </summary>
    public bool IsActive => Status == PlannedExpenseStatus.Planned;
}
