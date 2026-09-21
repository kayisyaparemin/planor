using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Kredi erken ödeme ve taksit takvimi yeniden oynatma motorunun (LoanPaymentScheduleBuilder)
/// davranışsal ve matematiksel doğruluğunu test eder.
/// </summary>
public sealed class LoanPaymentScheduleBuilderTests
{
    private readonly LoanScheduleCalculator _scheduleCalculator = new();
    private readonly LoanAmortizationCalculator _amortizationCalculator;
    private readonly LoanPaymentScheduleBuilder _builder;

    private static readonly DateOnly SixthInstallmentDate = new(2027, 2, 15);

    public LoanPaymentScheduleBuilderTests()
    {
        _amortizationCalculator = new LoanAmortizationCalculator(_scheduleCalculator);
        _builder = new LoanPaymentScheduleBuilder(_scheduleCalculator, _amortizationCalculator);
    }

    private static Loan CreateReferenceLoan(LoanKind kind = LoanKind.Consumer) => new()
    {
        Name = "Referans Kredi",
        Bank = "Test Bankası",
        MonthlyPayment = 10_046.21m,
        PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 9, 15),
        RemainingInstallmentCount = 12,
        RemainingDebt = 100_000m,
        Kind = kind
    };

    private static LoanPrepayment CreatePrepayment(
        Loan loan,
        LoanPrepaymentMode mode,
        DateOnly date,
        decimal? principal = null) => new()
    {
        LoanId = loan.Id,
        Date = date,
        Mode = mode,
        PrincipalAmount = principal
    };

    [Fact]
    public void Replay_OlayYokken_NormalTaksitPlaniniUretir()
    {
        var loan = CreateReferenceLoan();

        var replay = _builder.Replay(loan, []);

        Assert.Equal(12, replay.Payments.Count);
        Assert.All(replay.Payments, payment => Assert.Equal(LoanPaymentKind.Installment, payment.Kind));
        Assert.Equal(120_554.52m, replay.Total);
        Assert.True(replay.Payments[^1].IsFinal);
        Assert.Equal(new DateOnly(2027, 8, 15), replay.LastPaymentDate);
    }

    [Fact]
    public void Replay_TaksitGunundeErkenKapamaYapildiginda_SonrakiTumTaksitleriSilerVeKrediyiKapatir()
    {
        var loan = CreateReferenceLoan();
        var closure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        var replay = _builder.Replay(loan, [closure]);

        Assert.Equal(7, replay.Payments.Count);
        Assert.Equal(6, replay.Payments.Count(p => p.Kind == LoanPaymentKind.Installment));

        var payoff = replay.Payments[^1];
        Assert.Equal(LoanPaymentKind.EarlyClosure, payoff.Kind);
        Assert.Equal(closure.Id, payoff.SourceId);
        Assert.True(payoff.IsFinal);
        Assert.Equal(54_422.24m, payoff.Amount);

        var finalState = replay.StateAfterEvent[closure.Id];
        Assert.False(finalState.IsActive);
        Assert.Equal(0, finalState.RemainingInstallmentCount);
        Assert.Equal(0m, finalState.RemainingDebt);
    }

    [Fact]
    public void Replay_TaksitlerArasindaErkenKapamaYapildiginda_GecenGunlerinKistFaiziniTahsilEder()
    {
        var loan = CreateReferenceLoan();
        var closureDate = new DateOnly(2027, 2, 25); // 6. taksitten 10 gün sonra
        var closure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, closureDate);

        var replay = _builder.Replay(loan, [closure]);

        var payoff = replay.Payments[^1];
        Assert.Equal(LoanPaymentKind.EarlyClosure, payoff.Kind);
        Assert.Equal(54_966.46m, payoff.Amount);
    }

    [Fact]
    public void Replay_VadeKisaltmaYapildiginda_TaksitiKorurVeVadeyiKucultur()
    {
        var loan = CreateReferenceLoan();
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 20_000m);

        var replay = _builder.Replay(loan, [prepayment]);

        var afterPrepayment = replay.Payments
            .SkipWhile(p => p.Kind != LoanPaymentKind.PartialPrepayment)
            .Skip(1)
            .ToArray();

        var prepayItem = replay.Payments.Single(p => p.Kind == LoanPaymentKind.PartialPrepayment);
        Assert.Equal(20_000m, prepayItem.Amount);
        Assert.Equal(4, afterPrepayment.Length);
        Assert.All(afterPrepayment[..^1], p => Assert.Equal(10_046.21m, p.Amount));
        Assert.Equal(6_759.15m, afterPrepayment[^1].Amount);
        Assert.Equal(new DateOnly(2027, 6, 15), afterPrepayment[^1].Date);

        var state = replay.StateAfterEvent[prepayment.Id];
        Assert.Equal(4, state.RemainingInstallmentCount);
        Assert.Equal(6_759.15m, state.FinalPaymentAmount);
        Assert.Equal(34_422.24m, state.RemainingDebt);
        Assert.Equal(new DateOnly(2027, 3, 15), state.NextPaymentDate);
    }

    [Fact]
    public void Replay_TaksitAzaltmaYapildiginda_VadeyiKorurVeAylikTaksitiDusurur()
    {
        var loan = CreateReferenceLoan();
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceInstallment, SixthInstallmentDate, 20_000m);

        var replay = _builder.Replay(loan, [prepayment]);

        var afterPrepayment = replay.Payments
            .SkipWhile(p => p.Kind != LoanPaymentKind.PartialPrepayment)
            .Skip(1)
            .ToArray();

        Assert.Equal(6, afterPrepayment.Length);
        Assert.All(afterPrepayment, p => Assert.Equal(6_354.26m, p.Amount));
        Assert.Equal(6_354.26m, replay.StateAfterEvent[prepayment.Id].MonthlyPayment);
    }

    [Fact]
    public void Replay_VadeKisaltmaSonrasiOlusanDurumdan_AyniFaizOraniGeriCozulebilir()
    {
        var loan = CreateReferenceLoan();
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 20_000m);

        var state = _builder.Replay(loan, [prepayment]).StateAfterEvent[prepayment.Id];
        var amortization = _amortizationCalculator.Analyze(state).Amortization;

        Assert.NotNull(amortization);
        Assert.Equal(0.03m, amortization.MonthlyRate, 4);
    }

    [Fact]
    public void Replay_HerErkenOdemeTuru_ToplamOdenenTutariDusurur()
    {
        var loan = CreateReferenceLoan();
        var baselineTotal = _builder.Replay(loan, []).Total;

        foreach (var mode in Enum.GetValues<LoanPrepaymentMode>())
        {
            var prepayment = CreatePrepayment(loan, mode, SixthInstallmentDate, 20_000m);
            var replay = _builder.Replay(loan, [prepayment]);

            Assert.True(replay.Total < baselineTotal, $"{mode} modu toplam ödemeyi düşürmedi.");
        }
    }

    [Fact]
    public void Replay_FaiziTuretilemeyenKredide_TaksitleriAynenKorurVeBayragiIsaretler()
    {
        var loan = CreateReferenceLoan() with { RemainingDebt = null };
        var closure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        var replay = _builder.Replay(loan, [closure]);

        Assert.True(replay.IgnoredBecauseUnquotable);
        Assert.Equal(12, replay.Payments.Count);
        Assert.Empty(replay.StateAfterEvent);
    }

    [Fact]
    public void Replay_KrediPasifVeyaTaksitKalmamissa_BosSonucDondurur()
    {
        var inactiveLoan = CreateReferenceLoan() with { IsActive = false };
        var emptyCountLoan = CreateReferenceLoan() with { RemainingInstallmentCount = 0 };

        var replay1 = _builder.Replay(inactiveLoan, []);
        var replay2 = _builder.Replay(emptyCountLoan, []);

        Assert.Empty(replay1.Payments);
        Assert.Null(replay1.LastPaymentDate);
        Assert.Equal(0m, replay1.Total);
        Assert.Empty(replay2.Payments);
    }

    [Fact]
    public void Replay_OncekiVadedenOncekiOlaylar_DikkateAlinmaz()
    {
        var loan = CreateReferenceLoan();
        // PreviousDueDate: 15.08.2026. 10.08.2026'daki olay yoksayılmalı.
        var pastEvent = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, new DateOnly(2026, 8, 10), 10_000m);

        var replay = _builder.Replay(loan, [pastEvent]);

        Assert.Equal(12, replay.Payments.Count);
        Assert.False(replay.StateAfterEvent.ContainsKey(pastEvent.Id));
    }

    [Fact]
    public void Replay_SabitKonutKredisinde_YasalErkenOdemeKomisyonunuHesabaDahilEder()
    {
        // Sabit konut kredisi: Kalan vade 6 taksit (<= 36 ay), komisyon %1
        var housingLoan = CreateReferenceLoan(LoanKind.HousingFixed);
        var closure = CreatePrepayment(housingLoan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        var replay = _builder.Replay(housingLoan, [closure]);

        var payoff = replay.Payments[^1];
        // Kalan anapara 54.422,24 TL. %1 komisyon = 544,22 TL. Toplam = 54.966,46 TL.
        Assert.Equal(54_966.46m, payoff.Amount);
    }
}
