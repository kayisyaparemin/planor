using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Gelir formundaki tutarlar: yürürlükteki ve ileri tarihli tutarların listesi ve yerinde taşma, yeni
/// tutarın varsayılan tarihi ve kuralları, satıra dokununca silme ya da bilgi, değişiklik izleme
/// (EK-V6d, S67-5, S67 V6d2 notları).
/// </summary>
public sealed class IncomeAmountsViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);

    private readonly FakeDialogService _dialog = new();
    private readonly IncomeAmountsViewModel _amounts;
    private readonly RecurringIncome _income = new() { Name = "Kira geliri", PaymentDay = 20 };

    public IncomeAmountsViewModelTests()
    {
        _amounts = new IncomeAmountsViewModel(_dialog, new SabitSaat(Today));
    }

    [Fact]
    public void Load_GelirYoksa_ListeBosVeDegisiklikYok()
    {
        _amounts.Load(null, [Amount(12_000m, new DateOnly(2026, 7, 20))]);

        Assert.False(_amounts.HasItems);
        Assert.Empty(_amounts.Items);
        Assert.False(_amounts.HasChanges);
    }

    [Fact]
    public void Load_YururluktekiVeIleriTarihlileriGosterir_EskiVeBaskaGelirinkileriGostermez()
    {
        var old = Amount(10_000m, new DateOnly(2026, 1, 20));
        var current = Amount(12_000m, new DateOnly(2026, 7, 20));
        var raise = Amount(14_000m, new DateOnly(2027, 1, 20));
        var otherIncome = Amount(9_000m, new DateOnly(2027, 3, 5)) with { RecurringIncomeId = Guid.NewGuid() };

        _amounts.Load(_income, [raise, otherIncome, old, current]);

        Assert.Equal(
            [new IncomeAmountRow(current.Id, current.EffectiveDate, 12_000m, false), new IncomeAmountRow(raise.Id, raise.EffectiveDate, 14_000m, true)],
            _amounts.Items);
        Assert.True(_amounts.HasItems);
        Assert.False(_amounts.HasChanges);
    }

    [Fact]
    public void Load_DortSatirdanFazlasi_IlkDordunuGosterirKalaniTasar()
    {
        _amounts.Load(_income, Enumerable.Range(1, 6).Select(i => Amount(10_000m + i, new DateOnly(2027, i, 20))));

        Assert.Equal(4, _amounts.Items.Count);
        Assert.Equal(new DateOnly(2027, 1, 20), _amounts.Items[0].EffectiveDate);
        Assert.Equal(2, _amounts.HiddenCount);
        Assert.True(_amounts.HasOverflow);
    }

    [Fact]
    public void Expand_GizliSatirlariDaGosterir()
    {
        _amounts.Load(_income, Enumerable.Range(1, 6).Select(i => Amount(10_000m + i, new DateOnly(2027, i, 20))));

        _amounts.ExpandCommand.Execute(null);

        Assert.Equal(6, _amounts.Items.Count);
        Assert.Equal(0, _amounts.HiddenCount);
    }

    [Theory]
    [InlineData(20, 2026, 10, 20)]
    [InlineData(5, 2026, 11, 5)]
    [InlineData(12, 2026, 11, 12)]
    [InlineData(31, 2026, 10, 31)]
    public void OpenEntry_VarsayilanTarih_BugundenSonrakiIlkOdemeGunu(int paymentDay, int year, int month, int day)
    {
        _amounts.Load(_income with { PaymentDay = paymentDay }, []);

        _amounts.OpenEntryCommand.Execute(null);

        Assert.True(_amounts.IsEntryOpen);
        Assert.Equal(new DateOnly(year, month, day), _amounts.EntryDate);
        Assert.Equal(string.Empty, _amounts.AmountInput);
        Assert.Equal(Today, _amounts.MinimumDate);
    }

    [Fact]
    public void OpenEntry_OdemeGunuKisaAydaYoksa_AySonunaKenetlenir()
    {
        var amounts = new IncomeAmountsViewModel(_dialog, new SabitSaat(new DateOnly(2027, 1, 31)));
        amounts.Load(_income with { PaymentDay = 31 }, []);

        amounts.OpenEntryCommand.Execute(null);

        Assert.Equal(new DateOnly(2027, 2, 28), amounts.EntryDate);
    }

    [Fact]
    public async Task Add_GecerliGiris_ListeyeEklerGirisiKapatirDegisiklikSayilir()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 20));
        _amounts.Load(_income, [current]);
        Fill("14.000", new DateOnly(2027, 1, 20));

        await _amounts.AddCommand.ExecuteAsync(null);

        Assert.Equal([12_000m, 14_000m], _amounts.Items.Select(r => r.Amount));
        Assert.False(_amounts.IsEntryOpen);
        Assert.True(_amounts.HasChanges);
        var added = Assert.Single(_amounts.ToAdded());
        Assert.Equal((14_000m, new DateOnly(2027, 1, 20)), (added.Amount, added.EffectiveDate));
        Assert.Empty(_amounts.ToRemovedIds());
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task Add_BugunTarihli_YururluktekiTutarinYerineGecer()
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20))]);
        Fill("13.000", Today);

        await _amounts.AddCommand.ExecuteAsync(null);

        var row = Assert.Single(_amounts.Items);
        Assert.Equal((Today, 13_000m, true), (row.EffectiveDate, row.Amount, row.IsRemovable));
    }

    [Theory]
    [InlineData("", "Yeni tutarı sayı olarak gir.")]
    [InlineData("abc", "Yeni tutarı sayı olarak gir.")]
    [InlineData("0", "Gelir tutarı sıfırdan büyük olmalıdır.")]
    public async Task Add_TutarGecersizse_UyariVerirGirisAcikKalir(string input, string message)
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20))]);
        Fill(input, new DateOnly(2027, 1, 20));

        await _amounts.AddCommand.ExecuteAsync(null);

        Assert.Equal(("Tutar eklenemedi", message), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_amounts.IsEntryOpen);
        Assert.False(_amounts.HasChanges);
    }

    [Fact]
    public async Task Add_AyniTarihteTutarVarsa_UyariVerirEklemez()
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20)), Amount(14_000m, new DateOnly(2027, 1, 20))]);
        Fill("15.000", new DateOnly(2027, 1, 20));

        await _amounts.AddCommand.ExecuteAsync(null);

        Assert.Equal("Bu tarihte zaten bir tutar var.", _dialog.LastAlertMessage);
        Assert.Equal(2, _amounts.Items.Count);
        Assert.True(_amounts.IsEntryOpen);
    }

    [Fact]
    public async Task Add_TarihBugundenOnceyse_UyariVerirEklemez()
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20))]);
        Fill("13.000", Today.AddDays(-1));

        await _amounts.AddCommand.ExecuteAsync(null);

        Assert.Equal("Geçerlilik tarihi bugünden önce olamaz.", _dialog.LastAlertMessage);
        Assert.False(_amounts.HasChanges);
    }

    [Fact]
    public async Task Select_IleriTarihliSatirSilOnaylaninca_ListedenCikarSilinenKimlikOlur()
    {
        var current = Amount(12_000m, new DateOnly(2026, 7, 20));
        var raise = Amount(14_000m, new DateOnly(2027, 1, 20));
        _amounts.Load(_income, [current, raise]);
        _dialog.NextChooseResponse = "Sil";

        await _amounts.SelectCommand.ExecuteAsync(_amounts.Items[1]);

        Assert.Equal(("Tutar değişikliği", "Sil"), (_dialog.LastChooseTitle, _dialog.LastChooseDestruction));
        Assert.Equal([current.Id], _amounts.Items.Select(r => r.Id));
        Assert.Equal([raise.Id], _amounts.ToRemovedIds());
        Assert.True(_amounts.HasChanges);
    }

    [Fact]
    public async Task Select_Vazgecilirse_SatirKalir()
    {
        _amounts.Load(_income, [Amount(14_000m, new DateOnly(2027, 1, 20))]);
        _dialog.NextChooseResponse = "Vazgeç";

        await _amounts.SelectCommand.ExecuteAsync(_amounts.Items[0]);

        Assert.Single(_amounts.Items);
        Assert.False(_amounts.HasChanges);
    }

    [Fact]
    public async Task Select_YururlugeGirmisSatir_SilmezNedeniniSoyler()
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20))]);

        await _amounts.SelectCommand.ExecuteAsync(_amounts.Items[0]);

        Assert.Null(_dialog.LastChooseTitle);
        Assert.Equal("Yürürlükteki tutar", _dialog.LastAlertTitle);
        Assert.Equal("Yürürlüğe girmiş tutar silinmez. Değiştirmek için yeni tutar ekle.", _dialog.LastAlertMessage);
        Assert.Single(_amounts.Items);
    }

    [Fact]
    public async Task Select_BugunkuTekTutarSilinince_ListeBosalirYenisiEklenebilir()
    {
        var mistyped = Amount(1_250m, Today);
        _amounts.Load(_income, [mistyped]);
        _dialog.NextChooseResponse = "Sil";
        await _amounts.SelectCommand.ExecuteAsync(_amounts.Items[0]);
        Fill("12.500", Today);

        await _amounts.AddCommand.ExecuteAsync(null);

        Assert.Equal(12_500m, Assert.Single(_amounts.Items).Amount);
        Assert.Equal([mistyped.Id], _amounts.ToRemovedIds());
        Assert.Single(_amounts.ToAdded());
    }

    [Fact]
    public async Task Select_EklenmisSatirSilinirse_DegisiklikKalmaz()
    {
        _amounts.Load(_income, [Amount(12_000m, new DateOnly(2026, 7, 20))]);
        Fill("14.000", new DateOnly(2027, 1, 20));
        await _amounts.AddCommand.ExecuteAsync(null);
        _dialog.NextChooseResponse = "Sil";

        await _amounts.SelectCommand.ExecuteAsync(_amounts.Items[1]);

        Assert.Single(_amounts.Items);
        Assert.False(_amounts.HasChanges);
        Assert.Empty(_amounts.ToAdded());
        Assert.Empty(_amounts.ToRemovedIds());
    }

    [Fact]
    public void CancelEntry_GirisiEklemedenKapatir()
    {
        _amounts.Load(_income, []);
        _amounts.OpenEntryCommand.Execute(null);

        _amounts.CancelEntryCommand.Execute(null);

        Assert.False(_amounts.IsEntryOpen);
        Assert.False(_amounts.HasChanges);
    }

    private IncomeAmountHistory Amount(decimal amount, DateOnly effectiveDate) =>
        new() { RecurringIncomeId = _income.Id, Amount = amount, EffectiveDate = effectiveDate };

    private void Fill(string amount, DateOnly date)
    {
        _amounts.OpenEntryCommand.Execute(null);
        _amounts.AmountInput = amount;
        _amounts.EntryDate = date;
    }
}
