using Mizan.Application.Abstractions;
using Mizan.Application.Services;
using Mizan.Application.Tests.Fakes;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Kredi formunun erken ödeme yolu (S64-12–14): kredi ve erken ödemeleri tek işlemde, tek plan
/// revizyonuyla yazılır; erken ödemeler kredinin kaydedileceği hâline göre yeniden doğrulanır ve
/// hesaplanır; faiz çözülemezse mesaj formun dilindedir. Referans kredi: 12 × 10.000 TL kalan taksit,
/// 100.000 TL anapara, sonraki taksit 15.10.2026 (son taksit 15.09.2027), bugün 25.09.2026.
/// </summary>
public sealed class ObligationManagementServiceLoanPrepaymentTests
{
    private const string NeedsAmountMessage =
        "Erken ödemeyi hesaplamak için kalan anaparayı ya da bankanın kapatma tutarını gir.";

    private static readonly DateOnly Today = new(2026, 9, 25);

    private readonly InMemoryLoanRepository _loanRepository = new();
    private readonly RecordingPlanChangeRecorder _changeRecorder = new();
    private readonly LoanPaymentScheduleBuilder _scheduleBuilder;
    private readonly LoanPayoffService _loanPayoffService;
    private readonly ObligationManagementService _sut;

    public ObligationManagementServiceLoanPrepaymentTests()
    {
        var scheduleCalculator = new LoanScheduleCalculator();
        var amortizationCalculator = new LoanAmortizationCalculator(scheduleCalculator);
        _scheduleBuilder = new LoanPaymentScheduleBuilder(scheduleCalculator, amortizationCalculator);
        _loanPayoffService = new LoanPayoffService(
            new FixedClock(Today), amortizationCalculator, _scheduleBuilder,
            new LoanPrepaymentValidator(amortizationCalculator, _scheduleBuilder));
        _sut = new ObligationManagementService(
            _loanRepository, new InMemoryTemporaryPaymentPlanRepository(), new InMemoryPlannedLargeExpenseRepository(),
            _loanPayoffService, _changeRecorder);
    }

