using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Kredi itfa hesaplayıcısı ve erken kapama teklif motorunun matematiksel ve yasal doğruluğunu test eder.
/// </summary>
public sealed class LoanAmortizationCalculatorTests
{
    private readonly LoanAmortizationCalculator _calculator = new(new LoanScheduleCalculator());

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

    [Fact]
    public void Analyze_StandartKrediVerildiginde_AylikOranVeItfayiTuretir()
    {
        var loan = CreateReferenceLoan();

        var analysis = _calculator.Analyze(loan);

        Assert.Equal(LoanAnalysisIssue.None, analysis.Issue);
        Assert.NotNull(analysis.Amortization);
        Assert.True(analysis.IsSuccess);
        Assert.Equal(0.03m, analysis.Amortization.MonthlyRate, 4);
        Assert.Equal(LoanRateSource.RemainingPrincipal, analysis.Amortization.Source);
        Assert.Equal(new DateOnly(2026, 8, 15), analysis.Amortization.PreviousDueDate);
        Assert.Equal(100_000m, analysis.Amortization.Principal);
        Assert.Equal(20_554.52m, analysis.Amortization.RemainingInterest);
    }

    [Theory]
    [InlineData(190_188, 14_501.23, 22, 0.0504)]
    [InlineData(55_777, 7_374.59, 9, 0.0363)]
    public void SolveMonthlyRate_GercekHayatKredilerinde_MakulVeBeklenenOranlariCozumler(
        decimal principal,
        decimal payment,
        int count,
        decimal expectedRate)
    {
        var solvedRate = LoanAmortizationCalculator.SolveMonthlyRate(principal, payment, count);

        Assert.Equal(expectedRate, solvedRate, 4);
    }

    [Fact]
    public void PrincipalAfter_TaksitlerOdendikce_AnnüiteTablosunaGoreKalanAnaparayiHesaplar()
    {
        var amortization = _calculator.Analyze(CreateReferenceLoan()).Amortization!;

        Assert.Equal(100_000m, LoanAmortizationCalculator.PrincipalAfter(amortization, 0));
        Assert.Equal(78_220.88m, LoanAmortizationCalculator.PrincipalAfter(amortization, 3), 0);
        Assert.Equal(54_422.24m, LoanAmortizationCalculator.PrincipalAfter(amortization, 6), 0);
        Assert.Equal(0m, LoanAmortizationCalculator.PrincipalAfter(amortization, 12));
        Assert.Equal(0m, LoanAmortizationCalculator.PrincipalAfter(amortization, 15));
    }

    [Fact]
    public void PrincipalAfterPayment_TaksitOdendiginde_YalnizAnaparaPayiniDuser()
    {
        var amortization = new LoanAmortization
        {
            MonthlyRate = 0.03m,
            Principal = 100_000m,
            RemainingInstallments = 12,
            MonthlyPayment = 10_046.21m,
            FinalPayment = 10_046.21m,
            PreviousDueDate = new DateOnly(2026, 8, 15),
            Source = LoanRateSource.RemainingPrincipal
        };

        // Faiz payı = 100.000 * %3 = 3.000 TL; Anapara payı = 10.046,21 - 3.000 = 7.046,21 TL
        // Yeni anapara = 100.000 - 7.046,21 = 92.953,79 TL
        var newPrincipal = LoanAmortizationCalculator.PrincipalAfterPayment(amortization, 10_046.21m);

        Assert.Equal(92_953.79m, newPrincipal);
    }

    [Fact]
    public void PrincipalAfterPayment_OdenenTutarFaizdenKucukse_AnaparaBuyur()
    {
        var amortization = new LoanAmortization
        {
            MonthlyRate = 0.03m,
            Principal = 100_000m,
            RemainingInstallments = 12,
            MonthlyPayment = 10_046.21m,
            FinalPayment = 10_046.21m,
            PreviousDueDate = new DateOnly(2026, 8, 15),
            Source = LoanRateSource.RemainingPrincipal
        };

        // Faiz 3.000 TL iken sadece 1.000 TL ödenirse anapara 2.000 TL büyür
        var newPrincipal = LoanAmortizationCalculator.PrincipalAfterPayment(amortization, 1_000m);

        Assert.Equal(102_000m, newPrincipal);
    }

    [Fact]
    public void Analyze_KalanToplamTaksitBorcuAnaparaGirilmisIse_PrincipalNotBelowInstallmentsHatasiVerir()
    {
        var loan = CreateReferenceLoan() with
        {
            RemainingDebt = 10_046.21m * 12
        };

        var analysis = _calculator.Analyze(loan);

        Assert.False(analysis.IsSuccess);
        Assert.Null(analysis.Amortization);
        Assert.Equal(LoanAnalysisIssue.PrincipalNotBelowInstallments, analysis.Issue);
    }

