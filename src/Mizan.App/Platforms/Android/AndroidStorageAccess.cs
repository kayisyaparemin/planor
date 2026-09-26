using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using Mizan.Infrastructure.Backup;
using AndroidEnvironment = Android.OS.Environment;
using AndroidSettings = Android.Provider.Settings;
using AndroidUri = Android.Net.Uri;

namespace Mizan.App.Platforms.Android;

/// <summary>
/// Android harici depolama kök dizinindeki Mizan klasörüne erişim ve izin yönetimini sağlayan somut adaptör.
/// </summary>
public sealed class AndroidStorageAccess : IStorageAccess
{
    /// <summary>
    /// Yedeklerin saklanacağı klasörün adı.
    /// </summary>
    public const string FolderName = "Mizan";

    /// <summary>
    /// Cihazdaki mutlak klasör yolu.
    /// </summary>
    public static string FolderPath
    {
        get
        {
#pragma warning disable CS0618, CA1422 // Kök klasörün yolu için Android API.
            var root = AndroidEnvironment.ExternalStorageDirectory?.AbsolutePath ??
                       "/storage/emulated/0";
#pragma warning restore CS0618, CA1422
            return Path.Combine(root, FolderName);
        }
    }

    /// <inheritdoc />
    public bool HasAccess => OperatingSystem.IsAndroidVersionAtLeast(30)
        ? AndroidEnvironment.IsExternalStorageManager
        : ContextCompat.CheckSelfPermission(
            Platform.AppContext,
            Manifest.Permission.WriteExternalStorage) == Permission.Granted;

    /// <inheritdoc />
    public async Task<bool> RequestAccessAsync()
    {
        if (HasAccess)
        {
            return true;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            try
            {
                await ActivityResults.StartAsync(new Intent(
                    AndroidSettings.ActionManageAppAllFilesAccessPermission,
                    AndroidUri.Parse($"package:{Platform.AppContext.PackageName}")));
            }
            catch (ActivityNotFoundException)
            {
                await ActivityResults.StartAsync(new Intent(
                    AndroidSettings.ActionManageAllFilesAccessPermission));
            }

            return HasAccess;
        }

        var status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.RequestAsync<Permissions.StorageWrite>());
        return status == PermissionStatus.Granted;
    }
}
