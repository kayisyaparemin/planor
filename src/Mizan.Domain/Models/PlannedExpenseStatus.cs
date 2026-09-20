namespace Mizan.Domain.Models;

/// <summary>
/// Planlanan büyük harcamanın yaşam döngüsü durumu.
/// </summary>
public enum PlannedExpenseStatus
{
    /// <summary>Harcama planlandı, vadesi bekleniyor.</summary>
    Planned = 0,
    /// <summary>Harcama fiilen gerçekleşti.</summary>
    Completed = 1,
    /// <summary>Harcama planı iptal edildi.</summary>
    Cancelled = 2
}
