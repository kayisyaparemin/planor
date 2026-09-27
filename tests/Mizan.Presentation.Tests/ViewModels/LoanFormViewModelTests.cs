using Mizan.Application.Models;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kredi formu: yeni kredi ve düzenleme, ödeme gününün sonraki taksit tarihinden çözülmesi, formun
/// dokunmadığı alanların korunması, bayat kapatma tutarının düşmesi, faiz kartının forma bağlanması ve
/// kaydetmeden çıkış onayı (EK-V6c, S64).
/// </summary>
public sealed class LoanFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 27);

    private readonly FakeLoanRepository _repository = new();
    private readonly FakeObligationManagementService _service = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly LoanFormViewModel _viewModel;

    public LoanFormViewModelTests() =>
        _viewModel = new LoanFormViewModel(_repository, _service, _navigation, _dialog, new SabitSaat(Today));

    [Fact]
    public async Task Load_Kimliksiz_BosYeniKrediFormuAcar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(string.Empty, _viewModel.Fields.Name);
        Assert.Equal(string.Empty, _viewModel.Fields.PaymentInput);
        Assert.Equal(string.Empty, _viewModel.Fields.CountInput);
        Assert.Equal(new DateOnly(2026, 10, 27), _viewModel.Fields.NextPaymentDate);
        Assert.Equal(LoanKind.Consumer, _viewModel.Fields.Kind);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KrediKimligiyle_AlanlariKredidenDoldurur()
    {
        var loan = Add(Loan() with { MonthlyPayment = 7500.5m, Kind = LoanKind.HousingFixed });

        await _viewModel.LoadAsync(loan.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("İhtiyaç", _viewModel.Fields.Name);
        Assert.Equal("Akbank", _viewModel.Fields.Bank);
        Assert.Equal("7500,5", _viewModel.Fields.PaymentInput);
        Assert.Equal("24", _viewModel.Fields.CountInput);
        Assert.Equal(new DateOnly(2026, 10, 15), _viewModel.Fields.NextPaymentDate);
        Assert.Equal(LoanKind.HousingFixed, _viewModel.Fields.Kind);
        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KrediBulunamazsa_UyariVerirGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_OkumaHatasinda_HataDurumunaDuser()
    {
        _repository.ReadException = new InvalidOperationException("disk");

        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(ScreenState.Error, _viewModel.State);
        Assert.False(_viewModel.IsBusy);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Retry_OkumaHatasindanSonra_AyniKrediyiYukler()
    {
        var loan = Add(Loan());
        _repository.ReadException = new InvalidOperationException("disk");
        await _viewModel.LoadAsync(loan.Id);
        _repository.ReadException = null;

        await _viewModel.RetryCommand.ExecuteAsync(null);

        Assert.Equal(ScreenState.Content, _viewModel.State);
        Assert.True(_viewModel.IsEditing);
        Assert.Equal("İhtiyaç", _viewModel.Fields.Name);
    }

    [Fact]
    public async Task Save_YeniKredi_TanimlaYazilirGeriDonulur()
    {
        await FillNewLoan();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_service.SavedLoans);
        Assert.Equal("Taşıt", saved.Name);
        Assert.Equal("Akbank", saved.Bank);
        Assert.Equal(7500m, saved.MonthlyPayment);
        Assert.Equal(24, saved.RemainingInstallmentCount);
        Assert.Equal(new DateOnly(2026, 10, 15), saved.NextPaymentDate);
        Assert.Equal(15, saved.PaymentDay);
        Assert.Equal(LoanKind.HousingVariable, saved.Kind);
        Assert.True(saved.IsActive);
        Assert.Null(saved.RemainingDebt);
        Assert.Null(saved.FinalPaymentAmount);
        Assert.Null(saved.EarlyClosureAmount);
        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.True(_navigation.NavigateBackCalled);
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task Save_GecmisTarihliSonrakiTaksit_Kabul()
    {
        await FillNewLoan();
        _viewModel.Fields.NextPaymentDate = new DateOnly(2026, 9, 15);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(new DateOnly(2026, 9, 15), Assert.Single(_service.SavedLoans).NextPaymentDate);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Theory]
    [InlineData(2026, 9, 30, 31)]
    [InlineData(2026, 11, 30, 31)]
    [InlineData(2027, 2, 28, 31)]
    [InlineData(2026, 10, 30, 30)]
    [InlineData(2026, 10, 15, 15)]
    public async Task Save_Duzenleme_KayitliGunTarihleUyumluysaKorunur(int year, int month, int day, int expectedDay)
    {
        var loan = Add(Loan() with { PaymentDay = 31, NextPaymentDate = new DateOnly(2026, 9, 30) });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.NextPaymentDate = new DateOnly(year, month, day);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_service.SavedLoans);
        Assert.Equal(new DateOnly(year, month, day), saved.NextPaymentDate);
        Assert.Equal(expectedDay, saved.PaymentDay);
    }

    [Fact]
    public async Task Save_Duzenleme_FormunDokunmadigiAlanlariKorur()
    {
        var loan = Add(Loan() with
        {
            RemainingDebt = 150000m, FinalPaymentAmount = 1200m,
            EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 20)
        });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.Name = "İhtiyaç kredisi";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_service.SavedLoans);
        Assert.Equal(loan.Id, saved.Id);
        Assert.Equal("İhtiyaç kredisi", saved.Name);
        Assert.Equal(150000m, saved.RemainingDebt);
        Assert.Equal(1200m, saved.FinalPaymentAmount);
        Assert.Equal(152000m, saved.EarlyClosureAmount);
        Assert.Equal(new DateOnly(2026, 9, 20), saved.EarlyClosureAmountAsOf);
        Assert.Equal(15, saved.PaymentDay);
        Assert.True(saved.IsActive);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Theory]
    [InlineData("8.000", "24")]
    [InlineData("7.500", "23")]
    public async Task Save_TaksitYaDaKalanTaksitDegisirse_SonTaksitFarkiSifirlanir(string payment, string count)
    {
        var loan = Add(Loan() with { FinalPaymentAmount = 1200m });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.PaymentInput = payment;
        _viewModel.Fields.CountInput = count;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Null(Assert.Single(_service.SavedLoans).FinalPaymentAmount);
    }

    [Fact]
    public async Task Save_SonOdenenTaksittenEskiKapatmaTutari_Dusurulur()
    {
        var loan = Add(Loan() with
        {
            RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 1)
        });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.Bank = "Garanti BBVA";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_service.SavedLoans);
        Assert.Null(saved.EarlyClosureAmount);
        Assert.Null(saved.EarlyClosureAmountAsOf);
        Assert.Equal(150000m, saved.RemainingDebt);
    }

    [Fact]
    public async Task Save_TarihDegisipBayatlayanKapatmaTutari_SessizceDusmez()
    {
        var loan = Add(Loan() with
        {
            RemainingDebt = 150000m, EarlyClosureAmount = 152000m, EarlyClosureAmountAsOf = new DateOnly(2026, 9, 20)
        });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.NextPaymentDate = new DateOnly(2026, 11, 15);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Görünen tutar silinmez; kayıt servisi bayat tutarı mesajıyla reddeder (S64-10).
        var saved = Assert.Single(_service.SavedLoans);
        Assert.Equal(152000m, saved.EarlyClosureAmount);
        Assert.Equal(new DateOnly(2026, 9, 20), saved.EarlyClosureAmountAsOf);
    }

    [Fact]
    public async Task Save_FaizKartiTutarlari_KrediyleYazilir()
    {
        await FillNewLoan();
        _viewModel.Payoff.ClosureInput = "150.000";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_service.SavedLoans);
        Assert.Equal(150000m, saved.EarlyClosureAmount);
        Assert.Null(saved.EarlyClosureAmountAsOf);
    }

    [Fact]
    public async Task Save_GecersizTutar_UyariVerirKaydetmez()
    {
        await FillNewLoan();
        _viewModel.Payoff.PrincipalInput = "yüz bin";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Contains("Kalan anapara", _dialog.LastAlertMessage, StringComparison.Ordinal);
        Assert.Empty(_service.SavedLoans);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Load_FaizKarti_KredidenDoldurulurVeCozulur()
    {
        _service.Preview = Resolved;
        var loan = Add(Loan() with { RemainingDebt = 150000m });

        await _viewModel.LoadAsync(loan.Id);

        Assert.Equal("150000", _viewModel.Payoff.PrincipalInput);
        Assert.True(_viewModel.Payoff.IsResolved);
        Assert.Equal(150000m, LastDraft().RemainingDebt);
    }

    [Fact]
    public async Task FormDegisince_FaizKarti_YenidenCozulur()
    {
        _service.Preview = Resolved;
        var loan = Add(Loan() with { RemainingDebt = 150000m });
        await _viewModel.LoadAsync(loan.Id);

        _viewModel.Fields.PaymentInput = "8.000";
        _viewModel.Fields.Kind = LoanKind.HousingFixed;

        var draft = LastDraft();
        Assert.Equal(8000m, draft.MonthlyPayment);
        Assert.Equal(LoanKind.HousingFixed, draft.Kind);
        Assert.Equal(150000m, draft.RemainingDebt);
    }

    [Fact]
    public async Task FaizKarti_AdYokkenDeTaksitBilgisiyleCozulur()
    {
        _service.Preview = Resolved;
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.PaymentInput = "7.500";
        _viewModel.Fields.CountInput = "24";

        _viewModel.Payoff.PrincipalInput = "148000";

        Assert.True(_viewModel.Payoff.IsResolved);
        Assert.Equal(7500m, LastDraft().MonthlyPayment);
    }

    [Fact]
    public async Task FaizKarti_TaksitBilgisiGecersizse_Cozulmez()
    {
        _service.Preview = Resolved;
        await _viewModel.LoadAsync(null);
        _viewModel.Payoff.PrincipalInput = "148000";

        _viewModel.Fields.CountInput = "0";

        Assert.False(_viewModel.Payoff.IsResolved);
        Assert.False(_viewModel.Payoff.IsMismatch);
        Assert.False(_viewModel.Payoff.NeedsAmount);
    }

    [Theory]
    [InlineData("  ", "7500", "24")]
    [InlineData("Taşıt", "", "24")]
    [InlineData("Taşıt", "yedi bin", "24")]
    [InlineData("Taşıt", "0", "24")]
    [InlineData("Taşıt", "-500", "24")]
    [InlineData("Taşıt", "7500", "")]
    [InlineData("Taşıt", "7500", "0")]
    [InlineData("Taşıt", "7500", "iki")]
    public async Task Save_GecersizAlan_UyariVerirKaydetmez(string name, string payment, string count)
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = name;
        _viewModel.Fields.PaymentInput = payment;
        _viewModel.Fields.CountInput = count;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.Empty(_service.SavedLoans);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_KuralHatasinda_MesajiGosterirFormdaKalir()
    {
        _service.SaveException = new InvalidOperationException("Kalan anapara, kalan taksitlerin toplamından küçük olmalı.");
        await FillNewLoan();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Kalan anapara, kalan taksitlerin toplamından küçük olmalı.", _dialog.LastAlertMessage);
        Assert.False(_navigation.NavigateBackCalled);
        Assert.Equal("Taşıt", _viewModel.Fields.Name);
        Assert.False(_viewModel.IsBusy);
    }

    [Fact]
    public async Task Save_BeklenmeyenHatada_GenelMesajGosterir()
    {
        _service.SaveException = new IOException("disk dolu");
        await FillNewLoan();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastAlertMessage);
        Assert.DoesNotContain("disk", _dialog.LastAlertMessage, StringComparison.Ordinal);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikYoksa_SormadanGeriDoner()
    {
        var loan = Add(Loan());
        await _viewModel.LoadAsync(loan.Id);

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarVeKalinirsa_GeriDonmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = "Taşıt";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasChanges);
        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarVeCikilirsa_KaydetmedenGeriDoner()
    {
        var loan = Add(Loan());
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.PaymentInput = "8000";

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
        Assert.Empty(_service.SavedLoans);
    }

    [Fact]
    public async Task Cancel_TarihYaDaTurDegisirse_DegisiklikSayilir()
    {
        var loan = Add(Loan());
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.NextPaymentDate = new DateOnly(2026, 10, 20);
        var dateChanged = _viewModel.HasChanges;
        _viewModel.Fields.NextPaymentDate = loan.NextPaymentDate;
        _viewModel.Fields.Kind = LoanKind.HousingFixed;

        Assert.True(dateChanged);
        Assert.True(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Cancel_DegerEskiHalineDonerse_DegisiklikSayilmaz()
    {
        var loan = Add(Loan());
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Fields.Name = "Taşıt";
        _viewModel.Fields.Name = "İhtiyaç";

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.False(_viewModel.HasChanges);
        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_FaizKartindaTutarDegisirse_DegisiklikSayilir()
    {
        var loan = Add(Loan() with { RemainingDebt = 150000m });
        await _viewModel.LoadAsync(loan.Id);
        _viewModel.Payoff.ClosureInput = "152000";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.True(_viewModel.HasChanges);
        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.False(_navigation.NavigateBackCalled);
    }

    // Faizi çözülmüş görünüm; sayılar yalnız hâli belirler, kartın ayrıntıları LoanPayoffViewModelTests'te.
    private static LoanPayoffOverview? Resolved(Loan loan)
    {
        var amortization = new LoanAmortization
        {
            MonthlyRate = 0.0279m, Principal = 148000m, RemainingInstallments = loan.RemainingInstallmentCount,
            MonthlyPayment = loan.MonthlyPayment, FinalPayment = loan.MonthlyPayment,
            PreviousDueDate = new DateOnly(2026, 9, 15), Source = LoanRateSource.RemainingPrincipal
        };
        var quote = new LoanPayoffQuote
        {
            Date = Today, Principal = 148000m, AccruedInterest = 741m, InstallmentsRemoved = 24, RemovedInstallmentTotal = 180000m
        };
        return new LoanPayoffOverview(loan, new LoanAnalysis(loan, amortization, LoanAnalysisIssue.None), quote);
    }

    // Faiz kartının önizlemeye verdiği son taslak; hiç önizleme yapılmadıysa test burada düşer.
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

    private Loan Add(Loan loan)
    {
        _repository.Loans[loan.Id] = loan;
        return loan;
    }

    private async Task FillNewLoan()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Fields.Name = "Taşıt";
        _viewModel.Fields.Bank = "Akbank";
        _viewModel.Fields.PaymentInput = "7.500";
        _viewModel.Fields.CountInput = "24";
        _viewModel.Fields.NextPaymentDate = new DateOnly(2026, 10, 15);
        _viewModel.Fields.Kind = LoanKind.HousingVariable;
    }
}
