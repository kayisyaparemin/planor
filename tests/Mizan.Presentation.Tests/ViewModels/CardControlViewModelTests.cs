using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.ViewModels;
using Xunit;
using static Mizan.Presentation.Tests.ViewModels.CardControlFixture;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kart kontrol ekranının kart seçimi, boş / hata durumları, kart bilgisi ve sıradaki ödeme
/// kartının hâlleri (ödeme / ödeme yok / ekstre girişi) için birim testleri (EK-V7, S61).
/// </summary>
public sealed class CardControlViewModelTests
{
    private readonly CardControlFixture _fx = new();
    private CardControlViewModel ViewModel => _fx.ViewModel;

    [Fact]
    public async Task Load_KartYoksa_BosDurumaDuser()
    {
        await ViewModel.LoadAsync(null);

        Assert.Equal(ScreenState.Empty, ViewModel.State);
        Assert.False(ViewModel.IsBusy);
    }

    [Fact]
    public async Task Load_OkumaHatasinda_HataDurumunaDuser()
    {
        _fx.Repository.ReadException = new InvalidOperationException("disk");

        await ViewModel.LoadAsync(null);

        Assert.Equal(ScreenState.Error, ViewModel.State);
        Assert.False(ViewModel.IsBusy);
    }

    [Fact]
    public async Task Load_KartBilgisiVeLimitSatiriDolar()
    {
        var card = _fx.Add(Card("Bonus", statement: Statement(18200m, 3640m), plan: CurrentStatementPaymentMode.Full));

        await ViewModel.LoadAsync(null);

        Assert.Equal(ScreenState.Content, ViewModel.State);
        Assert.Equal(card.Id, ViewModel.CardId);
        Assert.Equal("Bonus", ViewModel.CardName);
        Assert.Equal("Garanti BBVA", ViewModel.BankName);
        Assert.Equal(15, ViewModel.StatementClosingDay);
        Assert.Equal(25, ViewModel.PaymentDueDay);
        Assert.Equal(50000m, ViewModel.Limit);
        Assert.Equal(31800m, ViewModel.AvailableLimit);
        Assert.True(ViewModel.HasStatement);
    }

    [Fact]
    public async Task Load_KimliksizAcilista_EnYakinOdemesiOlanKartAcilir()
    {
        _fx.Add(Card("Bonus"));
        var axess = _fx.Add(Card("Axess", carried: 4000m));

        await ViewModel.LoadAsync(null);

        Assert.Equal(axess.Id, ViewModel.CardId);
        Assert.True(ViewModel.HasUpcomingPayment);
        Assert.True(ViewModel.IsPaymentView);
        Assert.False(ViewModel.IsNoPaymentView);
    }

    [Fact]
    public async Task Load_OdemesiOlmayanKartta_OdemeYokHaliGosterilir()
    {
        _fx.Add(Card("Bonus"));

        await ViewModel.LoadAsync(null);

        Assert.Equal(ScreenState.Content, ViewModel.State);
        Assert.False(ViewModel.HasUpcomingPayment);
        Assert.True(ViewModel.IsNoPaymentView);
        Assert.False(ViewModel.IsPaymentView);
        Assert.False(ViewModel.Upcoming.HasItems);
    }

    [Fact]
    public async Task Load_BilinmeyenKimlikte_KartaDuser()
    {
        var card = _fx.Add(Card("Bonus", carried: 4000m));

        await ViewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(card.Id, ViewModel.CardId);
        Assert.Equal(ScreenState.Content, ViewModel.State);
    }

    [Fact]
    public async Task Load_KimliksizYenileme_AcikKartiKorur()
    {
        var bonus = _fx.Add(Card("Bonus"));
        _fx.Add(Card("Axess", carried: 4000m));
        await ViewModel.LoadAsync(bonus.Id);

        await ViewModel.LoadAsync(null);

        Assert.Equal(bonus.Id, ViewModel.CardId);
    }

    [Fact]
    public async Task Load_TekKartta_KartDegistirmeKapali_IkiKartta_Acik()
    {
        _fx.Add(Card("Bonus"));
        await ViewModel.LoadAsync(null);
        Assert.False(ViewModel.HasMultipleCards);

        _fx.Add(Card("World"));
        await ViewModel.LoadAsync(null);
        Assert.True(ViewModel.HasMultipleCards);
    }

    [Fact]
    public async Task SelectCard_SecilenKartiYukler()
    {
        _fx.Add(Card("Bonus", carried: 4000m));
        var world = _fx.Add(Card("World"));
        await ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = "World";

        await ViewModel.SelectCardCommand.ExecuteAsync(null);

        Assert.Equal(world.Id, ViewModel.CardId);
        Assert.Equal("World", ViewModel.CardName);
    }

    [Fact]
    public async Task SelectCard_Vazgecilirse_KartDegismez()
    {
        var bonus = _fx.Add(Card("Bonus", carried: 4000m));
        _fx.Add(Card("World"));
        await ViewModel.LoadAsync(null);
        _fx.Dialog.NextChooseResponse = null;

        await ViewModel.SelectCardCommand.ExecuteAsync(null);

        Assert.Equal(bonus.Id, ViewModel.CardId);
    }

    [Theory]
    [InlineData(4000)]
    [InlineData(0)]
    public async Task StartStatementEntry_TarihleriSiradakiVadedenDoldurur(decimal carried)
    {
        _fx.Add(Card("Bonus", carried: carried));
        await ViewModel.LoadAsync(null);

        ViewModel.StartStatementEntryCommand.Execute(null);

        Assert.True(ViewModel.Entry.IsOpen);
        Assert.False(ViewModel.IsPaymentView);
        Assert.False(ViewModel.IsNoPaymentView);
        Assert.Equal(StatementDate, ViewModel.Entry.StatementDate);
        Assert.Equal(DueDate, ViewModel.Entry.DueDate);
    }

    [Fact]
    public async Task EkstreKaydedilince_SiradakiOdemeGercegeDoner()
    {
        _fx.Add(Card("Bonus", carried: 4000m));
        await ViewModel.LoadAsync(null);
        ViewModel.StartStatementEntryCommand.Execute(null);
        ViewModel.Entry.AmountInput = "7.500";
        ViewModel.Entry.MinimumInput = "1.500";

        await ViewModel.Entry.SaveCommand.ExecuteAsync(null);

        Assert.False(ViewModel.Entry.IsOpen);
        Assert.True(ViewModel.IsPaymentView);
        Assert.True(ViewModel.HasStatement);
        Assert.False(ViewModel.NextPayment.IsEstimate);
        Assert.Equal(7500m, ViewModel.NextPayment.PaymentAmount);
        Assert.True(ViewModel.NextPayment.IsFullSelected);
    }

    [Fact]
    public async Task EkstreGirisindenVazgecilince_OncekiHaleDoner()
    {
        _fx.Add(Card("Bonus", carried: 4000m));
        await ViewModel.LoadAsync(null);
        ViewModel.StartStatementEntryCommand.Execute(null);

        ViewModel.Entry.CancelCommand.Execute(null);

        Assert.True(ViewModel.IsPaymentView);
    }
}
