using Mizan.Domain.Models;
using Xunit;
using static Mizan.Presentation.Tests.ViewModels.CardControlFixture;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Sıradaki ödeme kartının tutarı, tahmini / kesilmiş ekstre ayrımını, kararın bedelini ve
/// Asgari / Tamamı / Özel kararlarının kesilmiş ekstrede ve tahmini vadede nereye yazıldığını
/// doğrulayan birim testleri (EK-V7 S1, S2; S61). Beklenen tutarlar alan motorunun kurallarından:
/// tahmini ilk ekstreye devreden bakiyenin %5 faizi biner, asgari oran %20.
/// </summary>
public sealed class NextPaymentViewModelTests
{
    private readonly CardControlFixture _fx = new();

    [Fact]
    public async Task EkstreYoksa_SiradakiOdemeTahminiEkstredendir()
    {
        _fx.Add(Card("Bonus", carried: 4000m));

        await _fx.ViewModel.LoadAsync(null);

        var next = _fx.ViewModel.NextPayment;
        Assert.True(next.IsEstimate);
        Assert.Equal(DueDate, next.DueDate);
        Assert.Equal(4200m, next.StatementBalance);
        Assert.Equal(840m, next.MinimumPayment);
        Assert.Equal(4200m, next.PaymentAmount);
        Assert.True(next.IsFullSelected);
        Assert.False(next.HasCarry);
    }