    [Fact]
    public void Analyze_KalanBorcBelliDegilse_MissingPrincipalHatasiVerir()
    {
        var loan = CreateReferenceLoan() with
        {
            RemainingDebt = null
        };

        var analysis = _calculator.Analyze(loan);

        Assert.False(analysis.IsSuccess);
        Assert.Null(analysis.Amortization);
        Assert.Equal(LoanAnalysisIssue.MissingPrincipal, analysis.Issue);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(5, false)]
    public void Analyze_KrediBitmisVeyaPasifse_FinishedHatasiVerir(int remainingInstallments, bool isActive)
    {
        var loan = CreateReferenceLoan() with
        {
            RemainingInstallmentCount = remainingInstallments,
            IsActive = isActive
        };

        var analysis = _calculator.Analyze(loan);

        Assert.False(analysis.IsSuccess);
        Assert.Null(analysis.Amortization);
        Assert.Equal(LoanAnalysisIssue.Finished, analysis.Issue);
    }

    [Fact]
    public void Analyze_BayatAnaparaVeyaAsiriYuksekFaizCikarsa_ImplausibleRateHatasiVerir()
    {
        // 10 taksit kalmışken anaparanın 16.173 TL girilmesi aylık ~%89 gibi mantıksız bir faiz türetir
        var loan = CreateReferenceLoan() with
        {
            MonthlyPayment = 14_501.23m,
            RemainingInstallmentCount = 10,
            RemainingDebt = 16_173m
        };

        var analysis = _calculator.Analyze(loan);

        Assert.False(analysis.IsSuccess);
        Assert.Null(analysis.Amortization);
        Assert.Equal(LoanAnalysisIssue.ImplausibleRate, analysis.Issue);
    }

    [Fact]
    public void PayoffOn_TaksitGunundeErkenKapatildiginda_GunIsleyenFaizSifirdirVeAnaparaAlinir()
    {
        var loan = CreateReferenceLoan();
        var amortization = _calculator.Analyze(loan).Amortization!;

        // 6 taksit ödendikten sonraki vade günü (2027-02-15)
        var quote = _calculator.PayoffOn(loan, amortization, new DateOnly(2027, 2, 15));

        Assert.Equal(6, quote.InstallmentsPaidBefore);
        Assert.Equal(6, quote.InstallmentsRemoved);
        Assert.Equal(0m, quote.AccruedInterest);
        Assert.Equal(0m, quote.Fee);
        Assert.Equal(54_422.24m, quote.Principal, 0);
        Assert.Equal(54_422.24m, quote.Amount, 0);
        Assert.Equal(60_277.26m, quote.RemovedInstallmentTotal);
        Assert.Equal(5_855.02m, quote.InterestSaving, 0);
        Assert.True(quote.HasAnythingToClose);
    }

    [Fact]
    public void PayoffOn_TaksitlerArasindaKapatildiginda_GunIsleyenFaizEklenir()
    {
        var loan = CreateReferenceLoan();
        var amortization = _calculator.Analyze(loan).Amortization!;

        // Son ödenen taksit 2027-02-15 iken 10 gün sonra (2027-02-25) erken kapama
        // 54.422,24 * 0.03 * 10 / 30 = 544.22 TL işleyen faiz
        var quote = _calculator.PayoffOn(loan, amortization, new DateOnly(2027, 2, 25));

        Assert.Equal(544.22m, quote.AccruedInterest, 0);
        Assert.Equal(6, quote.InstallmentsRemoved);
        Assert.Equal(54_422.24m + 544.22m, quote.Amount, 0);
    }

    [Fact]
    public void PayoffOn_TumTaksitlerBittiktenSonrakiTarihte_KapatilacakBorcYoktur()
    {
        var loan = CreateReferenceLoan();
        var amortization = _calculator.Analyze(loan).Amortization!;

        var quote = _calculator.PayoffOn(loan, amortization, new DateOnly(2027, 9, 1));

        Assert.False(quote.HasAnythingToClose);
        Assert.Equal(0m, quote.Amount);
        Assert.Equal(0, quote.InstallmentsRemoved);
    }

    [Theory]
    [InlineData(LoanKind.Consumer, 60, 0)]
    [InlineData(LoanKind.HousingVariable, 60, 0)]
    [InlineData(LoanKind.HousingFixed, 36, 0.01)]
    [InlineData(LoanKind.HousingFixed, 37, 0.02)]
    public void PrepaymentFeeRate_6502SayiliKanunaGore_DogruYasalOranlariDover(
        LoanKind kind,
        int remaining,
        decimal expectedRate)
    {
        var feeRate = LoanAmortizationCalculator.PrepaymentFeeRate(kind, remaining);

        Assert.Equal(expectedRate, feeRate);
    }

    [Fact]
    public void PayoffOn_SabitFaizliKonutKredisiIcin_YasalErkenOdemeUcretiniDahilEder()
    {
        var loan = CreateReferenceLoan(LoanKind.HousingFixed);
        var amortization = _calculator.Analyze(loan).Amortization!;

        // 6 taksit kalmışken (<= 36 ay) %1 yasal ceza: 54.422,24 * 0.01 = 544.22 TL
        var quote = _calculator.PayoffOn(loan, amortization, new DateOnly(2027, 2, 15));

        Assert.Equal(544.22m, quote.Fee, 0);
        Assert.Equal(quote.Principal + quote.Fee, quote.Amount);
    }

