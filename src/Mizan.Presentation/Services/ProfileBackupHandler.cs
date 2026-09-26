using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;

namespace Mizan.Presentation.Services;

/// <summary>
/// Yedek dosyasını seçme, geri yükleme ve yedekten profil ekleme diyalog orkestrasyonunu yürüten servis.
/// </summary>
public sealed class ProfileBackupHandler : IProfileBackupHandler
{
    private readonly IBackupService backupService;
    private readonly IBackupFilePicker filePicker;
    private readonly IDialogService dialogService;

    /// <summary>
    /// <see cref="ProfileBackupHandler"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public ProfileBackupHandler(
        IBackupService backupService,
        IBackupFilePicker filePicker,
        IDialogService dialogService)
    {
        this.backupService = backupService;
        this.filePicker = filePicker;
        this.dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<BackupSummary?> RestoreAsync()
    {
        var stream = await filePicker.PickAndOpenAsync();
        if (stream is null) { return null; }

        await using (stream)
        {
            var summary = await backupService.RestoreAsync(stream);
            await dialogService.ShowAlertAsync(
                "Geri Yüklendi",
                $"{summary.Profiles.Count} profil geri yüklendi: {string.Join(", ", summary.ProfileNames)}.");
            return summary;
        }
    }

    /// <inheritdoc />
    public async Task<BackupImportResult?> AddAsync()
    {
        var stream = await filePicker.PickAndOpenAsync();
        if (stream is null) { return null; }

        await using (stream)
        {
            var result = await backupService.AddFromBackupAsync(stream, profileIds: null);
            await dialogService.ShowAlertAsync("Yedekten Eklendi", $"{result.Added.Count} profil eklendi.");
            return result;
        }
    }
}
