using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Yeni oluşturulan veya dondurulan finansal durum anlık görüntüsünü (FinancialSnapshot),
/// buna bağlı kilitlenen dönem planını (PeriodPlanSnapshot) ve güncellenen kullanıcı ayarlarını
/// (UserSettings) tek bir atomik paket olarak sunan veri transfer nesnesidir.
/// </summary>
public sealed record FinancialSnapshotBundle(
    FinancialSnapshot Snapshot,
    PeriodPlanSnapshot Plan,
    UserSettings UpdatedSettings);
