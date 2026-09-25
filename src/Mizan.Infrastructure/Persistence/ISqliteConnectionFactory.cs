using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// SQLite veritabanı bağlantılarını oluşturan, yabancı anahtarları açan
/// ve şemanın v1 olarak hazır olmasını garanti eden bağlantı fabrikası sözleşmesi.
/// </summary>
public interface ISqliteConnectionFactory
{
    /// <summary>
    /// Belirtilen dosya yolu için şeması doğrulanmış ve yabancı anahtarları etkinleştirilmiş bir SQLite bağlantısı üretir.
    /// </summary>
    Task<SQLiteAsyncConnection> CreateConnectionAsync(
        string databasePath,
        CancellationToken cancellationToken = default);
}
