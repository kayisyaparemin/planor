using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Eski uygulamanın (<c>com.coinflow.mobile</c>, şema v17) yedek dosyasından profil getiren tek seferlik
/// içe aktarıcı. Yeni app id'siyle kurulan Planör eski uygulamanın verisini göremez; kullanıcının yıllarca
/// biriken verisi bu port üzerinden taşınır. Normal geri yüklemeden (<see cref="IProfileBackupArchive"/>)
/// ayrıdır, çünkü eski yedeğin veritabanı yükseltilmez, yeniden yazılır (S83).
/// </summary>
public interface ILegacyBackupImporter
{
    /// <summary>
    /// Yedeğin manifestini okur ve içindeki profilleri listeler; veritabanlarına dokunmaz, akışı kapatmaz.
    /// Dosya eski uygulamanın yedeği değilse kullanıcıya gidecek mesajla <see cref="InvalidOperationException"/>
    /// fırlatır.
    /// </summary>
    Task<BackupSummary> ReadSummaryAsync(
        Stream source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seçilen eski profilleri hedef kimlik ve adlarıyla bu uygulamanın güncel şemasında ekler. Hep-ya-hiç
    /// çalışır: seçilenlerden biri okunamazsa, tanınmayan bir şema sürümündeyse ya da taşıma yarıda kalırsa
    /// hiçbiri eklenmez.
    /// </summary>
    Task ImportAsync(
        Stream source,
        IReadOnlyList<ProfileImport> profileImports,
        CancellationToken cancellationToken = default);
}
