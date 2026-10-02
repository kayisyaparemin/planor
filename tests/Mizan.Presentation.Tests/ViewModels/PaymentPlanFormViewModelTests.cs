using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Taksitli ödeme planı formu testleri: açılış, taksit üretimi, doğrulamalar, kaydetme ve çıkış onayı (EK-V6e, S65-3).
/// </summary>
public sealed class PaymentPlanFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    private readonly FakeObligationManagementService _obligations = new();
    private readonly FakePaymentPlanRepository _repository = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly PaymentPlanFormViewModel _viewModel;

    public PaymentPlanFormViewModelTests()
    {
        var clock = new SabitSaat(Today);
        _viewModel = new PaymentPlanFormViewModel(_obligations, _repository, clock, _navigation, _dialog);
    }

    [Fact]
    public async Task Load_YeniPlan_BosAlanlarVeGirisAcikAcar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(string.Empty, _viewModel.Name);
        Assert.True(_viewModel.Installments.IsEntryOpen);
        Assert.Empty(_viewModel.Installments.Items);
        Assert.False(_viewModel.HasChanges);
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Load_KayitliPlan_AlanlariVeTaksitleriDoldurur()
    {
        var plan = new TemporaryPaymentPlan
        {
            Name = "Okul taksiti",
            Installments =
            [
                new TemporaryPaymentInstallment { DueDate = Today.AddDays(10), Amount = 5_000m },
                new TemporaryPaymentInstallment { DueDate = Today.AddMonths(1), Amount = 5_000m }
            ]
        };
        _repository.Plans.Add(plan);

        await _viewModel.LoadAsync(plan.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Okul taksiti", _viewModel.Name);
        Assert.Equal(2, _viewModel.Installments.Items.Count);
        Assert.False(_viewModel.Installments.IsEntryOpen);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KayitBulunamayinca_UyarirVeGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(("Kayıt başarısız", "Düzenlenecek ödeme planı bulunamadı."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_AdBosken_UyarirVeKaydetmez()
    {
        await _viewModel.LoadAsync(null);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Kayıt başarısız", "Ödeme planı adı boş bırakılamaz."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_obligations.SavedPaymentPlans);
    }

    [Fact]
    public async Task Save_GirisAcikkenGecerliTaksitiOtomatikEklerVeKaydeder()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Senetli mobilya";
        _viewModel.Installments.AmountInput = "4000";
        _viewModel.Installments.CountInput = "3";
        _viewModel.Installments.EntryDate = Today.AddDays(5);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_obligations.SavedPaymentPlans);
        Assert.Equal("Senetli mobilya", saved.Name);
        Assert.Equal(3, saved.Installments.Count);
        Assert.All(saved.Installments, i => Assert.Equal(4_000m, i.Amount));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Installments_TaksitSayisiKadarAylikUretir()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Installments.AmountInput = "2500";
        _viewModel.Installments.CountInput = "4";
        _viewModel.Installments.EntryDate = new DateOnly(2026, 10, 31);

        await _viewModel.Installments.AddCommand.ExecuteAsync(null);

        Assert.Equal(4, _viewModel.Installments.Items.Count);
        Assert.Equal(new DateOnly(2026, 10, 31), _viewModel.Installments.Items[0].DueDate);
        Assert.Equal(new DateOnly(2026, 11, 30), _viewModel.Installments.Items[1].DueDate);
        Assert.Equal(new DateOnly(2026, 12, 31), _viewModel.Installments.Items[2].DueDate);
        Assert.Equal(new DateOnly(2027, 1, 31), _viewModel.Installments.Items[3].DueDate);
    }

    [Fact]
    public async Task Installments_OdenmisTaksitiSilmez_Uyarir()
    {
        var plan = new TemporaryPaymentPlan
        {
            Name = "Okul",
            Installments = [new TemporaryPaymentInstallment { Amount = 1000m, DueDate = Today.AddDays(-10), IsPaid = true }]
        };
        _repository.Plans.Add(plan);
        await _viewModel.LoadAsync(plan.Id);

        var row = _viewModel.Installments.Items[0];
        await _viewModel.Installments.SelectCommand.ExecuteAsync(row);

        Assert.Equal(("Ödenmiş taksit", "Ödenmiş taksitler silinemez."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Single(_viewModel.Installments.Items);
    }

    [Fact]
    public async Task Installments_OdenmemisTaksitiSiler()
    {
        var plan = new TemporaryPaymentPlan
        {
            Name = "Okul",
            Installments = [new TemporaryPaymentInstallment { Amount = 1000m, DueDate = Today.AddDays(10), IsPaid = false }]
        };
        _repository.Plans.Add(plan);
        await _viewModel.LoadAsync(plan.Id);

        _dialog.NextChooseResponse = "Sil";
        var row = _viewModel.Installments.Items[0];
        await _viewModel.Installments.SelectCommand.ExecuteAsync(row);

        Assert.Empty(_viewModel.Installments.Items);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarsa_OnaySorar()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Değişiklik";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.Equal("Kaydetmeden çık", _dialog.LastConfirmTitle);
        Assert.False(_navigation.NavigateBackCalled);
    }
}
