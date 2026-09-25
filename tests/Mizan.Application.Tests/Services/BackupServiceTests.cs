using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;

namespace Mizan.Application.Tests.Services;

public sealed class BackupServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 14);

    [Fact]
    public async Task BackUpNowAsync_WithoutAccess_ReturnsNoAccessOutcome()
    {
        var (sut, _, storage, _) = CreateSut();
        storage.HasAccess = false;

        var result = await sut.BackUpNowAsync();

        Assert.Equal(BackupOutcome.NoAccess, result.Outcome);
        Assert.Null(result.State);
    }

    [Fact]
    public async Task BackUpNowAsync_WhenNoProfilesExist_ReturnsNothingToBackUpOutcome()
    {
        var (sut, _, _, _) = CreateSut();

        var result = await sut.BackUpNowAsync();

        Assert.Equal(BackupOutcome.NothingToBackUp, result.Outcome);
        Assert.Null(result.State);
    }

    [Fact]
    public async Task BackUpNowAsync_CreatesBackupFile_SavesState_AndPrunesOldFiles()
    {
        var (sut, archive, storage, profiles) = CreateSut(keepCount: 2);
        await profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe", CreatedAt = DateTimeOffset.UtcNow });
        archive.Fingerprint = "fp-1";

        // Önceki eski dosyaları depoya ekle
        storage.AddFile("Mizan-yedek-2026-09-10.zip", [1, 2, 3], DateTimeOffset.UtcNow.AddDays(-4));
        storage.AddFile("Mizan-yedek-2026-09-11.zip", [1, 2, 3], DateTimeOffset.UtcNow.AddDays(-3));

        var result = await sut.BackUpNowAsync();

        Assert.Equal(BackupOutcome.Created, result.Outcome);
        Assert.NotNull(result.State);
        Assert.Equal("Mizan-yedek-2026-09-14.zip", result.State.FileName);
        Assert.Equal("fp-1", result.State.Fingerprint);

        var stored = await storage.ListAsync();
        Assert.Equal(2, stored.Count);
        Assert.Contains(stored, f => f.FileName == "Mizan-yedek-2026-09-14.zip");
        Assert.Contains(stored, f => f.FileName == "Mizan-yedek-2026-09-11.zip");
        Assert.DoesNotContain(stored, f => f.FileName == "Mizan-yedek-2026-09-10.zip");
    }

    [Fact]
    public async Task BackUpIfChangedAsync_WhenFingerprintUnchangedAndFileExists_ReturnsUnchangedOutcome()
    {
        var (sut, archive, storage, profiles) = CreateSut();
        await profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe", CreatedAt = DateTimeOffset.UtcNow });
        archive.Fingerprint = "fp-same";

        var state = new BackupState(DateTimeOffset.UtcNow, "Mizan-yedek-2026-09-14.zip", "fp-same");
        await archive.SaveStateAsync(state);
        storage.AddFile("Mizan-yedek-2026-09-14.zip", [1, 2], DateTimeOffset.UtcNow);

        var result = await sut.BackUpIfChangedAsync();

        Assert.Equal(BackupOutcome.Unchanged, result.Outcome);
        Assert.Equal(state, result.State);
    }

    [Fact]
    public async Task BackUpIfChangedAsync_WhenFingerprintUnchangedButFileMissing_CreatesNewBackup()
    {
        var (sut, archive, storage, profiles) = CreateSut();
        await profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Ayşe", CreatedAt = DateTimeOffset.UtcNow });
        archive.Fingerprint = "fp-same";

        var state = new BackupState(DateTimeOffset.UtcNow, "Mizan-yedek-2026-09-14.zip", "fp-same");
        await archive.SaveStateAsync(state);
        // Depoda dosya yok (kullanıcı silmiş olabilir)

        var result = await sut.BackUpIfChangedAsync();

        Assert.Equal(BackupOutcome.Created, result.Outcome);
        Assert.NotNull(result.State);
    }

    [Fact]
    public async Task ListBackupsAsync_ReturnsOnlyMizanBackups_OrderedByDateDescending()
    {
        var (sut, _, storage, _) = CreateSut();
        var at = DateTimeOffset.UtcNow;
        storage.AddFile("Mizan-yedek-2026-09-10.zip", [1], at.AddDays(-4));
        storage.AddFile("tatil-fotografi.jpg", [2], at);
        storage.AddFile("Mizan-yedek-2026-09-14.zip", [3], at);

        var list = await sut.ListBackupsAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal("Mizan-yedek-2026-09-14.zip", list[0].FileName);
        Assert.Equal("Mizan-yedek-2026-09-10.zip", list[1].FileName);
    }

    [Fact]
    public async Task RestoreAsync_WithStream_WhenProfilesAlreadyExist_ThrowsInvalidOperationException()
    {
        var (sut, _, _, profiles) = CreateSut();
        await profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Mevcut", CreatedAt = DateTimeOffset.UtcNow });

        using var stream = new MemoryStream([1, 2, 3]);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RestoreAsync(stream));
        Assert.Contains("hiç profil yokken", error.Message);
    }

    [Fact]
    public async Task RestoreAsync_WithStream_WhenNoProfilesExist_ImportsAllProfilesWithOriginalIdentities()
    {
        var (sut, archive, _, _) = CreateSut();
        var p1 = new BackupProfile(Guid.NewGuid(), "Ayşe", DateTimeOffset.UtcNow, null);
        var p2 = new BackupProfile(Guid.NewGuid(), "Mehmet", DateTimeOffset.UtcNow, null);
        archive.ProfilesInArchive.AddRange([p1, p2]);

        using var stream = new MemoryStream([1, 2, 3]);

        var summary = await sut.RestoreAsync(stream);

        Assert.Equal(["Ayşe", "Mehmet"], summary.ProfileNames);
        Assert.Equal(2, archive.LastImported.Count);
        Assert.Equal(p1.Id, archive.LastImported[0].Target.Id);
        Assert.Equal(p1.Name, archive.LastImported[0].Target.Name);
        Assert.Equal(p2.Id, archive.LastImported[1].Target.Id);
        Assert.Equal(p2.Name, archive.LastImported[1].Target.Name);
    }

    [Fact]
    public async Task RestoreAsync_WithFileName_ReadsFromStorageAndRestores()
    {
        var (sut, archive, storage, _) = CreateSut();
        var p1 = new BackupProfile(Guid.NewGuid(), "Ayşe", DateTimeOffset.UtcNow, null);
        archive.ProfilesInArchive.Add(p1);
        storage.AddFile("Mizan-yedek-2026-09-14.zip", [1, 2, 3], DateTimeOffset.UtcNow);

        var summary = await sut.RestoreAsync("Mizan-yedek-2026-09-14.zip");

        Assert.Single(summary.Profiles);
        Assert.Equal("Ayşe", summary.Profiles[0].Name);
    }

    [Fact]
    public async Task AddFromBackupAsync_WhenProfileAlreadyExists_AddsCopyProfileWithNewIdAndCopyName()
    {
        var (sut, archive, _, profiles) = CreateSut();
        var existingId = Guid.NewGuid();
        await profiles.SaveProfileAsync(new UserProfile { Id = existingId, Name = "Ayşe", CreatedAt = DateTimeOffset.UtcNow });

        var backupProfile = new BackupProfile(existingId, "Ayşe", DateTimeOffset.UtcNow.AddDays(-10), null);
        archive.ProfilesInArchive.Add(backupProfile);

        using var stream = new MemoryStream([1, 2, 3]);

        var result = await sut.AddFromBackupAsync(stream, [existingId]);

        var added = Assert.Single(result.Added);
        Assert.True(added.IsCopy);
        Assert.NotEqual(existingId, added.Profile.Id);
        Assert.Equal(BackupRetentionRules.FormatCopyName("Ayşe", archive.ArchiveCreatedAt), added.Profile.Name);
    }

    [Fact]
    public async Task AddFromBackupAsync_WhenProfileDoesNotExist_RetainsOriginalId_AndResolvesNameClashesWithSuffix()
    {
        var (sut, archive, _, profiles) = CreateSut();
        // Cihazda "ayşe" adında başka bir profil var (küçük harfle)
        await profiles.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "ayşe", CreatedAt = DateTimeOffset.UtcNow });

        var newProfileId = Guid.NewGuid();
        var backupProfile = new BackupProfile(newProfileId, "Ayşe", DateTimeOffset.UtcNow.AddDays(-10), null);
        archive.ProfilesInArchive.Add(backupProfile);

        using var stream = new MemoryStream([1, 2, 3]);

        var result = await sut.AddFromBackupAsync(stream, profileIds: null);

        var added = Assert.Single(result.Added);
        Assert.False(added.IsCopy);
        Assert.Equal(newProfileId, added.Profile.Id);
        Assert.Equal("Ayşe 2", added.Profile.Name);
    }

    [Fact]
    public async Task AddFromBackupAsync_WhenSelectedProfileNotInBackup_ThrowsInvalidOperationException()
    {
        var (sut, archive, _, _) = CreateSut();
        archive.ProfilesInArchive.Add(new BackupProfile(Guid.NewGuid(), "Ali", DateTimeOffset.UtcNow, null));

        using var stream = new MemoryStream([1, 2, 3]);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.AddFromBackupAsync(stream, [Guid.NewGuid()]));

        Assert.Contains("Seçilen profil yedekte yok", error.Message);
    }

    [Fact]
    public async Task ReadBackupAsync_ReturnsSummaryFromArchive()
    {
        var (sut, archive, _, _) = CreateSut();
        archive.ProfilesInArchive.Add(new BackupProfile(Guid.NewGuid(), "Zeynep", DateTimeOffset.UtcNow, null));

        using var stream = new MemoryStream([1, 2, 3]);

        var summary = await sut.ReadBackupAsync(stream);

        Assert.Single(summary.Profiles);
        Assert.Equal("Zeynep", summary.Profiles[0].Name);
    }

    [Fact]
    public async Task StorageDelegation_PropertiesAndRequestAccessWork()
    {
        var (sut, _, storage, _) = CreateSut();
        storage.HasAccess = true;
        storage.LocationDescription = "Özel Klasör";

        Assert.True(sut.HasAccess);
        Assert.Equal("Özel Klasör", sut.LocationDescription);
        Assert.True(await sut.RequestAccessAsync());
    }

    [Fact]
    public async Task GetLastBackupAsync_DelegatesToArchive()
    {
        var (sut, archive, _, _) = CreateSut();
        var state = new BackupState(DateTimeOffset.UtcNow, "yedek.zip", "hash");
        await archive.SaveStateAsync(state);

        var retrieved = await sut.GetLastBackupAsync();

        Assert.Equal(state, retrieved);
    }

    [Fact]
    public async Task RestoreAsync_WithNonSeekableStream_ThrowsArgumentException()
    {
        var (sut, _, _, _) = CreateSut();
        var nonSeekable = new NonSeekableStream();

        await Assert.ThrowsAsync<ArgumentException>(() => sut.RestoreAsync(nonSeekable));
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public override bool CanSeek => false;
    }

    private static (BackupService sut, InMemoryProfileBackupArchive archive, InMemoryBackupStorage storage, InMemoryProfileRepository profiles)
        CreateSut(int keepCount = 7)
    {
        var archive = new InMemoryProfileBackupArchive();
        var storage = new InMemoryBackupStorage();
        var profiles = new InMemoryProfileRepository();
        var clock = new FixedClock(Today, DateTimeOffset.UtcNow);
        var tempDir = Path.Combine(Path.GetTempPath(), $"mizan-backup-test-{Guid.NewGuid():N}");
        var options = new BackupOptions(tempDir, keepCount);

        var sut = new BackupService(archive, storage, profiles, clock, options);
        return (sut, archive, storage, profiles);
    }

    private sealed class FixedClock(DateOnly today, DateTimeOffset utcNow) : Mizan.Application.Abstractions.IClock
    {
        public DateOnly Today { get; set; } = today;
        public DateTimeOffset UtcNow { get; set; } = utcNow;
    }
}
