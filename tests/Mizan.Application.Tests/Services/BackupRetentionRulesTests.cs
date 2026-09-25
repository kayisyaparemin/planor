using Mizan.Application.Models;
using Mizan.Application.Services;

namespace Mizan.Application.Tests.Services;

public sealed class BackupRetentionRulesTests
{
    private static readonly DateOnly Today = new(2026, 9, 14);

    [Fact]
    public void FileNameFor_ProducesStandardFormattedZipName()
    {
        var fileName = BackupRetentionRules.FileNameFor(Today);
        Assert.Equal("Mizan-yedek-2026-09-14.zip", fileName);
    }

    [Fact]
    public void OnlyMizanBackups_FiltersOutNonMizanFiles()
    {
        var files = new StoredBackup[]
        {
            new("Mizan-yedek-2026-09-14.zip", DateTimeOffset.UtcNow),
            new("tatil-fotografi.jpg", DateTimeOffset.UtcNow),
            new("notlar.txt", DateTimeOffset.UtcNow),
            new("Mizan-yedek-2026-09-15.ZIP", DateTimeOffset.UtcNow),
            new("baska-yedek.zip", DateTimeOffset.UtcNow)
        };

        var filtered = BackupRetentionRules.OnlyMizanBackups(files).ToArray();

        Assert.Equal(2, filtered.Length);
        Assert.Contains(filtered, file => file.FileName == "Mizan-yedek-2026-09-14.zip");
        Assert.Contains(filtered, file => file.FileName == "Mizan-yedek-2026-09-15.ZIP");
    }

    [Fact]
    public void SelectForDeletion_KeepsTheNewestFiles_AndNeverTouchesForeignFiles()
    {
        var baseDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var files = Enumerable.Range(0, 9)
            .Select(day => new StoredBackup(
                BackupRetentionRules.FileNameFor(Today.AddDays(day)),
                baseDate.AddDays(day)))
            .Append(new StoredBackup("fotograf.jpg", baseDate.AddYears(-1)))
            .ToArray();

        // 9 adet Mizan yedeği var; keepCount = 7 verilirse en eski 2'si silinmeli
        var deleted = BackupRetentionRules.SelectForDeletion(files, keepCount: 7);

        Assert.Equal(2, deleted.Count);
        Assert.Equal(BackupRetentionRules.FileNameFor(Today.AddDays(1)), deleted[0].FileName);
        Assert.Equal(BackupRetentionRules.FileNameFor(Today), deleted[1].FileName);
        Assert.DoesNotContain(deleted, file => file.FileName == "fotograf.jpg");
    }

    [Fact]
    public void FormatCopyName_FormatsWithTurkishDate_AndFitsMaxLength()
    {
        var at = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);

        var copyName = BackupRetentionRules.FormatCopyName("Ayşe", at);
        Assert.Equal("Ayşe (14 Eylül yedeği)", copyName);

        var longName = BackupRetentionRules.FormatCopyName("Ailenin ortak bütçe profili", at);
        Assert.True(longName.Length <= UserProfile.MaxNameLength, $"İsim uzunluğu {longName.Length} > {UserProfile.MaxNameLength}");
        Assert.EndsWith("… (14 Eylül yedeği)", longName);
    }

    [Fact]
    public void Rewind_WhenSeekable_ResetsPositionToZero()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);
        stream.Position = 3;

        var rewound = BackupRetentionRules.Rewind(stream);

        Assert.Equal(0, rewound.Position);
    }

    [Fact]
    public void CreateImportedProfile_WhenIsCopy_CreatesCopyWithNewIdAndGeneratedName()
    {
        var existingId = Guid.NewGuid();
        var existing = new List<UserProfile>
        {
            new() { Id = existingId, Name = "Ayşe", CreatedAt = DateTimeOffset.UtcNow }
        };
        var taken = new List<string> { "Ayşe" };
        var profile = new BackupProfile(existingId, "Ayşe", DateTimeOffset.UtcNow.AddDays(-10), null);
        var backupAt = new DateTimeOffset(2026, 9, 14, 12, 0, 0, TimeSpan.Zero);
        var now = DateTimeOffset.UtcNow;

        var imported = BackupRetentionRules.CreateImportedProfile(profile, backupAt, existing, taken, now);

        Assert.True(imported.IsCopy);
        Assert.NotEqual(existingId, imported.Profile.Id);
        Assert.Equal(now, imported.Profile.CreatedAt);
        Assert.Null(imported.Profile.LastOpenedAt);
        Assert.Equal("Ayşe (14 Eylül yedeği)", imported.Profile.Name);
    }
}
