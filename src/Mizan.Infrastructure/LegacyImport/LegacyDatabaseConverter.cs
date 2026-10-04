using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;
using SQLite;

namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski şema v17 veritabanını bu uygulamanın şemasına çevirir. Eski dosya yükseltilmez, okunur: hedef boş
/// bir v1 veritabanıdır, eski dosya ona bağlanıp tek işlemde kopyalanır, sonra normal göç adımları
/// hedefi güncel sürüme getirir (S83-1). Hedefin v1 olması dönüşümü gelecekteki şema adımlarından bağımsız
/// kılar: v1 dondurulmuştur.
/// </summary>
internal static class LegacyDatabaseConverter
{
    private static readonly IReadOnlyList<string> Statements =
    [
        .. LegacyIncomeStatements.Commands,
        .. LegacyObligationStatements.Commands,
        .. LegacyPlanStatements.Commands,
        .. LegacyActualStatements.Commands
    ];

    /// <summary>
    /// <paramref name="legacyPath"/>'teki eski veritabanını okuyup <paramref name="targetPath"/>'te
    /// <paramref name="schema"/>'nın güncel sürümünde yeni bir veritabanı yazar. Eski dosya desteklenen
    /// sürümde değilse ya da dönüşüm düşerse kullanıcıya gidecek mesajla
    /// <see cref="InvalidOperationException"/> fırlatır.
    /// </summary>
    public static async Task ConvertAsync(
        string legacyPath,
        string targetPath,
        DatabaseSchema schema,
        string profileName,
        CancellationToken cancellationToken)
    {
        EnsureSupportedVersion(legacyPath, profileName);
        try
        {
            await RunAsync(targetPath, new DatabaseSchema([SchemaMigrations.All[0]]), cancellationToken);
            CopyData(legacyPath, targetPath);
            await RunAsync(targetPath, schema, cancellationToken);
        }
        catch (Exception exception) when (exception is SQLiteException or InvalidOperationException)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profileName}\" profilinin verisi dönüştürülemedi.", exception);
        }
    }

    private static void EnsureSupportedVersion(string legacyPath, string profileName)
    {
        int? version;
        try
        {
            using var connection = new SQLiteConnection(legacyPath, SQLiteOpenFlags.ReadOnly);
            if (!string.Equals(connection.ExecuteScalar<string>("PRAGMA quick_check;"), "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw BackupRestoreErrors.Corrupt($"\"{profileName}\" profilinin verisi bozuk.");
            }

            version = connection.ExecuteScalar<int?>("SELECT SchemaVersion FROM settings ORDER BY Id LIMIT 1;");
        }
        catch (SQLiteException exception)
        {
            throw BackupRestoreErrors.Corrupt($"\"{profileName}\" profilinin verisi okunamadı.", exception);
        }

        if (version > LegacySchemaV17.SupportedVersion)
        {
            throw BackupRestoreErrors.NewerVersion();
        }

        if (version != LegacySchemaV17.SupportedVersion)
        {
            throw BackupRestoreErrors.Corrupt(
                $"\"{profileName}\" profilinin verisi eski uygulamanın desteklenen sürümünde değil. " +
                "Eski uygulamayı açıp yeni bir yedek al.");
        }
    }

    private static async Task RunAsync(string path, DatabaseSchema schema, CancellationToken cancellationToken)
    {
        var connection = new SQLiteAsyncConnection(path);
        try
        {
            await schema.EnsureInitializedAsync(connection, cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Eski veritabanını hedefe takıp bütün kopyalamayı tek işlemde yapar; bir ifade düşerse hedefte hiçbir
    /// satır kalmaz. Yabancı anahtar denetimi açıktır: eski şemada zorlanmayan ilişkilerden doğan yetim satırlar
    /// ifadelerde elenir, geriye kalan her satır yeni şemanın kısıtlarına uymak zorundadır.
    /// </summary>
    private static void CopyData(string legacyPath, string targetPath)
    {
        using var connection = new SQLiteConnection(targetPath);
        connection.Execute("PRAGMA foreign_keys = ON;");
        connection.Execute($"ATTACH DATABASE ? AS {LegacySchemaV17.AttachedAlias};", legacyPath);
        connection.RunInTransaction(() =>
        {
            foreach (var statement in Statements)
            {
                connection.Execute(statement);
            }
        });
        connection.Execute($"DETACH DATABASE {LegacySchemaV17.AttachedAlias};");
    }
}
