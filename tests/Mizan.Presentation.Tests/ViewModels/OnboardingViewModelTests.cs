using Mizan.Domain.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kurulum sihirbazı görünüm modelinin 8 adımlı akışını, gelir/kart/kredi/harcama ekleme
/// ve silme işlemlerini, özet metriklerini ve başlatma senaryolarını doğrulayan birim testleri.
/// </summary>
public sealed class OnboardingViewModelTests
{
    private readonly FakeOnboardingService _onboardingService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SabitSaat _clock = new(new DateOnly(2026, 9, 27));
    private readonly OnboardingViewModel _viewModel;

    public OnboardingViewModelTests()
    {
        _viewModel = new OnboardingViewModel(
            _onboardingService,
            _navigation,
            _dialog,
            _clock);
    }

    [Fact]
    public void Baslangicta_IlkAdimda_DogruDurumdadir()
    {
        Assert.Equal(1, _viewModel.StepIndex);
        Assert.Equal("1/8", _viewModel.StepCounter);
        Assert.Equal(0.125, _viewModel.StepProgress, 3);
        Assert.False(_viewModel.CanGoBack);
        Assert.True(_viewModel.CanGoNext);
        Assert.False(_viewModel.IsSummaryStep);
    }

    [Fact]
    public void IleriVeGeri_AdimGecisleri_DogruGuncellenir()
    {
        _viewModel.NextCommand.Execute(null);

        Assert.Equal(2, _viewModel.StepIndex);
        Assert.Equal("2/8", _viewModel.StepCounter);
        Assert.True(_viewModel.CanGoBack);

        _viewModel.BackCommand.Execute(null);

        Assert.Equal(1, _viewModel.StepIndex);
        Assert.False(_viewModel.CanGoBack);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(31)]
    public void Adim1_DonemGunu_SinirDegerleriGecerlidir(int day)
    {
        _viewModel.PeriodDay = day;
        _viewModel.NextCommand.Execute(null);

        Assert.Equal(2, _viewModel.StepIndex);
    }

    [Fact]
    public void Adim2_GelirEkleVeSil_DraftKoleksiyonunuGunceller()
    {
        _viewModel.IncomeName = "Gelir";
        _viewModel.IncomeAmount = 45000m;
        _viewModel.IncomePaymentDay = 15;

        _viewModel.AddIncomeCommand.Execute(null);

        Assert.Single(_viewModel.DraftIncomes);
        Assert.Equal("Gelir", _viewModel.DraftIncomes[0].Title);
        Assert.Equal(45000m, _viewModel.DraftIncomes[0].Amount);

        _viewModel.RemoveIncomeCommand.Execute(_viewModel.DraftIncomes[0]);

        Assert.Empty(_viewModel.DraftIncomes);
    }

    [Fact]
    public void Adim3_KartEkleVeSil_DraftKoleksiyonunuGunceller()
    {
        _viewModel.CardName = "Bonus";
        _viewModel.CardBank = "Garanti";
        _viewModel.CardLimit = 50000m;
        _viewModel.CardClosingDay = 10;
        _viewModel.CardDueDay = 20;

        _viewModel.AddCardCommand.Execute(null);

        Assert.Single(_viewModel.DraftCards);
        Assert.Equal("Bonus", _viewModel.DraftCards[0].Title);

        _viewModel.RemoveCardCommand.Execute(_viewModel.DraftCards[0]);

        Assert.Empty(_viewModel.DraftCards);
    }

    [Fact]
    public void Adim3_KartEkle_GuncelBorcGirildiginde_DraftVeModeleAktarilir()
    {
        _viewModel.CardName = "World";
        _viewModel.CardBank = "Yapı Kredi";
        _viewModel.CardLimit = 40000m;
        _viewModel.CardClosingDay = 10;
        _viewModel.CardDueDay = 20;
        _viewModel.CardCurrentDebt = 15000m;

        _viewModel.AddCardCommand.Execute(null);

        var draft = Assert.Single(_viewModel.DraftCards);
        Assert.Equal(15000m, draft.Amount);
    }

    [Fact]
    public void Adim4_KrediEkleVeSil_DraftKoleksiyonunuGunceller()
    {
        _viewModel.LoanName = "Konut";
        _viewModel.LoanBank = "Ziraat";
        _viewModel.LoanMonthlyPayment = 12500m;
        _viewModel.LoanPaymentDay = 15;
        _viewModel.LoanInstallments = 24;

        _viewModel.AddLoanCommand.Execute(null);

        Assert.Single(_viewModel.DraftLoans);
        Assert.Equal("Konut", _viewModel.DraftLoans[0].Title);

        _viewModel.RemoveLoanCommand.Execute(_viewModel.DraftLoans[0]);

        Assert.Empty(_viewModel.DraftLoans);
    }

