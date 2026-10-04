using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Ayarlar ekranı görünüm modeli (EK-V13, S79, GS33) birim testleri.
/// </summary>
public sealed class SettingsViewModelTests
{
    private readonly FakeUserSettingsRepository _userSettingsRepository = new();
    private readonly FakePaymentReminderService _paymentReminderService = new();
    private readonly FakeBackupService _backupService = new();
    private readonly FakeDialogService _dialogService = new();
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelTests()
    {
        _viewModel = new SettingsViewModel(
            _userSettingsRepository,
            _paymentReminderService,
            _backupService,
            _dialogService);
    }

    [Fact]
    public async Task Yukle_AyarlariVeBildirimModunuDoldurur()
    {
        _userSettingsRepository.Settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15),
            PeriodVariableExpenseAllowance = 25000m,
            CreditCardCarryInterestRate = 0.045m,
            DeficitFinancingInterestRate = 0.06m
        };
        _paymentReminderService.Mode = PaymentReminderMode.Aggressive;

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal("15", _viewModel.AnchorDayInput);
        Assert.Equal("25000", _viewModel.PeriodVariableExpenseAllowanceInput.Replace(".", "").Replace(",", ""));
        Assert.Equal("4.5", _viewModel.CreditCardCarryInterestRateInput.Replace(",", "."));
        Assert.Equal("6", _viewModel.DeficitFinancingInterestRateInput.Replace(",", "."));
        Assert.Equal(PaymentReminderMode.Aggressive, _viewModel.SelectedReminderMode);
        Assert.True(_viewModel.IsReminderModeAggressive);
        Assert.False(_viewModel.IsReminderModeOff);
        Assert.False(_viewModel.IsDirty);
    }

    [Fact]
    public async Task Yukle_VeriTabaniHatasiOlursa_HataDurumuGosterir()
    {
        var throwingRepo = new ThrowingUserSettingsRepository();
        var vm = new SettingsViewModel(
            throwingRepo,
            _paymentReminderService,
            _backupService,
            _dialogService);

        await vm.LoadAsync();

        Assert.Equal(ScreenState.Error, vm.State);
    }

    [Fact]
    public async Task Degisiklik_IsDirty_DurumunuGunceller()
    {
        _userSettingsRepository.Settings = new UserSettings { PeriodAnchor = new PeriodAnchor(10) };
        await _viewModel.LoadAsync();
        Assert.False(_viewModel.IsDirty);

        _viewModel.AnchorDayInput = "20";

        Assert.True(_viewModel.IsDirty);
    }

    [Fact]
    public async Task Kaydet_GecerliDegerlerle_AyarlariVeBildirimModunuKaydeder()
    {
        await _viewModel.LoadAsync();
        _viewModel.AnchorDayInput = "20";
        _viewModel.PeriodVariableExpenseAllowanceInput = "30000";
        _viewModel.CreditCardCarryInterestRateInput = "4.25";
        _viewModel.DeficitFinancingInterestRateInput = "5.5";
        await _viewModel.SetReminderModeAsync(PaymentReminderMode.Aggressive);

        await _viewModel.SaveSettingsAsync();

        Assert.Equal(20, _userSettingsRepository.Settings.PeriodAnchor.DayOfMonth);
        Assert.Equal(30000m, _userSettingsRepository.Settings.PeriodVariableExpenseAllowance);
        Assert.Equal(0.0425m, _userSettingsRepository.Settings.CreditCardCarryInterestRate);
        Assert.Equal(0.055m, _userSettingsRepository.Settings.DeficitFinancingInterestRate);
        Assert.Equal(PaymentReminderMode.Aggressive, _paymentReminderService.Mode);
        Assert.False(_viewModel.IsDirty);
        Assert.NotNull(_dialogService.LastAlertMessage);
    }

    [Fact]
    public async Task Kaydet_GecersizGunIcin_KaydetmezVeUyarir()
    {
        await _viewModel.LoadAsync();
        _viewModel.AnchorDayInput = "35"; // Geçersiz: 1..31 olmalı

        await _viewModel.SaveSettingsAsync();

        Assert.Equal(10, _userSettingsRepository.Settings.PeriodAnchor.DayOfMonth);
        Assert.NotNull(_dialogService.LastAlertMessage);
    }

    [Fact]
    public async Task Kaydet_GecersizFaizIcin_KaydetmezVeUyarir()
    {
        await _viewModel.LoadAsync();
        _viewModel.CreditCardCarryInterestRateInput = "150"; // %150 > %100

        await _viewModel.SaveSettingsAsync();

        Assert.Equal(0.05m, _userSettingsRepository.Settings.CreditCardCarryInterestRate);
        Assert.NotNull(_dialogService.LastAlertMessage);
    }

    [Fact]
    public async Task SimdiYedekle_BasariliYedekAlir()
    {
        await _viewModel.BackUpNowAsync();

        Assert.NotNull(_dialogService.LastAlertMessage);
    }

    [Fact]
    public async Task YedekIzniTalepEt_ErisimDurumunuGunceller()
    {
        _backupService.HasAccess = false;
        _viewModel.HasBackupAccess = false;

        _backupService.HasAccess = true;
        await _viewModel.RequestBackupAccessAsync();

        Assert.True(_viewModel.HasBackupAccess);
        Assert.False(_viewModel.NeedsBackupAccess);
    }

    private sealed class ThrowingUserSettingsRepository : IUserSettingsRepository
    {
        public Task<UserSettings> GetSettingsAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simüle edilmiş veritabanı okuma hatası.");

        public Task SaveSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simüle edilmiş veritabanı yazma hatası.");
    }
}
