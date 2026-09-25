namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek alırken (<c>.backup-*</c>) ve geri yüklerken (<c>.restore-*</c>) kullanılan geçici çalışma
/// klasörleri. Uygulama veri kökünde açılırlar ki taşıma aynı birimde kalsın; iş bitince, başarılı
/// olsun olmasın, silinirler.
/// </summary>
internal static class BackupWorkDirectory
{
    /// <summary>
    /// Kökte <paramref name="prefix"/> ile başlayan, adı benzersiz yeni bir klasör açar ve yolunu döner.
    /// </summary>
    public static string Create(string rootDirectory, string prefix)
    {
        var path = Path.Combine(rootDirectory, $"{prefix}{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Klasörü içindekilerle siler; silinemezse sessiz kalır, çünkü temizlik işin sonucunu değiştirmez.
    /// </summary>
    public static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temizlik en iyi çabadır.
        }
        catch (UnauthorizedAccessException)
        {
            // Temizlik en iyi çabadır.
        }
    }
}
