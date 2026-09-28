using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kredi formundaki planlı erken ödemeler: tarihe göre liste ve yerinde taşma, tutarların taslak
/// krediyle önizlemeden gelmesi, girişin varsayılanları ve kuralları, satıra dokununca silme,
/// değişiklik izleme (EK-V6c, S64-6, S64-13–15).
/// </summary>
public sealed class LoanPrepaymentsViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 27);

    private readonly FakeObligationManagementService _service = new();
    private readonly FakeDialogService _dialog = new();
    private readonly LoanPrepaymentsViewModel _prepayments;

    public LoanPrepaymentsViewModelTests() =>
        _prepayments = new LoanPrepaymentsViewModel(_service, _dialog, new SabitSaat(Today));

    [Fact]
    public void Load_Bos_ListeGorunmez()
    {
        _prepayments.Load([]);

        Assert.False(_prepayments.HasItems);
        Assert.Empty(_prepayments.Items);
        Assert.False(_prepayments.HasOverflow);
        Assert.False(_prepayments.HasChanges);
    }

    [Fact]
    public void Load_TarihSirasiylaIlkDordunuGosterirKalaniTasar()
    {
        _prepayments.Load(Enumerable.Range(1, 6).Reverse().Select(i => Closure(new DateOnly(2027, i, 15))));

        Assert.True(_prepayments.HasItems);
        Assert.Equal(Enumerable.Range(1, 4).Select(i => new DateOnly(2027, i, 15)), _prepayments.Items.Select(r => r.Date));
        Assert.Equal(2, _prepayments.HiddenCount);
        Assert.True(_prepayments.HasOverflow);
    }

    [Fact]
    public void Expand_GizliSatirlariDaGosterir()
    {
        _prepayments.Load(Enumerable.Range(1, 6).Select(i => Closure(new DateOnly(2027, i, 15))));

        _prepayments.ExpandCommand.Execute(null);

        Assert.Equal(6, _prepayments.Items.Count);
        Assert.Equal(0, _prepayments.HiddenCount);
    }

    [Fact]
    public void Refresh_TaslakKrediyle_TutarlarOnizlemedenGelir()
    {
        var terms = Terms();
        _service.PrepaymentAmount = (_, p) => p.Mode == LoanPrepaymentMode.FullClosure ? 52_410m : 20_180m;
        _prepayments.Load([Closure(new DateOnly(2027, 3, 15)), Partial(new DateOnly(2027, 1, 15), 20_000m)]);

        _prepayments.Refresh(terms);

        Assert.Same(terms, _service.PrepaymentPreviewLoans[^1]);
        Assert.Equal([20_180m, 52_410m], _prepayments.Items.Select(r => r.Amount));
        Assert.Equal([20_000m, null], _prepayments.Items.Select(r => r.PrincipalAmount));
    }

    [Fact]
    public void Refresh_TaslakYoksa_TutarlarHesaplanmaz()
    {
        _service.PrepaymentAmount = (_, _) => 52_410m;
        _prepayments.Load([Closure(new DateOnly(2027, 3, 15))]);
        _prepayments.Refresh(Terms());

        _prepayments.Refresh(null);

        Assert.Null(Assert.Single(_prepayments.Items).Amount);
    }

    [Theory]
    [InlineData(2026, 10, 15, 2026, 10, 15)] // sonraki taksit ileride: o gün
    [InlineData(2026, 9, 15, 2026, 10, 15)]  // sonraki taksit geçmişte: bugünden sonraki ilk taksit
    [InlineData(2026, 8, 31, 2026, 9, 30)]   // ay sonu günü kenetlenir
    public void OpenEntry_TamamenKapatmaVeBugundenSonrakiIlkTaksitGunuyleAcilir(
        int year, int month, int day, int expectedYear, int expectedMonth, int expectedDay)
    {
        _prepayments.Load([]);
        _prepayments.Refresh(Terms() with { NextPaymentDate = new DateOnly(year, month, day), PaymentDay = day });

        _prepayments.OpenEntryCommand.Execute(null);

        Assert.True(_prepayments.IsEntryOpen);
        Assert.Equal(LoanPrepaymentMode.FullClosure, _prepayments.EntryMode);
        Assert.False(_prepayments.IsPartial);
        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), _prepayments.EntryDate);
        Assert.Equal(string.Empty, _prepayments.AmountInput);
        Assert.Equal(Today, _prepayments.MinimumDate);
    }

    [Fact]
    public void OpenEntry_TekTaksitKaldiysaYaDaTaslakYoksa_Bugun()
    {
        _prepayments.Load([]);
        _prepayments.Refresh(Terms() with { RemainingInstallmentCount = 1 });
        _prepayments.OpenEntryCommand.Execute(null);
        Assert.Equal(Today, _prepayments.EntryDate);

        _prepayments.Refresh(null);
        _prepayments.OpenEntryCommand.Execute(null);
        Assert.Equal(Today, _prepayments.EntryDate);
    }

    [Fact]
    public void EntryMode_AraOdemede_TutarSorulur()
    {
        _prepayments.EntryMode = LoanPrepaymentMode.ReduceInstallment;

        Assert.True(_prepayments.IsPartial);
    }

    [Fact]
    public async Task Add_TamamenKapatma_TutarsizEklenirGirisKapanir()
    {
        _service.PrepaymentAmount = (_, _) => 52_410m;
        OpenEntry(LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15), "abc");

        await _prepayments.AddCommand.ExecuteAsync(null);

        var row = Assert.Single(_prepayments.Items);
        Assert.Equal(LoanPrepaymentMode.FullClosure, row.Mode);
        Assert.Equal(new DateOnly(2027, 3, 15), row.Date);
        Assert.Null(row.PrincipalAmount);
        Assert.Equal(52_410m, row.Amount);
        Assert.False(_prepayments.IsEntryOpen);
        Assert.True(_prepayments.HasChanges);
        Assert.Null(_dialog.LastAlertTitle);
        var (loan, candidate) = Assert.Single(_service.ValidatedCandidates);
        Assert.Equal(24, loan.RemainingInstallmentCount);
        Assert.Null(candidate.PrincipalAmount);
    }

    [Fact]
    public async Task Add_AraOdeme_AnaparadanDusecekTutarlaEklenir()
    {
        OpenEntry(LoanPrepaymentMode.ReduceInstallment, new DateOnly(2027, 1, 15), "20.000");

        await _prepayments.AddCommand.ExecuteAsync(null);

        var row = Assert.Single(_prepayments.Items);
        Assert.Equal(LoanPrepaymentMode.ReduceInstallment, row.Mode);
        Assert.Equal(20_000m, row.PrincipalAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task Add_AraOdemeTutariGecersiz_UyariVerirEklemez(string amount)
    {
        OpenEntry(LoanPrepaymentMode.ReduceTerm, new DateOnly(2027, 1, 15), amount);

        await _prepayments.AddCommand.ExecuteAsync(null);

        AssertRejected("Anaparadan düşecek tutarı sayı olarak gir.");
        Assert.Empty(_service.ValidatedCandidates);
    }

    [Fact]
    public async Task Add_GecmisTarih_UyariVerirEklemez()
    {
        OpenEntry(LoanPrepaymentMode.FullClosure, Today.AddDays(-1), string.Empty);

        await _prepayments.AddCommand.ExecuteAsync(null);

        AssertRejected("Ödeme tarihi bugünden önce olamaz.");
    }

    [Fact]
    public async Task Add_TaslakKrediYoksa_UyariVerirEklemez()
    {
        OpenEntry(LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15), string.Empty);
        _prepayments.Refresh(null);

        await _prepayments.AddCommand.ExecuteAsync(null);

        AssertRejected("Önce aylık taksiti ve kalan taksiti gir.");
    }

    [Fact]
    public async Task Add_KaydinKuraliReddederse_MesajiGosterirGirisAcikKalir()
    {
        _service.Validate = _ => "Bu kredi 15.01.2027 tarihinde zaten kapatılıyor.";
        OpenEntry(LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15), string.Empty);

        await _prepayments.AddCommand.ExecuteAsync(null);

        AssertRejected("Bu kredi 15.01.2027 tarihinde zaten kapatılıyor.");
    }

    [Fact]
    public void CancelEntry_EklemedenKapatir()
    {
        OpenEntry(LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15), string.Empty);

        _prepayments.CancelEntryCommand.Execute(null);

        Assert.False(_prepayments.IsEntryOpen);
        Assert.Empty(_prepayments.Items);
    }

    [Theory]
    [InlineData(LoanPrepaymentMode.FullClosure, "Tamamen kapatma")]
    [InlineData(LoanPrepaymentMode.ReduceTerm, "Ara ödeme · vade kısalır")]
    [InlineData(LoanPrepaymentMode.ReduceInstallment, "Ara ödeme · taksit düşer")]
    public async Task Select_SilOnaylanirsa_SatirCikar(LoanPrepaymentMode mode, string title)
    {
        _prepayments.Load([new LoanPrepayment { Date = new DateOnly(2027, 3, 15), Mode = mode, PrincipalAmount = 1_000m }]);
        _dialog.NextChooseResponse = "Sil";

        await _prepayments.SelectCommand.ExecuteAsync(_prepayments.Items[0]);

        Assert.Empty(_prepayments.Items);
        Assert.False(_prepayments.HasItems);
        Assert.True(_prepayments.HasChanges);
        Assert.Equal(title, _dialog.LastChooseTitle);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
    }

    [Fact]
    public async Task Select_Vazgecilirse_SatirKalir()
    {
        _prepayments.Load([Closure(new DateOnly(2027, 3, 15))]);
        _dialog.NextChooseResponse = "Vazgeç";

        await _prepayments.SelectCommand.ExecuteAsync(_prepayments.Items[0]);

        Assert.Single(_prepayments.Items);
        Assert.False(_prepayments.HasChanges);
    }

    [Fact]
    public async Task ToPrepayments_YuklenenVeEklenenleriKimlikleriyleDoner()
    {
        var loaded = Partial(new DateOnly(2027, 1, 15), 20_000m);
        _prepayments.Load([loaded]);
        OpenEntry(LoanPrepaymentMode.FullClosure, new DateOnly(2027, 3, 15), string.Empty);
        await _prepayments.AddCommand.ExecuteAsync(null);

        var result = _prepayments.ToPrepayments();

        Assert.Equal(2, result.Count);
        Assert.Equal(loaded.Id, result[0].Id);
        Assert.Equal(LoanPrepaymentMode.ReduceTerm, result[0].Mode);
        Assert.Equal(new DateOnly(2027, 1, 15), result[0].Date);
        Assert.Equal(20_000m, result[0].PrincipalAmount);
        Assert.Equal(LoanPrepaymentMode.FullClosure, result[1].Mode);
        Assert.Equal(new DateOnly(2027, 3, 15), result[1].Date);
        Assert.Null(result[1].PrincipalAmount);
        Assert.NotEqual(Guid.Empty, result[1].Id);
    }

    [Fact]
    public async Task HasChanges_EklenenSilinirse_DegisiklikSayilmaz()
    {
        _prepayments.Load([Closure(new DateOnly(2027, 3, 15))]);
        OpenEntry(LoanPrepaymentMode.ReduceTerm, new DateOnly(2027, 1, 15), "5.000");
        await _prepayments.AddCommand.ExecuteAsync(null);
        _dialog.NextChooseResponse = "Sil";

        await _prepayments.SelectCommand.ExecuteAsync(_prepayments.Items.First(r => r.Mode == LoanPrepaymentMode.ReduceTerm));

        Assert.False(_prepayments.HasChanges);
    }

    private void OpenEntry(LoanPrepaymentMode mode, DateOnly date, string amount)
    {
        _prepayments.Refresh(Terms());
        _prepayments.OpenEntryCommand.Execute(null);
        _prepayments.EntryMode = mode;
        _prepayments.EntryDate = date;
        _prepayments.AmountInput = amount;
    }

    private void AssertRejected(string message)
    {
        Assert.Equal("Erken ödeme eklenemedi", _dialog.LastAlertTitle);
        Assert.Equal(message, _dialog.LastAlertMessage);
        Assert.Empty(_prepayments.Items);
        Assert.True(_prepayments.IsEntryOpen);
    }

    private static Loan Terms() => new()
    {
        Name = "İhtiyaç", MonthlyPayment = 7500m, PaymentDay = 15,
        NextPaymentDate = new DateOnly(2026, 10, 15), RemainingInstallmentCount = 24
    };

    private static LoanPrepayment Closure(DateOnly date) => new() { Date = date, Mode = LoanPrepaymentMode.FullClosure };

    private static LoanPrepayment Partial(DateOnly date, decimal principal) =>
        new() { Date = date, Mode = LoanPrepaymentMode.ReduceTerm, PrincipalAmount = principal };
}
