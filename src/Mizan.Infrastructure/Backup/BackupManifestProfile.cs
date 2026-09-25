namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Manifestteki tek bir profilin kimliği, adı ve verisinin yedekte olup olmadığı.
/// Hiç açılmamış bir profilin veritabanı yoktur; <see cref="HasData"/> bu yüzden ayrı tutulur ki
/// geri yükleme eksik girdiyi "bozuk yedek" sanmasın.
/// </summary>
internal sealed record BackupManifestProfile(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastOpenedAt,
    bool HasData);
