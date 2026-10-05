using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Abstractions;
using Mizan.Domain.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Onboarding.Models;
using Mizan.Presentation.Onboarding.Support;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Kurulum sihirbazının sekiz adımını, veri girişlerini, taslak listelerini
/// ve ilk finansal planın dondurulma sürecini MAUI'den bağımsız olarak yöneten görünüm modelidir.
/// </summary>
public sealed partial class OnboardingViewModel : ViewModelBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IClock _clock;

    /// <summary>Taslak gelirler listesi.</summary>
    public ObservableCollection<OnboardingDraftItem> DraftIncomes { get; } = [];
    /// <summary>Taslak kredi kartları listesi.</summary>
    public ObservableCollection<OnboardingDraftItem> DraftCards { get; } = [];
    /// <summary>Taslak krediler listesi.</summary>
    public ObservableCollection<OnboardingDraftItem> DraftLoans { get; } = [];
    /// <summary>Taslak harcamalar listesi.</summary>
    public ObservableCollection<OnboardingDraftItem> DraftExpenses { get; } = [];

    private readonly List<RecurringIncome> _incomes = [];
    private readonly List<CreditCard> _cards = [];
    private readonly List<Loan> _loans = [];
    private readonly List<TemporaryPaymentPlan> _paymentPlans = [];
    private readonly List<PlannedLargeExpense> _expenses = [];

    [ObservableProperty] private int stepIndex = 1;
    [ObservableProperty] private string stepCounter = "1/8";
    [ObservableProperty] private string stepTitle = "Dönemini Ayarlayalım";
    [ObservableProperty] private string stepLead = "Bilgilerini girerek ilk nakit akış dönemini planlayabilirsin.";
    [ObservableProperty] private double stepProgress = 0.125;
    [ObservableProperty] private bool canGoBack;
    [ObservableProperty] private bool canGoNext = true;
    [ObservableProperty] private bool isSummaryStep;

    [ObservableProperty] private int periodDay = 15;
    [ObservableProperty] private string incomeName = string.Empty;
    [ObservableProperty] private decimal? incomeAmount;
    [ObservableProperty] private int incomePaymentDay = 15;
    [ObservableProperty] private DateOnly incomeEffectiveDate;

    [ObservableProperty] private string cardName = string.Empty;
    [ObservableProperty] private string cardBank = string.Empty;
    [ObservableProperty] private decimal? cardLimit;
    [ObservableProperty] private int cardClosingDay = 10;
    [ObservableProperty] private int cardDueDay = 20;
    [ObservableProperty] private decimal? cardCurrentDebt;

    [ObservableProperty] private string loanName = string.Empty;
    [ObservableProperty] private string loanBank = string.Empty;
    [ObservableProperty] private decimal? loanMonthlyPayment;
    [ObservableProperty] private int loanPaymentDay = 15;
    [ObservableProperty] private int? loanInstallments;
    [ObservableProperty] private decimal? loanRemainingDebt;

    [ObservableProperty] private string expenseName = string.Empty;
    [ObservableProperty] private decimal? expenseAmount;
    [ObservableProperty] private DateOnly expenseDate;
    [ObservableProperty] private int? expenseInstallments;

    [ObservableProperty] private decimal? variableExpenseAllowance;
    [ObservableProperty] private decimal? currentBalance;
    [ObservableProperty] private OnboardingSummary summary = new();

    /// <summary>Görünüm modelini bağımlılıklarıyla ilklendirir.</summary>
    public OnboardingViewModel(
        IOnboardingService onboardingService, INavigationService navigationService,
        IDialogService dialogService, IClock clock)
    {
        _onboardingService = onboardingService ?? throw new ArgumentNullException(nameof(onboardingService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        incomeEffectiveDate = clock.Today;
        expenseDate = clock.Today;
    }

    private async Task<bool> ValidateAsync(string? error)
    {
        if (error is null) { return true; }
        await _dialogService.ShowAlertAsync("Eksik Bilgi", error);
        return false;
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (StepIndex == 1 && !await ValidateAsync(OnboardingValidator.ValidatePeriodDay(PeriodDay))) { return; }
        if (StepIndex < 8) { StepIndex++; RefreshStepState(); }
    }
    [RelayCommand] private void Back() { if (StepIndex > 1) { StepIndex--; RefreshStepState(); } }
    [RelayCommand] private void Skip() { if (StepIndex < 8) { StepIndex++; RefreshStepState(); } }

    [RelayCommand]
    private async Task AddIncomeAsync()
    {
        if (!await ValidateAsync(OnboardingValidator.ValidateIncome(IncomeName, IncomeAmount, IncomePaymentDay))) { return; }
        var (model, draft) = OnboardingDraftBuilder.CreateIncome(IncomeName, IncomeAmount!.Value, IncomePaymentDay);
        _incomes.Add(model);
        DraftIncomes.Add(draft);
        IncomeName = string.Empty; IncomeAmount = null;
    }

    [RelayCommand]
    private void RemoveIncome(OnboardingDraftItem? item) { if (item is null) { return; } _incomes.RemoveAll(x => x.Id == item.Id); DraftIncomes.Remove(item); }
    [RelayCommand]
    private async Task AddCardAsync()
    {
        if (!await ValidateAsync(OnboardingValidator.ValidateCard(CardName, CardLimit, CardClosingDay, CardDueDay))) { return; }
        var (model, draft) = OnboardingDraftBuilder.CreateCard(CardName, CardBank, CardLimit!.Value, CardClosingDay, CardDueDay, CardCurrentDebt, _clock.Today);
        _cards.Add(model);
        DraftCards.Add(draft);
        CardName = string.Empty; CardBank = string.Empty; CardLimit = null; CardCurrentDebt = null;
    }

    [RelayCommand]
    private void RemoveCard(OnboardingDraftItem? item) { if (item is null) { return; } _cards.RemoveAll(x => x.Id == item.Id); DraftCards.Remove(item); }

    [RelayCommand]
    private async Task AddLoanAsync()
    {
        if (!await ValidateAsync(OnboardingValidator.ValidateLoan(LoanName, LoanMonthlyPayment, LoanInstallments, LoanPaymentDay))) { return; }
        var (model, draft) = OnboardingDraftBuilder.CreateLoan(LoanName, LoanBank, LoanMonthlyPayment!.Value, LoanPaymentDay, LoanInstallments!.Value, LoanRemainingDebt, _clock.Today);
        _loans.Add(model);
        DraftLoans.Add(draft);
        LoanName = string.Empty; LoanBank = string.Empty; LoanMonthlyPayment = null; LoanInstallments = null;
    }

    [RelayCommand]
    private void RemoveLoan(OnboardingDraftItem? item) { if (item is null) { return; } _loans.RemoveAll(x => x.Id == item.Id); DraftLoans.Remove(item); }

    [RelayCommand]
    private async Task AddExpenseAsync()
    {
        if (!await ValidateAsync(OnboardingValidator.ValidateExpense(ExpenseName, ExpenseAmount))) { return; }
        var (plan, expense, draft) = OnboardingDraftBuilder.CreateExpense(ExpenseName, ExpenseAmount!.Value, ExpenseDate, ExpenseInstallments);
        if (plan is not null) { _paymentPlans.Add(plan); }
        if (expense is not null) { _expenses.Add(expense); }
        DraftExpenses.Add(draft);
        ExpenseName = string.Empty; ExpenseAmount = null; ExpenseInstallments = null;
    }

    [RelayCommand]
    private void RemoveExpense(OnboardingDraftItem? item)
    {
        if (item is null) { return; }
        _expenses.RemoveAll(x => x.Id == item.Id);
        _paymentPlans.RemoveAll(x => x.Id == item.Id);
        DraftExpenses.Remove(item);
    }

    [RelayCommand]
    private async Task StartPlanorAsync()
    {
        if (IsBusy) { return; }
        try
        {
            SetBusy(true, "Planör hazırlanıyor...");
            var draft = OnboardingDraftBuilder.Build(PeriodDay, VariableExpenseAllowance ?? 0m, CurrentBalance ?? 0m, _incomes, DraftIncomes, _cards, _loans, _paymentPlans, _expenses, _clock.Today);
            await _onboardingService.InitializeFromOnboardingAsync(draft);
            await _navigationService.NavigateToAsync(Routes.Dashboard);
        }
        catch (Exception ex) { await _dialogService.ShowAlertAsync("Hata", ex.Message); }
        finally { SetBusy(false); }
    }

    private void RefreshStepState()
    {
        CanGoBack = StepIndex > 1; CanGoNext = StepIndex < 8; IsSummaryStep = StepIndex == 8;
        StepCounter = $"{StepIndex}/8"; StepTitle = OnboardingDraftBuilder.ResolveTitle(StepIndex);
        StepProgress = (double)StepIndex / 8;
        if (IsSummaryStep) { RefreshSummary(); }
    }

    private void RefreshSummary()
    {
        Summary = new OnboardingSummary
        {
            PeriodDay = PeriodDay, IncomeCount = _incomes.Count, IncomeTotal = DraftIncomes.Sum(x => x.Amount),
            CardCount = _cards.Count, CardLimitTotal = _cards.Sum(x => x.Limit), CardDebtTotal = _cards.Sum(x => x.CarriedBalance),
            LoanCount = _loans.Count, LoanPaymentTotal = _loans.Sum(x => x.MonthlyPayment),
            ExpenseCount = _expenses.Count + _paymentPlans.Count, ExpenseTotal = DraftExpenses.Sum(x => x.Amount),
            VariableExpenseAllowance = VariableExpenseAllowance ?? 0m, CurrentBalance = CurrentBalance ?? 0m
        };
    }
}