    [Fact]
    public async Task KesilmisEkstreVarsa_OdemeEkstredendir()
    {
        _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Full));

        await _fx.ViewModel.LoadAsync(null);

        var next = _fx.ViewModel.NextPayment;
        Assert.False(next.IsEstimate);
        Assert.Equal(DueDate, next.DueDate);
        Assert.Equal(18200m, next.StatementBalance);
        Assert.Equal(3640m, next.MinimumPayment);
        Assert.Equal(18200m, next.PaymentAmount);
        Assert.True(next.IsFullSelected);
    }

    [Fact]
    public async Task AsgariSecilirse_DevirVeSonrakiFaizGorunur()
    {
        _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Minimum));

        await _fx.ViewModel.LoadAsync(null);

        var next = _fx.ViewModel.NextPayment;
        Assert.Equal(3640m, next.PaymentAmount);
        Assert.True(next.IsMinimumSelected);
        Assert.True(next.HasCarry);
        Assert.Equal(14560m, next.CarriedAmount);
        Assert.Equal(728m, next.NextInterest);
    }

    [Fact]
    public async Task KararYoksa_HicbirSecenekSeciliDegil_VarsayimTutariGosterilir()
    {
        _fx.Add(Card("Bonus", carried: 4000m, strategy: CreditCardPaymentStrategy.AskEachStatement));

        await _fx.ViewModel.LoadAsync(null);

        var next = _fx.ViewModel.NextPayment;
        Assert.Null(next.SelectedPaymentMode);
        Assert.Equal(840m, next.PaymentAmount);
        Assert.Equal(3360m, next.CarriedAmount);
        Assert.Equal(168m, next.NextInterest);
    }

    [Fact]
    public async Task SetFull_TahminiVadede_VadeyeOzelPlanYazar()
    {
        var card = _fx.Add(Card("Bonus", carried: 4000m, strategy: CreditCardPaymentStrategy.AskEachStatement));
        await _fx.ViewModel.LoadAsync(null);

        await _fx.ViewModel.NextPayment.SetFullCommand.ExecuteAsync(null);

        var plan = Assert.Single(_fx.Stored(card).PaymentPlans);
        Assert.Equal(DueDate, plan.DueDate);
        Assert.Equal(CreditCardPaymentType.FullStatement, plan.PaymentType);
        Assert.True(_fx.ViewModel.NextPayment.IsFullSelected);
        Assert.Equal(4200m, _fx.ViewModel.NextPayment.PaymentAmount);
        Assert.False(_fx.ViewModel.NextPayment.HasCarry);
    }

    [Fact]
    public async Task SetMinimum_KesilmisEkstrede_EkstreninPlaniniGunceller()
    {
        var card = _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Full));
        await _fx.ViewModel.LoadAsync(null);

        await _fx.ViewModel.NextPayment.SetMinimumCommand.ExecuteAsync(null);

        Assert.Equal(CurrentStatementPaymentMode.Minimum, _fx.Stored(card).CurrentStatementPaymentPlan?.Mode);
        Assert.Empty(_fx.Stored(card).PaymentPlans);
        Assert.Equal(3640m, _fx.ViewModel.NextPayment.PaymentAmount);
    }

    [Fact]
    public async Task SaveCustomAmount_KesilmisEkstrede_TurkceBicimiOkurVeKaydeder()
    {
        var card = _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Full));
        await _fx.ViewModel.LoadAsync(null);
        _fx.ViewModel.NextPayment.OpenCustomAmountCommand.Execute(null);
        Assert.True(_fx.ViewModel.NextPayment.IsCustomAmountOpen);
        _fx.ViewModel.NextPayment.CustomAmountInput = "5.000,50";

        await _fx.ViewModel.NextPayment.SaveCustomAmountCommand.ExecuteAsync(null);

        Assert.Equal(CurrentStatementPaymentMode.Custom, _fx.Stored(card).CurrentStatementPaymentPlan?.Mode);
        Assert.Equal(5000.50m, _fx.Stored(card).CurrentStatementPaymentPlan?.CustomAmount);
        Assert.Equal(5000.50m, _fx.ViewModel.NextPayment.PaymentAmount);
        Assert.True(_fx.ViewModel.NextPayment.IsCustomSelected);
        Assert.Equal("5000,5", _fx.ViewModel.NextPayment.CustomAmountInput);
    }

    [Fact]
    public async Task SaveCustomAmount_TahminiVadede_SabitTutarliVadePlaniYazar()
    {
        var card = _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));
        await _fx.ViewModel.LoadAsync(null);
        _fx.ViewModel.NextPayment.CustomAmountInput = "3000";

        await _fx.ViewModel.NextPayment.SaveCustomAmountCommand.ExecuteAsync(null);

        var plan = Assert.Single(_fx.Stored(card).PaymentPlans);
        Assert.Equal(CreditCardPaymentType.FixedAmount, plan.PaymentType);
        Assert.Equal(3000m, plan.Amount);
        Assert.Equal(3000m, _fx.ViewModel.NextPayment.PaymentAmount);
        Assert.True(_fx.ViewModel.NextPayment.IsCustomSelected);
    }

    [Fact]
    public async Task SaveCustomAmount_SayiDegilse_KaydetmezVeUyarir()
    {
        var card = _fx.Add(Card("Bonus", carried: 4000m));
        await _fx.ViewModel.LoadAsync(null);
        _fx.ViewModel.NextPayment.CustomAmountInput = "beş bin";

        await _fx.ViewModel.NextPayment.SaveCustomAmountCommand.ExecuteAsync(null);

        Assert.Empty(_fx.Stored(card).PaymentPlans);
        Assert.NotNull(_fx.Dialog.LastAlertMessage);
    }

    [Fact]
    public async Task SaveCustomAmount_EkstredenBuyukse_KuralinMesajiGosterilir()
    {
        var card = _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Full));
        await _fx.ViewModel.LoadAsync(null);
        _fx.ViewModel.NextPayment.CustomAmountInput = "20000";

        await _fx.ViewModel.NextPayment.SaveCustomAmountCommand.ExecuteAsync(null);

        Assert.Equal("Bu ekstre için özel ödeme tutarı 0 ile ekstre tutarı arasında olmalıdır.", _fx.Dialog.LastAlertMessage);
        Assert.Equal(CurrentStatementPaymentMode.Full, _fx.Stored(card).CurrentStatementPaymentPlan?.Mode);
        Assert.False(_fx.ViewModel.NextPayment.IsSaving);
    }
}
