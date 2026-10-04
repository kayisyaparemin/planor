using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Nakit akış döneminin çapa günü, serbest yaşam havuzu, projeksiyon faiz varsayımları,
/// hatırlatıcı bildirim modu ve yerel yedekleme aksiyonlarını yöneten görünüm modeli (EK-V13, S79).
/// </summary>
public sealed partial class SettingsViewModel(
    IUserSettingsRepository userSettingsRepository,
    IPaymentReminderService paymentReminderService,
    IBackupService backupService,
    IDialogService dialogService) : ViewModelBase
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private readonly IUserSettingsRepository _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
    private readonly IPaymentReminderService _paymentReminderService = paymentReminderService ?? throw new ArgumentNullException(nameof(paymentReminderService));
    private readonly IBackupService _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
    private readonly IDialogService _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

    private UserSettings _loadedSettings = new();
    private PaymentReminderMode _loadedReminderMode = PaymentReminderMode.Relaxed;
    private bool _isLoadingForm;

    /// <summary>Dönem çapa günü giriş metni.</summary>
    [ObservableProperty] private string anchorDayInput = "10";
    /// <summary>Dönemsel yaşam gideri havuzu giriş metni.</summary>
    [ObservableProperty] private string periodVariableExpenseAllowanceInput = string.Empty;
    /// <summary>Kredi kartı devreden borç faizi giriş metni.</summary>
    [ObservableProperty] private string creditCardCarryInterestRateInput = "5";
    /// <summary>Finansman açığı faizi giriş metni.</summary>
    [ObservableProperty] private string deficitFinancingInterestRateInput = "5";
    /// <summary>Seçili hatırlatıcı bildirim modu.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReminderModeOff), nameof(IsReminderModeRelaxed), nameof(IsReminderModeAggressive))]
    private PaymentReminderMode selectedReminderMode = PaymentReminderMode.Relaxed;
    /// <summary>Son yedek durumu açıklama metni.</summary>
    [ObservableProperty] private string lastBackupText = "Henüz yedek alınmadı.";
    /// <summary>Yedek klasörü erişim izninin olup olmadığı.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsBackupAccess))]
    private bool hasBackupAccess = true;
    /// <summary>Formda kaydedilmemiş değişiklik olup olmadığı.</summary>
    [ObservableProperty] private bool isDirty;

    /// <summary>Yedekleme iznine ihtiyaç duyulup duyulmadığı.</summary>
    public bool NeedsBackupAccess => !HasBackupAccess;
    /// <summary>Bildirim modunun kapalı olup olmadığı.</summary>
    public bool IsReminderModeOff => SelectedReminderMode == PaymentReminderMode.Off;
    /// <summary>Bildirim modunun rahat olup olmadığı.</summary>
    public bool IsReminderModeRelaxed => SelectedReminderMode == PaymentReminderMode.Relaxed;
    /// <summary>Bildirim modunun agresif olup olmadığı.</summary>
    public bool IsReminderModeAggressive => SelectedReminderMode == PaymentReminderMode.Aggressive;
    /// <summary>Uygulama adı sabiti (GK11).</summary>
    public string AppName => "Planör";
    /// <summary>Sürüm dizesi.</summary>
    public string VersionText => "Sürüm 2.0";

    /// <summary>Ayarları ve yedekleme durumunu veri depolarından yükler.</summary>
    [RelayCommand]
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        State = ScreenState.Loading;
        try
        {
            _isLoadingForm = true;
            _loadedSettings = await _userSettingsRepository.GetSettingsAsync(cancellationToken);
            _loadedReminderMode = await _paymentReminderService.GetModeAsync(cancellationToken);

            AnchorDayInput = _loadedSettings.PeriodAnchor.DayOfMonth.ToString(CultureInfo.InvariantCulture);
            PeriodVariableExpenseAllowanceInput = _loadedSettings.PeriodVariableExpenseAllowance == 0m ? string.Empty : _loadedSettings.PeriodVariableExpenseAllowance.ToString("0.##", Tr);
            CreditCardCarryInterestRateInput = (_loadedSettings.CreditCardCarryInterestRate * 100m).ToString("0.##", Tr);
            DeficitFinancingInterestRateInput = (_loadedSettings.DeficitFinancingInterestRate * 100m).ToString("0.##", Tr);
            SelectedReminderMode = _loadedReminderMode;
            HasBackupAccess = _backupService.HasAccess;
            var backupState = await _backupService.GetLastBackupAsync(cancellationToken);
            LastBackupText = backupState is null ? "Henüz yedek alınmadı." : $"Son yedek: {backupState.BackedUpAt.ToLocalTime():d MMMM yyyy HH:mm} · {backupState.FileName}";

            IsDirty = false;
            State = ScreenState.Content;
        }
        catch { State = ScreenState.Error; }
        finally { _isLoadingForm = false; }
    }

    /// <summary>Değiştirilen ayarları ve bildirim modunu doğrular ve kaydeder.</summary>
    [RelayCommand]
    public async Task SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) { return; }

        var error = SettingsFormParser.TryBuildSettings(_loadedSettings, AnchorDayInput, PeriodVariableExpenseAllowanceInput, CreditCardCarryInterestRateInput, DeficitFinancingInterestRateInput, out var updated);
        if (error is not null)
        {
            await _dialogService.ShowAlertAsync("Geçersiz Ayar", error);
            return;
        }

        try
        {
            SetBusy(true, "Kaydediliyor...");
            await _userSettingsRepository.SaveSettingsAsync(updated, cancellationToken);
            _loadedSettings = updated;
            if (SelectedReminderMode != _loadedReminderMode)
            {
                await _paymentReminderService.SaveModeAsync(SelectedReminderMode, cancellationToken);
                _loadedReminderMode = SelectedReminderMode;
            }
            IsDirty = false;
            await _dialogService.ShowAlertAsync("Kaydedildi", "Ayarlar başarıyla kaydedildi.");
        }
        catch (Exception ex) { await _dialogService.ShowAlertAsync("Hata", ex.Message); }
        finally { SetBusy(false); }
    }

    /// <summary>Hatırlatıcı bildirim modunu günceller.</summary>
    [RelayCommand]
    public Task SetReminderModeAsync(PaymentReminderMode mode)
    {
        SelectedReminderMode = mode;
        RefreshDirty();
        return Task.CompletedTask;
    }

    /// <summary>Yedekleme depolama klasörü erişim iznini talep eder.</summary>
    [RelayCommand]
    public async Task RequestBackupAccessAsync()
    {
        await _backupService.RequestAccessAsync();
        HasBackupAccess = _backupService.HasAccess;
    }

    /// <summary>Kullanıcının anlık isteğiyle yerel yedek alır.</summary>
    [RelayCommand]
    public async Task BackUpNowAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) { return; }
        try
        {
            SetBusy(true, "Yedekleniyor...");
            var result = await _backupService.BackUpNowAsync(cancellationToken);
            HasBackupAccess = _backupService.HasAccess;
            if (result.Outcome == BackupOutcome.Created && result.State is not null)
            {
                LastBackupText = $"Son yedek: {result.State.BackedUpAt.ToLocalTime():d MMMM yyyy HH:mm} · {result.State.FileName}";
                await _dialogService.ShowAlertAsync("Yedeklendi", $"Yedek başarıyla alındı:\n{result.State.FileName}");
            }
            else
            {
                var msg = result.Outcome == BackupOutcome.NoAccess ? "Yedek klasörüne erişim izni yok." : "Yedekleme tamamlandı.";
                await _dialogService.ShowAlertAsync("Yedekleme", msg);
            }
        }
        catch (Exception ex) { await _dialogService.ShowAlertAsync("Yedekleme Hatası", ex.Message); }
        finally { SetBusy(false); }
    }

    partial void OnAnchorDayInputChanged(string value) => RefreshDirty();
    partial void OnPeriodVariableExpenseAllowanceInputChanged(string value) => RefreshDirty();
    partial void OnCreditCardCarryInterestRateInputChanged(string value) => RefreshDirty();
    partial void OnDeficitFinancingInterestRateInputChanged(string value) => RefreshDirty();
    partial void OnSelectedReminderModeChanged(PaymentReminderMode value) => RefreshDirty();

    private void RefreshDirty()
    {
        if (_isLoadingForm) { return; }
        var currentDay = int.TryParse(AnchorDayInput.Trim(), out var d) ? d : 0;
        var currentAllowance = SettingsFormParser.TryParseDecimal(PeriodVariableExpenseAllowanceInput, out var a) ? a : 0m;
        var currentCardRate = SettingsFormParser.TryParseDecimal(CreditCardCarryInterestRateInput, out var c) ? c / 100m : 0m;
        var currentDeficitRate = SettingsFormParser.TryParseDecimal(DeficitFinancingInterestRateInput, out var f) ? f / 100m : 0m;

        IsDirty = currentDay != _loadedSettings.PeriodAnchor.DayOfMonth ||
                  currentAllowance != _loadedSettings.PeriodVariableExpenseAllowance ||
                  currentCardRate != _loadedSettings.CreditCardCarryInterestRate ||
                  currentDeficitRate != _loadedSettings.DeficitFinancingInterestRate ||
                  SelectedReminderMode != _loadedReminderMode;
    }
}
