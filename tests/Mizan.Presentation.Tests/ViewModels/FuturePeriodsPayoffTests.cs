using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// 12 Dönem ekranının erken kapama kartı (EK-V8 S5, S74-7): satırlar önerinin ham değerini taşır, öneri yoksa ya da
/// hesaplanamazsa kart görünmez ve sayfa düşmez, boş ya da hatalı sayfa öneri istemez, satır krediyi açar.
/// </summary>
public sealed class FuturePeriodsPayoffTests
{
    private static readonly DateOnly ChainStart = new(2026, 10, 10);
    private static readonly Guid VehicleLoanId = Guid.NewGuid();
    private static readonly Guid PersonalLoanId = Guid.NewGuid();

    private readonly FakeFutureProjectionService _projectionService = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FuturePeriodsViewModel _viewModel;

    public FuturePeriodsPayoffTests()
    {
        _viewModel = new FuturePeriodsViewModel(_projectionService, _navigation);
        _projectionService.Result = Projection();
    }

    [Fact]
    public async Task Yukle_OneriVarsa_SatirlarKrediSirasiylaHamDegerTasir()
    {
        _projectionService.PayoffAdvice = [Recommended(), NeedsPrincipal()];

        await _viewModel.LoadAsync();

        Assert.True(_viewModel.HasPayoffRows);
        Assert.Equal([VehicleLoanId, PersonalLoanId], _viewModel.PayoffRows.Select(x => x.LoanId));
        var recommended = _viewModel.PayoffRows[0];
        Assert.Equal("Taşıt kredisi", recommended.LoanName);
        Assert.Equal(LoanPayoffAdviceStatus.Recommended, recommended.Status);
        Assert.Equal(new DateOnly(2027, 9, 18), recommended.Date);
        Assert.Equal(21_400m, recommended.PayoffAmount);
        Assert.Equal(2_950m, recommended.NetGain);
        Assert.True(recommended.IsRecommended);
        Assert.False(_viewModel.PayoffRows[1].IsRecommended);
        Assert.Null(_viewModel.PayoffRows[1].NetGain);
    }

    [Fact]
    public async Task Yukle_OneriYoksa_KartGorunmez()
    {
        _projectionService.PayoffAdvice = [];

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Empty(_viewModel.PayoffRows);
        Assert.False(_viewModel.HasPayoffRows);
    }

    [Fact]
    public async Task Yukle_OneriHesaplanamazsa_SayfaDusmezKartGorunmez()
    {
        _projectionService.PayoffFailure = new InvalidOperationException("Öneri hesaplanamadı.");

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.Equal(12, _viewModel.Periods.Count);
        Assert.Empty(_viewModel.PayoffRows);
        Assert.False(_viewModel.HasPayoffRows);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Yukle_ProjeksiyonYoksa_OneriIstenmez()
    {
        _projectionService.Result = null;
        _projectionService.PayoffAdvice = [Recommended()];

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Empty, _viewModel.State);
        Assert.Equal(0, _projectionService.PayoffCallCount);
        Assert.False(_viewModel.HasPayoffRows);
    }

    [Fact]
    public async Task Yukle_ProjeksiyonOkunamazsa_OneriIstenmez()
    {
        _projectionService.Failure = new InvalidOperationException("Projeksiyon hesaplanamadı.");
        _projectionService.PayoffAdvice = [Recommended()];

        await _viewModel.LoadAsync();

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.Equal(0, _projectionService.PayoffCallCount);
        Assert.False(_viewModel.HasPayoffRows);
    }

    [Fact]
    public async Task Yukle_KrediFormundanDonunce_SatirYeniDurumuGosterir()
    {
        _projectionService.PayoffAdvice = [Recommended(), NeedsPrincipal()];
        await _viewModel.LoadAsync();
        _projectionService.PayoffAdvice = [Recommended() with { Status = LoanPayoffAdviceStatus.AlreadyClosing, PayoffAmount = null, NetGain = null }];

        await _viewModel.LoadAsync();

        var row = Assert.Single(_viewModel.PayoffRows);
        Assert.Equal(LoanPayoffAdviceStatus.AlreadyClosing, row.Status);
        Assert.Equal(new DateOnly(2027, 9, 18), row.Date);
    }

    [Fact]
    public async Task Yukle_SayfaBosaDonerse_EskiOneriKalmaz()
    {
        _projectionService.PayoffAdvice = [Recommended()];
        await _viewModel.LoadAsync();
        _projectionService.Result = null;

        await _viewModel.LoadAsync();

        Assert.Empty(_viewModel.PayoffRows);
        Assert.False(_viewModel.HasPayoffRows);
    }

    [Fact]
    public async Task KrediyiAc_SatirKrediFormunuKimligiyleAcar()
    {
        var row = new LoanPayoffRow { LoanId = VehicleLoanId, LoanName = "Taşıt kredisi", Status = LoanPayoffAdviceStatus.NotWorthIt };

        await _viewModel.OpenLoanAsync(row);

        Assert.Equal(Routes.LoanForm, _navigation.LastNavigatedRoute);
        Assert.NotNull(_navigation.LastParameters);
        Assert.Equal(VehicleLoanId, _navigation.LastParameters[Routes.LoanIdParameter]);
    }

    private static LoanPayoffAdvice Recommended() => new()
    {
        LoanId = VehicleLoanId,
        LoanName = "Taşıt kredisi",
        Status = LoanPayoffAdviceStatus.Recommended,
        Date = new DateOnly(2027, 9, 18),
        PayoffAmount = 21_400m,
        InterestSaving = 3_400m,
        NetGain = 2_950m
    };

    private static LoanPayoffAdvice NeedsPrincipal() => new()
    {
        LoanId = PersonalLoanId,
        LoanName = "İhtiyaç kredisi",
        Status = LoanPayoffAdviceStatus.NeedsPrincipal
    };

    // Kartın davranışı dönemlerin değerine bakmaz; zincir yalnız sayfanın içerikte olması için kurulur.
    private static FinancialProjectionResult Projection()
    {
        var periods = Enumerable.Range(0, 12)
            .Select(i => new CashFlowPeriodProjection
            {
                Period = new CashFlowPeriod(ChainStart.AddMonths(i), ChainStart.AddMonths(i + 1)),
                OpeningBalance = 40_000m,
                EndingBalance = 40_000m
            })
            .ToList();
        return new FinancialProjectionResult(periods, new PeriodObligationPlan([], [], []), []);
    }
}
