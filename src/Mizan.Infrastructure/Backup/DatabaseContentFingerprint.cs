using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Mizan.Application.Models;
using SQLite;
using SQLitePCL;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Profillerin parmak izine girecek içeriği karmaya ekler. Dosyanın değişiklik zamanına değil
/// içeriğine bakılır: aynı değeri yeniden kaydetmek, günlük işlemleri ve dosya kopyalamak zamanı
/// oynatır ama kullanıcının verisini değiştirmez (S57). Son açılış tarihi bilerek dışarıdadır —
/// yalnız profili açmak yeni bir yedek gerektirmez.
/// </summary>
internal static class DatabaseContentFingerprint
{
    private const string TableNamesQuery =
        "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name";

    private static readonly TimeSpan BusyTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Profilin kimliğini ve adını karmaya ekler; son açılış tarihini eklemez.
    /// </summary>
    public static void AppendProfile(IncrementalHash hash, UserProfile profile) =>
        AppendText(hash, $"profile|{profile.Id:N}|{profile.Name}");

    /// <summary>
    /// Veritabanındaki her tabloyu ad sırasıyla, her satırı <c>rowid</c> sırasıyla karmaya ekler.
    /// Veritabanı salt-okunur ve tek bir okuma işlemi içinde okunur.
    /// </summary>
    public static void AppendDatabase(IncrementalHash hash, string databasePath)
    {
        using var connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly);
        connection.BusyTimeout = BusyTimeout;
        connection.RunInTransaction(() =>
        {
            foreach (var table in connection.QueryScalars<string>(TableNamesQuery))
            {
                AppendText(hash, $"table|{table}");
                AppendRows(hash, connection, table);
            }
        });
    }

    private static void AppendRows(IncrementalHash hash, SQLiteConnection connection, string table)
    {
        var quotedTable = table.Replace("\"", "\"\"", StringComparison.Ordinal);
        var statement = SQLite3.Prepare2(connection.Handle, $"SELECT * FROM \"{quotedTable}\" ORDER BY rowid");
        try
        {
            while (SQLite3.Step(statement) == SQLite3.Result.Row)
            {
                var columnCount = SQLite3.ColumnCount(statement);
                for (var column = 0; column < columnCount; column++)
                {
                    AppendText(hash, ColumnValue(statement, column));
                }
            }
        }
        finally
        {
            SQLite3.Finalize(statement);
        }
    }

    // Tür öneki aynı metni taşıyan farklı türleri ayırır: 1 (tamsayı) ile "1" (metin) aynı karmayı vermez.
    private static string ColumnValue(sqlite3_stmt statement, int column) =>
        SQLite3.ColumnType(statement, column) switch
        {
            SQLite3.ColType.Integer => "i" + SQLite3.ColumnInt64(statement, column).ToString(CultureInfo.InvariantCulture),
            SQLite3.ColType.Float => "f" + SQLite3.ColumnDouble(statement, column).ToString("R", CultureInfo.InvariantCulture),
            SQLite3.ColType.Text => "t" + SQLite3.ColumnString(statement, column),
            SQLite3.ColType.Blob => "b" + Convert.ToBase64String(SQLite3.ColumnByteArray(statement, column)),
            _ => "n"
        };

    // Her değerden sonra sıfır bayt: "ab" + "c" ile "a" + "bc" aynı karmayı vermez.
    private static void AppendText(IncrementalHash hash, string value)
    {
        hash.AppendData(Encoding.UTF8.GetBytes(value));
        hash.AppendData([0]);
    }
}
