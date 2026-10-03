using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Simülatörün deneme formu, nakit ödeme (EK-V10, S76-3, 7, 8): yeni deneme, düzenleme, doğrulama, çalışma
/// listesine yazma ve kaydedilmemiş değişiklikte çıkış onayı.
/// </summary>
public sealed class SimulationConditionViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    private readonly FakeSimulationWorkflowService _service = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SimulationConditionViewModel _viewModel;

    public SimulationConditionViewModelTests()
    {
        _viewModel = new SimulationConditionViewModel(_service, new SabitSaat(Today), _navigation, _dialog);
    }

    [Fact]
    public async Task Load_YeniNakitOdeme_BosAlanlarVeBugunleAcar()
    {
        _viewModel.Prepare(SimulationScenarioCatalog.CashPayment.Key, null);

        await _viewModel.LoadAsync();

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(SimulationScenarioType.CashPurchase, _viewModel.ScenarioType);
        Assert.Equal(string.Empty, _viewModel.Name);
        Assert.Equal(string.Empty, _viewModel.AmountInput);
        Assert.Equal(Today, _viewModel.Date);
        Assert.False(_viewModel.HasChanges);
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Save_YeniDeneme_ListeninSonunaAcikEklenirVeGeriDoner()
    {
        var existing = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: false);
        _service.Seed(existing);
        await OpenNewAsync();
        _viewModel.Name = "  Telefon ";
        _viewModel.AmountInput = "30.000";
        _viewModel.Date = Today.AddDays(12);

        await _viewModel.SaveAsync();

        var saved = _service.LastSaved!;
        Assert.Equal(2, saved.Count);
        Assert.Equal(existing, saved[0]);
        var added = saved[1];
        Assert.True(added.IsEnabled);
        Assert.Equal(SimulationScenarioType.CashPurchase, added.Request.Type);
        Assert.Equal("Telefon", added.Request.Name);
        Assert.Equal(30_000m, added.Request.Amount);
        Assert.Equal(Today.AddDays(12), added.Request.StartDate);
        Assert.NotEqual(Guid.Empty, added.Request.ScenarioId);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_AdBos_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.AmountInput = "30.000";

        await _viewModel.SaveAsync();

        Assert.Equal("Denemeye bir ad ver.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_TutarSifir_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "0";

        await _viewModel.SaveAsync();

        Assert.Equal("Tutarı sıfırdan büyük bir sayı olarak gir.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Save_TarihDun_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "30.000";
        _viewModel.Date = Today.AddDays(-1);

        await _viewModel.SaveAsync();

        Assert.Equal(SimulationConditionRules.PastDateMessage, _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Load_Duzenleme_AlanlariDoldurur()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        _service.Seed(phone);
        _viewModel.Prepare(null, phone.Request.ScenarioId);

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Telefon", _viewModel.Name);
        Assert.Equal("30000", _viewModel.AmountInput);
        Assert.Equal(Today.AddDays(12), _viewModel.Date);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Save_Duzenleme_YerindeDegisirKimlikVeKapaliHaliKorunur()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: false);
        var holiday = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: true);
        _service.Seed(phone, holiday);
        _viewModel.Prepare(null, phone.Request.ScenarioId);
        await _viewModel.LoadAsync();
        _viewModel.AmountInput = "35.000";

        await _viewModel.SaveAsync();

        Assert.Equal(["Telefon", "Tatil"], _service.LastSaved!.Select(x => x.Request.Name));
        var edited = _service.LastSaved![0];
        Assert.False(edited.IsEnabled);
        Assert.Equal(phone.Request.ScenarioId, edited.Request.ScenarioId);
        Assert.Equal(35_000m, edited.Request.Amount);
    }

    [Fact]
    public async Task Save_TekSeferlikOdemeDuzenlenince_TuruKorunur()
    {
        var payment = new SimulationDraftCondition(
            new SimulationRequest(SimulationScenarioType.FutureOneTimePayment, "Aidat", 8_000m, Today.AddDays(5)));
        _service.Seed(payment);
        _viewModel.Prepare(null, payment.Request.ScenarioId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Yıllık aidat";

        await _viewModel.SaveAsync();

        Assert.Equal(SimulationScenarioType.FutureOneTimePayment, _service.LastSaved![0].Request.Type);
    }

    [Fact]
    public async Task Load_DenemeBulunamazsa_UyarirVeGeriDoner()
    {
        _viewModel.Prepare(null, Guid.NewGuid());

        await _viewModel.LoadAsync();

        Assert.Equal(("Deneme bulunamadı", "Deneme silinmiş olabilir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_OkumaHatasi_HataDurumunaGecer()
    {
        _service.ThrowOnGet = new InvalidOperationException("okunamadı");
        _viewModel.Prepare(null, Guid.NewGuid());

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsError);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarken_OnaySorarKalirsaDonmez()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelAsync();

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikYokken_SormadanDoner()
    {
        await OpenNewAsync();

        await _viewModel.CancelAsync();

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    private async Task OpenNewAsync()
    {
        _viewModel.Prepare(SimulationScenarioCatalog.CashPayment.Key, null);
        await _viewModel.LoadAsync();
    }

    private static SimulationDraftCondition Condition(string name, decimal amount, DateOnly date, bool isEnabled) =>
        new(new SimulationRequest(SimulationScenarioType.CashPurchase, name, amount, date), isEnabled);
}
