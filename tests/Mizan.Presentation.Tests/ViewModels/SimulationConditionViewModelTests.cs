using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Simülatörün deneme formu, nakit ödeme (EK-V10, S76-3, 7, 8): yeni deneme, düzenleme, doğrulama, çalışma
/// listesine yazma ve kaydedilmemiş değişiklikte çıkış onayı.
/// </summary>
public sealed class SimulationConditionViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    private readonly FakeSimulationWorkflowService _service = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly FakePlanReader _reader = new();
    private readonly SimulationConditionViewModel _viewModel;

    public SimulationConditionViewModelTests()
    {
        _viewModel = new SimulationConditionViewModel(_service, new SabitSaat(Today), _navigation, _dialog, _reader);
    }

    [Fact]
    public async Task Load_YeniNakitOdeme_BosAlanlarVeBugunleAcar()
    {
        _viewModel.Prepare(SimulationScenarioCatalog.CashPayment.Key, null);

        await _viewModel.LoadAsync();

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(SimulationScenarioType.CashPurchase, _viewModel.ScenarioType);
        Assert.Equal(string.Empty, _viewModel.Name);
        Assert.Equal(string.Empty, _viewModel.AmountInput);
        Assert.Equal(Today, _viewModel.Date);
        Assert.False(_viewModel.HasChanges);
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Save_YeniDeneme_ListeninSonunaAcikEklenirVeGeriDoner()
    {
        var existing = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: false);
        _service.Seed(existing);
        await OpenNewAsync();
        _viewModel.Name = "  Telefon ";
        _viewModel.AmountInput = "30.000";
        _viewModel.Date = Today.AddDays(12);

        await _viewModel.SaveAsync();

        var saved = _service.LastSaved!;
        Assert.Equal(2, saved.Count);
        Assert.Equal(existing, saved[0]);
        var added = saved[1];
        Assert.True(added.IsEnabled);
        Assert.Equal(SimulationScenarioType.CashPurchase, added.Request.Type);
        Assert.Equal("Telefon", added.Request.Name);
        Assert.Equal(30_000m, added.Request.Amount);
        Assert.Equal(Today.AddDays(12), added.Request.StartDate);
        Assert.NotEqual(Guid.Empty, added.Request.ScenarioId);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_AdBos_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.AmountInput = "30.000";

        await _viewModel.SaveAsync();

        Assert.Equal("Denemeye bir ad ver.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_TutarSifir_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "0";

        await _viewModel.SaveAsync();

        Assert.Equal("Tutarı sıfırdan büyük bir sayı olarak gir.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Save_TarihDun_UyarirYazmaz()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "30.000";
        _viewModel.Date = Today.AddDays(-1);

        await _viewModel.SaveAsync();

        Assert.Equal(SimulationConditionRules.PastDateMessage, _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Load_Duzenleme_AlanlariDoldurur()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: true);
        _service.Seed(phone);
        _viewModel.Prepare(null, phone.Request.ScenarioId);

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Telefon", _viewModel.Name);
        Assert.Equal("30000", _viewModel.AmountInput);
        Assert.Equal(Today.AddDays(12), _viewModel.Date);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Save_Duzenleme_YerindeDegisirKimlikVeKapaliHaliKorunur()
    {
        var phone = Condition("Telefon", 30_000m, Today.AddDays(12), isEnabled: false);
        var holiday = Condition("Tatil", 45_000m, Today.AddDays(80), isEnabled: true);
        _service.Seed(phone, holiday);
        _viewModel.Prepare(null, phone.Request.ScenarioId);
        await _viewModel.LoadAsync();
        _viewModel.AmountInput = "35.000";

        await _viewModel.SaveAsync();

        Assert.Equal(["Telefon", "Tatil"], _service.LastSaved!.Select(x => x.Request.Name));
        var edited = _service.LastSaved![0];
        Assert.False(edited.IsEnabled);
        Assert.Equal(phone.Request.ScenarioId, edited.Request.ScenarioId);
        Assert.Equal(35_000m, edited.Request.Amount);
    }

    [Fact]
    public async Task Save_TekSeferlikOdemeDuzenlenince_TuruKorunur()
    {
        var payment = new SimulationDraftCondition(
            new SimulationRequest(SimulationScenarioType.FutureOneTimePayment, "Aidat", 8_000m, Today.AddDays(5)));
        _service.Seed(payment);
        _viewModel.Prepare(null, payment.Request.ScenarioId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Yıllık aidat";

        await _viewModel.SaveAsync();

        Assert.Equal(SimulationScenarioType.FutureOneTimePayment, _service.LastSaved![0].Request.Type);
    }

    [Fact]
    public async Task Load_DenemeBulunamazsa_UyarirVeGeriDoner()
    {
        _viewModel.Prepare(null, Guid.NewGuid());

        await _viewModel.LoadAsync();

        Assert.Equal(("Deneme bulunamadı", "Deneme silinmiş olabilir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_OkumaHatasi_HataDurumunaGecer()
    {
        _service.ThrowOnGet = new InvalidOperationException("okunamadı");
        _viewModel.Prepare(null, Guid.NewGuid());

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsError);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarken_OnaySorarKalirsaDonmez()
    {
        await OpenNewAsync();
        _viewModel.Name = "Telefon";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelAsync();

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikYokken_SormadanDoner()
    {
        await OpenNewAsync();

        await _viewModel.CancelAsync();

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_KartlaHarcama_TekCekim_Kaydeder()
    {
        var cardId = Guid.NewGuid();
        _viewModel.Prepare("card", null, cardId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "30.000";
        _viewModel.PaymentCountInput = "";

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.CreditCardSinglePayment, saved.Request.Type);
        Assert.Equal(cardId, saved.Request.CreditCardId);
        Assert.Equal(1, saved.Request.PaymentCount);
        Assert.Equal(30_000m, saved.Request.Amount);
    }

    [Fact]
    public async Task Save_KartlaHarcama_Taksitli_Kaydeder()
    {
        var cardId = Guid.NewGuid();
        _viewModel.Prepare("card", null, cardId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Telefon";
        _viewModel.AmountInput = "30.000";
        _viewModel.PaymentCountInput = "6";

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.CreditCardInstallmentPurchase, saved.Request.Type);
        Assert.Equal(cardId, saved.Request.CreditCardId);
        Assert.Equal(6, saved.Request.PaymentCount);
    }

    [Fact]
    public async Task Save_DuzenliOdeme_Kaydeder()
    {
        _viewModel.Prepare("recurring", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Kira";
        _viewModel.AmountInput = "20.000";
        _viewModel.PaymentCountInput = "12";

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.RecurringPayment, saved.Request.Type);
        Assert.Equal(12, saved.Request.PaymentCount);
        Assert.Equal(20_000m, saved.Request.Amount);
    }

    [Fact]
    public async Task Save_DuzenliOdeme_OdemeSayisiGecersiz_Uyarir()
    {
        _viewModel.Prepare("recurring", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Kira";
        _viewModel.AmountInput = "20.000";
        _viewModel.PaymentCountInput = "0";

        await _viewModel.SaveAsync();

        Assert.Equal("Ödeme sayısını 1 ile 120 arasında bir sayı olarak gir.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Save_KartOdemeSekli_CreditCardPaymentModeKaydeder()
    {
        var cardId = Guid.NewGuid();
        _viewModel.Prepare("card-payment-mode", null, cardId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Kart tercihi";
        _viewModel.SelectedCardPaymentMode = CreditCardPaymentType.Minimum;
        _viewModel.SelectedCardPaymentScope = true;

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.CreditCardPaymentMode, saved.Request.Type);
        Assert.Equal(cardId, saved.Request.CreditCardId);
        Assert.Equal(CreditCardPaymentType.Minimum, saved.Request.CardPaymentType);
        Assert.True(saved.Request.AppliesToAllStatements);
        Assert.Equal(0m, saved.Request.Amount);
    }

    [Fact]
    public async Task Load_VarOlanKartlaHarcama_DuzenlemeModundaYukler()
    {
        var cardId = Guid.NewGuid();
        var card = new CreditCard { Id = cardId, Name = "Bonus", Bank = "Garanti" };
        _reader.Plan = new FinancialPlan { CreditCards = [card] };

        var conditionId = Guid.NewGuid();
        var request = new SimulationRequest
        {
            ScenarioId = conditionId,
            Type = SimulationScenarioType.CreditCardInstallmentPurchase,
            Name = "Televizyon",
            Amount = 40_000m,
            StartDate = Today.AddDays(5),
            PaymentCount = 6,
            CreditCardId = cardId
        };
        _service.Seed(new SimulationDraftCondition(request));

        _viewModel.Prepare(null, conditionId);
        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Televizyon", _viewModel.Name);
        Assert.Equal(cardId, _viewModel.CardId);
        Assert.Equal("Garanti Bonus", _viewModel.CardName);
        Assert.Equal("6", _viewModel.PaymentCountInput);
    }

    [Fact]
    public async Task Save_KrediCekme_FinancingLoanKaydeder()
    {
        _viewModel.Prepare("financing", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "İhtiyaç kredisi";
        _viewModel.AmountInput = "50.000";
        _viewModel.PaymentCountInput = "12";
        _viewModel.TotalRepaymentAmountInput = "68.400";
        _viewModel.FirstPaymentDate = Today.AddMonths(1);

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.FinancingLoan, saved.Request.Type);
        Assert.Equal("İhtiyaç kredisi", saved.Request.Name);
        Assert.Equal(50_000m, saved.Request.Amount);
        Assert.Equal(12, saved.Request.PaymentCount);
        Assert.Equal(68_400m, saved.Request.TotalRepaymentAmount);
        Assert.Equal(Today.AddMonths(1), saved.Request.FirstPaymentDate);
    }

    [Fact]
    public async Task Save_KrediCekme_ToplamGeriOdemeKucukse_Uyarir()
    {
        _viewModel.Prepare("financing", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "İhtiyaç kredisi";
        _viewModel.AmountInput = "50.000";
        _viewModel.PaymentCountInput = "12";
        _viewModel.TotalRepaymentAmountInput = "40.000";
        _viewModel.FirstPaymentDate = Today.AddMonths(1);

        await _viewModel.SaveAsync();

        Assert.Equal("Toplam geri ödeme ana tutardan düşük olamaz.", _dialog.LastAlertMessage);
        Assert.Null(_service.LastSaved);
    }

    [Fact]
    public async Task Save_TaksitliNakitBorc_CashDebtKaydeder()
    {
        _viewModel.Prepare("cash-debt", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Elden borç";
        _viewModel.AmountInput = "24.000";
        _viewModel.PaymentCountInput = "6";

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.CashDebt, saved.Request.Type);
        Assert.Equal("Elden borç", saved.Request.Name);
        Assert.Equal(24_000m, saved.Request.Amount);
        Assert.Equal(6, saved.Request.PaymentCount);
    }

    [Fact]
    public async Task Save_KrediyeErkenOdeme_TamamenKapat_LoanEarlyClosureKaydeder()
    {
        var loanId = Guid.NewGuid();
        _viewModel.Prepare("loan-prepayment", null, null, loanId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Krediyi kapat";
        _viewModel.SelectedPrepaymentMode = LoanPrepaymentMode.FullClosure;

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.LoanEarlyClosure, saved.Request.Type);
        Assert.Equal(loanId, saved.Request.LoanId);
        Assert.Equal(0m, saved.Request.Amount);
    }

    [Fact]
    public async Task Save_KrediyeErkenOdeme_VadeyiKisalt_LoanPartialPrepaymentKaydeder()
    {
        var loanId = Guid.NewGuid();
        _viewModel.Prepare("loan-prepayment", null, null, loanId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Krediye ara ödeme";
        _viewModel.SelectedPrepaymentMode = LoanPrepaymentMode.ReduceTerm;
        _viewModel.AmountInput = "15.000";

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.LoanPartialPrepayment, saved.Request.Type);
        Assert.Equal(loanId, saved.Request.LoanId);
        Assert.Equal(15_000m, saved.Request.Amount);
        Assert.Equal(LoanPrepaymentMode.ReduceTerm, saved.Request.PrepaymentMode);
    }

    [Fact]
    public async Task Load_VarOlanKrediyeErkenOdeme_DuzenlemeModundaYukler()
    {
        var loanId = Guid.NewGuid();
        var loan = new Loan { Id = loanId, Name = "İhtiyaç Kredisi", Bank = "Garanti" };
        _reader.Plan = new FinancialPlan { Loans = [loan] };

        var conditionId = Guid.NewGuid();
        var request = new SimulationRequest
        {
            ScenarioId = conditionId,
            Type = SimulationScenarioType.LoanPartialPrepayment,
            Name = "Erken Ödeme",
            Amount = 10_000m,
            StartDate = Today.AddDays(15),
            LoanId = loanId,
            PrepaymentMode = LoanPrepaymentMode.ReduceTerm
        };
        _service.Seed(new SimulationDraftCondition(request));

        _viewModel.Prepare(null, conditionId);
        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Erken Ödeme", _viewModel.Name);
        Assert.Equal(loanId, _viewModel.LoanId);
        Assert.Equal("Garanti İhtiyaç Kredisi", _viewModel.LoanName);
        Assert.Equal("10000", _viewModel.AmountInput);
        Assert.Equal(LoanPrepaymentMode.ReduceTerm, _viewModel.SelectedPrepaymentMode);
    }

    [Fact]
    public async Task Save_TekSeferlikGelir_FutureIncomeKaydeder()
    {
        _viewModel.Prepare("income", null);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Prim";
        _viewModel.AmountInput = "25.000";
        _viewModel.Date = Today.AddDays(10);

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.FutureIncome, saved.Request.Type);
        Assert.Equal("Prim", saved.Request.Name);
        Assert.Equal(25_000m, saved.Request.Amount);
        Assert.Equal(Today.AddDays(10), saved.Request.StartDate);
        Assert.Equal(1, saved.Request.PaymentCount);
    }

    [Fact]
    public async Task Save_GelirDegisikligi_IncomeChangeKaydeder()
    {
        var incomeId = Guid.NewGuid();
        _viewModel.Prepare("income-change", null, null, null, incomeId);
        await _viewModel.LoadAsync();
        _viewModel.Name = "Maaş zammı";
        _viewModel.AmountInput = "60.000";
        _viewModel.Date = Today.AddDays(20);

        await _viewModel.SaveAsync();

        var saved = Assert.Single(_service.LastSaved!);
        Assert.Equal(SimulationScenarioType.IncomeChange, saved.Request.Type);
        Assert.Equal("Maaş zammı", saved.Request.Name);
        Assert.Equal(60_000m, saved.Request.Amount);
        Assert.Equal(incomeId, saved.Request.RecurringIncomeId);
        Assert.Equal(Today.AddDays(20), saved.Request.StartDate);
    }

    [Fact]
    public async Task Load_VarOlanGelirDegisikligi_DuzenlemeModundaYukler()
    {
        var incomeId = Guid.NewGuid();
        var income = new RecurringIncome { Id = incomeId, Name = "Maaş" };
        _reader.Plan = new FinancialPlan { RecurringIncomes = [income] };

        var conditionId = Guid.NewGuid();
        var request = new SimulationRequest
        {
            ScenarioId = conditionId,
            Type = SimulationScenarioType.IncomeChange,
            Name = "Zam",
            Amount = 60_000m,
            StartDate = Today.AddDays(20),
            RecurringIncomeId = incomeId
        };
        _service.Seed(new SimulationDraftCondition(request));

        _viewModel.Prepare(null, conditionId);
        await _viewModel.LoadAsync();

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Zam", _viewModel.Name);
        Assert.Equal(incomeId, _viewModel.IncomeId);
        Assert.Equal("Maaş", _viewModel.IncomeName);
        Assert.Equal("60000", _viewModel.AmountInput);
    }

    private async Task OpenNewAsync()
    {
        _viewModel.Prepare(SimulationScenarioCatalog.CashPayment.Key, null);
        await _viewModel.LoadAsync();
    }

    private static SimulationDraftCondition Condition(string name, decimal amount, DateOnly date, bool isEnabled) =>
        new(new SimulationRequest(SimulationScenarioType.CashPurchase, name, amount, date), isEnabled);
}
