using Mizan.Application.Services;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Düzenli gelir formu: yeni gelir ve düzenleme, yeni gelirin ilk tutarının bugünden ve gelirle tek
/// işlemde yazılması, düzenlemenin tutar geçmişine dokunmaması ve kaydetmeden çıkış onayı
/// (EK-V6d, S67-2, S67-3). Testler sahte depo üstünde gerçek gelir servisiyle çalışır.
/// </summary>
public sealed class IncomeFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);

    private readonly FakeRecurringIncomeRepository _repository = new();
    private readonly FakePlanChangeRecorder _recorder = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly IncomeFormViewModel _viewModel;

    public IncomeFormViewModelTests()
    {
        var clock = new SabitSaat(Today);
        var service = new IncomePlanService(_repository, new FakeAdHocIncomeRepository(), _recorder, clock);
        _viewModel = new IncomeFormViewModel(_repository, service, _navigation, _dialog, clock);
    }

    [Fact]
    public async Task Load_Duzenlemede_YalnizBuGelirinTutarlariniYukler()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        var current = AddAmount(income.Id, 12_500m, new DateOnly(2026, 7, 20));
        AddAmount(Guid.NewGuid(), 9_000m, new DateOnly(2026, 7, 5));

        await _viewModel.LoadAsync(income.Id);

        Assert.Equal([current.Id], _viewModel.Amounts.Items.Select(r => r.Id));
    }

    [Fact]
    public async Task Save_TutarEklenipPlanliDegisiklikSilinince_GelirleTekIslemdeYazilir()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        var current = AddAmount(income.Id, 12_500m, new DateOnly(2026, 7, 20));
        var planned = AddAmount(income.Id, 14_000m, new DateOnly(2027, 1, 20));
        await _viewModel.LoadAsync(income.Id);
        _dialog.NextChooseResponse = "Sil";
        await _viewModel.Amounts.SelectCommand.ExecuteAsync(_viewModel.Amounts.Items[1]);
        _viewModel.Amounts.OpenEntryCommand.Execute(null);
        _viewModel.Amounts.AmountInput = "15.000";
        _viewModel.Amounts.EntryDate = new DateOnly(2027, 2, 20);
        await _viewModel.Amounts.AddCommand.ExecuteAsync(null);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(
            [(current.Id, 12_500m), (_repository.Amounts[^1].Id, 15_000m)],
            _repository.Amounts.Select(x => (x.Id, x.Amount)));
        Assert.DoesNotContain(_repository.Amounts, x => x.Id == planned.Id);
        Assert.Equal(1, _repository.CombinedWriteCount);
        Assert.Equal(1, _recorder.ChangeCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_SonTutarSilinipYenisiEklenmezse_UyariVerirKaydetmez()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        AddAmount(income.Id, 1_250m, Today);
        await _viewModel.LoadAsync(income.Id);
        _dialog.NextChooseResponse = "Sil";
        await _viewModel.Amounts.SelectCommand.ExecuteAsync(_viewModel.Amounts.Items[0]);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Gelir kaydedilemedi", "Gelirin en az bir tutarı kalmalı."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Single(_repository.Amounts);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_YalnizTutarDegistiyse_OnaySorar()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        AddAmount(income.Id, 12_500m, new DateOnly(2026, 7, 20));
        AddAmount(income.Id, 14_000m, new DateOnly(2027, 1, 20));
        await _viewModel.LoadAsync(income.Id);
        _dialog.NextChooseResponse = "Sil";
        await _viewModel.Amounts.SelectCommand.ExecuteAsync(_viewModel.Amounts.Items[1]);
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasChanges);
        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_Kimliksiz_BosYeniGelirFormuAcarTutarSorar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.True(_viewModel.Fields.ShowsAmount);
        Assert.Equal(string.Empty, _viewModel.Fields.Name);
        Assert.Equal(string.Empty, _viewModel.Fields.AmountInput);
        Assert.Equal(string.Empty, _viewModel.Fields.PaymentDayInput);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_GelirKimligiyle_AlanlariDoldururTutarSormaz()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });

        await _viewModel.LoadAsync(income.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.False(_viewModel.Fields.ShowsAmount);
        Assert.Equal("Kira geliri", _viewModel.Fields.Name);
        Assert.Equal("20", _viewModel.Fields.PaymentDayInput);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_GelirBulunamazsa_UyariVerirGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal("Gelir bulunamadı", _dialog.LastAlertTitle);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_OkumaHatasinda_HataDurumunaDuser()
    {
        _repository.ReadException = new IOException("disk");

        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Retry_OkumaHatasindanSonra_AyniGeliriYukler()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        _repository.ReadException = new IOException("disk");
        await _viewModel.LoadAsync(income.Id);
        _repository.ReadException = null;

        await _viewModel.RetryCommand.ExecuteAsync(null);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal("Kira geliri", _viewModel.Fields.Name);
    }

    [Fact]
    public async Task Save_YeniGelir_IlkTutarBugundenGelirleTekIslemdeYazilirGeriDonulur()
    {
        await _viewModel.LoadAsync(null);
        Fill(" Kira geliri ", "12.500", "20");

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var income = Assert.Single(_repository.Incomes.Values);
        Assert.Equal(("Kira geliri", 20, true), (income.Name, income.PaymentDay, income.IsActive));
        var amount = Assert.Single(_repository.Amounts);
        Assert.Equal((income.Id, 12_500m, Today), (amount.RecurringIncomeId, amount.Amount, amount.EffectiveDate));
        Assert.Equal(1, _repository.CombinedWriteCount);
        Assert.Equal(1, _recorder.ChangeCount);
        Assert.True(_navigation.NavigateBackCalled);
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task Save_Duzenleme_AdVeGunDegisirTutarGecmisineDokunulmaz()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        var first = new IncomeAmountHistory { RecurringIncomeId = income.Id, Amount = 12_500m, EffectiveDate = new DateOnly(2026, 1, 1) };
        _repository.Amounts.Add(first);
        await _viewModel.LoadAsync(income.Id);
        _viewModel.Fields.Name = "Dükkân kirası";
        _viewModel.Fields.PaymentDayInput = "5";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_repository.Incomes.Values);
        Assert.Equal((income.Id, "Dükkân kirası", 5), (saved.Id, saved.Name, saved.PaymentDay));
        Assert.Equal([first], _repository.Amounts);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Theory]
    [InlineData("", "12.500", "20", "Lütfen gelirin adını gir.")]
    [InlineData("Kira", "", "20", "Lütfen geçerli bir aylık net tutar gir.")]
    [InlineData("Kira", "0", "20", "Lütfen geçerli bir aylık net tutar gir.")]
    [InlineData("Kira", "abc", "20", "Lütfen geçerli bir aylık net tutar gir.")]
    [InlineData("Kira", "12.500", "", "Ödeme günü 1 ile 31 arasında olmalıdır.")]
    [InlineData("Kira", "12.500", "32", "Ödeme günü 1 ile 31 arasında olmalıdır.")]
    public async Task Save_GecersizAlan_UyariVerirKaydetmez(string name, string amount, string day, string message)
    {
        await _viewModel.LoadAsync(null);
        Fill(name, amount, day);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Gelir kaydedilemedi", message), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_repository.Incomes);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_DuzenlemedeGunGecersizse_UyariVerirKaydetmez()
    {
        var income = Add(new RecurringIncome { Name = "Kira geliri", PaymentDay = 20 });
        await _viewModel.LoadAsync(income.Id);
        _viewModel.Fields.PaymentDayInput = "0";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Ödeme günü 1 ile 31 arasında olmalıdır.", _dialog.LastAlertMessage);
        Assert.Equal(20, _repository.Incomes[income.Id].PaymentDay);
    }

    [Fact]
    public async Task Save_YazmaHatasinda_GenelMesajiGosterirFormdaKalir()
    {
        await _viewModel.LoadAsync(null);
        Fill("Kira geliri", "12.500", "20");
        _repository.WriteException = new IOException("disk");

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Gelir kaydedilemedi", _dialog.LastAlertTitle);
        Assert.Equal("Gelir kaydedilirken bir sorun oluştu. Tekrar dene.", _dialog.LastAlertMessage);
        Assert.False(_navigation.NavigateBackCalled);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Cancel_DegisiklikYoksa_OnaySormadanGeriDoner()
    {
        await _viewModel.LoadAsync(null);

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarsaVeKalinirsa_FormdaKalir()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = "Kira geliri";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasChanges);
        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    private RecurringIncome Add(RecurringIncome income)
    {
        _repository.Incomes[income.Id] = income;
        return income;
    }

    private IncomeAmountHistory AddAmount(Guid incomeId, decimal amount, DateOnly effectiveDate)
    {
        var history = new IncomeAmountHistory { RecurringIncomeId = incomeId, Amount = amount, EffectiveDate = effectiveDate };
        _repository.Amounts.Add(history);
        return history;
    }

    private void Fill(string name, string amount, string day)
    {
        _viewModel.Fields.Name = name;
        _viewModel.Fields.AmountInput = amount;
        _viewModel.Fields.PaymentDayInput = day;
    }
}
