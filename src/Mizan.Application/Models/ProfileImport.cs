namespace Mizan.Application.Models;

/// <summary>
/// Yedek arşivindeki bir profilin hedef cihaza hangi kimlik ve ad ile aktarılacağını belirten eşleme kaydı.
/// İçe aktarma sırasında profil kimliğini yenilemek veya çakışan isimleri değiştirmek için vardır.
/// </summary>
public sealed record ProfileImport(Guid SourceId, UserProfile Target);
