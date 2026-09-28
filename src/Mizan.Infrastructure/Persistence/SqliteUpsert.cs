using SQLite;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Bir satırı silmeden yazmanın tek yolu. SQLite'ın <c>INSERT OR REPLACE</c>'i çakışan satırı
/// silip yeniden ekler; üst/alt bağlar <c>ON DELETE CASCADE</c> ile kurulu olduğu için bu silme
/// kredinin erken ödemelerini, gelirin tutar geçmişini ve ödeme planının taksitlerini de
/// götürüyordu. Burada satır önce birincil anahtarıyla güncellenir, yoksa eklenir; var olan satır
/// hiç silinmez. <c>INSERT OR REPLACE</c> yasağını <c>ArchitectureTests.InsertOrReplace_Yasak</c> korur.
/// </summary>
internal static class SqliteUpsert
{
    public static void Upsert(this SQLiteConnection connection, object entity)
    {
        if (connection.Update(entity) == 0)
        {
            connection.Insert(entity);
        }
    }

    public static Task UpsertAsync(this SQLiteAsyncConnection connection, object entity) =>
        connection.RunInTransactionAsync(conn => conn.Upsert(entity));
}
