using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Birim testlerinde ve doğrudan bağlantı senaryolarında sabit bir SQLite bağlantısı sunan sağlayıcı.
/// </summary>
public sealed class StaticSqliteConnectionProvider(SQLiteAsyncConnection connection) : ISqliteConnectionProvider
{
    private readonly SQLiteAsyncConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    /// <inheritdoc />
    public SQLiteAsyncConnection Connection => _connection;
}
