namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek klasörüne yazma izninin platforma özgü kısmı. Klasör işi (<see cref="FolderBackupStorage"/>)
/// Android olmadan test edilebilsin diye izin sorma ondan ayrılmıştır; Android uygulaması bu arayüzü
/// "Tüm dosyalara erişim" (Android 11+) ya da klasik depolama izniyle (Android 10 ve altı) uygular.
/// </summary>
public interface IStorageAccess
{
    /// <summary>
    /// Yedek klasörüne yazma ve okuma izninin şu anda verili olup olmadığı.
    /// </summary>
    bool HasAccess { get; }

    /// <summary>
    /// İzni kullanıcıdan ister ve istek sonunda iznin verili olup olmadığını döner.
    /// </summary>
    Task<bool> RequestAccessAsync();
}
