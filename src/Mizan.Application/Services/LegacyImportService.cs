using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Eski yedekten gelen profillere hedef kimlik ve ad veren, sonra içe aktarıcıyı çağıran servis.
/// Yedekten eklemeyle aynı adlandırma kuralını (<see cref="BackupRetentionRules"/>) kullanır ki iki yol
/// aynı çakışmada farklı davranmasın; fark yalnızca verinin eski şemadan dönüştürülmesidir (S83).
/// </summary>
public sealed class LegacyImportService(
    ILegacyBackupImporter importer,
    IProfileRepository profiles,
    IClock clock) : ILegacyImportService
{
    private readonly ILegacyBackupImporter _importer = importer;
    private readonly IProfileRepository _profiles = profiles;
    private readonly IClock _clock = clock;

    /// <inheritdoc />
    public async Task<BackupImportResult> ImportAsync(Stream source, CancellationToken cancellationToken = default)
    {
        var backup = await _importer.ReadSummaryAsync(BackupRetentionRules.Rewind(source), cancellationToken);
        if (backup.Profiles.Count == 0)
        {
            throw new InvalidOperationException("Bu yedekte içe aktarılacak profil yok.");
        }

        var existing = await _profiles.GetProfilesAsync(cancellationToken);
        var takenNames = existing.Select(p => p.Name).ToList();
        var added = backup.Profiles
            .Select(p => BackupRetentionRules.CreateImportedProfile(p, backup.CreatedAt, existing, takenNames, _clock.UtcNow))
            .ToArray();
        var imports = backup.Profiles.Zip(added, (p, a) => new ProfileImport(p.Id, a.Profile)).ToArray();

        // Özeti okumak akışı sona taşır; içe aktarıcı zip'i baştan açacağı için geri sarılır.
        await _importer.ImportAsync(BackupRetentionRules.Rewind(source), imports, cancellationToken);
        return new BackupImportResult(backup, added);
    }
}
