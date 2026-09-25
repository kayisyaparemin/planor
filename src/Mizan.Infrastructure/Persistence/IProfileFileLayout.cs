namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Profil dizinlerinin ve veritabanı dosyalarının disk üzerindeki yerleşimini
/// sağlayan sözleşme. Yedekleme ve veritabanı bağlantı yönetiminin dosya yapısını bilmesini sağlar.
/// </summary>
public interface IProfileFileLayout
{
    /// <summary>
    /// Uygulama veri kök dizini.
    /// </summary>
    string RootDirectory { get; }

    /// <summary>
    /// Belirtilen profilin kök dizin yolunu döner.
    /// </summary>
    string GetProfileDirectory(Guid profileId);

    /// <summary>
    /// Belirtilen profilin SQLite veritabanı dosya yolunu döner.
    /// </summary>
    string GetDatabasePath(Guid profileId);
}
