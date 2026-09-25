using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Yedekleme, geri yükleme ve profil aktarma yaşam döngüsünü yöneten kullanım senaryosu servisi.
/// Gün başına tek dosya kuralı, değişiklik yoksa yedek almama ve güvenli içe aktarma orkestrasyonu için vardır.
/// </summary>
public sealed class BackupService(
    IProfileBackupArchive archive,
    IBackupStorage storage,
    IProfileRepository profiles,
    IClock clock,
    BackupOptions options) : IBackupService, IDisposable
{
    private readonly IProfileBackupArchive _archive = archive;
    private readonly IBackupStorage _storage = storage;
    private readonly IProfileRepository _profiles = profiles;
    private readonly IClock _clock = clock;
    private readonly BackupOptions _options = options;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <inheritdoc />
    public bool HasAccess => _storage.HasAccess;

    /// <inheritdoc />
    public string LocationDescription => _storage.LocationDescription;

    /// <inheritdoc />
    public Task<bool> RequestAccessAsync() => _storage.RequestAccessAsync();

    /// <inheritdoc />
    public Task<BackupState?> GetLastBackupAsync(CancellationToken cancellationToken = default) =>
        _archive.GetStateAsync(cancellationToken);

    /// <inheritdoc />
    public Task<BackupResult> BackUpIfChangedAsync(CancellationToken cancellationToken = default) =>
        BackUpAsync(onlyIfChanged: true, cancellationToken);

    /// <inheritdoc />
    public Task<BackupResult> BackUpNowAsync(CancellationToken cancellationToken = default) =>
        BackUpAsync(onlyIfChanged: false, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoredBackup>> ListBackupsAsync(CancellationToken cancellationToken = default)
    {
        var backups = await _storage.ListAsync(cancellationToken);
        return BackupRetentionRules.OnlyMizanBackups(backups)
            .OrderByDescending(b => b.ModifiedAt)
            .ThenByDescending(b => b.FileName, StringComparer.Ordinal)
            .ToArray();
    }

    /// <inheritdoc />
    public Task<BackupSummary> ReadBackupAsync(Stream source, CancellationToken cancellationToken = default) =>
        _archive.ReadSummaryAsync(BackupRetentionRules.Rewind(source), cancellationToken);

    /// <summary>
    /// Depodaki dosya adından yedeği okuyarak hiç profil yokken tüm profilleri geri yükler.
    /// </summary>
    public async Task<BackupSummary> RestoreAsync(string fileName, CancellationToken cancellationToken = default)
    {
        await using var source = await _storage.OpenReadAsync(fileName, cancellationToken);
        return await RestoreAsync(source, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BackupSummary> RestoreAsync(Stream source, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if ((await _profiles.GetProfilesAsync(cancellationToken)).Count > 0)
            {
                throw new InvalidOperationException("Yedek yalnız hiç profil yokken geri yüklenebilir.");
            }

            var backup = await _archive.ReadSummaryAsync(BackupRetentionRules.Rewind(source), cancellationToken);
            var imports = backup.Profiles
                .Select(p => new ProfileImport(p.Id, new UserProfile
                {
                    Id = p.Id, Name = p.Name, CreatedAt = p.CreatedAt, LastOpenedAt = p.LastOpenedAt
                }))
                .ToArray();
            await _archive.ImportAsync(BackupRetentionRules.Rewind(source), imports, cancellationToken);
            return backup;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<BackupImportResult> AddFromBackupAsync(
        Stream source,
        IReadOnlyCollection<Guid>? profileIds,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var backup = await _archive.ReadSummaryAsync(BackupRetentionRules.Rewind(source), cancellationToken);
            var selected = profileIds is null
                ? backup.Profiles
                : backup.Profiles.Where(p => profileIds.Contains(p.Id)).ToArray();

            if (selected.Count == 0 || (profileIds is not null && selected.Count != profileIds.Count))
            {
                throw new InvalidOperationException("Seçilen profil yedekte yok.");
            }

            var existing = await _profiles.GetProfilesAsync(cancellationToken);
            var taken = existing.Select(p => p.Name).ToList();
            var added = selected
                .Select(p => BackupRetentionRules.CreateImportedProfile(p, backup.CreatedAt, existing, taken, _clock.UtcNow))
                .ToArray();
            var imports = selected.Zip(added, (p, a) => new ProfileImport(p.Id, a.Profile)).ToArray();

            await _archive.ImportAsync(BackupRetentionRules.Rewind(source), imports, cancellationToken);
            return new BackupImportResult(backup, added);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<BackupResult> BackUpAsync(bool onlyIfChanged, CancellationToken cancellationToken)
    {
        if (!_storage.HasAccess)
        {
            return new BackupResult(BackupOutcome.NoAccess);
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if ((await _profiles.GetProfilesAsync(cancellationToken)).Count == 0)
            {
                return new BackupResult(BackupOutcome.NothingToBackUp);
            }

            var fingerprint = await _archive.ComputeFingerprintAsync(cancellationToken);
            var previous = await _archive.GetStateAsync(cancellationToken);
            var stored = await _storage.ListAsync(cancellationToken);

            if (onlyIfChanged && previous?.Fingerprint == fingerprint && stored.Any(b => b.FileName == previous.FileName))
            {
                return new BackupResult(BackupOutcome.Unchanged, previous);
            }

            var state = await PersistBackupAsync(fingerprint, cancellationToken);
            return new BackupResult(BackupOutcome.Created, state);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<BackupState> PersistBackupAsync(string fingerprint, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.WorkingDirectory);
        var tempPath = Path.Combine(_options.WorkingDirectory, $"backup-{Guid.NewGuid():N}.zip");
        try
        {
            await using (var fileStream = File.Create(tempPath))
            {
                await _archive.WriteAsync(fileStream, cancellationToken);
            }

            var fileName = BackupRetentionRules.FileNameFor(_clock.Today);
            await _storage.SaveAsync(fileName, tempPath, cancellationToken);

            var stored = await _storage.ListAsync(cancellationToken);
            foreach (var old in BackupRetentionRules.SelectForDeletion(stored, _options.KeepCount))
            {
                await _storage.DeleteAsync(old.FileName, cancellationToken);
            }

            var state = new BackupState(_clock.UtcNow, fileName, fingerprint);
            await _archive.SaveStateAsync(state, cancellationToken);
            return state;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => _lock.Dispose();
}
