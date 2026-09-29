using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Bekleyen göç adımlarını tek işlemde çalıştırır: bir adım düşerse veritabanı açılıştaki sürümünde,
/// dokunulmamış kalır (S69). Yabancı anahtar denetimi işlemden önce kapatılır, çünkü bir tablonun
/// şeklini değiştirmenin tek güvenli yolu onu yeniden kurmaktır — yenisini kur, veriyi taşı, eskisini
/// sil, yenisinin adını değiştir — ve denetim açıkken eskisini silmek <c>ON DELETE CASCADE</c> ile çocuk
/// kayıtları götürür. Sıra önemlidir: önce eskisinin adı değiştirilirse çocukların bağı eski tabloya
/// çevrilir. Bağlar işlem kapanmadan topluca denetlenir.
/// </summary>
internal static class SchemaMigrationRunner
{
    /// <summary>
    /// <paramref name="pending"/>'deki adımları sırayla çalıştırır ve <c>user_version</c>'ı sonuncusunun
    /// sürümüne çeker; hepsi tek işlemdir. Göçten sonra kopuk bir bağ kalırsa hiçbir şey yazılmaz.
    /// Yabancı anahtar denetimi sonunda, adım düşse de, yeniden açılır.
    /// </summary>
    public static async Task RunAsync(
        SQLiteAsyncConnection connection,
        IReadOnlyList<SchemaMigration> pending,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // SQLite bu ayarı açık bir işlemin içinde sessizce yok sayar; işlem başlamadan kapatılır.
        await connection.ExecuteAsync("PRAGMA foreign_keys = OFF;");
        try
        {
            await connection.RunInTransactionAsync(transaction => Apply(transaction, pending));
        }
        finally
        {
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }
    }

    private static void Apply(SQLiteConnection transaction, IReadOnlyList<SchemaMigration> pending)
    {
        foreach (var command in pending.SelectMany(migration => migration.Commands))
        {
            transaction.Execute(command);
        }

        var targetVersion = pending[^1].Version;
        var brokenReferences = transaction.ExecuteScalar<int>("SELECT COUNT(*) FROM pragma_foreign_key_check;");
        if (brokenReferences > 0)
        {
            throw new InvalidOperationException(
                $"Veritabanı v{targetVersion} sürümüne yükseltilemedi: {brokenReferences} kayıt bağlı olduğu " +
                "kaydı kaybediyordu. Hiçbir değişiklik yazılmadı.");
        }

        transaction.Execute($"PRAGMA user_version = {targetVersion};");
    }
}
