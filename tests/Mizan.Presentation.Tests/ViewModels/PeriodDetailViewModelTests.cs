using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Dönem ayrıntısının (EK-V9, S75) davranışı: dönem 12 dönem zincirinden ilk günüyle seçilir, akış dönem sonuna
/// kuruşu kuruşuna iner, ödemeler gün sırasıyla ve dört satırdan sonrası yerinde açılır, kart faizi kart başına,
/// kart satırı Kart Kontrol'ü açar, üç durum.
/// </summary>
public sealed class PeriodDetailViewModelTests
{
    private static readonly DateOnly ChainStart = new(2026, 10, 10);
    private static readonly DateOnly Target = new(2027, 1, 10);
    private static readonly Guid BonusCard = Guid.NewGuid();
    private static readonly Guid AxessCard = Guid.NewGuid();
    private static readonly string[] FirstFourByDueDate = ["Telefon taksiti", "Konut kredisi", "Aidat", "Bonus"];

    private readonly FakeFutureProjectionService _projectionService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly PeriodDetailViewModel _viewModel;

    public PeriodDetailViewModelTests()
    {
        _viewModel = new PeriodDetailViewModel(_projectionService, _navigation);
    }

    [Fact]
    public async Task Yukle_SecilenDonem_AkisVeDonemSonunuSunar()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(Target, _viewModel.PeriodStart);
        Assert.Equal(new DateOnly(2027, 2, 9), _viewModel.PeriodLastDay);
        Assert.Equal(3_000m, _viewModel.EndingBalance);
        Assert.Equal(2_000m, _viewModel.OpeningBalance);
        Assert.Equal(48_000m, _viewModel.Income);
        Assert.Equal(27_000m, _viewModel.PaymentsTotal);
        Assert.Equal(20_000m, _viewModel.VariableExpenseAllowance);
        Assert.Equal(0m, _viewModel.DeficitInterest);
        Assert.False(_viewModel.HasDeficitInterest);
        Assert.False(_viewModel.IsOpeningNegative);
    }

    [Fact]
    public async Task Yukle_AkisSatirlari_DonemSonunaKurusuKurusunaIner()
    {
        _projectionService.Result = Chain(DeficitPeriod());

        await _viewModel.LoadAsync(Target);

        var flow = _viewModel.OpeningBalance + _viewModel.Income - _viewModel.PaymentsTotal
                   - _viewModel.VariableExpenseAllowance - _viewModel.DeficitInterest;
        Assert.Equal(-6_532.47m, _viewModel.EndingBalance);
        Assert.Equal(_viewModel.EndingBalance, flow);
    }

    [Fact]
    public async Task Yukle_DonemBasiEksiVeKmhFaiziVarsa_IkisiDeIsaretlenir()
    {
        _projectionService.Result = Chain(DeficitPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.True(_viewModel.IsOpeningNegative);
        Assert.True(_viewModel.HasDeficitInterest);
        Assert.Equal(132.47m, _viewModel.DeficitInterest);
    }

    [Fact]
    public async Task Yukle_DonemSonuDonemBasindanDusukse_NetDegisimEksidir()
    {
        _projectionService.Result = Chain(DeficitPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.Equal(-2_532.47m, _viewModel.NetChange);
        Assert.True(_viewModel.IsNetChangeNegative);
        Assert.False(_viewModel.IsNetChangePositive);
    }

    [Fact]
    public async Task Yukle_DonemSonuDonemBasindanYuksekse_NetDegisimArtidir()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.Equal(1_000m, _viewModel.NetChange);
        Assert.True(_viewModel.IsNetChangePositive);
        Assert.False(_viewModel.IsNetChangeNegative);
    }

    [Fact]
    public async Task Yukle_Odemeler_GunVeAdSirasiylaDortSatirSonrasiGizlidir()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.Equal(FirstFourByDueDate, _viewModel.Payments.Select(x => x.Name));
    }

    [Fact]
    public async Task Yukle_Odemeler_BuyukHarcamaVeTutarsizKartEkstresiDahilSayilir()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.True(_viewModel.HasPayments);
        Assert.Equal(6, _viewModel.PaymentCount);
        Assert.Equal(2, _viewModel.HiddenPaymentCount);
        Assert.True(_viewModel.HasHiddenPayments);
    }

    [Fact]
    public async Task GizliOdemeleriAc_HepsiGorunurTasmaKalmaz()
    {
        _projectionService.Result = Chain(TargetPeriod());
        await _viewModel.LoadAsync(Target);

        _viewModel.ExpandPayments();

        Assert.Equal(6, _viewModel.Payments.Count);
        Assert.Equal("Tatil", _viewModel.Payments[^1].Name);
        Assert.Equal(0, _viewModel.HiddenPaymentCount);
        Assert.False(_viewModel.HasHiddenPayments);
    }

    [Fact]
    public async Task Yukle_KartOdemesi_KartKimliginiTahminiVeBelirsizligiTasir()
    {
        _projectionService.Result = Chain(TargetPeriod());
        await _viewModel.LoadAsync(Target);

        _viewModel.ExpandPayments();

        var bonus = Assert.Single(_viewModel.Payments, x => x.Name == "Bonus");
        var axess = Assert.Single(_viewModel.Payments, x => x.Name == "Axess");
        var loan = Assert.Single(_viewModel.Payments, x => x.Name == "Konut kredisi");
        Assert.Equal((BonusCard, 8_200m, true), (bonus.CardId!.Value, bonus.Amount!.Value, bonus.IsEstimate));
        Assert.Equal(AxessCard, axess.CardId);
        Assert.True(axess.IsUndetermined);
        Assert.False(loan.IsCard);
        Assert.Equal(new DateOnly(2027, 1, 15), loan.DueDate);
    }

    [Fact]
    public async Task Yukle_KartFaizi_YalnizFaizDoganKartlarVeToplam()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.True(_viewModel.HasCardInterest);
        Assert.Equal(new[] { new PeriodCardInterestRow("Bonus", 420m), new PeriodCardInterestRow("Axess", 220m) }, _viewModel.CardInterests);
        Assert.Equal(640m, _viewModel.CardInterestTotal);
    }

    [Fact]
    public async Task Yukle_KartFaiziVeOdemeYoksa_IkiListeDeGizlidir()
    {
        _projectionService.Result = Chain(DeficitPeriod());

        await _viewModel.LoadAsync(Target);

        Assert.False(_viewModel.HasCardInterest);
        Assert.Empty(_viewModel.CardInterests);
        Assert.False(_viewModel.HasPayments);
        Assert.Equal(0, _viewModel.PaymentCount);
        Assert.False(_viewModel.HasHiddenPayments);
    }

    [Fact]
    public async Task Yukle_DonemZincirdeYoksa_BosDurumdur()
    {
        _projectionService.Result = Chain(TargetPeriod());

        await _viewModel.LoadAsync(new DateOnly(2030, 1, 10));

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.Null(_viewModel.EndingBalance);
        Assert.Empty(_viewModel.Payments);
    }

    [Fact]
    public async Task Yukle_ZincirKurulamazsa_BosDurumdur()
    {
        _projectionService.Result = null;

        await _viewModel.LoadAsync(Target);

        Assert.Equal(ScreenState.Empty, _viewModel.State);
    }

    [Fact]
    public async Task Yukle_HesapHatasi_HataDurumudur()
    {
        _projectionService.Failure = new InvalidOperationException("Projeksiyon kurulamadı.");

        await _viewModel.LoadAsync(Target);

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Yukle_Surerken_YukleniyorDurumudur()
    {
        var pending = new TaskCompletionSource<FinancialProjectionResult?>();
        _projectionService.Pending = pending;

        var load = _viewModel.LoadAsync(Target);

        Assert.Equal(ScreenState.Loading, _viewModel.State);
        pending.SetResult(null);
        await load;
    }

    [Fact]
    public async Task Yukle_ParametresizTekrarlanirsa_AyniDonemiYenidenOkurVeListeyiDaraltir()
    {
        _projectionService.Result = Chain(TargetPeriod());
        await _viewModel.LoadAsync(Target);
        _viewModel.ExpandPayments();
        _projectionService.Result = Chain(TargetPeriod() with { EndingBalance = 2_500m });

        await _viewModel.LoadAsync(null);

        Assert.Equal(Target, _viewModel.PeriodStart);
        Assert.Equal(2_500m, _viewModel.EndingBalance);
        Assert.Equal(4, _viewModel.Payments.Count);
    }

    [Fact]
    public async Task KartSatirinaDokunmak_KartKontroluAcar()
    {
        var row = new PeriodPaymentRow { DueDate = Target, Name = "Bonus", Amount = 8_200m, CardId = BonusCard };

        await _viewModel.OpenCardControlAsync(row);

        Assert.Equal(Routes.CardControl, _navigation.LastNavigatedRoute);
        Assert.Equal(BonusCard, _navigation.LastParameters![Routes.CardIdParameter]);
    }

    [Fact]
    public async Task KartOlmayanSatiraDokunmak_HicbirYereGitmez()
    {
        var row = new PeriodPaymentRow { DueDate = Target, Name = "Konut kredisi", Amount = 12_450m };

        await _viewModel.OpenCardControlAsync(row);

        Assert.Null(_navigation.LastNavigatedRoute);
    }

    [Fact]
    public async Task GeriDon_OncekiSayfayaDoner()
    {
        await _viewModel.GoBackAsync();

        Assert.True(_navigation.NavigateBackCalled);
    }

    // Dönem: 10 Ocak – 10 Şubat 2027. 2.000 + 48.000 − (22.000 zorunlu + 5.000 tatil) − 20.000 yaşam = 3.000.
    // Aidat ve Bonus aynı gün: ada göre sıralanır. Axess ekstresinin tutarı belirlenemedi.
    private static CashFlowPeriodProjection TargetPeriod() => new()
    {
        Period = new CashFlowPeriod(Target, Target.AddMonths(1)),
        OpeningBalance = 2_000m, TotalIncome = 48_000m, MandatoryOutflow = 22_000m, PlannedLargeCashExpenses = 5_000m,
        VariableExpenseAllowance = 20_000m, EndingBalance = 3_000m, CardInterestGenerated = 640m,
        MandatoryItems =
        [
            new ObligationItem("Konut kredisi", ObligationType.Loan, new DateOnly(2027, 1, 15), 12_450m),
            new ObligationItem("Bonus", ObligationType.CreditCard, new DateOnly(2027, 1, 20), 8_200m) { IsEstimate = true, PaymentId = BonusCard },
            new ObligationItem("Aidat", ObligationType.OtherScheduledPayment, new DateOnly(2027, 1, 20), 0m),
            new ObligationItem("Telefon taksiti", ObligationType.InstallmentPayment, new DateOnly(2027, 1, 12), 1_350m)
        ],
        LargeExpenseItems = [new PlannedLargeExpense { Name = "Tatil", Amount = 5_000m, ExactDate = new DateOnly(2027, 2, 1) }],
        CardPaymentStatuses =
        [
            new CreditCardPaymentProjectionStatus { CardId = BonusCard, CardName = "Bonus", PaymentDueDate = new DateOnly(2027, 1, 20), Payment = 8_200m, CarryInterest = 420m },
            new CreditCardPaymentProjectionStatus { CardId = AxessCard, CardName = "Axess", PaymentDueDate = new DateOnly(2027, 1, 25), CarryInterest = 220m },
            new CreditCardPaymentProjectionStatus { CardId = Guid.NewGuid(), CardName = "World", PaymentDueDate = new DateOnly(2027, 1, 28), Payment = 0m }
        ]
    };

    // Eksi açılır, KMH faiziyle daha da eksi kapanır: −4.000 + 30.000 − 18.400 − 14.000 − 132,47 = −6.532,47.
    private static CashFlowPeriodProjection DeficitPeriod() => new()
    {
        Period = new CashFlowPeriod(Target, Target.AddMonths(1)),
        OpeningBalance = -4_000m, TotalIncome = 30_000m, MandatoryOutflow = 18_400m, VariableExpenseAllowance = 14_000m,
        DeficitFinancingInterest = 132.47m, EndingBalance = -6_532.47m
    };

    // Hedef dönemden önce ve sonra zincirin başka dönemleri de var; sayfa doğrusunu ilk günüyle seçmeli.
    private static FinancialProjectionResult Chain(CashFlowPeriodProjection target)
    {
        var periods = new List<CashFlowPeriodProjection>();
        for (var start = ChainStart; start < ChainStart.AddMonths(12); start = start.AddMonths(1))
        {
            periods.Add(start == target.PeriodStart
                ? target
                : new CashFlowPeriodProjection { Period = new CashFlowPeriod(start, start.AddMonths(1)), EndingBalance = 99_999m });
        }

        return new FinancialProjectionResult(periods, new PeriodObligationPlan([], [], []), []);
    }
}
