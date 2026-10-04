using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Testler için eski yedek içe aktarıcısını taklit eden test çifti: özeti verir, aldığı eşlemeleri kaydeder.
/// </summary>
public sealed class FakeLegacyBackupImporter : ILegacyBackupImporter
{
    public DateTimeOffset BackupCreatedAt { get; set; } = new(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
    public List<BackupProfile> ProfilesInBackup { get; } = [];
    public List<ProfileImport> LastImported { get; } = [];
    public int ImportCallCount { get; private set; }
    public long? PositionAtImport { get; private set; }
    public Exception? SummaryException { get; set; }

    public Task<BackupSummary> ReadSummaryAsync(Stream source, CancellationToken cancellationToken = default)
    {
        if (SummaryException is not null)
        {
            throw SummaryException;
        }

        // Gerçek zip okuyucu akışı sona doğru ilerletir; servis başa sarmazsa içe aktarma boş okur.
        source.Position = source.Length;
        return Task.FromResult(new BackupSummary(BackupCreatedAt, ProfilesInBackup.ToArray()));
    }

    public Task ImportAsync(
        Stream source,
        IReadOnlyList<ProfileImport> profileImports,
        CancellationToken cancellationToken = default)
    {
        ImportCallCount++;
        PositionAtImport = source.Position;
        LastImported.Clear();
        LastImported.AddRange(profileImports);
        return Task.CompletedTask;
    }
}
