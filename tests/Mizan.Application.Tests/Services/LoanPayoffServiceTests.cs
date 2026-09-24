using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// <see cref="LoanPayoffService"/> için test kalkanı: bugünkü kapatma bedeli, planlı erken
/// ödemelerin tutarları ve kaydetme kapısında bankanın kapatma tutarının otoritesi (I25).
/// Referans kredi: 12 × 10.000 TL kalan taksit, 100.000 TL anapara (aylık ~%2,9),
/// son ödenen taksit 15.09.2026, sonraki taksit 15.10.2026.
/// </summary>
public sealed class LoanPayoffServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 25);
    private static readonly DateOnly PreviousDueDate = new(2026, 9, 15);

    private static readonly LoanScheduleCalculator ScheduleCalculator = new();
    private static readonly LoanAmortizationCalculator AmortizationCalculator = new(ScheduleCalculator);
    private static readonly LoanPaymentScheduleBuilder ScheduleBuilder = new(ScheduleCalculator, AmortizationCalculator);

    [Fact]
    public void PrepareForSave_BankaTutariVarken_AnaparayiTutardanCozupElleGirileninYerineYazar()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { RemainingDebt = 95_000m, EarlyClosureAmount = 101_000m };

        var saved = service.PrepareForSave(loan);

        var amortization = AmortizationCalculator.Analyze(saved).Amortization!;
        Assert.Equal(LoanRateSource.BankQuote, amortization.Source);
        Assert.Equal(amortization.Principal, saved.RemainingDebt);
        Assert.NotEqual(95_000m, saved.RemainingDebt);
        var todayQuote = AmortizationCalculator.PayoffOn(saved, amortization, Today);
        Assert.InRange(todayQuote.Amount, 100_999.99m, 101_000.01m);
    }

    [Fact]
    public void PrepareForSave_TarihsizBankaTutari_BugununTarihiyleDamgalanir()
    {
        var service = CreateService(Today);

        var saved = service.PrepareForSave(ReferenceLoan() with { EarlyClosureAmount = 101_000m });

        Assert.Equal(Today, saved.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void PrepareForSave_KayitliTarihliTutar_KendiTarihiniKorur()
    {
        var service = CreateService(Today);
        var quoteDate = new DateOnly(2026, 9, 20);

        var saved = service.PrepareForSave(ReferenceLoan() with
        {
            RemainingDebt = null,
            EarlyClosureAmount = 100_500m,
            EarlyClosureAmountAsOf = quoteDate
        });

        Assert.Equal(quoteDate, saved.EarlyClosureAmountAsOf);
        Assert.Equal(AmortizationCalculator.Analyze(saved).Amortization!.Principal, saved.RemainingDebt);
    }

    /// <summary>Son taksit günü alınan tutarda işleyen faiz sıfırdır; anapara tutarın kendisidir.</summary>
    [Fact]
    public void PrepareForSave_TutarTamSonOdenenTaksitGunuAlinmissa_AnaparaTutaraEsitOlur()
    {
        var service = CreateService(Today);

        var saved = service.PrepareForSave(ReferenceLoan() with
        {
            RemainingDebt = null,
            EarlyClosureAmount = 100_000m,
            EarlyClosureAmountAsOf = PreviousDueDate
        });

        Assert.Equal(PreviousDueDate, saved.EarlyClosureAmountAsOf);
        Assert.NotNull(saved.RemainingDebt);
        Assert.InRange(saved.RemainingDebt.Value, 99_999.99m, 100_000.01m);
    }

    [Fact]
    public void PrepareForSave_GelecekTarihliTutar_Reddedilir()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { EarlyClosureAmount = 101_000m, EarlyClosureAmountAsOf = Today.AddDays(1) };

        var error = Assert.Throws<InvalidOperationException>(() => service.PrepareForSave(loan));

        Assert.Contains("bugünden sonra olamaz", error.Message);
    }

    [Fact]
    public void PrepareForSave_SonOdenenTaksittenOnceAlinmisTutar_BugunkuTutariIster()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { EarlyClosureAmount = 101_000m, EarlyClosureAmountAsOf = PreviousDueDate.AddDays(-1) };

        var error = Assert.Throws<InvalidOperationException>(() => service.PrepareForSave(loan));

        Assert.Contains("son ödenen taksitten (15.09.2026) önce", error.Message);
        Assert.Contains("bugünkü tutarı", error.Message);
    }

    [Fact]
    public void PrepareForSave_SonrakiOdemeBirAydanFazlaIleride_SonrakiOdemeTarihiniGosterir()
    {
        var service = CreateService(PreviousDueDate.AddDays(-5));

        var error = Assert.Throws<InvalidOperationException>(() =>
            service.PrepareForSave(ReferenceLoan() with { EarlyClosureAmount = 101_000m }));

        Assert.Contains("Sonraki ödeme tarihi (15.10.2026)", error.Message);
    }

    [Theory]
    [InlineData(120_000)]
    [InlineData(50_000)]
    public void PrepareForSave_TaksitlerleUyusmayanTutar_Reddedilir(int closureAmount)
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { EarlyClosureAmount = closureAmount };

        var error = Assert.Throws<InvalidOperationException>(() => service.PrepareForSave(loan));

        Assert.Contains("uyuşmuyor", error.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-5)]
    public void PrepareForSave_GecerliTutarYoksa_TutariVeBasibosTarihiTemizler(int? closureAmount)
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { EarlyClosureAmount = closureAmount, EarlyClosureAmountAsOf = Today };

        var saved = service.PrepareForSave(loan);

        Assert.Null(saved.EarlyClosureAmount);
        Assert.Null(saved.EarlyClosureAmountAsOf);
        Assert.Equal(100_000m, saved.RemainingDebt);
    }

    [Fact]
    public void PrepareForSave_ToplamBorcAnaparaDiyeGirilmis_KapatmaTutariniIster()
    {
        var service = CreateService(Today);

        var error = Assert.Throws<InvalidOperationException>(() =>
            service.PrepareForSave(ReferenceLoan() with { RemainingDebt = 120_000m }));

        Assert.Contains("kalan taksitlerin toplamından küçük olmalı", error.Message);
        Assert.Contains("kapatma tutarını gir", error.Message);
    }

    [Fact]
    public void PrepareForSave_AnaparaMantiksizFaizVeriyorsa_Reddedilir()
    {
        var service = CreateService(Today);

        var error = Assert.Throws<InvalidOperationException>(() =>
            service.PrepareForSave(ReferenceLoan() with { RemainingDebt = 50_000m }));

        Assert.Contains("gerçekçi olmayan", error.Message);
    }

    [Fact]
    public void PrepareForSave_AnaparaVeTutarGirilmemis_KrediyiOlduguGibiDondurur()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { RemainingDebt = null };

        var saved = service.PrepareForSave(loan);

        Assert.Equal(loan, saved);
    }

    [Fact]
    public void Describe_FaiziCozulenKredi_BugunKapatmaninDokumunuVerir()
    {
        var service = CreateService(Today);

        var overview = service.Describe(ReferenceLoan());

        Assert.Equal(LoanAnalysisIssue.None, overview.Analysis.Issue);
        var quote = Assert.IsType<LoanPayoffQuote>(overview.Today);
        Assert.Equal(Today, quote.Date);
        Assert.Equal(100_000m, quote.Principal);
        Assert.True(quote.AccruedInterest > 0m, "Son taksitten bu yana 10 günlük faiz işlemiş olmalı.");
        Assert.Equal(quote.Principal + quote.AccruedInterest, quote.Amount);
    }

    [Fact]
    public void Describe_AnaparasizKredi_TeklifUretmezEngeliBildirir()
    {
        var service = CreateService(Today);

        var overview = service.Describe(ReferenceLoan() with { RemainingDebt = null });

        Assert.Null(overview.Today);
        Assert.Equal(LoanAnalysisIssue.MissingPrincipal, overview.Analysis.Issue);
    }

    [Fact]
    public void Describe_KrediListesi_HerKrediIcinAyniSiradaBirGorunumUretir()
    {
        var service = CreateService(Today);
        var first = ReferenceLoan() with { Name = "Birinci" };
        var second = ReferenceLoan() with { Name = "İkinci", RemainingDebt = null };

        var overviews = service.Describe([first, second]);

        Assert.Equal([first, second], overviews.Select(x => x.Loan));
    }

    [Fact]
    public void DescribePrepayments_PlanliOdemeler_OGunkuTutarlariylaTarihSirasinaGoreListelenir()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan();
        var closure = new LoanPrepayment { LoanId = loan.Id, Date = new DateOnly(2027, 3, 15), Mode = LoanPrepaymentMode.FullClosure };
        var partial = new LoanPrepayment
        {
            LoanId = loan.Id, Date = new DateOnly(2026, 12, 20), Mode = LoanPrepaymentMode.ReduceTerm, PrincipalAmount = 20_000m
        };
        var plan = new FinancialPlan { Loans = [loan], LoanPrepayments = [closure, partial] };

        var planned = service.DescribePrepayments(plan);

        Assert.Equal([partial, closure], planned.Select(x => x.Prepayment));
        var replay = ScheduleBuilder.Replay(loan, plan.LoanPrepayments);
        Assert.All(planned, x => Assert.Equal(replay.Payments.Single(p => p.SourceId == x.Prepayment.Id).Amount, x.Amount));
        Assert.All(planned, x => Assert.False(x.IsUnquotable));
    }

    [Fact]
    public void DescribePrepayments_FaiziCozulemeyenKredi_TutarsizVeHesaplanamazIsaretlenir()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan() with { RemainingDebt = null };
        var closure = new LoanPrepayment { LoanId = loan.Id, Date = new DateOnly(2027, 3, 15), Mode = LoanPrepaymentMode.FullClosure };

        var planned = Assert.Single(service.DescribePrepayments(new FinancialPlan { Loans = [loan], LoanPrepayments = [closure] }));

        Assert.Null(planned.Amount);
        Assert.True(planned.IsUnquotable);
    }

    [Fact]
    public void DescribePrepayments_KrediOTarihtenOnceBitiyorsa_TutarNullAmaHesaplanabilirKalir()
    {
        var service = CreateService(Today);
        var loan = ReferenceLoan();
        var afterEnd = new LoanPrepayment { LoanId = loan.Id, Date = new DateOnly(2027, 12, 1), Mode = LoanPrepaymentMode.FullClosure };

        var planned = Assert.Single(service.DescribePrepayments(new FinancialPlan { Loans = [loan], LoanPrepayments = [afterEnd] }));

        Assert.Null(planned.Amount);
        Assert.False(planned.IsUnquotable);
    }

    [Fact]
    public void Yapici_EksikBagimlilik_ArgumentNullExceptionFirlatir()
    {
        var clock = new FixedClock(Today);

        Assert.Throws<ArgumentNullException>(() => new LoanPayoffService(null!, AmortizationCalculator, ScheduleBuilder));
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffService(clock, null!, ScheduleBuilder));
        Assert.Throws<ArgumentNullException>(() => new LoanPayoffService(clock, AmortizationCalculator, null!));
    }

    private static LoanPayoffService CreateService(DateOnly today) =>
        new(new FixedClock(today), AmortizationCalculator, ScheduleBuilder);

    private static Loan ReferenceLoan() => new()
    {
        Name = "İhtiyaç",
        Bank = "Test Bankası",
        MonthlyPayment = 10_000m,
        PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 10, 15),
        RemainingInstallmentCount = 12,
        RemainingDebt = 100_000m
    };

    private sealed class FixedClock(DateOnly today) : IClock
    {
        public DateOnly Today { get; } = today;
        public DateTimeOffset UtcNow { get; } = new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}
