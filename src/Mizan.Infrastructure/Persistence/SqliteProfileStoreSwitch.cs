using Mizan.Application.Abstractions;
using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Açık olan profilin veritabanı bağlantısını yöneten ve depolara o anki bağlantıyı sunan adaptör.
/// Kapalıyken veya profil seçilmemişken depolara yapılan çağrıları engeller.
/// </summary>
public sealed class SqliteProfileStoreSwitch(
    IProfileFileLayout layout,
    ISqliteConnectionFactory connectionFactory) : IProfileStoreSwitch, ISqliteConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly IProfileFileLayout _layout = layout ?? throw new ArgumentNullException(nameof(layout));
    private readonly ISqliteConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private SQLiteAsyncConnection? _current;

    /// <inheritdoc />
    public SQLiteAsyncConnection Connection =>
        _current ?? throw new InvalidOperationException(
            "Açık bir profil yok. Devam etmek için bir profil seç.");

    /// <inheritdoc />
    public async Task OpenAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        // Önceki profil açıksa kapat
        await CloseAsync();

        var databasePath = _layout.GetDatabasePath(profileId);
        _current = await _connectionFactory.CreateConnectionAsync(databasePath, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CloseAsync()
    {
        if (_current is null)
        {
            return;
        }

        await _current.CloseAsync();
        _current = null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_current is not null)
        {
            _current.CloseAsync().GetAwaiter().GetResult();
            _current = null;
        }
    }
}
