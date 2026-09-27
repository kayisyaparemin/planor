using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kart formundaki gelecek kart harcamaları: tarihe göre liste ve yerinde taşma, aylık tutar ×
/// taksit sayısıyla ekleme, satıra dokununca silme, değişiklik izleme (EK-V6b, S63-5).
/// </summary>
public sealed class CardChargesViewModelTests
{
    private static readonly DateOnly Today = new(2026, 9, 27);

    private readonly FakeDialogService _dialog = new();
    private readonly CardChargesViewModel _charges;

    public CardChargesViewModelTests()
    {
        _charges = new CardChargesViewModel(_dialog, new SabitSaat(Today));
    }

    [Fact]
    public void Load_Bos_ListeGorunmez()
    {
        _charges.Load([]);

        Assert.False(_charges.HasItems);
        Assert.Empty(_charges.Items);
        Assert.False(_charges.HasOverflow);
    }

    [Fact]
    public void Load_TarihSirasiylaIlkDordunuGosterirKalaniTasar()
    {
        _charges.Load(Enumerable.Range(1, 6).Reverse().Select(i => Charge($"Taksit {i}", new DateOnly(2026, 10 + (i / 4), i), 100m * i)));

        Assert.True(_charges.HasItems);
        Assert.Equal(["Taksit 1", "Taksit 2", "Taksit 3", "Taksit 4"], _charges.Items.Select(r => r.Description));
        Assert.Equal(2, _charges.HiddenCount);
        Assert.True(_charges.HasOverflow);
    }

    [Fact]
    public void Expand_GizliSatirlariDaGosterir()
    {
        _charges.Load(Enumerable.Range(1, 6).Select(i => Charge($"Taksit {i}", new DateOnly(2026, 11, i), 100m)));

        _charges.ExpandCommand.Execute(null);

        Assert.Equal(6, _charges.Items.Count);
        Assert.Equal(0, _charges.HiddenCount);
    }

    [Fact]
    public void OpenEntry_TekTaksitVeBirAySonraIleAcilir()
    {
        _charges.Load([]);

        _charges.OpenEntryCommand.Execute(null);

        Assert.True(_charges.IsEntryOpen);
        Assert.Equal("1", _charges.CountInput);
        Assert.Equal(new DateOnly(2026, 10, 27), _charges.FirstDate);
        Assert.Equal(Today, _charges.MinimumDate);
        Assert.Equal(string.Empty, _charges.DescriptionInput);
    }

