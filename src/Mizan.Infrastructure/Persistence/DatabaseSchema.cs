using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// SQLite veritabanı şema kurulumunu, sürüm yönetimini (PRAGMA user_version)
/// ve yabancı anahtar kısıtlarını (PRAGMA foreign_keys) koordine eden temel şema sınıfı.
/// </summary>
public sealed class DatabaseSchema
{
    /// <summary>
    /// Veritabanı bağlantısında yabancı anahtarları etkinleştirir ve şema v1'in hazır olmasını sağlar.
    /// Şema sürümü 0 ise tüm tabloları oluşturup sürümü 1 yapar; sürüm 1 ise DDL çalıştırmadan döner.
    /// </summary>
    public async Task EnsureInitializedAsync(
        SQLiteAsyncConnection connection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await EnableForeignKeysAsync(connection, cancellationToken);

        var version = await GetUserVersionAsync(connection, cancellationToken);
        if (version == 0)
        {
            await CreateAllTablesAsync(connection);
            await SetUserVersionAsync(connection, DatabaseConstants.CurrentSchemaVersion, cancellationToken);
            return;
        }

        if (version > DatabaseConstants.CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Veritabanı şema sürümü ({version}) bu uygulamanın desteklediği sürümden ({DatabaseConstants.CurrentSchemaVersion}) daha yenidir.");
        }
    }

    /// <summary>
    /// Veritabanı bağlantısında SQLite yabancı anahtar (foreign key) denetimini etkinleştirir.
    /// </summary>
    public Task EnableForeignKeysAsync(
        SQLiteAsyncConnection connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
    }

    /// <summary>
    /// SQLite PRAGMA user_version değerini sorgular.
    /// </summary>
    public Task<int> GetUserVersionAsync(
        SQLiteAsyncConnection connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return connection.ExecuteScalarAsync<int>("PRAGMA user_version;");
    }

    /// <summary>
    /// SQLite PRAGMA user_version değerini günceller.
    /// </summary>
    public Task SetUserVersionAsync(
        SQLiteAsyncConnection connection,
        int version,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return connection.ExecuteAsync($"PRAGMA user_version = {version};");
    }

    /// <summary>
    /// Veritabanında kayıtlı tüm kullanıcı tanımlı tablo isimlerini alfabetik sırada getirir.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetTableNamesAsync(
        SQLiteAsyncConnection connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        const string query = """
            SELECT name FROM sqlite_master
            WHERE type = 'table' AND name NOT LIKE 'sqlite_%'
            ORDER BY name;
            """;
        var rows = await connection.QueryScalarsAsync<string>(query);
        return rows;
    }

    private static async Task CreateAllTablesAsync(SQLiteAsyncConnection connection)
    {
        var allCommands = SchemaIncomeLoanTables.Commands
            .Concat(SchemaCardTables.Commands)
            .Concat(SchemaSnapshotTables.Commands)
            .Concat(SchemaActualAndObservationTables.Commands);

        foreach (var sql in allCommands)
        {
            await connection.ExecuteAsync(sql);
        }
    }
}
