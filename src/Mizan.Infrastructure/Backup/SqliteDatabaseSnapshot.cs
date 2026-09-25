using SQLite;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Bir profil veritabanının tutarlı kopyasını çıkarır. Dosya olduğu gibi kopyalanmaz:
/// açık bir profil o anda yazıyorsa kopya yarım bir işlem içerebilir. <c>VACUUM INTO</c>
/// tek bir okuma işlemi içinde tutarlı bir anlık görüntü üretir.
/// </summary>
internal static class SqliteDatabaseSnapshot
{
    /// <summary>
    /// Açık profil o anda yazıyorsa okumanın bekleyeceği azami süre.
    /// </summary>
    private static readonly TimeSpan BusyTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// <paramref name="databasePath"/>'in anlık görüntüsünü <paramref name="snapshotPath"/>'e yazar.
    /// Kaynak salt-okunur açılır; hedef dosya henüz var olmamalıdır.
    /// </summary>
    public static void Create(string databasePath, string snapshotPath)
    {
        using var connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
        connection.BusyTimeout = BusyTimeout;
        connection.Execute("VACUUM INTO ?", snapshotPath);
    }
}
