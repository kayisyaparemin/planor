using System.Globalization;
using Mizan.Application.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Profil varlıklarını arayüz kart modeline dönüştüren yardımcı sınıf.
/// </summary>
public static class ProfileCardMapper
{
    /// <summary>
    /// Belirtilen kullanıcı profilinden arayüz kart modeli üretir.
    /// </summary>
    public static ProfileCardItem ToCard(UserProfile profile)
    {
        var initial = string.IsNullOrWhiteSpace(profile.Name)
            ? "P"
            : StringInfo.GetNextTextElement(profile.Name).ToUpper(CultureInfo.CurrentCulture);

        var detail = profile.LastOpenedAt is { } openedAt
            ? $"Son açılış: {openedAt.ToLocalTime():d MMMM yyyy}"
            : "Henüz açılmadı";

        return new ProfileCardItem(profile.Id, profile.Name, initial, detail);
    }
}
