using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kredi formunun faiz kartı: açılışta hangi tutarın gösterildiği, canlı hesabın üç hâli (tutar gir,
/// uyuşmuyor, çözüldü), kaydedilecek krediye yazılan tutarlar ve tarihleri (EK-V6c, S64-4, S64-9–11).
/// Referans kredi: 24 × 7.500 kalan taksit, sonraki taksit 15.10.2026, son ödenen 15.09.2026.
/// </summary>
public sealed class LoanPayoffViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 27);
    private static readonly DateOnly QuoteDate = new(2026, 9, 20);

    private readonly FakeObligationManagementService _service = new();
    private readonly LoanPayoffViewModel _viewModel;

    public LoanPayoffViewModelTests() => _viewModel = new LoanPayoffViewModel(_service);

    [Fact]
    public void Fill_GuncelKapatmaTutariVarsa_YalnizOnuGosterir()
    {
        _viewModel.Fill(Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = QuoteDate });

        Assert.Equal(string.Empty, _viewModel.PrincipalInput);
        Assert.Equal("152000", _viewModel.ClosureInput);
    }

    [Fact]
    public void Fill_KapatmaTutariSonOdenenTaksittenEskiyse_KalanAnaparayiGosterir()
    {
        _viewModel.Fill(Loan() with
        {
            RemainingDebt = 150000.5m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 1)
        });

        Assert.Equal("150000,5", _viewModel.PrincipalInput);
        Assert.Equal(string.Empty, _viewModel.ClosureInput);
    }

    [Fact]
    public void Fill_YeniKredi_IkiAlanBosTutarGirCumlesi()
    {
        _viewModel.Fill(null);
        _viewModel.Refresh(Loan());

        Assert.Equal(string.Empty, _viewModel.PrincipalInput);
        Assert.Equal(string.Empty, _viewModel.ClosureInput);
        Assert.True(_viewModel.NeedsAmount);
        Assert.False(_viewModel.IsMismatch);
        Assert.False(_viewModel.IsResolved);
        Assert.Empty(_service.PreviewedLoans);
    }

    [Theory]
    [InlineData("abc", "")]
    [InlineData("0", "")]
    [InlineData("-5", "")]
    [InlineData("", "abc")]
    [InlineData("150000", "0")]
    public void Refresh_GecersizTutar_TutarGirCumlesi(string principal, string closure)
    {
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = principal;
        _viewModel.ClosureInput = closure;

        _viewModel.Refresh(Loan());

        Assert.True(_viewModel.NeedsAmount);
        Assert.False(_viewModel.IsMismatch);
        Assert.False(_viewModel.IsResolved);
    }

    [Fact]
    public void Refresh_TaksitBilgisiGecersizse_NeCumleNeSatir()
    {
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "150000";

        _viewModel.Refresh(null);

        Assert.False(_viewModel.NeedsAmount);
        Assert.False(_viewModel.IsMismatch);
        Assert.False(_viewModel.IsResolved);
    }

    [Fact]
    public void Refresh_KayitTutariReddederse_UyusmuyorCumlesi()
    {
        _service.Preview = _ => null;
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "250000";

        _viewModel.Refresh(Loan());

        Assert.True(_viewModel.IsMismatch);
        Assert.False(_viewModel.NeedsAmount);
        Assert.False(_viewModel.IsResolved);
    }

    [Fact]
    public void Refresh_FaizCozulurse_BedelUcretKurtulunanFaizVeAylikFaiziSunar()
    {
        _service.Preview = loan => Resolved(loan, 0.0279m, Quote(fee: 1489m, removedTotal: 180000m));
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "148000";

        _viewModel.Refresh(Loan());

        Assert.True(_viewModel.IsResolved);
        Assert.False(_viewModel.NeedsAmount);
        Assert.False(_viewModel.IsMismatch);
        Assert.Equal(0.0279m, _viewModel.MonthlyRate);
        Assert.True(_viewModel.HasPayoff);
        Assert.Equal(150230m, _viewModel.PayoffAmount);
        Assert.True(_viewModel.HasFee);
        Assert.Equal(1489m, _viewModel.Fee);
        Assert.True(_viewModel.HasInterestSaving);
        Assert.Equal(29770m, _viewModel.InterestSaving);
    }

    [Fact]
    public void Refresh_UcretYoksa_UcretSatiriGizli()
    {
        _service.Preview = loan => Resolved(loan, 0.0279m, Quote(fee: 0m, removedTotal: 180000m));
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "148000";

        _viewModel.Refresh(Loan());

        Assert.True(_viewModel.HasPayoff);
        Assert.False(_viewModel.HasFee);
    }

    [Theory]
    [InlineData(150230)]
    [InlineData(150000)]
    public void Refresh_KurtulunanFaizSifirYaDaEksiyse_SatirGizli(int removedTotal)
    {
        _service.Preview = loan => Resolved(loan, 0.0279m, Quote(fee: 1489m, removedTotal: removedTotal));
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "148000";

        _viewModel.Refresh(Loan());

        Assert.True(_viewModel.HasPayoff);
        Assert.False(_viewModel.HasInterestSaving);
    }

    [Fact]
    public void Refresh_KapatilacakTaksitKalmadiysa_YalnizAylikFaiz()
    {
        _service.Preview = loan => Resolved(loan, 0.0279m, new LoanPayoffQuote { Date = Today, InstallmentsPaidBefore = 1 });
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "7400";

        _viewModel.Refresh(Loan() with { RemainingInstallmentCount = 1, NextPaymentDate = new DateOnly(2026, 9, 15) });

        Assert.True(_viewModel.IsResolved);
        Assert.Equal(0.0279m, _viewModel.MonthlyRate);
        Assert.False(_viewModel.HasPayoff);
        Assert.False(_viewModel.HasFee);
        Assert.False(_viewModel.HasInterestSaving);
    }

    [Fact]
    public void TutarDegisince_Kendiliginden_YenidenCozulur()
    {
        _service.Preview = loan => Resolved(loan, 0.0279m, Quote(fee: 0m, removedTotal: 180000m));
        _viewModel.Fill(null);
        _viewModel.Refresh(Loan());

        _viewModel.PrincipalInput = "148000";

        Assert.Equal(148000m, LastDraft().RemainingDebt);
        Assert.True(_viewModel.IsResolved);
    }

    [Fact]
    public void Taslak_YalnizAnapara_AnaparaGiderKapatmaTutariTemizlenir()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 1) };
        _viewModel.Fill(loan);

        _viewModel.Refresh(loan);

        var draft = LastDraft();
        Assert.Equal(150000m, draft.RemainingDebt);
        Assert.Null(draft.EarlyClosureAmount);
        Assert.Null(draft.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void Taslak_KapatmaTutariDegismediyse_KendiTarihiniTasir()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = QuoteDate };
        _viewModel.Fill(loan);

        _viewModel.Refresh(loan);

        var draft = LastDraft();
        Assert.Equal(152000m, draft.EarlyClosureAmount);
        Assert.Equal(QuoteDate, draft.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void Taslak_KapatmaTutariDegistiyse_TarihsizGider()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = QuoteDate };
        _viewModel.Fill(loan);
        _viewModel.ClosureInput = "153.000";

        _viewModel.Refresh(loan);

        var draft = LastDraft();
        Assert.Equal(153000m, draft.EarlyClosureAmount);
        Assert.Null(draft.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void Taslak_IkiTutarBirlikte_KapatmaTutariGider()
    {
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "100000";
        _viewModel.ClosureInput = "101000";

        _viewModel.Refresh(Loan());

        var draft = LastDraft();
        Assert.Equal(101000m, draft.EarlyClosureAmount);
        Assert.Null(draft.EarlyClosureAmountAsOf);
    }

    [Theory]
    [InlineData("abc", "", "Kalan anapara")]
    [InlineData("0", "", "Kalan anapara")]
    [InlineData("", "-5", "kapatma tutarı")]
    public void TryApply_GecersizTutar_MesajDonerKrediDegismez(string principal, string closure, string expected)
    {
        var loan = Loan();
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = principal;
        _viewModel.ClosureInput = closure;

        var error = _viewModel.TryApply(loan, out var result);

        Assert.NotNull(error);
        Assert.Contains(expected, error, StringComparison.Ordinal);
        Assert.Same(loan, result);
    }

    [Fact]
    public void TryApply_IkiAlanBosaltilirsa_TutarlarTemizlenir()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = QuoteDate };
        _viewModel.Fill(loan);
        _viewModel.ClosureInput = string.Empty;

        Assert.Null(_viewModel.TryApply(loan, out var result));

        Assert.Null(result.RemainingDebt);
        Assert.Null(result.EarlyClosureAmount);
        Assert.Null(result.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void TryApply_AcilistaBayatKapatmaTutari_Dusurulur()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 1) };
        _viewModel.Fill(loan);

        Assert.Null(_viewModel.TryApply(loan, out var result));

        Assert.Equal(150000m, result.RemainingDebt);
        Assert.Null(result.EarlyClosureAmount);
        Assert.Null(result.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void TryApply_TarihDegisipBayatlayanTutar_SessizceDusmez()
    {
        var loan = Loan() with { RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = QuoteDate };
        _viewModel.Fill(loan);

        Assert.Null(_viewModel.TryApply(loan with { NextPaymentDate = new DateOnly(2026, 11, 15) }, out var result));

        Assert.Equal(152000m, result.EarlyClosureAmount);
        Assert.Equal(QuoteDate, result.EarlyClosureAmountAsOf);
    }

    [Fact]
    public void TryApply_YeniAnapara_AnaparaYazilir()
    {
        _viewModel.Fill(null);
        _viewModel.PrincipalInput = "120.500,75";

        Assert.Null(_viewModel.TryApply(Loan(), out var result));

        Assert.Equal(120500.75m, result.RemainingDebt);
        Assert.Null(result.EarlyClosureAmount);
    }

    [Fact]
    public void HasChanges_TutarDegisinceVarEskiyeDonunceYok()
    {
        _viewModel.Fill(Loan() with { RemainingDebt = 150000m });

        _viewModel.PrincipalInput = "149000";
        Assert.True(_viewModel.HasChanges);

        _viewModel.PrincipalInput = "150000";
        Assert.False(_viewModel.HasChanges);
    }

    // Önizlemeye giden son taslak; hiç önizleme yapılmadıysa test burada düşer.
    private Loan LastDraft()
    {
        Assert.NotEmpty(_service.PreviewedLoans);
        return _service.PreviewedLoans[^1];
    }

    private static Loan Loan() => new()
    {
        Name = "İhtiyaç", Bank = "Akbank", MonthlyPayment = 7500m, PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 10, 15), RemainingInstallmentCount = 24
    };

    // Bugün kapatmanın bedeli: 148.000 anapara + 741 gün faizi + ücret.
    private static LoanPayoffQuote Quote(decimal fee, decimal removedTotal) => new()
    {
        Date = Today, Principal = 148000m, AccruedInterest = 741m, Fee = fee,
        InstallmentsRemoved = 24, RemovedInstallmentTotal = removedTotal
    };

    private static LoanPayoffOverview Resolved(Loan loan, decimal rate, LoanPayoffQuote quote)
    {
        var amortization = new LoanAmortization
        {
            MonthlyRate = rate, Principal = quote.Principal, RemainingInstallments = loan.RemainingInstallmentCount,
            MonthlyPayment = loan.MonthlyPayment, FinalPayment = loan.MonthlyPayment,
            PreviousDueDate = new DateOnly(2026, 9, 15), Source = LoanRateSource.RemainingPrincipal
        };
        return new LoanPayoffOverview(loan, new LoanAnalysis(loan, amortization, LoanAnalysisIssue.None), quote);
    }
}
