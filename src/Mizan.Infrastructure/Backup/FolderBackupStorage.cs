using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedekleri düz bir klasöre yazar. Uygulamanın kendi klasörü uygulama kaldırılınca silindiği için
/// vardır: Android'de bu klasör depolamanın en üstündedir, kaldırmadan sonra da durur ve izin verilince
/// yeniden kurulan uygulama içindekileri okuyabilir. Hangi dosyanın Mizan yedeği olduğuna bu sınıf
/// değil <c>BackupRetentionRules</c> karar verir; klasör deposu klasördeki bütün dosyaları listeler (S59).
/// </summary>
public sealed class FolderBackupStorage(
    string directory,
    string locationDescription,
    IStorageAccess access) : IBackupStorage
{
    private readonly string _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    private readonly string _locationDescription =
        locationDescription ?? throw new ArgumentNullException(nameof(locationDescription));
    private readonly IStorageAccess _access = access ?? throw new ArgumentNullException(nameof(access));

    /// <inheritdoc />
    public string LocationDescription => _locationDescription;

    /// <inheritdoc />
    public bool HasAccess => _access.HasAccess;

    /// <inheritdoc />
    public Task<bool> RequestAccessAsync() => _access.RequestAccessAsync();

    /// <summary>
    /// Kaynak dosyayı klasöre kopyalar; aynı adlı dosyanın üzerine yazar. Kopya önce geçici bir dosyaya
    /// yazılıp sonra yerine taşınır: yarıda kalırsa aynı günün önceki sağlam yedeği bozulmaz ve geride
    /// geçici dosya kalmaz. Adın yalnız dosya adı kısmı kullanılır; ad klasörün dışını gösteremez.
    /// </summary>
    public async Task SaveAsync(string fileName, string sourcePath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var target = PathOf(fileName);
        var temporaryPath = Path.Combine(_directory, $".{Path.GetFileName(target)}.tmp");
        try
        {
            await using (var input = File.OpenRead(sourcePath))
            await using (var output = File.Create(temporaryPath))
            {
                await input.CopyToAsync(output, cancellationToken);
            }

            File.Move(temporaryPath, target, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    /// <summary>
    /// Klasördeki bütün dosyaları son değişiklik zamanlarıyla (UTC) listeler; klasör yoksa boş döner.
    /// </summary>
    public Task<IReadOnlyList<StoredBackup>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StoredBackup> files = Directory.Exists(_directory)
            ? new DirectoryInfo(_directory)
                .EnumerateFiles()
                .Select(file => new StoredBackup(
                    file.Name,
                    new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero)))
                .ToArray()
            : [];
        return Task.FromResult(files);
    }

    /// <summary>
    /// Klasördeki dosyayı salt okunur akış olarak açar.
    /// </summary>
    public Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(File.OpenRead(PathOf(fileName)));

    /// <summary>
    /// Klasördeki dosyayı siler; dosya yoksa sessizce geçer.
    /// </summary>
    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = PathOf(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    // Yalnız dosya adı kullanılır; "../" gibi bir ad klasörün dışına çıkamaz.
    private string PathOf(string fileName) => Path.Combine(_directory, Path.GetFileName(fileName));
}
