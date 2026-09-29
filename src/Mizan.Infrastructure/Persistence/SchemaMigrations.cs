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
    public static IReadOnlyList<SchemaMigration> All { get; } = [V1()];

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
}
