using Mizan.Application.Models;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;

namespace Mizan.Presentation.Tests.Services;

/// <summary>
/// ProfileBackupHandler servisinin yedek seçme, geri yükleme ve ekleme davranışlarını doğrulayan testler.
/// </summary>
public sealed class ProfileBackupHandlerTests
{
    private readonly FakeBackupService _backup = new();
    private readonly FakeBackupFilePicker _filePicker = new();
    private readonly FakeDialogService _dialog = new();
    private readonly ProfileBackupHandler _handler;

    public ProfileBackupHandlerTests()
    {
        _handler = new ProfileBackupHandler(_backup, _filePicker, _dialog);
    }

    [Fact]
    public async Task RestoreAsync_DosyaSecilirse_YedegiGeriYuklerVeBildirir()
    {
        _filePicker.NextPickedStream = new MemoryStream();
        _backup.NextSummary = new BackupSummary(
            DateTimeOffset.UtcNow,
            [new BackupProfile(Guid.NewGuid(), "Yedek Profil", DateTimeOffset.UtcNow, null)]);

        var result = await _handler.RestoreAsync();

        Assert.NotNull(result);
        Assert.True(_backup.RestoreCalled);
        Assert.Equal("Geri Yüklendi", _dialog.LastAlertTitle);
    }

    [Fact]
    public async Task RestoreAsync_SecimIptalEdilirse_NullDonerVeGeriYuklemez()
    {
        _filePicker.NextPickedStream = null;

        var result = await _handler.RestoreAsync();

        Assert.Null(result);
        Assert.False(_backup.RestoreCalled);
    }

    [Fact]
    public async Task AddAsync_DosyaSecilirse_YedektenEklerVeBildirir()
    {
        _filePicker.NextPickedStream = new MemoryStream();
        _backup.NextImportResult = new BackupImportResult(
            new BackupSummary(DateTimeOffset.UtcNow, []),
            [new ImportedProfile(new UserProfile { Id = Guid.NewGuid(), Name = "Eklenen" }, false)]);

        var result = await _handler.AddAsync();

        Assert.NotNull(result);
        Assert.True(_backup.AddCalled);
        Assert.Equal("Yedekten Eklendi", _dialog.LastAlertTitle);
    }

    [Fact]
    public async Task AddAsync_SecimIptalEdilirse_NullDonerVeEklemez()
    {
        _filePicker.NextPickedStream = null;

        var result = await _handler.AddAsync();

        Assert.Null(result);
        Assert.False(_backup.AddCalled);
    }
}
