using Android.App;
using Android.Content;
using Android.Provider;
using Mizan.Application.Abstractions;
using Mizan.Presentation.Services;

namespace Mizan.App.Platforms.Android;

/// <summary>
/// Android ortamında sistem dosya seçicisini Mizan klasöründe açan veya depodaki yedeği okuyan somut adaptör.
/// </summary>
public sealed class AndroidBackupFilePicker(IBackupStorage backupStorage) : IBackupFilePicker
{
    /// <inheritdoc />
    public async Task<Stream?> PickAndOpenAsync()
    {
        var intent = new Intent(Intent.ActionOpenDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("*/*");
        intent.PutExtra(
            Intent.ExtraMimeTypes,
            ["application/zip", "application/x-zip-compressed", "application/octet-stream"]);
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            intent.PutExtra(
                DocumentsContract.ExtraInitialUri,
                DocumentsContract.BuildDocumentUri(
                    "com.android.externalstorage.documents",
                    $"primary:{AndroidStorageAccess.FolderName}"));
        }

        var (resultCode, data) = await ActivityResults.StartAsync(intent);
        if (resultCode != Result.Ok || data?.Data is not { } uri)
        {
            return null;
        }

        return Platform.AppContext.ContentResolver?.OpenInputStream(uri) ??
               throw new IOException("Seçilen dosya açılamadı.");
    }

    /// <inheritdoc />
    public Task<Stream> OpenStoredAsync(string fileName)
    {
        return backupStorage.OpenReadAsync(fileName);
    }
}
