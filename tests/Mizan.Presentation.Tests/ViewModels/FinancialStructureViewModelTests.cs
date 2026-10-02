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
/// Finansal Yapı ekranının yükleme durumları ve satır diyaloğu: kartta ödemeyi yönetme, her türde
/// onaylı silme, silme hatasında listenin korunması (EK-V6, S62-3).
/// </summary>
public sealed class FinancialStructureViewModelTests
{
    private readonly FakePlanReader _reader = new();
    private readonly FakeObligationManagementService _obligations = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly FinancialStructureViewModel _viewModel;

    public FinancialStructureViewModelTests()
    {
        var remover = new FinancialRecordRemover(new FakeCreditCardObligationService(), _obligations, new FakeIncomePlanService());
        var builder = new FinancialRecordRowBuilder(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));
        _viewModel = new FinancialStructureViewModel(_reader, builder, remover, _navigation, _dialog);
    }

    [Fact]
    public async Task Load_KayitYoksa_BosDurumaDuser()
    {
        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Load_OkumaHatasinda_HataDurumunaDuser()
    {
        _reader.ReadException = new InvalidOperationException("disk");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Load_DortGrupDolarIcerikDurumunaGecer()
    {
        var (income, histories) = Income("Gelir", 15, true, (45000m, new DateOnly(2026, 1, 1)));
        _reader.Plan = new FinancialPlan
        {
            RecurringIncomes = [income], IncomeHistories = histories, CreditCards = [Card("Axess", carried: 4000m)],
            Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))], PlannedLargeExpenses = [Expense("Tatil", 30000m, new DateOnly(2027, 7, 12))]
        };

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal("Gelir", Assert.Single(_viewModel.Incomes.Items).Name);
        Assert.Equal("Axess", Assert.Single(_viewModel.Cards.Items).Name);
        Assert.Equal("İhtiyaç", Assert.Single(_viewModel.Loans.Items).Name);
        Assert.Equal("Tatil", Assert.Single(_viewModel.Payments.Items).Name);
    }

    [Fact]
    public async Task SelectRecord_Kart_OdemeyiYonetVeSilSunar()
    {
        var card = await LoadWith(new FinancialPlan { CreditCards = [Card("Axess", carried: 4000m)] }, vm => vm.Cards);

        await _viewModel.SelectRecordCommand.ExecuteAsync(card);

        Assert.Equal("Axess", _dialog.LastChooseTitle);
        Assert.Equal(["Ödemeyi yönet", "Düzenle"], _dialog.LastChooseOptions);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task SelectRecord_KartDuzenle_KartFormunuKartKimligiyleAcar()
    {
        var card = await LoadWith(new FinancialPlan { CreditCards = [Card("Axess", carried: 4000m)] }, vm => vm.Cards);
        _dialog.NextChooseResponse = "Düzenle";

        await _viewModel.SelectRecordCommand.ExecuteAsync(card);

        Assert.Equal(Routes.CardForm, _navigation.LastNavigatedRoute);
        Assert.Equal(card.Id.ToString(), _navigation.LastParameters?[Routes.CardIdParameter].ToString());
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task Add_KrediKartiSecilince_KartFormunuKimliksizAcar()
    {
        _dialog.NextChooseResponse = "Kredi kartı";

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Equal(["Düzenli gelir", "Tek seferlik gelir", "Kredi kartı", "Kredi"], _dialog.LastChooseOptions);
        Assert.Null(_dialog.LastChooseDestruction);
        Assert.Equal(Routes.CardForm, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task Add_DuzenliGelirSecilince_GelirFormunuKimliksizAcar()
    {
        _dialog.NextChooseResponse = "Düzenli gelir";

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Equal(Routes.IncomeForm, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task Add_TekSeferlikGelirSecilince_AdHocGelirFormunuKimliksizAcar()
    {
        _dialog.NextChooseResponse = "Tek seferlik gelir";

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Equal(Routes.AdHocIncomeForm, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task SelectRecord_DuzenliGelir_DuzenleVeSilSunar()
    {
        var (income, histories) = Income("Kira geliri", 20, true, (12_500m, new DateOnly(2026, 1, 1)));
        var row = await LoadWith(new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories }, vm => vm.Incomes);

        await _viewModel.SelectRecordCommand.ExecuteAsync(row);

        Assert.Equal("Kira geliri", _dialog.LastChooseTitle);
        Assert.Equal(["Düzenle"], _dialog.LastChooseOptions);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task SelectRecord_DuzenliGelirDuzenle_GelirFormunuGelirKimligiyleAcar()
    {
        var (income, histories) = Income("Kira geliri", 20, true, (12_500m, new DateOnly(2026, 1, 1)));
        var row = await LoadWith(new FinancialPlan { RecurringIncomes = [income], IncomeHistories = histories }, vm => vm.Incomes);
        _dialog.NextChooseResponse = "Düzenle";

        await _viewModel.SelectRecordCommand.ExecuteAsync(row);

        Assert.Equal(Routes.IncomeForm, _navigation.LastNavigatedRoute);
        Assert.Equal(income.Id.ToString(), _navigation.LastParameters?[Routes.IncomeIdParameter].ToString());
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task SelectRecord_TekSeferlikGelir_DuzenleVeSilSunar()
    {
        var adHoc = new AdHocIncome { Description = "İkramiye", Amount = 15_000m, ExactDate = Today.AddDays(10) };
        var row = await LoadWith(new FinancialPlan { AdHocIncomes = [adHoc] }, vm => vm.Incomes);

        await _viewModel.SelectRecordCommand.ExecuteAsync(row);

        Assert.Equal("İkramiye", _dialog.LastChooseTitle);
        Assert.Equal(["Düzenle"], _dialog.LastChooseOptions);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task SelectRecord_TekSeferlikGelirDuzenle_AdHocIncomeFormunuKimligiyleAcar()
    {
        var adHoc = new AdHocIncome { Description = "İkramiye", Amount = 15_000m, ExactDate = Today.AddDays(10) };
        var row = await LoadWith(new FinancialPlan { AdHocIncomes = [adHoc] }, vm => vm.Incomes);
        _dialog.NextChooseResponse = "Düzenle";

        await _viewModel.SelectRecordCommand.ExecuteAsync(row);

        Assert.Equal(Routes.AdHocIncomeForm, _navigation.LastNavigatedRoute);
        Assert.Equal(adHoc.Id.ToString(), _navigation.LastParameters?[Routes.AdHocIncomeIdParameter].ToString());
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task Add_Vazgecilirse_Gezinmez()
    {
        _dialog.NextChooseResponse = null;

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastChooseOptions);
        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task SelectRecord_OdemeyiYonet_KartKontroluKartKimligiyleAcar()
    {
        var card = await LoadWith(new FinancialPlan { CreditCards = [Card("Axess", carried: 4000m)] }, vm => vm.Cards);
        _dialog.NextChooseResponse = "Ödemeyi yönet";

        await _viewModel.SelectRecordCommand.ExecuteAsync(card);

        Assert.Equal(Routes.CardControl, _navigation.LastNavigatedRoute);
        Assert.Equal(card.Id.ToString(), _navigation.LastParameters?[Routes.CardIdParameter].ToString());
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task Add_KrediSecilince_KrediFormunuKimliksizAcar()
    {
        _dialog.NextChooseResponse = "Kredi";

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.Equal(Routes.LoanForm, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task SelectRecord_Kredi_DuzenleVeSilSunar()
    {
        var loan = await LoadWith(new FinancialPlan { Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))] }, vm => vm.Loans);

        await _viewModel.SelectRecordCommand.ExecuteAsync(loan);

        Assert.Equal("İhtiyaç", _dialog.LastChooseTitle);
        Assert.Equal(["Düzenle"], _dialog.LastChooseOptions);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task SelectRecord_KrediDuzenle_KrediFormunuKrediKimligiyleAcar()
    {
        var loan = await LoadWith(new FinancialPlan { Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))] }, vm => vm.Loans);
        _dialog.NextChooseResponse = "Düzenle";

        await _viewModel.SelectRecordCommand.ExecuteAsync(loan);

        Assert.Equal(Routes.LoanForm, _navigation.LastNavigatedRoute);
        Assert.Equal(loan.Id.ToString(), _navigation.LastParameters?[Routes.LoanIdParameter].ToString());
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task SelectRecord_KartVeKrediDisindakiKayit_YalnizSilSunar()
    {
        var expense = await LoadWith(
            new FinancialPlan { PlannedLargeExpenses = [Expense("Tatil", 30000m, new DateOnly(2027, 7, 12))] }, vm => vm.Payments);

        await _viewModel.SelectRecordCommand.ExecuteAsync(expense);

        Assert.Equal("Tatil", _dialog.LastChooseTitle);
        Assert.Empty(_dialog.LastChooseOptions!);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task SelectRecord_SilOnaylaninca_KaydiSilerVeListeyiYeniler()
    {
        var loan = await LoadWith(new FinancialPlan { Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))] }, vm => vm.Loans);
        _reader.Plan = new FinancialPlan();
        _dialog.NextChooseResponse = "Sil";

        await _viewModel.SelectRecordCommand.ExecuteAsync(loan);

        Assert.Equal([loan.Id], _obligations.DeletedLoanIds);
        Assert.Contains("İhtiyaç", _dialog.LastConfirmMessage);
        Assert.Equal(2, _reader.ReadCount);
        Assert.Empty(_viewModel.Loans.Items);
        Assert.Equal(ScreenState.Empty, _viewModel.State);
    }

    [Fact]
    public async Task SelectRecord_SilOnaylanmazsa_HicbirSeySilinmez()
    {
        var loan = await LoadWith(new FinancialPlan { Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))] }, vm => vm.Loans);
        _dialog.NextChooseResponse = "Sil";
        _dialog.NextConfirmResponse = false;

        await _viewModel.SelectRecordCommand.ExecuteAsync(loan);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.Empty(_obligations.DeletedLoanIds);
        Assert.Equal(1, _reader.ReadCount);
    }

    [Fact]
    public async Task SelectRecord_Vazgecilirse_NeGezinirNeSorar()
    {
        var card = await LoadWith(new FinancialPlan { CreditCards = [Card("Axess", carried: 4000m)] }, vm => vm.Cards);
        _dialog.NextChooseResponse = null;

        await _viewModel.SelectRecordCommand.ExecuteAsync(card);

        Assert.Null(_navigation.LastNavigatedRoute);
        Assert.Equal(0, _dialog.ConfirmCount);
    }

    [Fact]
    public async Task SelectRecord_SilmeHatasinda_UyariVerirListeOlduguGibiKalir()
    {
        var loan = await LoadWith(new FinancialPlan { Loans = [Loan("İhtiyaç", new DateOnly(2026, 10, 15))] }, vm => vm.Loans);
        _obligations.DeleteException = new InvalidOperationException("Kredi silinemedi.");
        _dialog.NextChooseResponse = "Sil";

        await _viewModel.SelectRecordCommand.ExecuteAsync(loan);

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.Equal("Kredi silinemedi.", _dialog.LastAlertMessage);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Single(_viewModel.Loans.Items);
    }

    private async Task<FinancialRecordRow> LoadWith(FinancialPlan plan, Func<FinancialStructureViewModel, FinancialRecordGroup> group)
    {
        _reader.Plan = plan;
        await _viewModel.LoadAsync();
        return Assert.Single(group(_viewModel).Items);
    }
}
