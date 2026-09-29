using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Açık dönemin bir ödeme satırına konmuş işaretin SQLite tablo varlığı. Plana bağlıdır, gözleme değil (S68-8);
/// satır başına tek işaret kuralı tablonun <c>UNIQUE (PeriodPlanSnapshotId, PeriodPlanPaymentLineId)</c>
/// kısıtındadır.
/// </summary>
[Table("period_payment_marks")]
internal sealed class PeriodPaymentMarkEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string PeriodPlanSnapshotId { get; set; } = string.Empty;

    public string PeriodPlanPaymentLineId { get; set; } = string.Empty;

    public int Status { get; set; }

    public decimal ActualAmount { get; set; }

    public string? ActualPaymentDate { get; set; }

    public string Note { get; set; } = string.Empty;
}
