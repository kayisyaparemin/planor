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
/// Kayıt türü seçici: Finansal Yapı listesinin dört grubu ayrı bölümler olarak sunulur, her karo kendi formunu
/// seçicinin yerine açar; var olan kayda eklenen türlerde önce kayıt çözülür — hiç yoksa ekleme önerilir, tekse
/// sorulmaz, birden fazlaysa "Hangi kart?" ikinci seviyesi açılır (EK-V6f, S77-1, S77-2, S77-5).
/// </summary>
public sealed class RecordEntryPickerViewModelTests
{
    private readonly FakeNavigationService _navigation = new();
    private readonly FakePlanReader _reader = new();
    private readonly FakeDialogService _dialog = new();
    private readonly RecordEntryPickerViewModel _viewModel;

    public RecordEntryPickerViewModelTests()
    {
        var builder = new FinancialRecordRowBuilder(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));
        _viewModel = new RecordEntryPickerViewModel(_navigation, _reader, builder, _dialog);
    }

    [Fact]
    public void Acilis_DortBolumuListeninGruplariylaSunar()
    {
        EntryTypeSection[] sections = [_viewModel.IncomeSection, _viewModel.CardSection, _viewModel.LoanSection, _viewModel.PaymentSection];

        Assert.Equal<Enum>(
            [RecordEntryGroup.Income, RecordEntryGroup.Card, RecordEntryGroup.Loan, RecordEntryGroup.Payment],
            sections.Select(x => x.Group));
        Assert.Equal(
            ["recurring-income|income|income-change", "credit-card|card", "bank-loan|loan-prepayment", "cash|recurring|cash-debt|payment-plan"],
            sections.Select(x => string.Join('|', x.Options.Select(o => o.Key))));
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public void Acilis_KaroBolumIcindekiSirasiniTasir()
    {
        var indexes = _viewModel.PaymentSection.Options.Select(x => x.Index);

        Assert.Equal([0, 1, 2, 3], indexes);
    }

    [Theory]
    [InlineData("recurring-income", Routes.IncomeForm)]
    [InlineData("income", Routes.AdHocIncomeForm)]
    [InlineData("credit-card", Routes.CardForm)]
    [InlineData("bank-loan", Routes.LoanForm)]
    [InlineData("cash", Routes.PlannedExpenseForm)]
    [InlineData("recurring", Routes.PaymentPlanForm)]
    [InlineData("cash-debt", Routes.PaymentPlanForm)]
    [InlineData("payment-plan", Routes.PaymentPlanForm)]
    public async Task SelectOption_FormunuSecicininYerineKimliksizAcar(string key, string route)
    {
        await Select(key);

        Assert.Equal("../" + route, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
        Assert.Equal(0, _reader.ReadCount);
    }

    [Fact]
    public async Task SelectOption_BosSecim_GezinmeYapmaz()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(null);

        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Theory]
    [InlineData("card", Routes.CardForm, Routes.CardIdParameter)]
    [InlineData("loan-prepayment", Routes.LoanForm, Routes.LoanIdParameter)]
    [InlineData("income-change", Routes.IncomeForm, Routes.IncomeIdParameter)]
    public async Task SelectOption_TekAday_FormuKaydinKimligiyleSecicininYerineAcar(string key, string route, string parameter)
    {
        var ids = UseOneOfEach();

        await Select(key);

        Assert.Equal("../" + route, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal(ids[key], Assert.Single(_navigation.LastParameters, x => x.Key == parameter).Value);
        Assert.False(_viewModel.IsChoosing);
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Theory]
    [InlineData("card")]
    [InlineData("loan-prepayment")]
    [InlineData("income-change")]
    public async Task SelectOption_FinansalYapidaGorunmeyenKayitAdayOlmaz(string key)
    {
        var ids = UseOneOfEach();
        var plan = _reader.Plan;
        var (passive, passiveHistories) = Income("Eski kira", 5, active: false, (9000m, new DateOnly(2026, 1, 1)));
        _reader.Plan = new FinancialPlan
        {
            RecurringIncomes = [.. plan.RecurringIncomes, passive], IncomeHistories = [.. plan.IncomeHistories, .. passiveHistories],
            AdHocIncomes = [AdHoc("Prim", 5000m, Today.AddDays(10))],
            CreditCards = [.. plan.CreditCards, Card("Kapalı kart", active: false)],
            Loans = [.. plan.Loans, Loan("Biten", Today.AddDays(5), remaining: 0), Loan("Kapalı", Today.AddDays(5), active: false)]
        };

        await Select(key);

        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal(ids[key], Assert.Single(_navigation.LastParameters).Value);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_BirdenFazlaAday_IkinciSeviyedeSorar()
    {
        _reader.Plan = new FinancialPlan { CreditCards = [Card("Axess"), Card("Bonus")] };

        await Select("card");

        Assert.True(_viewModel.IsChoosing);
        Assert.Equal(RecordEntryGroup.Card, _viewModel.ChoiceGroup);
        Assert.Equal(["Axess", "Bonus"], _viewModel.Choices.Select(x => x.Name));
        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task ChooseRecord_FormuSecilenKaydinKimligiyleSecicininYerineAcar()
    {
        _reader.Plan = new FinancialPlan { Loans = [Loan("İhtiyaç", Today.AddDays(5)), Loan("Taşıt", Today.AddDays(9))] };
        await Select("loan-prepayment");

        Assert.Equal(2, _viewModel.Choices.Count);
        var chosen = _viewModel.Choices[1];
        await _viewModel.ChooseRecordCommand.ExecuteAsync(chosen);

        Assert.Equal("../" + Routes.LoanForm, _navigation.LastNavigatedRoute);
        Assert.Equal(chosen.Id, Assert.Single(_navigation.LastParameters!, x => x.Key == Routes.LoanIdParameter).Value);
    }

    [Theory]
    [InlineData("card", Routes.CardForm, "Henüz kartın yok")]
    [InlineData("loan-prepayment", Routes.LoanForm, "Henüz kredin yok")]
    [InlineData("income-change", Routes.IncomeForm, "Henüz düzenli gelirin yok")]
    public async Task SelectOption_AdayYoksa_OnceEklemeyiOnerir_KabuldeBosFormuAcar(string key, string route, string title)
    {
        _reader.Plan = new FinancialPlan { AdHocIncomes = [AdHoc("Prim", 5000m, Today.AddDays(10))] };

        await Select(key);

        Assert.Equal(title, _dialog.LastConfirmTitle);
        Assert.Equal("../" + route, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task SelectOption_AdayYoksa_Vazgecilirse_SecicideKalir()
    {
        _dialog.NextConfirmResponse = false;

        await Select("card");

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task SelectOption_OkumaHatasi_UyariVerirSecicideKalir()
    {
        _reader.ReadException = new InvalidOperationException("disk");

        await Select("card");

        Assert.Equal("Kayıtlar okunamadı", _dialog.LastAlertTitle);
        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.False(_viewModel.IsChoosing);
    }

    [Fact]
    public async Task Back_IkinciSeviyede_KarolaraDoner()
    {
        _reader.Plan = new FinancialPlan { CreditCards = [Card("Axess"), Card("Bonus")] };
        await Select("card");
        Assert.True(_viewModel.IsChoosing);

        await _viewModel.BackCommand.ExecuteAsync(null);

        Assert.False(_viewModel.IsChoosing);
        Assert.Empty(_viewModel.Choices);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Back_BirinciSeviyede_ListeyeDoner()
    {
        await _viewModel.BackCommand.ExecuteAsync(null);

        Assert.True(_navigation.NavigateBackCalled);
    }

    private Task Select(string key) => _viewModel.SelectOptionCommand.ExecuteAsync(new EntryTypeOptionItem(key, 0));

    private Dictionary<string, object> UseOneOfEach()
    {
        var (income, histories) = Income("İş geliri", 15, true, (45000m, new DateOnly(2026, 1, 1)));
        var card = Card("Axess");
        var loan = Loan("İhtiyaç", Today.AddDays(5));
        _reader.Plan = new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories, CreditCards = [card], Loans = [loan] };
        return new() { ["card"] = card.Id, ["loan-prepayment"] = loan.Id, ["income-change"] = income.Id };
    }
}
