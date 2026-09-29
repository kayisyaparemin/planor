namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Uygulamanın şema tarihçesi: v1'den bugüne her adım, sırasıyla. Güncel sürüm ayrı bir sabitte değil
/// listenin son adımında durur; ikisi ayrı yerde dursaydı birini güncelleyip ötekini unutmak sürüm
/// numarasını tabloların şeklinden koparırdı (S69).
/// </summary>
public static class SchemaMigrations
{
    /// <summary>
    /// Üretimdeki adımlar. v1 temiz şemanın 30 tablosudur (S53) ve dondurulmuştur; bugünkü şema,
    /// v1 ile ondan sonraki adımların toplamıdır.
    /// </summary>
    public static IReadOnlyList<SchemaMigration> All { get; } = [V1(), V2(), V3()];

    /// <summary>
    /// Bu uygulamanın kurduğu ve açabildiği en yeni şema sürümü.
    /// </summary>
    public static int CurrentVersion => All[^1].Version;

    private static SchemaMigration V1() =>
        new(1,
        [
            .. SchemaIncomeLoanTables.Commands,
            .. SchemaCardTables.Commands,
            .. SchemaSnapshotTables.Commands,
            .. SchemaActualAndObservationTables.Commands
        ]);

    /// <summary>
    /// v2, ödeme işaretini gözlemin çocuğu olmaktan çıkarıp plana bağlar (S68-8). Yeni tablo kurulur,
    /// eski tablonun satırları gözlemin planıyla birlikte kopyalanır, sonra eskisi düşer; kopyalamadan
    /// önce düşürmek işaretleri silerdi. Gözlem tablosuna dokunulmaz.
    /// </summary>
    private static SchemaMigration V2() =>
        new(2,
        [
            """
            CREATE TABLE period_payment_marks (
                Id TEXT PRIMARY KEY NOT NULL,
                PeriodPlanSnapshotId TEXT NOT NULL,
                PeriodPlanPaymentLineId TEXT NOT NULL,
                Status INTEGER NOT NULL,
                ActualAmount decimal NOT NULL,
                ActualPaymentDate TEXT,
                Note TEXT NOT NULL,
                FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE,
                UNIQUE (PeriodPlanSnapshotId, PeriodPlanPaymentLineId)
            );
            """,
            """
            INSERT INTO period_payment_marks (Id, PeriodPlanSnapshotId, PeriodPlanPaymentLineId, Status, ActualAmount, ActualPaymentDate, Note)
            SELECT p.Id, o.PeriodPlanSnapshotId, p.PeriodPlanPaymentLineId, p.Status, p.ActualAmount, p.ActualPaymentDate, p.Note
            FROM period_observation_payments p
            JOIN period_observations o ON o.Id = p.PeriodObservationId;
            """,
            """
            DROP TABLE period_observation_payments;
            """
        ]);

    /// <summary>
    /// v3, gözlemi yeni şekline getirir (S68-2, S68-9): (plan, gün) tektir, bakiye zorunludur, hesaplanmış
    /// yaşam harcaması ile not gider, iki damga tek kayıt zamanı olur. UNIQUE ancak tablo yeniden kurularak
    /// eklenir: yenisi kurulur, bakiyesi olan satırlar kopyalanır, eskisi düşer, yenisinin adı değişir. Ödeme
    /// işaretleri v2'de gözlemden ayrıldığı için eski tabloya bağlı çocuk kalmadı. Ayrı bir plan indeksi kurulmaz:
    /// UNIQUE'in öneki plan kimliğini zaten indeksler.
    /// </summary>
    private static SchemaMigration V3() =>
        new(3,
        [
            """
            CREATE TABLE period_observations_new (
                Id TEXT PRIMARY KEY NOT NULL,
                PeriodPlanSnapshotId TEXT NOT NULL,
                ObservedOn TEXT NOT NULL,
                ObservedBalance decimal NOT NULL,
                RecordedAtUtc TEXT NOT NULL,
                FOREIGN KEY (PeriodPlanSnapshotId) REFERENCES period_plan_snapshots (Id) ON DELETE CASCADE,
                UNIQUE (PeriodPlanSnapshotId, ObservedOn)
            );
            """,
            """
            INSERT INTO period_observations_new (Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, RecordedAtUtc)
            SELECT Id, PeriodPlanSnapshotId, ObservedOn, ObservedBalance, UpdatedAtUtc
            FROM period_observations
            WHERE ObservedBalance IS NOT NULL;
            """,
            """
            DROP TABLE period_observations;
            """,
            """
            ALTER TABLE period_observations_new RENAME TO period_observations;
            """
        ]);
}
