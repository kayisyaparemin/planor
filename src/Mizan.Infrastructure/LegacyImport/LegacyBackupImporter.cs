using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;

namespace Mizan.Infrastructure.LegacyImport;

/// <summary>
/// Eski uygulamanın yedek zip'ini (biçim 1, şema v17) açıp her profilin veritabanını bu uygulamanın
/// şemasına çeviren içe aktarıcı. Kullanıcının eski telefondaki verisi bu yol olmadan Planör'e geçemez;
/// normal geri yükleme eski yedeği bilerek reddeder (S57). Profil kaydı yalnız
/// <see cref="IProfileRepository"/> üzerinden yazılır (S58); dönüşümün kuralları S83'tedir.
/// </summary>
public sealed class LegacyBackupImporter(
    IProfileRepository profiles,
    IProfileFileLayout layout,
    DatabaseSchema schema) : ILegacyBackupImporter
{
    private readonly IProfileRepository _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
    private readonly IProfileFileLayout _layout = layout ?? throw new ArgumentNullException(nameof(layout));
    private readonly DatabaseSchema _schema = schema ?? throw new ArgumentNullException(nameof(schema));

    /// <summary>
    /// Yedeğin eski uygulamadan geldiğini doğrular ve profillerini listeler; veritabanlarına dokunmaz,
    /// akışı kapatmaz.
    /// </summary>
    public async Task<BackupSummary> ReadSummaryAsync(Stream source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var zip = BackupManifestReader.Open(source);
        var manifest = await BackupManifestReader.ReadLegacyAsync(zip, cancellationToken);
        return new BackupSummary(
            manifest.CreatedAt,
            manifest.Profiles
                .Select(profile => new BackupProfile(profile.Id, profile.Name, profile.CreatedAt, profile.LastOpenedAt))
                .ToArray());
    }

    /// <summary>
    /// Seçilen eski profilleri dönüştürüp ekler; biri düşerse hiçbiri eklenmez.
    /// </summary>
    public async Task ImportAsync(
        Stream source,
        IReadOnlyList<ProfileImport> profileImports,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(profileImports);
        ProfileImportTransaction.EnsureValidSelection(profileImports);
        var staging = BackupWorkDirectory.Create(_layout.RootDirectory, ".restore-");
        try
        {
            var staged = await LegacyProfileStager.StageAsync(source, profileImports, staging, _schema, cancellationToken);
            await ProfileImportTransaction.CommitAsync(staged, _profiles, _layout, cancellationToken);
        }
        finally
        {
            BackupWorkDirectory.TryDelete(staging);
        }
    }
}
