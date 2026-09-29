using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Bir profil veritabanını açılırken bu uygulamanın şema sürümüne getiren kapı. Her profil kendisini
/// oluşturan uygulama sürümünün şeklini taşır; açılışta sürümü (<c>PRAGMA user_version</c>) okunur ve
/// eksik göç adımları çalıştırılır. Boş veritabanı da 0'dan başlayan aynı yoldan kurulur (S69).
/// </summary>
public sealed class DatabaseSchema
{
    private readonly IReadOnlyList<SchemaMigration> _migrations;

    /// <summary>
    /// Üretimdeki göç listesiyle (<see cref="SchemaMigrations.All"/>) kurar.
    /// </summary>
    public DatabaseSchema()
        : this(SchemaMigrations.All)
    {
    }

    /// <summary>
    /// Verilen göç listesiyle kurar; testler üretim listesine sahte adımlar ekleyerek yükseltmeyi dener.
    /// Sürümleri 1'den başlayıp birer birer artmayan liste reddedilir: arada eksik bir adım, o sürümdeki
    /// veritabanlarının hiç yükseltilemeyeceği demektir.
    /// </summary>
    public DatabaseSchema(IReadOnlyList<SchemaMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(migrations);
        var isContiguous = migrations.Select((migration, index) => migration.Version == index + 1).All(ok => ok);
        if (migrations.Count == 0 || !isContiguous)
        {
            throw new ArgumentException(
                "Göç adımlarının sürümleri 1'den başlayıp birer birer artmalı.",
                nameof(migrations));
        }

        _migrations = migrations;
    }

    /// <summary>
    /// Bu şemanın getirdiği sürüm: göç listesinin son adımı.
    /// </summary>
    public int CurrentVersion => _migrations[^1].Version;

    /// <summary>
    /// Yabancı anahtarları açar ve veritabanını <see cref="CurrentVersion"/>'a getirir: sürümü eksik
    /// adımları tek işlemde çalıştırır, güncelse dokunmaz. Daha yeni bir sürümü reddeder; o dosyayı
    /// bu uygulama anlayamaz.
    /// </summary>
    public async Task EnsureInitializedAsync(
        SQLiteAsyncConnection connection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await EnableForeignKeysAsync(connection, cancellationToken);

        var version = await GetUserVersionAsync(connection, cancellationToken);
        if (version > CurrentVersion)
        {
            throw new InvalidOperationException(
                $"Veritabanı şema sürümü ({version}) bu uygulamanın desteklediği sürümden ({CurrentVersion}) daha yenidir.");
        }

        // Sürüm N, 1..N adımlarının çalışmış olduğu demektir; boş veritabanı 0'dır ve v1'den başlar.
        var pending = _migrations.Where(migration => migration.Version > version).ToArray();
        if (pending.Length > 0)
        {
            await SchemaMigrationRunner.RunAsync(connection, pending, cancellationToken);
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
}
