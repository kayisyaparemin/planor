using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Profil seçimi, yeni profil oluşturma, ad değiştirme, silme ve yedekten geri yükleme
/// işlemlerini yöneten ekran görünüm modeli.
/// </summary>
public sealed partial class ProfileSelectionViewModel(
    ProfileService profileService,
    IProfileBackupHandler backupHandler,
    INavigationService navigationService,
    IDialogService dialogService) : ViewModelBase
{
    /// <summary>Ekrandaki profil kartları listesi.</summary>
    public ObservableCollection<ProfileCardItem> Profiles { get; } = [];

    /// <summary>En az bir profil bulunup bulunmadığı.</summary>
    public bool HasProfiles => Profiles.Count > 0;

    /// <summary>Birden fazla profil varken silme seçeneğinin aktif olup olmadığı.</summary>
    public bool CanDeleteProfiles => Profiles.Count > 1;

    /// <summary>Uygulama ilk kez kurulmuş ve hiç profil yok mu durumu.</summary>
    public bool IsFirstRun => !IsBusy && Profiles.Count == 0;

    /// <summary>Kayıtlı profilleri depodan yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) { return; }
        try
        {
            SetBusy(true);
            State = ScreenState.Loading;
            await ReloadInternalAsync();
        }
        finally
        {
            SetBusy(false);
            OnPropertyChanged(nameof(IsFirstRun));
        }
    }

    /// <summary>Belirtilen profili açar ve oturumu başlatıp ana sayfaya yönlendirir.</summary>
    [RelayCommand]
    public async Task OpenProfileAsync(ProfileCardItem? profile)
    {
        if (profile is null || IsBusy) { return; }
        try
        {
            SetBusy(true);
            await profileService.OpenAsync(profile.Id);
            await navigationService.NavigateToAsync(Routes.Dashboard);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Kullanıcıdan profil adı alarak yeni bir profil oluşturur ve açar.</summary>
    [RelayCommand]
    public async Task CreateProfileAsync()
    {
        if (IsBusy) { return; }
        var name = await dialogService.PromptAsync(
            "Yeni Profil", "Profilin adı ne olsun? Yeni profil temiz bir başlangıçla açılır.",
            accept: "Oluştur", cancel: "Vazgeç", placeholder: "Örn. Ev Bütçesi",
            maxLength: UserProfile.MaxNameLength);

        if (string.IsNullOrWhiteSpace(name)) { return; }
        try
        {
            SetBusy(true);
            var created = await profileService.CreateAsync(name.Trim());
            await profileService.OpenAsync(created.Id);
            await navigationService.NavigateToAsync(Routes.Dashboard);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Profil üzerinde ad değiştirme veya silme seçenekleri diyaloğunu açar.</summary>
    [RelayCommand]
    public async Task ShowProfileOptionsAsync(ProfileCardItem? profile)
    {
        if (profile is null || IsBusy) { return; }
        var choice = await dialogService.ChooseAsync(
            profile.Name, cancel: "Vazgeç", destruction: CanDeleteProfiles ? "Sil" : null,
            options: "Adını Değiştir");

        if (choice == "Adını Değiştir")
        {
            await RenameProfileAsync(profile);
        }
        else if (choice == "Sil" && CanDeleteProfiles)
        {
            await DeleteProfileAsync(profile);
        }
    }

    /// <summary>İlk kurulumda yedekten tüm profilleri geri yükler.</summary>
    [RelayCommand]
    public Task RestoreFromBackupAsync() =>
        ReloadWhenAddedAsync(async () => await backupHandler.RestoreAsync() is not null);

    /// <summary>Mevcut profiller korunarak yedekten profil ekler.</summary>
    [RelayCommand]
    public Task AddFromBackupAsync() =>
        ReloadWhenAddedAsync(async () => await backupHandler.AddAsync() is not null);

    /// <summary>Eski uygulamanın yedeğinden profilleri mevcutların yanına ekler.</summary>
    [RelayCommand]
    public Task ImportFromLegacyAsync() =>
        ReloadWhenAddedAsync(async () => await backupHandler.ImportLegacyAsync() is not null);

    /// <summary>
    /// Üç yedek akışının ortak iskeleti: meşgulken ikinci dokunuşu yok sayar, akış profil getirdiyse listeyi
    /// yeniler (iptalde dokunmaz).
    /// </summary>
    private async Task ReloadWhenAddedAsync(Func<Task<bool>> backupFlow)
    {
        if (IsBusy) { return; }
        try
        {
            SetBusy(true);
            if (await backupFlow())
            {
                await ReloadInternalAsync();
            }
        }
        finally
        {
            SetBusy(false);
            OnPropertyChanged(nameof(IsFirstRun));
        }
    }

    private async Task RenameProfileAsync(ProfileCardItem profile)
    {
        var name = await dialogService.PromptAsync(
            "Adını Değiştir", "Profilin yeni adı:", accept: "Kaydet", cancel: "Vazgeç",
            placeholder: profile.Name, maxLength: UserProfile.MaxNameLength);

        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name.Trim(), profile.Name, StringComparison.Ordinal))
        {
            await profileService.RenameAsync(profile.Id, name.Trim());
            await ReloadInternalAsync();
        }
    }

    private async Task DeleteProfileAsync(ProfileCardItem profile)
    {
        var confirmed = await dialogService.ConfirmAsync(
            "Profili Sil", $"\"{profile.Name}\" profili kalıcı olarak silinecek. Bu işlem geri alınamaz.",
            accept: "Profili Sil", cancel: "Vazgeç");

        if (confirmed)
        {
            await profileService.DeleteAsync(profile.Id);
            await ReloadInternalAsync();
            OnPropertyChanged(nameof(IsFirstRun));
        }
    }

    private async Task ReloadInternalAsync()
    {
        var list = await profileService.GetProfilesAsync();
        Profiles.Clear();
        foreach (var item in list)
        {
            Profiles.Add(ProfileCardMapper.ToCard(item));
        }

        OnPropertyChanged(nameof(HasProfiles));
        OnPropertyChanged(nameof(CanDeleteProfiles));
        State = Profiles.Count == 0 ? ScreenState.Empty : ScreenState.Content;
    }
}
