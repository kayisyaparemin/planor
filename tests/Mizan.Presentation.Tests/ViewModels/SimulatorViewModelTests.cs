using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Simülatörün deneme listesi (EK-V10 S3, S76-4, 5): okuma, aç/kapa, ekleme seçicisi, düzenleme ve silme.
/// Liste her değişiklikte çalışma listesine yazılır; sorunlu denemenin anahtarı kilitlidir.
/// </summary>
public sealed class SimulatorViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    private readonly FakeSimulationWorkflowService _service = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SimulatorViewModel _viewModel;

    public SimulatorViewModelTests()
    {
        _viewModel = new SimulatorViewModel(_service, _navigation, _dialog);
    }

    [Fact]
    public async Task Load_ListeBos_IcerikGosterirDenemeYok()
    {
        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsContent);
        Assert.False(_viewModel.HasConditions);
        Assert.Empty(_viewModel.Conditions);
    }

    [Fact]
    public async Task Load_IkiDeneme_SatirlarKurulduguSirayla()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        var holiday = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: false);
        _service.Seed(phone, holiday);

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasConditions);
        Assert.Equal(["Telefon", "Tatil"], _viewModel.Conditions.Select(x => x.Name));
        Assert.Equal([30_000m, 45_000m], _viewModel.Conditions.Select(x => x.Amount));
        Assert.Equal([true, false], _viewModel.Conditions.Select(x => x.IsEnabled));
        Assert.Equal(Today.AddDays(12), _viewModel.Conditions[0].Date);
        Assert.Equal(SimulationScenarioType.CashPurchase, _viewModel.Conditions[0].Type);
    }

    [Fact]
    public async Task Load_TarihiGecenDeneme_AnahtariKilitliVeIsaretli()
    {
        var old = Condition("Kurs", 12_000m, Today.AddDays(-2), isEnabled: true);
        _service.Seed(old);
        _service.Issues[old.Request.ScenarioId] = SimulationConditionIssue.DatePassed;

        await _viewModel.LoadAsync();

        var row = Assert.Single(_viewModel.Conditions);
        Assert.False(row.CanToggle);
        Assert.True(row.HasIssue);
        Assert.Equal(SimulationConditionIssue.DatePassed, row.Issue);
    }

    [Fact]
    public async Task Load_OkumaHatasi_HataDurumunaGecer()
    {
        _service.ThrowOnGet = new InvalidOperationException("okunamadı");

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsError);
    }

    [Fact]
    public async Task Load_AltiDeneme_DordunuGosterirIkisiTasmada()
    {
        _service.Seed(Enumerable.Range(1, 6).Select(i => Condition($"Deneme {i}", 1_000m * i, Today.AddDays(i), true)).ToArray());

        await _viewModel.LoadAsync();

        Assert.Equal(4, _viewModel.Conditions.Count);
        Assert.Equal(2, _viewModel.HiddenCount);
        Assert.True(_viewModel.HasOverflow);
    }

    [Fact]
    public async Task Expand_TasmaAcilinca_HepsiGorunur()
    {
        _service.Seed(Enumerable.Range(1, 6).Select(i => Condition($"Deneme {i}", 1_000m * i, Today.AddDays(i), true)).ToArray());
        await _viewModel.LoadAsync();

        _viewModel.ExpandCommand.Execute(null);

        Assert.Equal(6, _viewModel.Conditions.Count);
        Assert.False(_viewModel.HasOverflow);
    }

    [Fact]
    public async Task Toggle_DenemeKapaninca_ListeKapaliOlarakYazilir()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        var holiday = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: true);
        _service.Seed(phone, holiday);
        await _viewModel.LoadAsync();

        _viewModel.Conditions[0].IsEnabled = false;

        Assert.Equal(1, _service.SaveCount);
        Assert.Equal([false, true], _service.LastSaved!.Select(x => x.IsEnabled));
        Assert.Equal([phone.Request, holiday.Request], _service.LastSaved!.Select(x => x.Request));
    }

    [Fact]
    public async Task Add_NakitOdemeSecilince_FormaSecenekAnahtariylaGider()
    {
        _dialog.NextChooseResponse = SimulationScenarioCatalog.CashPayment.Title;
        await _viewModel.LoadAsync();

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Equal([SimulationScenarioCatalog.CashPayment.Title], _dialog.LastChooseOptions);
        Assert.Equal(Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.Equal(SimulationScenarioCatalog.CashPayment.Key, _navigation.LastParameters![Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task Add_Vazgecilince_HicbirYereGitmez()
    {
        _dialog.NextChooseResponse = null;
        await _viewModel.LoadAsync();

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task Select_Duzenle_FormaDenemeKimligiyleGider()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        _service.Seed(phone);
        await _viewModel.LoadAsync();
        _dialog.NextChooseResponse = "Düzenle";

        await _viewModel.SelectConditionCommand.ExecuteAsync(_viewModel.Conditions[0]);

        Assert.Equal("Sil", _dialog.LastChooseDestruction);
        Assert.Equal(Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.Equal(phone.Request.ScenarioId, _navigation.LastParameters![Routes.ConditionIdParameter]);
    }

    [Fact]
    public async Task Select_Sil_DenemeListedenCikarVeYazilir()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        var holiday = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: false);
        _service.Seed(phone, holiday);
        await _viewModel.LoadAsync();
        _dialog.NextChooseResponse = "Sil";

        await _viewModel.SelectConditionCommand.ExecuteAsync(_viewModel.Conditions[0]);

        Assert.Equal([holiday.Request], _service.LastSaved!.Select(x => x.Request));
        Assert.Equal(["Tatil"], _viewModel.Conditions.Select(x => x.Name));
    }

    [Fact]
    public async Task Select_SonDenemeSilinince_ListeBosalir()
    {
        _service.Seed(Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true));
        await _viewModel.LoadAsync();
        _dialog.NextChooseResponse = "Sil";

        await _viewModel.SelectConditionCommand.ExecuteAsync(_viewModel.Conditions[0]);

        Assert.Empty(_service.LastSaved!);
        Assert.False(_viewModel.HasConditions);
    }

    private static SimulationDraftCondition Condition(string name, decimal amount, DateOnly date, bool isEnabled) =>
        new(new SimulationRequest(SimulationScenarioType.CashPurchase, name, amount, date), isEnabled);
}