    [Fact]
    public async Task Add_TekTaksit_AciklamaOlduguGibiEklenirGirisKapanir()
    {
        _charges.Load([]);
        Fill("Buzdolabı", "12.500", "1", new DateOnly(2026, 11, 5));

        await _charges.AddCommand.ExecuteAsync(null);

        var row = Assert.Single(_charges.Items);
        Assert.Equal("Buzdolabı", row.Description);
        Assert.Equal(new DateOnly(2026, 11, 5), row.PostingDate);
        Assert.Equal(12500m, row.Amount);
        Assert.False(_charges.IsEntryOpen);
        Assert.True(_charges.HasChanges);
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task Add_CokTaksit_HerAyAyniTutarlaNumaraliKayitlarEklenir()
    {
        _charges.Load([]);
        Fill(" Telefon ", "2.000", "3", new DateOnly(2027, 1, 31));

        await _charges.AddCommand.ExecuteAsync(null);

        Assert.Equal(["Telefon (1/3)", "Telefon (2/3)", "Telefon (3/3)"], _charges.Items.Select(r => r.Description));
        Assert.Equal(
            [new DateOnly(2027, 1, 31), new DateOnly(2027, 2, 28), new DateOnly(2027, 3, 31)],
            _charges.Items.Select(r => r.PostingDate));
        Assert.All(_charges.Items, r => Assert.Equal(2000m, r.Amount));
    }

    [Fact]
    public async Task Add_MevcutListeyeTarihSirasiylaKarisir()
    {
        _charges.Load([Charge("Tatil", new DateOnly(2026, 12, 1), 5000m)]);
        Fill("Telefon", "2000", "2", new DateOnly(2026, 11, 15));

        await _charges.AddCommand.ExecuteAsync(null);

        Assert.Equal(["Telefon (1/2)", "Tatil", "Telefon (2/2)"], _charges.Items.Select(r => r.Description));
    }

    [Theory]
    [InlineData("  ", "2000", "1", 0)]
    [InlineData("Telefon", "", "1", 0)]
    [InlineData("Telefon", "iki bin", "1", 0)]
    [InlineData("Telefon", "0", "1", 0)]
    [InlineData("Telefon", "2000", "0", 0)]
    [InlineData("Telefon", "2000", "121", 0)]
    [InlineData("Telefon", "2000", "", 0)]
    [InlineData("Telefon", "2000", "1", -1)]
    public async Task Add_GecersizGirdi_UyariVerirEklemezGirisAcikKalir(string description, string amount, string count, int dayOffset)
    {
        _charges.Load([]);
        Fill(description, amount, count, Today.AddDays(dayOffset));

        await _charges.AddCommand.ExecuteAsync(null);

        Assert.NotNull(_dialog.LastAlertTitle);
        Assert.Empty(_charges.Items);
        Assert.True(_charges.IsEntryOpen);
        Assert.False(_charges.HasChanges);
    }

    [Fact]
    public async Task Add_BugunTarihliTaksitKabulEdilir()
    {
        _charges.Load([]);
        Fill("Telefon", "2000", "120", Today);

        await _charges.AddCommand.ExecuteAsync(null);

        Assert.Equal(CardChargesViewModel.CollapsedRowLimit, _charges.Items.Count);
        Assert.Equal(116, _charges.HiddenCount);
    }

    [Fact]
    public void CancelEntry_GirisiEklemedenKapatir()
    {
        _charges.Load([]);
        Fill("Telefon", "2000", "1", new DateOnly(2026, 11, 5));

        _charges.CancelEntryCommand.Execute(null);

        Assert.False(_charges.IsEntryOpen);
        Assert.Empty(_charges.Items);
        Assert.False(_charges.HasChanges);
    }

    [Fact]
    public async Task Select_SilSecilince_SatiriKaldirir()
    {
        var charge = Charge("Tatil", new DateOnly(2026, 12, 1), 5000m);
        _charges.Load([charge, Charge("Telefon", new DateOnly(2026, 11, 1), 2000m)]);
        _dialog.NextChooseResponse = "Sil";

        await _charges.SelectCommand.ExecuteAsync(_charges.Items.Single(r => r.Id == charge.Id));

        Assert.Equal("Tatil", _dialog.LastChooseTitle);
        Assert.Equal("Sil", _dialog.LastChooseDestruction);
        Assert.Equal("Telefon", Assert.Single(_charges.Items).Description);
        Assert.True(_charges.HasChanges);
    }

    [Fact]
    public async Task Select_Vazgecilirse_SatirKalir()
    {
        _charges.Load([Charge("Tatil", new DateOnly(2026, 12, 1), 5000m)]);
        _dialog.NextChooseResponse = null;

        await _charges.SelectCommand.ExecuteAsync(_charges.Items[0]);

        Assert.Single(_charges.Items);
        Assert.False(_charges.HasChanges);
    }

    [Fact]
    public async Task HasChanges_EklenenSilinince_DegisiklikKalmaz()
    {
        _charges.Load([Charge("Tatil", new DateOnly(2026, 12, 1), 5000m)]);
        Fill("Telefon", "2000", "1", new DateOnly(2026, 11, 5));
        await _charges.AddCommand.ExecuteAsync(null);
        _dialog.NextChooseResponse = "Sil";

        await _charges.SelectCommand.ExecuteAsync(_charges.Items.Single(r => r.Description == "Telefon"));

        Assert.False(_charges.HasChanges);
    }

    [Fact]
    public async Task ToCharges_KayitliKimliklerKorunurYenilerEklenir()
    {
        var existing = Charge("Tatil", new DateOnly(2026, 12, 1), 5000m);
        _charges.Load([existing]);
        Fill("Telefon", "2000", "1", new DateOnly(2026, 11, 5));
        await _charges.AddCommand.ExecuteAsync(null);

        var charges = _charges.ToCharges();

        Assert.Equal(2, charges.Count);
        Assert.Equal(existing.Id, charges.Single(c => c.Description == "Tatil").Id);
        var added = charges.Single(c => c.Description == "Telefon");
        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.Equal(new DateOnly(2026, 11, 5), added.PostingDate);
        Assert.Equal(2000m, added.Amount);
    }

    private void Fill(string description, string amount, string count, DateOnly firstDate)
    {
        _charges.OpenEntryCommand.Execute(null);
        _charges.DescriptionInput = description;
        _charges.AmountInput = amount;
        _charges.CountInput = count;
        _charges.FirstDate = firstDate;
    }

    private static CardCharge Charge(string description, DateOnly date, decimal amount) =>
        new() { Description = description, PostingDate = date, Amount = amount };
}
