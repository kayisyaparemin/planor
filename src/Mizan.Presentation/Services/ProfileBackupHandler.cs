using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;

namespace Mizan.Presentation.Services;

/// <summary>
/// Yedek dosyasını seçme, geri yükleme ve yedekten profil ekleme diyalog orkestrasyonunu yürüten servis.
/// </summary>
public sealed class ProfileBackupHandler : IProfileBackupHandler
{
    private const string LegacyImportFailedTitle = "İçe Aktarılamadı";
    private const string LegacyImportUnexpectedMessage =
        "Eski yedek içe aktarılamadı. Dosyayı kontrol edip tekrar dene.";

    private readonly IBackupService backupService;
    private readonly ILegacyImportService legacyImportService;
    private readonly IBackupFilePicker filePicker;
    private readonly IDialogService dialogService;

    /// <summary>
    /// <see cref="ProfileBackupHandler"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public ProfileBackupHandler(
        IBackupService backupService,
        ILegacyImportService legacyImportService,
        IBackupFilePicker filePicker,
        IDialogService dialogService)
    {
        this.backupService = backupService;
        this.legacyImportService = legacyImportService;
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

    /// <inheritdoc />
    public async Task<BackupImportResult?> ImportLegacyAsync()
    {
        var stream = await filePicker.PickAndOpenAsync();
        if (stream is null) { return null; }

        try
        {
            await using (stream)
            {
                var result = await legacyImportService.ImportAsync(stream);
                var names = string.Join(", ", result.Added.Select(added => added.Profile.Name));
                await dialogService.ShowAlertAsync(
                    "Eski Uygulamadan Alındı",
                    $"{result.Added.Count} profil eklendi: {names}. Eski simülasyon taslakları taşınmaz.");
                return result;
            }
        }
        catch (InvalidOperationException ex)
        {
            await dialogService.ShowAlertAsync(LegacyImportFailedTitle, ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileBackupHandler ERROR] {ex}");
            await dialogService.ShowAlertAsync(LegacyImportFailedTitle, LegacyImportUnexpectedMessage);
        }

        return null;
    }
}
