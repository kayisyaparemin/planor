using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Tek seferlik gelir formu testleri: yeni gelir açılışı, düzenleme, alan doğrulamaları,
/// kaydetme ve kaydedilmemiş değişiklikte çıkış onayı (EK-V6d, S67-6, S67-7).
/// </summary>
public sealed class AdHocIncomeFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    private readonly FakeAdHocIncomeRepository _repository = new();
    private readonly FakePlanChangeRecorder _recorder = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly AdHocIncomeFormViewModel _viewModel;

    public AdHocIncomeFormViewModelTests()
    {
        var clock = new SabitSaat(Today);
        var service = new IncomePlanService(new FakeRecurringIncomeRepository(), _repository, _recorder, clock);
        _viewModel = new AdHocIncomeFormViewModel(_repository, service, _navigation, _dialog, clock);
    }

    [Fact]
    public async Task Load_YeniGelir_BosAlanlarVeBugununTarihiyleAcar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(string.Empty, _viewModel.Description);
        Assert.Equal(string.Empty, _viewModel.AmountInput);
        Assert.Equal(Today, _viewModel.ExactDate);
        Assert.False(_viewModel.HasChanges);
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Load_KayitliGelir_AlanlariDoldurur()
    {
        var income = new AdHocIncome
        {
            Description = "Yıl sonu primi",
            Amount = 25_000m,
            ExactDate = Today.AddDays(5)
        };
        await _repository.UpsertAdHocIncomeAsync(income);

        await _viewModel.LoadAsync(income.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Yıl sonu primi", _viewModel.Description);
        Assert.Equal("25000", _viewModel.AmountInput);
        Assert.Equal(Today.AddDays(5), _viewModel.ExactDate);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KayitBulunamayinca_UyarirVeGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(("Gelir bulunamadı", "Gelir silinmiş olabilir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_DepoHataVerince_HataDurumunaGecer()
    {
        _repository.ThrowOnGet = new InvalidOperationException("Okuma hatası");

        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.True(_viewModel.IsError);
    }

    [Fact]
    public async Task Save_AciklamaBosken_UyarirVeKaydetmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.AmountInput = "1000";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Gelir kaydedilemedi", "Lütfen bir açıklama gir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_repository.Incomes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-50")]
    [InlineData("abc")]
    public async Task Save_TutarGecersizken_UyarirVeKaydetmez(string invalidAmount)
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Description = "İkramiye";
        _viewModel.AmountInput = invalidAmount;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Gelir kaydedilemedi", "Lütfen geçerli bir tutar gir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_repository.Incomes);
    }

    [Fact]
    public async Task Save_TarihBugundenOnceyse_UyarirVeKaydetmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Description = "İkramiye";
        _viewModel.AmountInput = "5000";
        _viewModel.ExactDate = Today.AddDays(-1);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Gelir kaydedilemedi", "Tarih bugünden önce olamaz."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_repository.Incomes);
    }

    [Fact]
    public async Task Save_YeniGelir_ServiseKaydederVeGeriDoner()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Description = "  Yıl sonu primi  ";
        _viewModel.AmountInput = "15.000";
        _viewModel.ExactDate = Today.AddDays(10);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Single(_repository.Incomes);
        var saved = _repository.Incomes.Values.Single();
        Assert.Equal("Yıl sonu primi", saved.Description);
        Assert.Equal(15_000m, saved.Amount);
        Assert.Equal(Today.AddDays(10), saved.ExactDate);
        Assert.Equal(1, _recorder.ChangeCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_Duzenlemede_GelirinUzerineYazarVeGeriDoner()
    {
        var income = new AdHocIncome
        {
            Description = "Eski Prim",
            Amount = 10_000m,
            ExactDate = Today.AddDays(2)
        };
        await _repository.UpsertAdHocIncomeAsync(income);
        await _viewModel.LoadAsync(income.Id);

        _viewModel.Description = "Güncel Prim";
        _viewModel.AmountInput = "12.000";
        _viewModel.ExactDate = Today.AddDays(4);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var updated = _repository.Incomes[income.Id];
        Assert.Equal("Güncel Prim", updated.Description);
        Assert.Equal(12_000m, updated.Amount);
        Assert.Equal(Today.AddDays(4), updated.ExactDate);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikYokken_OnaysizGeriDoner()
    {
        await _viewModel.LoadAsync(null);

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_navigation.NavigateBackCalled);
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarkenOnaylanirsa_GeriDoner()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Description = "Değişiklik";
        _dialog.NextConfirmResponse = true;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_navigation.NavigateBackCalled);
        Assert.Equal(1, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarkenVazgecilirse_SayfadaKalir()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Description = "Değişiklik";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.False(_navigation.NavigateBackCalled);
        Assert.Equal(1, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task HasChanges_AlanlarDegisinceDogruCevaplar()
    {
        var income = new AdHocIncome
        {
            Description = "Satış",
            Amount = 3_000m,
            ExactDate = Today
        };
        await _repository.UpsertAdHocIncomeAsync(income);
        await _viewModel.LoadAsync(income.Id);
        Assert.False(_viewModel.HasChanges);

        _viewModel.Description = "Araba satışı";
        Assert.True(_viewModel.HasChanges);
        _viewModel.Description = "Satış";
        Assert.False(_viewModel.HasChanges);

        _viewModel.AmountInput = "4.000";
        Assert.True(_viewModel.HasChanges);
        _viewModel.AmountInput = "3000";
        Assert.False(_viewModel.HasChanges);

        _viewModel.ExactDate = Today.AddDays(1);
        Assert.True(_viewModel.HasChanges);
        _viewModel.ExactDate = Today;
        Assert.False(_viewModel.HasChanges);
    }
}
