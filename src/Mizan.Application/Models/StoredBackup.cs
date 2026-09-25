namespace Mizan.Application.Models;

/// <summary>
/// Yedek depolama konumunda bulunan mevcut bir yedek dosyasını temsil eden kayıt.
/// Depodaki Mizan yedeklerini listelemek ve saklama politikasını işletmek için vardır.
/// </summary>
public sealed record StoredBackup(
    string FileName,
    DateTimeOffset ModifiedAt);