    [Fact]
    public void Analyze_BankaErkenKapamaTeklifiGirilmisIse_FaizVeAnaparayiBirlikteKalibreEder()
    {
        // 25.09.2026 tarihinde alınan 93.883,33 TL kapama teklifi
        // 15.09 taksiti ödenmiş, 10 gün faiz işlemiş durumu yansıtır
        var loan = CreateReferenceLoan() with
        {
            RemainingDebt = null,
            EarlyClosureAmount = 93_883.33m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 9, 25)
        };

        var analysis = _calculator.Analyze(loan);

        Assert.True(analysis.IsSuccess);
        Assert.NotNull(analysis.Amortization);
        Assert.Equal(LoanRateSource.BankQuote, analysis.Amortization.Source);
        Assert.Equal(0.03m, analysis.Amortization.MonthlyRate, 4);
        Assert.Equal(100_000m, analysis.Amortization.Principal);
    }

    [Fact]
    public void Analyze_BankaTeklifiSonOdenenTaksittenEskiIse_GozardiEdilirVeGirilenAnaparaKullanilir()
    {
        var loan = CreateReferenceLoan() with
        {
            EarlyClosureAmount = 93_883.33m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 8, 1) // PreviousDueDate (15 Ağustos)'tan eski
        };

        var analysis = _calculator.Analyze(loan);

        Assert.True(analysis.IsSuccess);
        Assert.NotNull(analysis.Amortization);
        Assert.Equal(LoanRateSource.RemainingPrincipal, analysis.Amortization.Source);
    }

    [Fact]
    public void PreviousDueDate_GelecekVadeyeGore_TakvimKuraliylaBirOncekiVadeyiUretir()
    {
        var loan = CreateReferenceLoan();

        var previousDue = LoanAmortizationCalculator.PreviousDueDate(loan);

        Assert.Equal(new DateOnly(2026, 8, 15), previousDue);
    }

    [Fact]
    public void SolveMonthlyRate_SonTaksitFarkliIse_OraniBasariylaCozumler()
    {
        // 100.000 TL anapara, 11 taksit 10.000 TL, son taksit 5.000 TL
        var rate = LoanAmortizationCalculator.SolveMonthlyRate(100_000m, 10_000m, 12, 5_000m);

        Assert.True(rate > 0m);
        Assert.True(rate < LoanAmortizationCalculator.MaxPlausibleMonthlyRate);
    }

    [Fact]
    public void Metotlar_NullParametreVerildiginde_ArgumentNullExceptionFirlatir()
    {
        var loan = CreateReferenceLoan();
        var amortization = _calculator.Analyze(loan).Amortization!;

        Assert.Throws<ArgumentNullException>(() => _calculator.Analyze(null!));
        Assert.Throws<ArgumentNullException>(() => LoanAmortizationCalculator.PrincipalAfter(null!, 1));
        Assert.Throws<ArgumentNullException>(() => LoanAmortizationCalculator.PrincipalAfterPayment(null!, 1_000m));
        Assert.Throws<ArgumentNullException>(() => _calculator.PayoffOn(null!, amortization, new DateOnly(2027, 2, 15)));
        Assert.Throws<ArgumentNullException>(() => _calculator.PayoffOn(loan, null!, new DateOnly(2027, 2, 15)));
        Assert.Throws<ArgumentNullException>(() => LoanAmortizationCalculator.PreviousDueDate(null!));
    }

    [Fact]
    public void Analyze_BankaTeklifiKalanTaksitToplamindanBuyukse_BankaTeklifiGecersizSayilir()
    {
        // Kalan taksitlerin toplamı 10.046,21 * 12 = 120.554,52 TL iken banka teklifi 130.000 TL girilmiş
        var loan = CreateReferenceLoan() with
        {
            EarlyClosureAmount = 130_000m,
            EarlyClosureAmountAsOf = new DateOnly(2026, 9, 25)
        };

        var analysis = _calculator.Analyze(loan);

        // BankQuote geçersiz sayılır; kullanıcı anaparası varsa ona düşer
        Assert.True(analysis.IsSuccess);
        Assert.NotNull(analysis.Amortization);
        Assert.Equal(LoanRateSource.RemainingPrincipal, analysis.Amortization.Source);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Analyze_BankaTeklifTutariSifirVeyaNegatifse_GozardiEdilir(decimal invalidAmount)
    {
        var loan = CreateReferenceLoan() with
        {
            EarlyClosureAmount = invalidAmount,
            EarlyClosureAmountAsOf = new DateOnly(2026, 9, 25)
        };

        var analysis = _calculator.Analyze(loan);

        Assert.True(analysis.IsSuccess);
        Assert.NotNull(analysis.Amortization);
        Assert.Equal(LoanRateSource.RemainingPrincipal, analysis.Amortization.Source);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Valuation_SifirVeyaNegatifVade_SifirDoner(int count)
    {
        var value = LoanAmortizationCalculator.Valuation(0.03d, count, 10_000d, 10_000d);

        Assert.Equal(0d, value);
    }
}
