using Mizan.Application.Models;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde profil yedekleme ve geri yükleme akışını taklit eden test dublörü.
/// </summary>
public sealed class FakeProfileBackupHandler : IProfileBackupHandler
{
    public BackupSummary? NextRestoreSummary { get; set; }
    public BackupImportResult? NextAddResult { get; set; }
    public bool RestoreCalled { get; private set; }
    public bool AddCalled { get; private set; }

    public Task<BackupSummary?> RestoreAsync()
    {
        RestoreCalled = true;
        return Task.FromResult(NextRestoreSummary);
    }

    public Task<BackupImportResult?> AddAsync()
    {
        AddCalled = true;
        return Task.FromResult(NextAddResult);
    }
}
