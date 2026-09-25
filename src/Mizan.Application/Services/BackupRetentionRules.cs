using System.Globalization;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Mizan yedekleme dosyası adlandırma, filtreleme, kopya profil ismi üretme ve saklama kotası kurallarını
/// yöneten saf yardımcı sınıf.
/// Dosya sisteminden ve I/O işlemlerinden bağımsız deterministik hesaplama kurallarını tek yerde toplamak için vardır.
/// </summary>
public static class BackupRetentionRules
{
    /// <summary>
    /// Mizan yedek dosyalarının standart dosya adı öneki. Eski uygulamanın <c>Mizan-yedek-</c> önekinden
    /// harf büyüklüğü gözetmeden de ayrışır: iki uygulama aynı klasörü paylaşsa bile biri diğerinin
    /// yedeğini ezmez, listelemez, temizlikte silmez (S59, I39).
    /// </summary>
    public const string FilePrefix = "Mizan-yedegi-";

    /// <summary>
    /// Mizan yedek dosyalarının standart dosya uzantısı.
    /// </summary>
    public const string FileExtension = ".zip";

    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Belirtilen takvim tarihi için standart Mizan yedek dosya adını (örn. "Mizan-yedegi-2026-09-14.zip") üretir.
    /// </summary>
    public static string FileNameFor(DateOnly date) =>
        FilePrefix +
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
        FileExtension;

    /// <summary>
    /// Listelenen dosyalar arasından yalnızca Mizan yedek adlandırma kuralına uyanları filtreler.
    /// Başka uygulamalara ait dosyalara yanlışlıkla dokunulmasını önlemek için vardır.
    /// </summary>
    public static IEnumerable<StoredBackup> OnlyMizanBackups(IEnumerable<StoredBackup> backups) =>
        backups.Where(backup =>
            backup.FileName.StartsWith(FilePrefix, StringComparison.Ordinal) &&
            backup.FileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Saklama politikasını işletir: en yeni <paramref name="keepCount"/> adet Mizan yedeği kalır,
    /// fazlalık olan en eski dosyalar silinmek üzere seçilir. Yabancı dosyalara dokunulmaz.
    /// </summary>
    public static IReadOnlyList<StoredBackup> SelectForDeletion(
        IEnumerable<StoredBackup> backups,
        int keepCount) =>
        OnlyMizanBackups(backups)
            .OrderByDescending(backup => backup.ModifiedAt)
            .ThenByDescending(backup => backup.FileName, StringComparer.Ordinal)
            .Skip(Math.Max(keepCount, 1))
            .ToArray();

    /// <summary>
    /// Yedekten eklenen kopya profil için "Ayşe (14 Eylül yedeği)" formatında isim üretir;
    /// isim sınırı aşılıyorsa asıl adı kırpar.
    /// </summary>
    public static string FormatCopyName(string name, DateTimeOffset backupCreatedAt)
    {
        var suffix = $" ({backupCreatedAt.ToLocalTime().ToString("d MMMM", TurkishCulture)} yedeği)";
        var room = UserProfile.MaxNameLength - suffix.Length;
        var trimmed = name.Length <= room
            ? name
            : name[..Math.Max(room - 1, 1)].TrimEnd() + "…";
        return trimmed + suffix;
    }

    /// <summary>
    /// Akışın başa sarılabilir olduğunu doğrular ve konumunu sıfırlar; akış konumlanamazsa hata fırlatır.
    /// </summary>
    public static Stream Rewind(Stream source)
    {
        if (!source.CanSeek)
        {
            throw new ArgumentException("Yedek dosyası konumlanabilir bir akıştan okunmalı.", nameof(source));
        }

        source.Position = 0;
        return source;
    }

    /// <summary>
    /// Yedekten eklenen profil için kimlik ve adlandırma kurallarını işleterek aktarılan profil kaydını üretir.
    /// </summary>
    public static ImportedProfile CreateImportedProfile(
        BackupProfile profile,
        DateTimeOffset backupCreatedAt,
        IReadOnlyList<UserProfile> existingProfiles,
        List<string> takenNames,
        DateTimeOffset utcNow)
    {
        var isCopy = existingProfiles.Any(c => c.Id == profile.Id);
        var baseName = isCopy ? FormatCopyName(profile.Name, backupCreatedAt) : profile.Name;
        var name = ProfileNameValidator.GenerateUniqueName(baseName, takenNames);
        takenNames.Add(name);

        var target = new UserProfile
        {
            Id = isCopy ? Guid.NewGuid() : profile.Id,
            Name = name,
            CreatedAt = isCopy ? utcNow : profile.CreatedAt,
            LastOpenedAt = isCopy ? null : profile.LastOpenedAt
        };
        return new ImportedProfile(target, isCopy);
    }
}