    [Fact]
    public void Adim5_TekilVeTaksitliOdeme_DraftKoleksiyonunaEklenir()
    {
        _viewModel.ExpenseName = "Sigorta";
        _viewModel.ExpenseAmount = 3000m;
        _viewModel.ExpenseDate = new DateOnly(2026, 10, 1);
        _viewModel.ExpenseInstallments = null;

        _viewModel.AddExpenseCommand.Execute(null);

        Assert.Single(_viewModel.DraftExpenses);

        _viewModel.ExpenseName = "Eğitim";
        _viewModel.ExpenseAmount = 5000m;
        _viewModel.ExpenseDate = new DateOnly(2026, 10, 5);
        _viewModel.ExpenseInstallments = 6;

        _viewModel.AddExpenseCommand.Execute(null);

        Assert.Equal(2, _viewModel.DraftExpenses.Count);
    }

    [Fact]
    public void Adim7_NegatifBakiye_KabulEdilir()
    {
        _viewModel.CurrentBalance = -2500m;

        Assert.Equal(-2500m, _viewModel.CurrentBalance);
    }

    [Fact]
    public void Adim8_OzetEkraninda_MetriklerDogruHesaplanir()
    {
        _viewModel.PeriodDay = 15;
        _viewModel.VariableExpenseAllowance = 18000m;
        _viewModel.CurrentBalance = 12000m;

        _viewModel.IncomeName = "Gelir";
        _viewModel.IncomeAmount = 50000m;
        _viewModel.AddIncomeCommand.Execute(null);

        _viewModel.CardName = "World";
        _viewModel.CardLimit = 60000m;
        _viewModel.CardCurrentDebt = 12000m;
        _viewModel.AddCardCommand.Execute(null);

        _viewModel.LoanName = "İhtiyaç";
        _viewModel.LoanMonthlyPayment = 8000m;
        _viewModel.LoanInstallments = 12;
        _viewModel.AddLoanCommand.Execute(null);

        _viewModel.ExpenseName = "Aidat";
        _viewModel.ExpenseAmount = 1500m;
        _viewModel.AddExpenseCommand.Execute(null);

        // 8. Adıma ilerle
        for (int i = 1; i < 8; i++)
        {
            _viewModel.NextCommand.Execute(null);
        }

        Assert.Equal(8, _viewModel.StepIndex);
        Assert.True(_viewModel.IsSummaryStep);
        Assert.Equal(15, _viewModel.Summary.PeriodDay);
        Assert.Equal(1, _viewModel.Summary.IncomeCount);
        Assert.Equal(50000m, _viewModel.Summary.IncomeTotal);
        Assert.Equal(1, _viewModel.Summary.CardCount);
        Assert.Equal(60000m, _viewModel.Summary.CardLimitTotal);
        Assert.Equal(12000m, _viewModel.Summary.CardDebtTotal);
        Assert.Equal(1, _viewModel.Summary.LoanCount);
        Assert.Equal(8000m, _viewModel.Summary.LoanPaymentTotal);
        Assert.Equal(1, _viewModel.Summary.ExpenseCount);
        Assert.Equal(1500m, _viewModel.Summary.ExpenseTotal);
        Assert.Equal(18000m, _viewModel.Summary.VariableExpenseAllowance);
        Assert.Equal(12000m, _viewModel.Summary.CurrentBalance);
    }

    [Fact]
    public async Task StartPlanorCommand_BasariliIse_TaslagiKaydederVeDashboardaGider()
    {
        _viewModel.PeriodDay = 15;
        _viewModel.CurrentBalance = 10000m;

        await _viewModel.StartPlanorCommand.ExecuteAsync(null);

        Assert.Equal(1, _onboardingService.CallCount);
        Assert.NotNull(_onboardingService.LastDraft);
        Assert.Equal(15, _onboardingService.LastDraft.Settings.PeriodAnchor.DayOfMonth);
        Assert.Equal(10000m, _onboardingService.LastDraft.Settings.ProjectionOpeningBalance);
        Assert.Equal("//dashboard", _navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task StartPlanorCommand_KartEklendiginde_BakiyeTarihiDoluVeGecerlidir()
    {
        _viewModel.CardName = "World";
        _viewModel.CardBank = "Yapı Kredi";
        _viewModel.CardLimit = 40000m;
        _viewModel.CardClosingDay = 10;
        _viewModel.CardDueDay = 20;
        _viewModel.AddCardCommand.Execute(null);

        await _viewModel.StartPlanorCommand.ExecuteAsync(null);

        Assert.Equal(1, _onboardingService.CallCount);
        Assert.NotNull(_onboardingService.LastDraft);
        var card = Assert.Single(_onboardingService.LastDraft.CreditCards);
        Assert.Equal(new DateOnly(2026, 9, 27), card.BalanceAsOfDate);
    }
}
