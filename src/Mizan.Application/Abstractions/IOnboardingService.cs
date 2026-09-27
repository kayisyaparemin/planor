using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Kurulum sihirbazında girilen başlangıç finansal verilerini doğrulayıp
/// ilgili dar veri depolarına atomik olarak kalıcılaştıran ve ilk açık dönem planını donduran servis sözleşmesidir.
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Kurulum sihirbazı taslağını sisteme işler ve ilk resmi finansal durumu dondurur.
    /// </summary>
    /// <param name="draft">Kullanıcının girdiği başlangıç ayar ve enstrüman taslağı.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    Task InitializeFromOnboardingAsync(OnboardingDraft draft, CancellationToken cancellationToken = default);
}
