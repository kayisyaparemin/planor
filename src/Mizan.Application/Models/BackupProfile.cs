namespace Mizan.Application.Models;

/// <summary>
/// Yedek arşivinin içinde bulunan tek bir profilin meta verilerini temsil eden kayıt.
/// Yedeğin içindeki kullanıcı veritabanlarını açmadan profilleri listelemek için vardır.
/// </summary>
public sealed record BackupProfile(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastOpenedAt);
