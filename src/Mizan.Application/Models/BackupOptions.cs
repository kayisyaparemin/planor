namespace Mizan.Application.Models;

/// <summary>
/// Yedekleme servisinin geçici çalışma dizinini ve saklanacak azami yedek sayısını belirleyen ayarlar.
/// Geçici zip dosyası oluşturma konumunu ve temizleme politikasını yapılandırmak için vardır.
/// </summary>
public sealed record BackupOptions(string WorkingDirectory, int KeepCount = 7);
