namespace Mizan.Application.Models;

/// <summary>
/// Yedek dosyasının oluşturulma zamanını ve içerdiği profillerin özetini taşıyan kayıt.
/// Yedek dosyasını geri yüklemeden veya profilleri içeri aktarmadan önce kullanıcıya bilgi vermek için vardır.
/// </summary>
public sealed record BackupSummary(
    DateTimeOffset CreatedAt,
    IReadOnlyList<BackupProfile> Profiles)
{
    /// <summary>
    /// Yedekteki profil isimlerinin listesi.
    /// </summary>
    public IReadOnlyList<string> ProfileNames =>
        Profiles.Select(profile => profile.Name).ToArray();
}
