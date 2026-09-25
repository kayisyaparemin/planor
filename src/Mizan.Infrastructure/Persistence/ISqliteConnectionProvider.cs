using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// O an açık olan profilin etkin SQLite veritabanı bağlantısını sağlayan arayüz.
/// Hiçbir profil açık değilken bağlantı istendiğinde InvalidOperationException fırlatır.
/// </summary>
public interface ISqliteConnectionProvider
{
    /// <summary>
    /// O an açık olan profilin aktif SQLite bağlantısını döner.
    /// </summary>
    SQLiteAsyncConnection Connection { get; }
}
