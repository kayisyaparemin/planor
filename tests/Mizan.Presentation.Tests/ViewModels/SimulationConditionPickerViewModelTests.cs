using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;
using static Mizan.Presentation.Tests.Services.FinancialStructureData;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Simülatörün deneme türü seçicisi: Finansal Yapı'nın karoları ve aday kart çözümleme deseniyle çalışır (S77 V10 notları i).
/// </summary>
public sealed class SimulationConditionPickerViewModelTests
{
    private readonly FakeNavigationService _navigation = new();
    private readonly FakePlanReader _reader = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SimulationConditionPickerViewModel _viewModel;

    public SimulationConditionPickerViewModelTests()
    {
        var builder = new FinancialRecordRowBuilder(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));
        var resolver = new RecordCandidateResolver(_reader, builder);
        _viewModel = new SimulationConditionPickerViewModel(_navigation, resolver, _dialog);
    }

    [Fact]
    public void Acilis_GelirOdemeKartVeBorcBolumleriniSunar()
    {
        Assert.Equal(
            ["income", "income-change"],
            _viewModel.IncomeSection.Options.Select(x => x.Key));
        Assert.Equal(
            ["cash", "recurring"],
            _viewModel.PaymentSection.Options.Select(x => x.Key));
        Assert.Equal(
            ["card", "card-payment-mode"],
            _viewModel.CardSection.Options.Select(x => x.Key));
        Assert.Equal(
            ["financing", "cash-debt", "loan-prepayment"],
            _viewModel.DebtSection.Options.Select(x => x.Key));
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_TekSeferlikGelir_DogrudanFormuAcar()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.IncomeSection.Options[0]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("income", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task SelectOption_GelirDegisikligi_AdayYoksa_UyarirFormAcmaz()
    {
        _reader.Plan = new FinancialPlan();

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.IncomeSection.Options[1]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.Equal("Kayıtlı düzenli gelir bulunamadı", _dialog.LastAlertTitle);
        Assert.Contains("önce Finansal Yapı'dan bir düzenli gelir eklemelisin", _dialog.LastAlertMessage);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_GelirDegisikligi_TekGelir_FormuGelirleAcar()
    {
        var (income, histories) = Income("Maaş", 15, true, (45000m, Today));
        _reader.Plan = new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.IncomeSection.Options[1]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("income-change", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(income.Id, _navigation.LastParameters[Routes.IncomeIdParameter]);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_GelirDegisikligi_CokGelir_IkinciSeviyeyiAcar()
    {
        var (income1, h1) = Income("Maaş", 15, true, (45000m, Today));
        var (income2, h2) = Income("Kira Geliri", 1, true, (12000m, Today));
        _reader.Plan = new FinancialPlan
        {
            RecurringIncomes = [income1, income2],
            IncomeHistories = [.. h1, .. h2]
        };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.IncomeSection.Options[1]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.True(_viewModel.IsChoosing);
        Assert.Equal(RecordEntryGroup.Income, _viewModel.ChoiceGroup);
        Assert.Equal(2, _viewModel.Choices.Count);
    }

    [Fact]
    public async Task ChooseRecord_AdayGelirSecildiginde_FormuAcar()
    {
        var (income1, h1) = Income("Maaş", 15, true, (45000m, Today));
        var (income2, h2) = Income("Kira Geliri", 1, true, (12000m, Today));
        _reader.Plan = new FinancialPlan
        {
            RecurringIncomes = [income1, income2],
            IncomeHistories = [.. h1, .. h2]
        };
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.IncomeSection.Options[1]);

        var chosen = _viewModel.Choices[1];
        await _viewModel.ChooseRecordCommand.ExecuteAsync(chosen);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("income-change", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(chosen.Id, _navigation.LastParameters[Routes.IncomeIdParameter]);
    }

    [Fact]
    public async Task SelectOption_KrediCekme_DogrudanFormuAcar()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[0]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("financing", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task SelectOption_TaksitliBorc_DogrudanFormuAcar()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[1]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("cash-debt", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task SelectOption_ErkenOdeme_AdayYoksa_UyarirFormAcmaz()
    {
        _reader.Plan = new FinancialPlan();

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[2]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.Equal("Kayıtlı kredi bulunamadı", _dialog.LastAlertTitle);
        Assert.Contains("önce Finansal Yapı'dan bir kredi eklemelisin", _dialog.LastAlertMessage);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_ErkenOdeme_TekKredi_FormuKrediyleAcar()
    {
        var loanId = Guid.NewGuid();
        _reader.Plan = new FinancialPlan { Loans = [Loan("İhtiyaç", Today.AddDays(15)) with { Id = loanId }] };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[2]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("loan-prepayment", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(loanId, _navigation.LastParameters[Routes.LoanIdParameter]);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_ErkenOdeme_CokKredi_IkinciSeviyeyiAcar()
    {
        var loan1 = Loan("İhtiyaç", Today.AddDays(15)) with { Id = Guid.NewGuid() };
        var loan2 = Loan("Taşıt", Today.AddDays(20), bank: "Yapı Kredi") with { Id = Guid.NewGuid() };
        _reader.Plan = new FinancialPlan { Loans = [loan1, loan2] };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[2]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.True(_viewModel.IsChoosing);
        Assert.Equal(RecordEntryGroup.Loan, _viewModel.ChoiceGroup);
        Assert.Equal(2, _viewModel.Choices.Count);
    }

    [Fact]
    public async Task ChooseRecord_AdayKrediSecildiginde_FormuAcar()
    {
        var loan1 = Loan("İhtiyaç", Today.AddDays(15)) with { Id = Guid.NewGuid() };
        var loan2 = Loan("Taşıt", Today.AddDays(20), bank: "Yapı Kredi") with { Id = Guid.NewGuid() };
        _reader.Plan = new FinancialPlan { Loans = [loan1, loan2] };
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.DebtSection.Options[2]);

        var chosen = _viewModel.Choices[1];
        await _viewModel.ChooseRecordCommand.ExecuteAsync(chosen);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("loan-prepayment", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(loan2.Id, _navigation.LastParameters[Routes.LoanIdParameter]);
    }

    [Fact]
    public async Task SelectOption_NakitOdeme_DogrudanFormuAcar()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.PaymentSection.Options[0]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("cash", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task SelectOption_DuzenliOdeme_DogrudanFormuAcar()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.PaymentSection.Options[1]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("recurring", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
    }

    [Fact]
    public async Task SelectOption_KartlaHarcama_AdayYoksa_UyarirFormAcmaz()
    {
        _reader.Plan = new FinancialPlan();

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[0]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.Equal("Kayıtlı kart bulunamadı", _dialog.LastAlertTitle);
        Assert.Contains("önce Finansal Yapı'dan bir kredi kartı eklemelisin", _dialog.LastAlertMessage);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_KartlaHarcama_TekKart_FormuKartlaAcar()
    {
        var cardId = Guid.NewGuid();
        _reader.Plan = new FinancialPlan { CreditCards = [Card("Bonus") with { Id = cardId }] };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[0]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("card", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(cardId, _navigation.LastParameters[Routes.CardIdParameter]);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_KartlaHarcama_CokKart_IkinciSeviyeyiAcar()
    {
        var card1 = Card("Bonus") with { Id = Guid.NewGuid() };
        var card2 = Card("World", bank: "Yapı Kredi") with { Id = Guid.NewGuid() };
        _reader.Plan = new FinancialPlan { CreditCards = [card1, card2] };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[0]);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.True(_viewModel.IsChoosing);
        Assert.Equal(RecordEntryGroup.Card, _viewModel.ChoiceGroup);
        Assert.Equal(2, _viewModel.Choices.Count);
    }

    [Fact]
    public async Task ChooseRecord_AdaySecildiginde_FormuAcar()
    {
        var card1 = Card("Bonus") with { Id = Guid.NewGuid() };
        var card2 = Card("World", bank: "Yapı Kredi") with { Id = Guid.NewGuid() };
        _reader.Plan = new FinancialPlan { CreditCards = [card1, card2] };
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[0]);

        var chosen = _viewModel.Choices[1];
        await _viewModel.ChooseRecordCommand.ExecuteAsync(chosen);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("card", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(card2.Id, _navigation.LastParameters[Routes.CardIdParameter]);
    }

    [Fact]
    public async Task Back_IkinciSeviyede_KarolaraDoner()
    {
        var card1 = Card("Bonus") with { Id = Guid.NewGuid() };
        var card2 = Card("World", bank: "Yapı Kredi") with { Id = Guid.NewGuid() };
        _reader.Plan = new FinancialPlan { CreditCards = [card1, card2] };
        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[0]);

        await _viewModel.BackCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsChoosing);
        Assert.Empty(_viewModel.Choices);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Back_IlkSeviyede_GeriDoner()
    {
        await _viewModel.BackCommand.ExecuteAsync(null);

        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task SelectOption_KartOdemeSekli_TekKart_FormuKartlaAcar()
    {
        var cardId = Guid.NewGuid();
        _reader.Plan = new FinancialPlan { CreditCards = [Card("Bonus") with { Id = cardId }] };

        await _viewModel.SelectOptionCommand.ExecuteAsync(_viewModel.CardSection.Options[1]);

        Assert.Equal("../" + Routes.SimulationCondition, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal("card-payment-mode", _navigation.LastParameters[Routes.ScenarioOptionParameter]);
        Assert.Equal(cardId, _navigation.LastParameters[Routes.CardIdParameter]);
    }
}
