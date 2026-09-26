using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

public sealed class AppShellViewModelTests
{
    private sealed class FakeClock : IClock
    {
        public DateOnly Today => new(2026, 9, 26);
        public DateTimeOffset UtcNow => new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly List<UserProfile> profiles = [];

        public Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserProfile>>(profiles);

        public Task SaveProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
        {
            profiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            profiles.RemoveAll(p => p.Id == profileId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProfileStoreSwitch : IProfileStoreSwitch
    {
        public Guid? CurrentProfileId { get; private set; }

        public Task OpenAsync(Guid profileId, CancellationToken cancellationToken = default)
        {
            CurrentProfileId = profileId;
            return Task.CompletedTask;
        }

        public Task CloseAsync()
        {
            CurrentProfileId = null;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SwitchProfileCommand_ProfilSecimiRotasinaGider()
    {
        var repo = new FakeProfileRepository();
        var switchStore = new FakeProfileStoreSwitch();
        var clock = new FakeClock();
        using var profileService = new ProfileService(repo, switchStore, clock);

        var navigation = new FakeNavigationService();
        var vm = new AppShellViewModel(profileService, navigation);

        await vm.SwitchProfileCommand.ExecuteAsync(null);

        Assert.Equal(Routes.ProfileSelection, navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task RefreshActiveProfile_AktifProfilAdiniYeniler()
    {
        var repo = new FakeProfileRepository();
        var switchStore = new FakeProfileStoreSwitch();
        var clock = new FakeClock();
        using var profileService = new ProfileService(repo, switchStore, clock);

        var profile = await profileService.CreateAsync("Kişisel Bütçe");
        await profileService.OpenAsync(profile.Id);

        var navigation = new FakeNavigationService();
        var vm = new AppShellViewModel(profileService, navigation);

        Assert.Equal("Kişisel Bütçe", vm.ActiveProfileName);

        await profileService.CloseAsync();
        vm.RefreshActiveProfile();

        Assert.Empty(vm.ActiveProfileName);
    }
}
