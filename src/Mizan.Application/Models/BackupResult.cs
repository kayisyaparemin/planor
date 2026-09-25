namespace Mizan.Application.Models;

/// <summary>
/// Yedekleme operasyonunun sonucunu ve oluşturulan son durum bilgisini içeren yanıt kaydı.
/// UI ve arka plan işlerine operasyonun başarısını ve oluşan dosya durumunu bildirmek için vardır.
/// </summary>
public sealed record BackupResult(
    BackupOutcome Outcome,
    BackupState? State = null);
