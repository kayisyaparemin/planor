using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// Kredi erken ödeme doğrulayıcısının (LoanPrepaymentValidator) iş kurallarını denetleyen testler.
/// </summary>
public sealed class LoanPrepaymentValidatorTests
{
    private readonly LoanScheduleCalculator _scheduleCalculator = new();
    private readonly LoanAmortizationCalculator _amortizationCalculator;
    private readonly LoanPaymentScheduleBuilder _builder;
    private readonly LoanPrepaymentValidator _validator;

    private static readonly DateOnly SixthInstallmentDate = new(2027, 2, 15);

    public LoanPrepaymentValidatorTests()
    {
        _amortizationCalculator = new LoanAmortizationCalculator(_scheduleCalculator);
        _builder = new LoanPaymentScheduleBuilder(_scheduleCalculator, _amortizationCalculator);
        _validator = new LoanPrepaymentValidator(_amortizationCalculator, _builder);
    }

    private static Loan CreateReferenceLoan() => new()
    {
        Name = "Referans Kredi",
        Bank = "Test Bankası",
        MonthlyPayment = 10_046.21m,
        PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 9, 15),
        RemainingInstallmentCount = 12,
        RemainingDebt = 100_000m
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
    public void Validate_KrediNullVeyaPasifVeyaTaksitsizIse_HataFirlatir()
    {
        var validPrepayment = CreatePrepayment(CreateReferenceLoan(), LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(null, [], validPrepayment));

        var inactiveLoan = CreateReferenceLoan() with { IsActive = false };
        Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(inactiveLoan, [], validPrepayment));

        var zeroInstallmentLoan = CreateReferenceLoan() with { RemainingInstallmentCount = 0 };
        Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(zeroInstallmentLoan, [], validPrepayment));
    }

    [Fact]
    public void Validate_FaiziTuretilemeyenKredide_KalanAnaparaGerektiginiBildirir()
    {
        var loan = CreateReferenceLoan() with { RemainingDebt = null };
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        var error = Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], prepayment));

        Assert.Contains("kalan anaparası", error.Message);
    }

    [Fact]
    public void Validate_SonOdenenTaksittenOncekiTarihVerildiginde_HataFirlatir()
    {
        var loan = CreateReferenceLoan();
        // PreviousDueDate: 15.08.2026
        var earlyDate = new DateOnly(2026, 8, 10);
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, earlyDate);

        var error = Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], prepayment));

        Assert.Contains("son ödenen taksitinden", error.Message);
    }

    [Fact]
    public void Validate_AyniKrediDahaOnceKapatilmissa_TekrarKapatilamaz()
    {
        var loan = CreateReferenceLoan();
        var firstClosure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);
        var secondClosure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15));

        var error = Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [firstClosure], secondClosure));

        Assert.Contains("zaten kapatılıyor", error.Message);
    }

    [Fact]
    public void Validate_SonTaksittenSonrakiTarihVerildiginde_HataFirlatir()
    {
        var loan = CreateReferenceLoan();
        // Son taksit: 15.08.2027
        var afterLastDate = new DateOnly(2027, 8, 20);
        var prepayment = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, afterLastDate);

        var error = Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], prepayment));

        Assert.Contains("kapatılacak taksiti kalmıyor", error.Message);
    }

    [Fact]
    public void Validate_AraOdemeTutariSifirVeyaNegatifIse_HataFirlatir()
    {
        var loan = CreateReferenceLoan();
        var zeroPayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 0m);
        var negativePayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, -1_000m);

        Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], zeroPayment));
        Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], negativePayment));
    }

    [Fact]
    public void Validate_AraOdemeTutariKalanAnaparayaEsitVeyaBuyukIse_ErkenKapamaOnererekHataFirlatir()
    {
        var loan = CreateReferenceLoan();
        // 6. taksit günündeki kalan anapara: 54.422,24 TL. 60.000 TL ara ödeme deneniyor.
        var excessivePrepayment = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 60_000m);

        var error = Assert.Throws<InvalidOperationException>(() =>
            _validator.Validate(loan, [], excessivePrepayment));

        Assert.Contains("erken kapamayı seç", error.Message);
    }

    [Fact]
    public void Validate_GecerliErkenKapamaVeAraOdeme_BasariylaGecer()
    {
        var loan = CreateReferenceLoan();
        var validClosure = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, SixthInstallmentDate);
        var validPartial = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 20_000m);

        _validator.Validate(loan, [], validClosure);
        _validator.Validate(loan, [], validPartial);
    }

    [Fact]
    public void Check_GecerliErkenOdemede_NullDoner()
    {
        var loan = CreateReferenceLoan();

        Assert.Null(_validator.Check(loan, [], CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 20_000m)));
    }

    [Fact]
    public void Check_GecersizErkenOdemede_FirlatmadanValidateIleAyniMesajiDoner()
    {
        var loan = CreateReferenceLoan();
        var afterLast = CreatePrepayment(loan, LoanPrepaymentMode.FullClosure, new DateOnly(2027, 8, 20));
        var excessive = CreatePrepayment(loan, LoanPrepaymentMode.ReduceTerm, SixthInstallmentDate, 60_000m);

        foreach (var prepayment in new[] { afterLast, excessive })
        {
            var thrown = Assert.Throws<InvalidOperationException>(() => _validator.Validate(loan, [], prepayment));
            Assert.Equal(thrown.Message, _validator.Check(loan, [], prepayment));
        }
    }

    [Fact]
    public void Check_KrediYoksa_AktifKrediIstenir()
    {
        var prepayment = CreatePrepayment(CreateReferenceLoan(), LoanPrepaymentMode.FullClosure, SixthInstallmentDate);

        Assert.Equal("Erken ödeme için aktif bir kredi seçmelisin.", _validator.Check(null, [], prepayment));
    }
}
