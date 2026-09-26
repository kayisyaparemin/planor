using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde yedekleme listeleme, geri yükleme ve içe aktarma senaryolarını simüle eden sahte servis.
/// </summary>
public sealed class FakeBackupService : IBackupService
{
    public bool HasAccess { get; set; } = true;
    public string LocationDescription => "Dahili depolama › Mizan";
    public List<StoredBackup> StoredBackups { get; } = [];
    public BackupSummary? NextSummary { get; set; }
    public BackupImportResult? NextImportResult { get; set; }
    public bool RestoreCalled { get; private set; }
    public bool AddCalled { get; private set; }

    public Task<bool> RequestAccessAsync() => Task.FromResult(HasAccess);

    public Task<BackupState?> GetLastBackupAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<BackupState?>(null);

    public Task<BackupResult> BackUpIfChangedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new BackupResult(BackupOutcome.Unchanged));

    public Task<BackupResult> BackUpNowAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new BackupResult(BackupOutcome.Created));

    public Task<IReadOnlyList<StoredBackup>> ListBackupsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<StoredBackup>>(StoredBackups);

    public Task<BackupSummary> ReadBackupAsync(Stream source, CancellationToken cancellationToken = default) =>
        Task.FromResult(NextSummary ?? new BackupSummary(DateTimeOffset.UtcNow, []));

    public Task<BackupSummary> RestoreAsync(Stream source, CancellationToken cancellationToken = default)
    {
        RestoreCalled = true;
        return Task.FromResult(NextSummary ?? new BackupSummary(DateTimeOffset.UtcNow, []));
    }

    public Task<BackupImportResult> AddFromBackupAsync(
        Stream source,
        IReadOnlyCollection<Guid>? profileIds,
        CancellationToken cancellationToken = default)
    {
        AddCalled = true;
        return Task.FromResult(NextImportResult ?? new BackupImportResult(
            NextSummary ?? new BackupSummary(DateTimeOffset.UtcNow, []),
            []));
    }
}
