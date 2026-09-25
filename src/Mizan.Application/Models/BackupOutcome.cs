namespace Mizan.Application.Models;

/// <summary>
/// Yedekleme operasyonunun nihai sonucunu belirten durum sınıflandırması.
/// Yedekleme isteğinin oluşturulduğunu, değişmediğini veya neden yapılamadığını açıkça raporlamak için vardır.
/// </summary>
public enum BackupOutcome
{
    /// <summary>Yeni bir yedek dosyası başarıyla oluşturuldu.</summary>
    Created,

    /// <summary>Son yedekten beri hiçbir profilde değişiklik olmadığı için dosya yazılmadı.</summary>
    Unchanged,

    /// <summary>Uygulamada yedeklenecek herhangi bir profil bulunmuyor.</summary>
    NothingToBackUp,

    /// <summary>Yedekleme depolama klasörüne erişim izni verilmemiş.</summary>
    NoAccess
}
