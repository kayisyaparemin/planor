using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;
using static Mizan.Presentation.Tests.ViewModels.CardControlFixture;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kart formu: yeni kart ve düzenleme, formun dokunmadığı alanların korunması, asgari oranın
/// limitten çözülmesi, güncel borcun faizsiz harcama olarak yazılması ve kaydetmeden çıkış onayı
/// (EK-V6b, S63). Testler sahte depo üstünde gerçek kart servisiyle çalışır.
/// </summary>
public sealed class CardFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 27);

    private readonly FakeCreditCardRepository _repository = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly CardFormViewModel _viewModel;

    public CardFormViewModelTests()
    {
        var clock = new SabitSaat(Today);
        var service = new CreditCardObligationService(
            _repository, clock, new CreditCardPaymentPreferenceResolver(), new FakePlanChangeRecorder());
        _viewModel = new CardFormViewModel(_repository, service, _navigation, _dialog, clock);
    }

    [Fact]
    public async Task Load_Kimliksiz_BosYeniKartFormuAcar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.True(_viewModel.Fields.ShowsDebt);
        Assert.Equal(string.Empty, _viewModel.Fields.Name);
        Assert.Equal(string.Empty, _viewModel.Fields.LimitInput);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KartKimligiyle_AlanlariKarttanDoldurur()
    {
        var card = Add(Card("Bonus", carried: 3000m) with { UnbilledSpending = 1250.5m });

        await _viewModel.LoadAsync(card.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Bonus", _viewModel.Fields.Name);
        Assert.Equal("Garanti BBVA", _viewModel.Fields.Bank);
        Assert.Equal("50000", _viewModel.Fields.LimitInput);
        Assert.Equal("4250,5", _viewModel.Fields.DebtInput);
        Assert.Equal("15", _viewModel.Fields.ClosingDayInput);
        Assert.Equal("25", _viewModel.Fields.DueDayInput);
        Assert.True(_viewModel.Fields.ShowsDebt);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KesilmisEkstreliKart_GuncelBorcuGostermez()
    {
        var card = Add(Card("Bonus", statement: Statement(8000m, 1600m), plan: CurrentStatementPaymentMode.Full));

        await _viewModel.LoadAsync(card.Id);

        Assert.True(_viewModel.Fields.HasStatement);
        Assert.False(_viewModel.Fields.ShowsDebt);
    }

    [Fact]
    public async Task Load_KartBulunamazsa_UyariVerirGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_OkumaHatasinda_HataDurumunaDuser()
    {
        _repository.ReadException = new InvalidOperationException("disk");

        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Retry_OkumaHatasindanSonra_AyniKartiYukler()
    {
        var card = Add(Card("Bonus"));
        _repository.ReadException = new InvalidOperationException("disk");
        await _viewModel.LoadAsync(card.Id);
        _repository.ReadException = null;

        await _viewModel.RetryCommand.ExecuteAsync(null);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Bonus", _viewModel.Fields.Name);
    }

    [Theory]
    [InlineData("25.000", 0.20)]
    [InlineData("25.000,01", 0.40)]
    public async Task Save_YeniKart_AsgariOraniLimittenCozer(string limit, decimal expectedRate)
    {
        await FillNewCard(limit: limit);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expectedRate, Assert.Single(_repository.Cards.Values).MinimumPaymentRate);
    }

    [Fact]
    public async Task Save_YeniKart_BorcFaizsizHarcamaOlarakTamamiIleYazilirGeriDonulur()
    {
        await FillNewCard(debt: "4.000");

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_repository.Cards.Values);
        Assert.Equal("Bonus", saved.Name);
        Assert.Equal("Akbank", saved.Bank);
        Assert.Equal(50000m, saved.Limit);
        Assert.Equal(4000m, saved.UnbilledSpending);
        Assert.Equal(0m, saved.CarriedBalance);
        Assert.Equal(Today, saved.BalanceAsOfDate);
        Assert.Equal(1, saved.StatementClosingDay);
        Assert.Equal(31, saved.PaymentDueDay);
        Assert.Equal(CreditCardPaymentStrategy.FullStatement, saved.PaymentStrategy);
        Assert.True(_navigation.NavigateBackCalled);
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task Save_YeniKartBorcBos_SifirYazilir()
    {
        await FillNewCard(debt: string.Empty);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(0m, Assert.Single(_repository.Cards.Values).UnbilledSpending);
    }

    [Fact]
    public async Task Save_Duzenleme_FormunDokunmadigiAlanlariKorur()
    {
        var charge = new CardCharge { Description = "Telefon (3/12)", PostingDate = new DateOnly(2026, 11, 5), Amount = 2000m };
        var plan = new CreditCardPaymentPlan { DueDate = new DateOnly(2026, 11, 25), PaymentType = CreditCardPaymentType.Minimum };
        var card = Add(Card("Bonus", statement: Statement(8000m, 1600m), plan: CurrentStatementPaymentMode.Full,
            strategy: CreditCardPaymentStrategy.Minimum) with { Charges = [charge], PaymentPlans = [plan] });
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.Name = "Bonus Platinum";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = _repository.Cards[card.Id];
        Assert.Equal("Bonus Platinum", saved.Name);
        Assert.Equal(8000m, saved.CurrentStatement!.StatementAmount);
        Assert.Equal(CurrentStatementPaymentMode.Full, saved.CurrentStatementPaymentPlan!.Mode);
        Assert.Equal(CreditCardPaymentStrategy.Minimum, saved.PaymentStrategy);
        Assert.Equal("Telefon (3/12)", Assert.Single(saved.Charges).Description);
        Assert.Equal(plan.DueDate, Assert.Single(saved.PaymentPlans).DueDate);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_BorcDegisirse_FaizsizHarcamaOlurBakiyeTarihiBugun()
    {
        var card = Add(Card("Bonus", carried: 3000m) with { UnbilledSpending = 1000m });
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.DebtInput = "6.500";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = _repository.Cards[card.Id];
        Assert.Equal(6500m, saved.UnbilledSpending);
        Assert.Equal(0m, saved.CarriedBalance);
        Assert.Equal(Today, saved.BalanceAsOfDate);
    }

    [Fact]
    public async Task Save_BorcDegismezse_DevredenVeBakiyeTarihiKorunur()
    {
        var card = Add(Card("Bonus", carried: 3000m) with { UnbilledSpending = 1000m });
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.Name = "Bonus Platinum";
        _viewModel.Fields.DebtInput = "4.000";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = _repository.Cards[card.Id];
        Assert.Equal("Bonus Platinum", saved.Name);
        Assert.Equal(3000m, saved.CarriedBalance);
        Assert.Equal(1000m, saved.UnbilledSpending);
        Assert.Equal(card.BalanceAsOfDate, saved.BalanceAsOfDate);
    }

    [Fact]
    public async Task Save_EkstreliKart_BorcAlaniYokSayilir()
    {
        var card = Add(Card("Bonus", carried: 0m, statement: Statement(8000m, 1600m), plan: CurrentStatementPaymentMode.Full));
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.Bank = "Akbank";
        _viewModel.Fields.DebtInput = "9.999";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = _repository.Cards[card.Id];
        Assert.Equal("Akbank", saved.Bank);
        Assert.Equal(0m, saved.UnbilledSpending);
        Assert.Equal(card.BalanceAsOfDate, saved.BalanceAsOfDate);
    }

    [Theory]
    [InlineData("50.000", 0.33, 0.33)]
    [InlineData("20.000", 0.33, 0.20)]
    [InlineData("50.000", 0.00, 0.40)]
    public async Task Save_Duzenleme_OranYalnizLimitDegisinceYaDaSifirkenCozulur(string limit, decimal storedRate, decimal expectedRate)
    {
        var card = Add(Card("Bonus", limit: 50000m) with { MinimumPaymentRate = storedRate });
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.LimitInput = limit;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expectedRate, _repository.Cards[card.Id].MinimumPaymentRate);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Theory]
    [InlineData("  ", "50000", "15", "25")]
    [InlineData("Bonus", "", "15", "25")]
    [InlineData("Bonus", "elli bin", "15", "25")]
    [InlineData("Bonus", "0", "15", "25")]
    [InlineData("Bonus", "50000", "0", "25")]
    [InlineData("Bonus", "50000", "15", "32")]
    [InlineData("Bonus", "50000", "", "25")]
    public async Task Save_GecersizAlan_UyariVerirKaydetmez(string name, string limit, string closingDay, string dueDay)
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = name;
        _viewModel.Fields.LimitInput = limit;
        _viewModel.Fields.ClosingDayInput = closingDay;
        _viewModel.Fields.DueDayInput = dueDay;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.Empty(_repository.Cards);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Theory]
    [InlineData("borç")]
    [InlineData("-500")]
    public async Task Save_GecersizBorc_UyariVerirKaydetmez(string debt)
    {
        await FillNewCard(debt: debt);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.Empty(_repository.Cards);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_KayitHatasinda_UyariVerirFormdaKalir()
    {
        var service = new FakeCreditCardObligationService { SaveException = new InvalidOperationException("Kart kaydedilemedi.") };
        var viewModel = new CardFormViewModel(_repository, service, _navigation, _dialog, new SabitSaat(Today));
        await viewModel.LoadAsync(null);
        viewModel.Fields.Name = "Bonus";
        viewModel.Fields.LimitInput = "50000";
        viewModel.Fields.ClosingDayInput = "15";
        viewModel.Fields.DueDayInput = "25";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Kart kaydedilemedi.", _dialog.LastAlertMessage);
        Assert.False(_navigation.NavigateBackCalled);
        Assert.Equal("Bonus", viewModel.Fields.Name);
    }

    [Fact]
    public async Task Cancel_DegisiklikYoksa_SormadanGeriDoner()
    {
        var card = Add(Card("Bonus"));
        await _viewModel.LoadAsync(card.Id);

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarVeKalinirsa_GeriDonmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = "Bonus";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasChanges);
        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarVeCikilirsa_KaydetmedenGeriDoner()
    {
        var card = Add(Card("Bonus"));
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.LimitInput = "60000";

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
        Assert.Equal(50000m, _repository.Cards[card.Id].Limit);
    }

    [Fact]
    public async Task Cancel_DegerEskiHalineDonerse_DegisiklikSayilmaz()
    {
        var card = Add(Card("Bonus"));
        await _viewModel.LoadAsync(card.Id);
        _viewModel.Fields.Name = "Bonus Platinum";
        _viewModel.Fields.Name = "Bonus";

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.False(_viewModel.HasChanges);
        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    private CreditCard Add(CreditCard card)
    {
        _repository.Cards[card.Id] = card;
        return card;
    }

    private async Task FillNewCard(string limit = "50.000", string debt = "")
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = "Bonus";
        _viewModel.Fields.Bank = "Akbank";
        _viewModel.Fields.LimitInput = limit;
        _viewModel.Fields.DebtInput = debt;
        _viewModel.Fields.ClosingDayInput = "1";
        _viewModel.Fields.DueDayInput = "31";
    }
}
