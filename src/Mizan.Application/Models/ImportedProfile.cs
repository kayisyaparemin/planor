namespace Mizan.Application.Models;

/// <summary>
/// Yedekten cihaza başarıyla eklenen bir profili ve kopya olarak mı eklendiğini bildiren kayıt.
/// Aynı profil zaten varken içeri aktarılan kaydın orijinalden ayrıştırılabilmesi için vardır.
/// </summary>
public sealed record ImportedProfile(UserProfile Profile, bool IsCopy);