    [Fact]
    public async Task SaveLoanAsync_KrediVeErkenOdemeler_TekIslemdeVeTekRevizyonlaYazilir()
    {
        var loan = ReferenceLoan();
        var closure = Closure(new DateOnly(2027, 3, 15)) with { LoanId = Guid.Empty };
        var partial = Partial(new DateOnly(2026, 12, 15), 20_000m) with { LoanId = Guid.Empty };

        await _sut.SaveLoanAsync(loan, [closure, partial]);

        Assert.Equal(1, _loanRepository.CombinedWriteCount);
        Assert.Equal(loan.Id, Assert.Single(await _loanRepository.GetLoansAsync()).Id);
        var saved = await _loanRepository.GetLoanPrepaymentsAsync();
        Assert.Equal([partial.Id, closure.Id], saved.Select(p => p.Id));
        Assert.All(saved, p => Assert.Equal(loan.Id, p.LoanId));
        Assert.Equal(["Kredi planı değişti"], _changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveLoanAsync_ListedeOlmayanErkenOdeme_SilinirBaskaKredininkineDokunulmaz()
    {
        var loan = ReferenceLoan();
        var otherLoanPrepayment = Closure(new DateOnly(2027, 1, 15)) with { LoanId = Guid.NewGuid() };
        await _loanRepository.UpsertLoanPrepaymentAsync(Closure(new DateOnly(2027, 2, 15)) with { LoanId = loan.Id });
        await _loanRepository.UpsertLoanPrepaymentAsync(otherLoanPrepayment);

        await _sut.SaveLoanAsync(loan, []);

        Assert.Equal(otherLoanPrepayment.Id, Assert.Single(await _loanRepository.GetLoanPrepaymentsAsync()).Id);
    }

    [Fact]
    public async Task SaveLoanAsync_ErkenOdemeKrediyeUymazsa_TarihliMesajlaReddedilirHicbirSeyYazilmaz()
    {
        var afterEnd = Closure(new DateOnly(2028, 1, 1));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SaveLoanAsync(ReferenceLoan(), [afterEnd, Partial(new DateOnly(2026, 12, 15), 20_000m)]));

        Assert.StartsWith("01.01.2028 tarihli erken ödeme:", error.Message);
        Assert.Contains("kapatılacak taksiti kalmıyor", error.Message);
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(await _loanRepository.GetLoanPrepaymentsAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public async Task SaveLoanAsync_FaiziCozulemeyenKredideErkenOdeme_FormunDiliyleReddedilir()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SaveLoanAsync(ReferenceLoan() with { RemainingDebt = null }, [Closure(new DateOnly(2027, 3, 15))]));

        Assert.EndsWith(NeedsAmountMessage, error.Message);
        Assert.DoesNotContain("Finansal Yapı", error.Message);
        Assert.Empty(await _loanRepository.GetLoansAsync());
    }

    [Fact]
    public async Task SaveLoanAsync_ErkenOdemesizFaiziCozulemeyenKredi_Kaydedilir()
    {
        await _sut.SaveLoanAsync(ReferenceLoan() with { RemainingDebt = null }, []);

        Assert.Single(await _loanRepository.GetLoansAsync());
    }

    [Fact]
    public async Task PreviewLoanPrepayments_KaydedilecekHaldenTarihSirasiylaTutarVerirHicbirSeyYazmaz()
    {
        // Anapara yok, bankanın tutarı var: tutarlar kaydın tutardan çözeceği anaparayla hesaplanır.
        var loan = ReferenceLoan() with { RemainingDebt = null, EarlyClosureAmount = 101_000m };
        var closure = Closure(new DateOnly(2027, 3, 15)) with { LoanId = Guid.Empty };
        var partial = Partial(new DateOnly(2026, 12, 15), 20_000m) with { LoanId = Guid.Empty };

        var planned = _sut.PreviewLoanPrepayments(loan, [closure, partial]);

        Assert.Equal([partial.Id, closure.Id], planned.Select(x => x.Prepayment.Id));
        Assert.All(planned, x => Assert.Equal(loan.Id, x.Prepayment.LoanId));
        var prepared = _loanPayoffService.PrepareForSave(loan);
        var replay = _scheduleBuilder.Replay(prepared, planned.Select(x => x.Prepayment));
        Assert.All(planned, x => Assert.Equal(replay.Payments.Single(p => p.SourceId == x.Prepayment.Id).Amount, x.Amount));
        Assert.All(planned, x => Assert.False(x.IsUnquotable));
        Assert.Empty(await _loanRepository.GetLoansAsync());
        Assert.Empty(_changeRecorder.RecordedTriggers);
    }

    [Fact]
    public void PreviewLoanPrepayments_KayitReddedecekse_HerErkenOdemeTutarsizGelir()
    {
        var mismatched = ReferenceLoan() with { RemainingDebt = 80_000m, EarlyClosureAmount = 150_000m };

        var planned = _sut.PreviewLoanPrepayments(mismatched, [Closure(new DateOnly(2027, 3, 15)), Partial(new DateOnly(2026, 12, 15), 5_000m)]);

        Assert.Equal(2, planned.Count);
        Assert.All(planned, x => Assert.Null(x.Amount));
    }

    [Fact]
    public void ValidateLoanPrepayment_GecerliAday_KabulEdilir()
    {
        Assert.Null(_sut.ValidateLoanPrepayment(ReferenceLoan(), [], Partial(new DateOnly(2026, 12, 15), 20_000m)));
    }

    [Fact]
    public void ValidateLoanPrepayment_YeniKredininKapatmasindanSonrakiAday_Reddedilir()
    {
        // Yeni kredi: erken ödemelerin kimliği henüz boş; servis onları taslak krediye bağlar.
        var closure = Closure(new DateOnly(2027, 1, 15)) with { LoanId = Guid.Empty };
        var later = Partial(new DateOnly(2027, 3, 15), 5_000m) with { LoanId = Guid.Empty };

        var message = _sut.ValidateLoanPrepayment(ReferenceLoan(), [closure], later);

        Assert.Equal("Bu kredi 15.01.2027 tarihinde zaten kapatılıyor.", message);
    }

    [Fact]
    public void ValidateLoanPrepayment_KayitKrediyiReddedecekse_KaydinMesajiniDoner()
    {
        var mismatched = ReferenceLoan() with { RemainingDebt = 80_000m, EarlyClosureAmount = 150_000m };

        var message = _sut.ValidateLoanPrepayment(mismatched, [], Closure(new DateOnly(2027, 3, 15)));

        Assert.NotNull(message);
        Assert.Contains("uyuşmuyor", message);
    }

    [Fact]
    public void ValidateLoanPrepayment_FaizCozulemezse_FormunDiliyleMesajDoner()
    {
        var message = _sut.ValidateLoanPrepayment(ReferenceLoan() with { RemainingDebt = null }, [], Closure(new DateOnly(2027, 3, 15)));

        Assert.Equal(NeedsAmountMessage, message);
    }

    private static Loan ReferenceLoan() => new()
    {
        Name = "İhtiyaç",
        MonthlyPayment = 10_000m,
        PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 10, 15),
        RemainingInstallmentCount = 12,
        RemainingDebt = 100_000m
    };

    private static LoanPrepayment Closure(DateOnly date) => new() { Date = date, Mode = LoanPrepaymentMode.FullClosure };

    private static LoanPrepayment Partial(DateOnly date, decimal principal) =>
        new() { Date = date, Mode = LoanPrepaymentMode.ReduceTerm, PrincipalAmount = principal };

    private sealed class RecordingPlanChangeRecorder : IPlanChangeRecorder
    {
        public List<string> RecordedTriggers { get; } = [];

        public Task RecordChangeAsync(string trigger, CancellationToken cancellationToken = default)
        {
            RecordedTriggers.Add(trigger);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateOnly today) : IClock
    {
        public DateOnly Today { get; } = today;
        public DateTimeOffset UtcNow { get; } = new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}
