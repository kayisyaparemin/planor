using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Bir nakit akış döneminin kapatılmaya hazır olup olmadığını belirten durum sözleşmesidir.
/// Kullanıcının açıkta bekleyen bir dönemi olup olmadığını ve mutabakat gününün gelip gelmediğini sunar.
/// Ekran metni taşımaz (S36); sunum katmanı bu verileri kullanarak arayüz metnini üretir.
/// </summary>
public sealed record PeriodSettlementAvailability
{
    /// <summary>Kayıtlı güncel bir finansal durum anlık görüntüsünün var olup olmadığını belirtir.</summary>
    public required bool HasCurrentSnapshot { get; init; }

    /// <summary>Mutabakat tarihinin gelip gelmediğini (dönemin kapatılmaya hazır olduğunu) belirtir.</summary>
    public required bool IsDue { get; init; }

    /// <summary>Sistemdeki yürürlükte olan güncel finansal durum görüntüsü.</summary>
    public FinancialSnapshot? CurrentSnapshot { get; init; }

    /// <summary>Kapatılmayı bekleyen açık dönem planı; mutabakat günü gelmediyse veya açık dönem yoksa null olabilir.</summary>
    public PeriodPlanSnapshot? PendingPlan { get; init; }

    /// <summary>Güncel durumun en son kaydedildiği tarih.</summary>
    public DateOnly? LastUpdatedDate => CurrentSnapshot?.SnapshotDate;
}
