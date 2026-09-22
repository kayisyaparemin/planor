using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class CreditCardDateResolverTests
{
    private readonly CreditCardDateResolver _resolver = new();

    [Fact]
    public void ResolveStatementCloseOnOrAfter_TarihKesimdenOnceyse_AyniAydakiKesimGununuUretir()
    {
        var result = _resolver.ResolveStatementCloseOnOrAfter(new DateOnly(2026, 8, 10), 25);

        Assert.Equal(new DateOnly(2026, 8, 25), result);
    }

    [Fact]
    public void ResolveStatementCloseOnOrAfter_TarihKesimdenSonraysa_SonrakiAydakiKesimGununuUretir()
    {
        var result = _resolver.ResolveStatementCloseOnOrAfter(new DateOnly(2026, 8, 26), 25);

        Assert.Equal(new DateOnly(2026, 9, 25), result);
    }

    [Fact]
    public void ResolveStatementCloseOnOrAfter_SubatAySonuKenetlenmesi_28SubataKenetlenir()
    {
        var result = _resolver.ResolveStatementCloseOnOrAfter(new DateOnly(2027, 2, 1), 31);

        Assert.Equal(new DateOnly(2027, 2, 28), result);
    }

    [Fact]
    public void ResolvePaymentDueDate_KesimTarihindenSonrakiGunu_DogruAydaBelirler()
    {
        // 25 Eylül kesim, 5 vade -> 5 Ekim vade
        var result = _resolver.ResolvePaymentDueDate(new DateOnly(2026, 9, 25), 5);

        Assert.Equal(new DateOnly(2026, 10, 5), result);
    }

    [Fact]
    public void ResolveChargeStatementClose_KesimdenOncekiHarcama_IlkEkstreyeDuser()
    {
        var result = _resolver.ResolveChargeStatementClose(
            new DateOnly(2026, 9, 24),
            firstProjectionClose: new DateOnly(2026, 9, 25),
            statementClosingDay: 25);

        Assert.Equal(new DateOnly(2026, 9, 25), result);
    }

    [Fact]
    public void ResolveChargeStatementClose_KesimdenSonrakiHarcama_SonrakiEkstreyeDuser()
    {
        var result = _resolver.ResolveChargeStatementClose(
            new DateOnly(2026, 9, 26),
            firstProjectionClose: new DateOnly(2026, 9, 25),
            statementClosingDay: 25);

        Assert.Equal(new DateOnly(2026, 10, 25), result);
    }

    [Fact]
    public void ResolveChargeStatementClose_KesilmisEkstreVeNextDateVarsa_OrayaAtar()
    {
        var card = new CreditCard
        {
            BalanceAsOfDate = new DateOnly(2026, 8, 20),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 50_000m,
                MinimumPaymentAmount = 20_000m,
                NextStatementDate = new DateOnly(2026, 9, 28),
                NextDueDate = new DateOnly(2026, 10, 8)
            }
        };

        // 20 Eylül harcaması: 28 Ağustos'tan sonra ve 28 Eylül'den önce -> 28 Eylül'e düşer
        var result = _resolver.ResolveChargeStatementClose(
            card,
            new DateOnly(2026, 9, 20),
            firstProjectionClose: new DateOnly(2026, 8, 28));

        Assert.Equal(new DateOnly(2026, 9, 28), result);
    }

    [Fact]
    public void ResolveChargeStatementClose_SettlementSonrasiKnownNextStatementDateVarsa_GenelGuneKaydirmaz()
    {
        // I11 kuralı: CurrentStatement olmasa bile bankanın bildirdiği KnownNextStatementDate korunur
        var card = new CreditCard
        {
            BalanceAsOfDate = new DateOnly(2026, 8, 29),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            KnownNextStatementDate = new DateOnly(2026, 9, 28),
            KnownNextDueDate = new DateOnly(2026, 10, 8)
        };

        var result = _resolver.ResolveChargeStatementClose(
            card,
            new DateOnly(2026, 9, 27),
            firstProjectionClose: new DateOnly(2026, 9, 28));

        Assert.Equal(new DateOnly(2026, 9, 28), result);
    }

    [Fact]
    public void ResolvePaymentDueDate_KesilmisEkstreIcin_EkstreVadesiniDondurur()
    {
        var card = new CreditCard
        {
            BalanceAsOfDate = new DateOnly(2026, 8, 20),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 50_000m,
                MinimumPaymentAmount = 20_000m
            }
        };

        var result = _resolver.ResolvePaymentDueDate(card, new DateOnly(2026, 8, 28), isCurrentActualStatement: true);

        Assert.Equal(new DateOnly(2026, 9, 7), result);
    }

    [Fact]
    public void ResolveNextStatementCloseDate_KesilmisEkstredeNextDateVarsa_OnuKullanir()
    {
        var card = new CreditCard
        {
            BalanceAsOfDate = new DateOnly(2026, 8, 20),
            StatementClosingDay = 25,
            PaymentDueDay = 5,
            CurrentStatement = new CreditCardStatement
            {
                StatementDate = new DateOnly(2026, 8, 28),
                DueDate = new DateOnly(2026, 9, 7),
                StatementAmount = 50_000m,
                MinimumPaymentAmount = 20_000m,
                NextStatementDate = new DateOnly(2026, 9, 28)
            }
        };

        var result = _resolver.ResolveNextStatementCloseDate(card, new DateOnly(2026, 8, 28), wasCurrentActualStatement: true);

        Assert.Equal(new DateOnly(2026, 9, 28), result);
    }
}
