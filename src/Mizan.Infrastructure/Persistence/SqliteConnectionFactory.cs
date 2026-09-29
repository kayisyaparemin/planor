using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// SQLite veritabanı bağlantılarını oluşturan, yerel SQLite kütüphanesini başlatan,
/// yabancı anahtarları etkinleştiren ve şemayı güncel sürüme getiren somut fabrika sınıfı.
/// </summary>
public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private static readonly object InitLock = new();
    private static bool _sqliteInitialized;

    private readonly DatabaseSchema _schema;

    /// <summary>
    /// Varsayılan şema yöneticisi ile bağlantı fabrikasını ilklendirir.
    /// </summary>
    public SqliteConnectionFactory(DatabaseSchema? schema = null)
    {
        _schema = schema ?? new DatabaseSchema();
    }

    /// <inheritdoc />
    public async Task<SQLiteAsyncConnection> CreateConnectionAsync(
        string databasePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Veritabanı dosya yolu boş olamaz.", nameof(databasePath));
        }

        EnsureNativeSqliteInitialized();

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connection = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache);

        await _schema.EnsureInitializedAsync(connection, cancellationToken);
        return connection;
    }

    private static void EnsureNativeSqliteInitialized()
    {
        if (_sqliteInitialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (_sqliteInitialized)
            {
                return;
            }

            SQLitePCL.Batteries_V2.Init();
            _sqliteInitialized = true;
        }
    }
}
