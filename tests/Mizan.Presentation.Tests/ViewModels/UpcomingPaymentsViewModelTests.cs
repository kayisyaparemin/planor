using Mizan.Domain.Models;
using Xunit;
using static Mizan.Presentation.Tests.ViewModels.CardControlFixture;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Sonraki ödemeler listesinin (en fazla dört vade, sıfır tutarlı vadeler atlanır), vadeye
/// özel kararın ve kartın varsayılan ödeme şeklinin davranışını doğrulayan birim testleri
/// (EK-V7 S3, S4). Asgari stratejili 10.000 ₺ devreden kartta her ay devir kalır.
/// </summary>
public sealed class UpcomingPaymentsViewModelTests
{
    private static readonly DateOnly OctoberDue = new(2026, 10, 25);
    private readonly CardControlFixture _fx = new();

    [Fact]
    public async Task Load_SonrakiOdemeler_DortVadeyleSinirlanir()
    {
        _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));

        await _fx.ViewModel.LoadAsync(null);

        var upcoming = _fx.ViewModel.Upcoming;
        Assert.True(upcoming.HasItems);
        Assert.Equal(4, upcoming.Items.Count);
        var first = upcoming.Items[0];
        Assert.Equal(OctoberDue, first.DueDate);
        Assert.Equal(8820m, first.StatementBalance);
        Assert.Equal(1764m, first.PaymentAmount);
        Assert.Equal(CreditCardPaymentType.Minimum, first.AppliedPaymentType);
        Assert.Equal(CreditCardPaymentResolution.GeneralStrategy, first.Resolution);
    }

    [Fact]
    public async Task Load_TamamiOdenenKartta_SifirTutarliVadelerAtlanir()
    {
        _fx.Add(Card("Bonus", carried: 4000m, strategy: CreditCardPaymentStrategy.FullStatement));

        await _fx.ViewModel.LoadAsync(null);

        Assert.True(_fx.ViewModel.HasUpcomingPayment);
        Assert.False(_fx.ViewModel.Upcoming.HasItems);
        Assert.Empty(_fx.ViewModel.Upcoming.Items);
    }

    [Fact]
    public async Task Load_VarsayilanKuralKarttanGelir()
    {
        _fx.Add(Card("Bonus", carried: 4000m, strategy: CreditCardPaymentStrategy.AskEachStatement, fallback: ProjectionFallbackStrategy.Minimum));

        await _fx.ViewModel.LoadAsync(null);

        Assert.Equal(CreditCardPaymentStrategy.AskEachStatement, _fx.ViewModel.Upcoming.DefaultRule?.Strategy);
        Assert.Equal(ProjectionFallbackStrategy.Minimum, _fx.ViewModel.Upcoming.DefaultRule?.Fallback);
    }

    [Fact]
    public async Task Decide_Tamami_VadeyeOzelPlanYazar()
    {
        var card = _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = "Tamamı";

        Assert.NotEmpty(_fx.ViewModel.Upcoming.Items);

        await _fx.ViewModel.Upcoming.DecideCommand.ExecuteAsync(_fx.ViewModel.Upcoming.Items[0]);

        var plan = Assert.Single(_fx.Stored(card).PaymentPlans);
        Assert.Equal(OctoberDue, plan.DueDate);
        Assert.Equal(CreditCardPaymentType.FullStatement, plan.PaymentType);
        var first = Assert.Single(_fx.ViewModel.Upcoming.Items);
        Assert.True(first.IsDueDateOverride);
        Assert.Equal(8820m, first.PaymentAmount);
    }

    [Fact]
    public async Task Decide_VarsayilanaDon_VadePlaniniKaldirir()
    {
        var card = Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum);
        card = _fx.Add(card with
        {
            PaymentPlans = [new CreditCardPaymentPlan { CreditCardId = card.Id, DueDate = OctoberDue, PaymentType = CreditCardPaymentType.FullStatement }]
        });
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = "Kartın varsayılanına dön";

        Assert.NotEmpty(_fx.ViewModel.Upcoming.Items);

        await _fx.ViewModel.Upcoming.DecideCommand.ExecuteAsync(_fx.ViewModel.Upcoming.Items[0]);

        Assert.Contains("Kartın varsayılanına dön", _fx.Dialog.LastChooseOptions!);
        Assert.Empty(_fx.Stored(card).PaymentPlans);
        Assert.Equal(CreditCardPaymentResolution.GeneralStrategy, _fx.ViewModel.Upcoming.Items[0].Resolution);
    }

    [Fact]
    public async Task Decide_VadeyeOzelDegilse_VarsayilanaDonSunulmaz_VazgecilinceDegisiklikYok()
    {
        var card = _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = null;

        Assert.NotEmpty(_fx.ViewModel.Upcoming.Items);

        await _fx.ViewModel.Upcoming.DecideCommand.ExecuteAsync(_fx.ViewModel.Upcoming.Items[0]);

        Assert.DoesNotContain("Kartın varsayılanına dön", _fx.Dialog.LastChooseOptions!);
        Assert.Empty(_fx.Stored(card).PaymentPlans);
        Assert.Equal(4, _fx.ViewModel.Upcoming.Items.Count);
    }

    [Fact]
    public async Task ChangeDefault_Tamami_KartinVarsayilaniniYazar()
    {
        var card = _fx.Add(Card("Bonus", carried: 4000m, strategy: CreditCardPaymentStrategy.AskEachStatement));
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = "Tamamı";

        await _fx.ViewModel.Upcoming.ChangeDefaultCommand.ExecuteAsync(null);

        Assert.Equal(CreditCardPaymentStrategy.FullStatement, _fx.Stored(card).PaymentStrategy);
        Assert.Equal(CreditCardPaymentStrategy.FullStatement, _fx.ViewModel.Upcoming.DefaultRule?.Strategy);
        Assert.True(_fx.ViewModel.NextPayment.IsFullSelected);
    }

    [Fact]
    public async Task ChangeDefault_HerEkstredeSor_VarsayimdaSorulurVeYazilir()
    {
        var card = _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.ChooseResponses.Enqueue("Her ekstrede sor");
        _fx.Dialog.ChooseResponses.Enqueue("Tamamı");

        await _fx.ViewModel.Upcoming.ChangeDefaultCommand.ExecuteAsync(null);

        Assert.Equal(CreditCardPaymentStrategy.AskEachStatement, _fx.Stored(card).PaymentStrategy);
        Assert.Equal(ProjectionFallbackStrategy.FullStatement, _fx.Stored(card).ProjectionFallbackStrategy);
        Assert.Null(_fx.ViewModel.NextPayment.SelectedPaymentMode);
    }

    [Fact]
    public async Task ChangeDefault_VarsayimSorusundaVazgecilirse_HicbirSeyYazilmaz()
    {
        var card = _fx.Add(Card("Bonus", carried: 10000m, strategy: CreditCardPaymentStrategy.Minimum));
        await _fx.ViewModel.LoadAsync(null);
        _fx.Dialog.ChooseResponses.Enqueue("Her ekstrede sor");
        _fx.Dialog.ChooseResponses.Enqueue(null);

        await _fx.ViewModel.Upcoming.ChangeDefaultCommand.ExecuteAsync(null);

        Assert.DoesNotContain("Her ekstrede sor", _fx.Dialog.LastChooseOptions ?? ["Her ekstrede sor"]);
        Assert.Equal(CreditCardPaymentStrategy.Minimum, _fx.Stored(card).PaymentStrategy);
    }
}
