using System.Text.Json;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Son başarılı yedeğin kaydını (<c>backup-state.json</c>) okur ve yazar. Gece yedeğinin
/// "değişiklik yoksa yazma" kararı bu kayda dayanır; kayıt bozulursa yedek durmamalı, yeniden
/// alınmalıdır.
/// </summary>
internal static class BackupStateFile
{
    /// <summary>
    /// Durum dosyasının uygulama veri kökündeki adı.
    /// </summary>
    public const string FileName = "backup-state.json";

    /// <summary>
    /// Kaydı okur; dosya yoksa ya da okunamıyorsa <see langword="null"/> döner.
    /// </summary>
    public static async Task<BackupState?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync(
                stream,
                BackupJsonContext.Default.BackupState,
                cancellationToken);
        }
        catch (JsonException)
        {
            // Bozuk kayıt "hiç yedek yok" sayılır: gece yedeği durmaz, yeni yedek alınır.
            return null;
        }
    }

    /// <summary>
    /// Kaydı önce geçici dosyaya yazar, sonra yerine taşır: yazma yarıda kesilirse önceki kayıt
    /// bozulmadan kalır.
    /// </summary>
    public static async Task WriteAsync(string path, BackupState state, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                state,
                BackupJsonContext.Default.BackupState,
                cancellationToken);
        }

        File.Move(temporaryPath, path, overwrite: true);
    }
}
