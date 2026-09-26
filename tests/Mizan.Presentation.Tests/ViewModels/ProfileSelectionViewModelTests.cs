using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Profil seçim ekranının yükleme, oluşturma, oturum açma ve yönetim davranışlarını doğrulayan testler.
/// </summary>
public sealed class ProfileSelectionViewModelTests : IDisposable
{
    private readonly FakeProfileRepository _repo = new();
    private readonly FakeProfileStoreSwitch _storeSwitch = new();
    private readonly SabitSaat _clock = new();
    private readonly FakeProfileBackupHandler _backupHandler = new();
    private readonly FakeNavigationService _nav = new();
    private readonly FakeDialogService _dialog = new();
    private readonly ProfileService _profileService;
    private readonly ProfileSelectionViewModel _viewModel;

    public ProfileSelectionViewModelTests()
    {
        _profileService = new ProfileService(_repo, _storeSwitch, _clock);
        _viewModel = new ProfileSelectionViewModel(_profileService, _backupHandler, _nav, _dialog);
    }

    public void Dispose() => _profileService.Dispose();

    [Fact]
    public async Task YukleAsync_ProfilYoksa_BosDurumVeIsFirstRunTrue()
    {
        await _viewModel.LoadAsync();

        Assert.Empty(_viewModel.Profiles);
        Assert.True(_viewModel.IsFirstRun);
        Assert.False(_viewModel.HasProfiles);
        Assert.Equal(ScreenState.Empty, _viewModel.State);
    }

    [Fact]
    public async Task YukleAsync_ProfilVarsa_ProfilleriListelerVeIcerikDurumunaGecer()
    {
        await _repo.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Ev Bütçesi" });

        await _viewModel.LoadAsync();

        Assert.Single(_viewModel.Profiles);
        Assert.Equal("Ev Bütçesi", _viewModel.Profiles[0].Name);
        Assert.False(_viewModel.CanDeleteProfiles);
        Assert.Equal(ScreenState.Content, _viewModel.State);
    }

    [Fact]
    public async Task YukleAsync_CokluProfilVarsa_SilmeSecenegiAktiftir()
    {
        await _repo.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Profil 1" });
        await _repo.SaveProfileAsync(new UserProfile { Id = Guid.NewGuid(), Name = "Profil 2" });

        await _viewModel.LoadAsync();

        Assert.Equal(2, _viewModel.Profiles.Count);
        Assert.True(_viewModel.CanDeleteProfiles);
    }

    [Fact]
    public async Task ProfiliAcAsync_SecilenProfiliAcarVeDashboardaGezinir()
    {
        var id = Guid.NewGuid();
        await _repo.SaveProfileAsync(new UserProfile { Id = id, Name = "Ev Bütçesi" });
        await _viewModel.LoadAsync();

        await _viewModel.OpenProfileCommand.ExecuteAsync(_viewModel.Profiles[0]);

        Assert.Equal(id, _profileService.ActiveProfile?.Id);
        Assert.Equal(Routes.Dashboard, _nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task YeniProfilOlusturAsync_GecerliAdGirildiginde_OlustururAcarVeGezinir()
    {
        _dialog.NextPromptResponse = "İş Bütçesi";

        await _viewModel.CreateProfileCommand.ExecuteAsync(null);

        Assert.Equal("İş Bütçesi", _profileService.ActiveProfile?.Name);
        Assert.Equal(Routes.Dashboard, _nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task YeniProfilOlusturAsync_IptalEdilirse_ProfilOlusturmaz()
    {
        _dialog.NextPromptResponse = null;

        await _viewModel.CreateProfileCommand.ExecuteAsync(null);

        Assert.Null(_profileService.ActiveProfile);
        Assert.Null(_nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task ProfilSecenekleriAsync_AdDegistirSecilirse_AdiGuncellerVeListeyiYeniler()
    {
        var id = Guid.NewGuid();
        await _repo.SaveProfileAsync(new UserProfile { Id = id, Name = "Eski Ad" });
        await _viewModel.LoadAsync();
        _dialog.NextChooseResponse = "Adını Değiştir";
        _dialog.NextPromptResponse = "Yeni Ad";

        await _viewModel.ShowProfileOptionsCommand.ExecuteAsync(_viewModel.Profiles[0]);

        var updated = Assert.Single(_viewModel.Profiles);
        Assert.Equal("Yeni Ad", updated.Name);
    }

    [Fact]
    public async Task ProfilSecenekleriAsync_SilSecilipOnaylanirsa_ProfiliSilerVeListeyiYeniler()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await _repo.SaveProfileAsync(new UserProfile { Id = id1, Name = "Profil 1", CreatedAt = _clock.UtcNow });
        await _repo.SaveProfileAsync(new UserProfile { Id = id2, Name = "Profil 2", CreatedAt = _clock.UtcNow.AddMinutes(1) });
        await _viewModel.LoadAsync();
        _dialog.NextChooseResponse = "Sil";
        _dialog.NextConfirmResponse = true;

        await _viewModel.ShowProfileOptionsCommand.ExecuteAsync(_viewModel.Profiles[0]);

        Assert.Single(_viewModel.Profiles);
        Assert.Equal("Profil 2", _viewModel.Profiles[0].Name);
    }

    [Fact]
    public async Task YedektenGeriYukleAsync_YedekSecildiginde_ProfilleriGeriYukler()
    {
        _backupHandler.NextRestoreSummary = new BackupSummary(
            DateTimeOffset.UtcNow,
            [new BackupProfile(Guid.NewGuid(), "Yedek Profil", DateTimeOffset.UtcNow, null)]);

        await _viewModel.RestoreFromBackupCommand.ExecuteAsync(null);

        Assert.True(_backupHandler.RestoreCalled);
    }

    [Fact]
    public async Task YedektenEkleAsync_YedekSecildiginde_YeniProfilleriEkler()
    {
        _backupHandler.NextAddResult = new BackupImportResult(
            new BackupSummary(DateTimeOffset.UtcNow, []),
            [new ImportedProfile(new UserProfile { Id = Guid.NewGuid(), Name = "Eklenen" }, false)]);

        await _viewModel.AddFromBackupCommand.ExecuteAsync(null);

        Assert.True(_backupHandler.AddCalled);
    }
}
