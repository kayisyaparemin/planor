using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Services;
using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Uygulama kabuğunun (AppShell) profil başlığı ve profil değiştirme aksiyonunu
/// MAUI UI katmanından bağımsız olarak yöneten görünüm modeli.
/// </summary>
public sealed partial class AppShellViewModel : ViewModelBase
{
    private readonly ProfileService profileService;
    private readonly INavigationService navigationService;

    [ObservableProperty]
    private string activeProfileName;

    /// <summary>
    /// <see cref="AppShellViewModel"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public AppShellViewModel(ProfileService profileService, INavigationService navigationService)
    {
        this.profileService = profileService;
        this.navigationService = navigationService;
        activeProfileName = profileService.ActiveProfile?.Name ?? string.Empty;
    }

    /// <summary>
    /// Profil seçim ekranına yönlendirme komutunu çalıştırır.
    /// </summary>
    [RelayCommand]
    private Task SwitchProfileAsync() =>
        navigationService.NavigateToAsync(Routes.ProfileSelection);

    /// <summary>
    /// Aktif profil adını günceller.
    /// </summary>
    public void RefreshActiveProfile()
    {
        ActiveProfileName = profileService.ActiveProfile?.Name ?? string.Empty;
    }
}
