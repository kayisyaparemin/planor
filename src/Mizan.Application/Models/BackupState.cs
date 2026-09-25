namespace Mizan.Application.Models;

/// <summary>
/// Uygulamanın aldığı en son yedeğin zamanını, dosya adını ve veri parmak izini tutan durum kaydı.
/// Gece yedekleme görevinde gereksiz mükerrer yedek oluşturulmasını engellemek için vardır.
/// </summary>
public sealed record BackupState(
    DateTimeOffset BackedUpAt,
    string FileName,
    string Fingerprint);
