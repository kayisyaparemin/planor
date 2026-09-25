using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için bellek içi profil yedek arşivleme test çifti.
/// </summary>
public sealed class InMemoryProfileBackupArchive : IProfileBackupArchive
{
    private BackupState? _state;
    public string Fingerprint { get; set; } = "initial-fingerprint";
    public List<BackupProfile> ProfilesInArchive { get; } = [];
    public DateTimeOffset ArchiveCreatedAt { get; set; } = new(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
    public List<ProfileImport> LastImported { get; } = [];
    public bool ThrowOnReadSummary { get; set; }

    public Task<string> ComputeFingerprintAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Fingerprint);

    public Task<BackupSummary> WriteAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        destination.Write([0x50, 0x4B, 0x03, 0x04]); // Sahte zip başlığı
        var summary = new BackupSummary(ArchiveCreatedAt, ProfilesInArchive.ToArray());
        return Task.FromResult(summary);
    }

    public Task<BackupSummary> ReadSummaryAsync(Stream source, CancellationToken cancellationToken = default)
    {
        if (ThrowOnReadSummary)
        {
            throw new InvalidOperationException("Geçersiz yedek arşivi.");
        }

        var summary = new BackupSummary(ArchiveCreatedAt, ProfilesInArchive.ToArray());
        return Task.FromResult(summary);
    }

    public Task ImportAsync(Stream source, IReadOnlyList<ProfileImport> profileImports, CancellationToken cancellationToken = default)
    {
        LastImported.Clear();
        LastImported.AddRange(profileImports);
        return Task.CompletedTask;
    }

    public Task<BackupState?> GetStateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_state);

    public Task SaveStateAsync(BackupState state, CancellationToken cancellationToken = default)
    {
        _state = state;
        return Task.CompletedTask;
    }
}
