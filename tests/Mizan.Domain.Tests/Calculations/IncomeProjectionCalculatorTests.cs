using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class IncomeProjectionCalculatorTests
{
    private readonly IncomeResolver _resolver = new();
    private readonly IncomeProjectionCalculator _calculator;

    public IncomeProjectionCalculatorTests()
    {
        _calculator = new IncomeProjectionCalculator(_resolver);
    }

    [Fact]
    public void Yapici_ResolverNullGecilirse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => new IncomeProjectionCalculator(null!));
    }

    [Fact]
    public void Calculate_ZorunluParametrelerNullGecilirse_ArgumentNullExceptionFirlatir()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));

        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(null!, [], [], []));
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(period, (IEnumerable<RecurringIncome>)null!, [], []));
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(period, [], (IEnumerable<IncomeAmountHistory>)null!, []));
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(period, [], [], (IEnumerable<AdHocIncome>)null!));
        Assert.Throws<ArgumentNullException>(() => _calculator.Calculate(period, (IEnumerable<ActiveRecurringIncome>)null!, []));
    }

    [Fact]
    public void Calculate_HicGelirYoksa_BosListeVeSifirToplamDondurur()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));

        var result = _calculator.Calculate(period, [], [], []);

        Assert.Empty(result.Items);
        Assert.Equal(0m, result.RecurringTotal);
        Assert.Equal(0m, result.AdHocTotal);
        Assert.Equal(0m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_TekDuzenliGelirVarsa_GuncelTutariVeOdemeGunununTarihiniCozumler()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));
        var streamId = Guid.NewGuid();
        var streams = new[]
        {
            new RecurringIncome { Id = streamId, Name = "Maaş", PaymentDay = 15, IsActive = true }
        };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 45_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Single(result.Items);
        var item = result.Items[0];
        Assert.Equal("Maaş", item.Name);
        Assert.Equal(IncomeSourceType.Recurring, item.Type);
        Assert.Equal(new DateOnly(2026, 9, 15), item.SourceDate);
        Assert.Equal(45_000m, item.Amount);
        Assert.Equal(streamId, item.RecurringIncomeId);
        Assert.Equal(45_000m, result.RecurringTotal);
        Assert.Equal(0m, result.AdHocTotal);
        Assert.Equal(45_000m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_BirdenFazlaAktifDuzenliGelirVarsa_HerIkiGeliriToplar_BirincisiEzilmez()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));
        var stream1Id = Guid.NewGuid();
        var stream2Id = Guid.NewGuid();

        var streams = new[]
        {
            new RecurringIncome { Id = stream1Id, Name = "Birincil Gelir", PaymentDay = 15, IsActive = true },
            new RecurringIncome { Id = stream2Id, Name = "Kira Geliri", PaymentDay = 12, IsActive = true }
        };

        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = stream1Id, Amount = 60_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = stream2Id, Amount = 20_000m, EffectiveDate = new DateOnly(2026, 5, 1) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(80_000m, result.RecurringTotal);
        Assert.Equal(0m, result.AdHocTotal);
        Assert.Equal(80_000m, result.TotalIncome);

        // Tarihe göre sıralanmış olmalı: Ayın 12'si önce, 15'i sonra
        Assert.Equal(new DateOnly(2026, 9, 12), result.Items[0].SourceDate);
        Assert.Equal(20_000m, result.Items[0].Amount);
        Assert.Equal(new DateOnly(2026, 9, 15), result.Items[1].SourceDate);
        Assert.Equal(60_000m, result.Items[1].Amount);
    }

    [Fact]
    public void Calculate_ZamYatistanSonraYururlugeGirerse_OYatisEskiTutarla()
    {
        // Dönem [10 Aralık, 10 Ocak), yatış 15 Aralık; zam 1 Ocak'ta, yani yatıştan sonra.
        var period = new CashFlowPeriod(new DateOnly(2026, 12, 10), new DateOnly(2027, 1, 10));
        var streamId = Guid.NewGuid();
        var streams = new[]
        {
            new RecurringIncome { Id = streamId, Name = "Gelir", PaymentDay = 15, IsActive = true }
        };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 50_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 75_000m, EffectiveDate = new DateOnly(2027, 1, 1) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Single(result.Items);
        Assert.Equal(50_000m, result.Items[0].Amount);
        Assert.Equal(50_000m, result.RecurringTotal);
    }

    [Fact]
    public void Calculate_ZamDonemIcindeYatistanOnceYururlugeGirerse_OYatisYeniTutarla()
    {
        // S67-4: çapa 1, gelir günü 15; "15 Ocak'tan itibaren 60.000" 15 Ocak yatışına uygulanır.
        var period = new CashFlowPeriod(new DateOnly(2027, 1, 1), new DateOnly(2027, 2, 1));
        var streamId = Guid.NewGuid();
        var streams = new[] { new RecurringIncome { Id = streamId, Name = "Gelir", PaymentDay = 15 } };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 50_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 60_000m, EffectiveDate = new DateOnly(2027, 1, 15) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        var item = Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2027, 1, 15), item.SourceDate);
        Assert.Equal(60_000m, item.Amount);
    }

    [Fact]
    public void Calculate_DonemIcindeEklenenGelirYatisGunuSonraysa_BuDonemeGirer()
    {
        // S67-4: çapa 1; ayın 12'sinde eklenen "her ayın 20'si 10.000" bu dönemin 20'sinde yatar.
        var period = new CashFlowPeriod(new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1));
        var streamId = Guid.NewGuid();
        var streams = new[] { new RecurringIncome { Id = streamId, Name = "Kira", PaymentDay = 20 } };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 10_000m, EffectiveDate = new DateOnly(2026, 10, 12) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        var item = Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2026, 10, 20), item.SourceDate);
        Assert.Equal(10_000m, result.RecurringTotal);
    }

    [Fact]
    public void Calculate_DonemIcindeEklenenGelirYatisGunuGecmisse_BuDonemeGirmez()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1));
        var streamId = Guid.NewGuid();
        var streams = new[] { new RecurringIncome { Id = streamId, Name = "Kira", PaymentDay = 5 } };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 10_000m, EffectiveDate = new DateOnly(2026, 10, 12) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Empty(result.Items);
        Assert.Equal(0m, result.RecurringTotal);
    }

    [Fact]
    public void Calculate_DuzenliGelirOdemeGunu_DonemIcineDenkGelenGuneYerlestirilir()
    {
        // Dönem: [10 Eylül 2026, 10 Ekim 2026)
        // Gelirin ödeme günü ayın 5'i. Bu dönem içine 5 Ekim 2026 düşmeli.
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));
        var streamId = Guid.NewGuid();
        var streams = new[]
        {
            new RecurringIncome { Id = streamId, Name = "Kira", PaymentDay = 5, IsActive = true }
        };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 15_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2026, 10, 5), result.Items[0].SourceDate);
        Assert.True(period.Contains(result.Items[0].SourceDate));
    }

    [Fact]
    public void Calculate_DuzenliGelirOdemeGunu31Ise_KisaAylardaAySonunaKenetlenir()
    {
        // Dönem: [10 Şubat 2027, 10 Mart 2027)
        // Gelir ödeme günü 31. Şubat ayı 28 çeker, bu dönemde ödeme tarihi 28 Şubat 2027 olmalı.
        var period = new CashFlowPeriod(new DateOnly(2027, 2, 10), new DateOnly(2027, 3, 10));
        var streamId = Guid.NewGuid();
        var streams = new[]
        {
            new RecurringIncome { Id = streamId, Name = "Hakediş", PaymentDay = 31, IsActive = true }
        };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = streamId, Amount = 30_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
        };

        var result = _calculator.Calculate(period, streams, history, []);

        Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2027, 2, 28), result.Items[0].SourceDate);
        Assert.True(period.Contains(result.Items[0].SourceDate));
    }

    [Fact]
    public void Calculate_TekSeferlikGelirler_DonemIcineDusenleriEklerVeTariheGoreSiralar()
    {
        var period = new CashFlowPeriod(new DateOnly(2027, 3, 10), new DateOnly(2027, 4, 10));
        var adHoc1Id = Guid.NewGuid();
        var adHoc2Id = Guid.NewGuid();
        var adHocIncomes = new[]
        {
            new AdHocIncome { Id = adHoc1Id, Amount = 25_000m, ExactDate = new DateOnly(2027, 3, 15), Description = "Prim" },
            new AdHocIncome { Id = adHoc2Id, Amount = 50_000m, ExactDate = new DateOnly(2027, 4, 10), Description = "Sonraki Dönem Geliri" } // Dışarıda (End dahil değil)
        };

        var result = _calculator.Calculate(period, [], [], adHocIncomes);

        Assert.Single(result.Items);
        var item = result.Items[0];
        Assert.Equal(adHoc1Id, item.AdHocIncomeId);
        Assert.Equal("Prim", item.Name);
        Assert.Equal(IncomeSourceType.AdHoc, item.Type);
        Assert.Equal(new DateOnly(2027, 3, 15), item.SourceDate);
        Assert.Equal(25_000m, item.Amount);
        Assert.Equal(0m, result.RecurringTotal);
        Assert.Equal(25_000m, result.AdHocTotal);
        Assert.Equal(25_000m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_CapaOncesiPencereVerildiginde_PencereyeDusenTekSeferlikGelirleriDahilEder()
    {
        // Dönem: [10 Mart 2027, 10 Nisan 2027)
        // Çapa öncesi pencere başlangıcı: 5 Mart 2027
        // Gelir tarihi: 6 Mart 2027 -> [5 Mart, 10 Mart) penceresine düşüyor, ilk döneme yazılmalı!
        var period = new CashFlowPeriod(new DateOnly(2027, 3, 10), new DateOnly(2027, 4, 10));
        var adHocIncomes = new[]
        {
            new AdHocIncome { Amount = 12_000m, ExactDate = new DateOnly(2027, 3, 6), Description = "Ufuk Öncesi İade" }
        };

        var result = _calculator.Calculate(
            period,
            [],
            [],
            adHocIncomes,
            prePeriodIncomeStart: new DateOnly(2027, 3, 5));

        Assert.Single(result.Items);
        Assert.Equal(12_000m, result.AdHocTotal);
        Assert.Equal(12_000m, result.TotalIncome);
        Assert.Equal("Ufuk Öncesi İade", result.Items[0].Name);
        Assert.Equal(new DateOnly(2027, 3, 6), result.Items[0].SourceDate);
    }

    [Fact]
    public void Calculate_CapaOncesiPencereOncesindekiGelirler_ProjeksiyonaGiremez()
    {
        // Çapa penceresi 5 Mart'ta başlıyor, gelir 4 Mart'ta gerçekleşmiş (zaten açılış bakiyesinde).
        var period = new CashFlowPeriod(new DateOnly(2027, 3, 10), new DateOnly(2027, 4, 10));
        var adHocIncomes = new[]
        {
            new AdHocIncome { Amount = 12_000m, ExactDate = new DateOnly(2027, 3, 4), Description = "Eski Gelir" }
        };

        var result = _calculator.Calculate(
            period,
            [],
            [],
            adHocIncomes,
            prePeriodIncomeStart: new DateOnly(2027, 3, 5));

        Assert.Empty(result.Items);
        Assert.Equal(0m, result.AdHocTotal);
        Assert.Equal(0m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_CapaOncesiPencereVerilmediginde_DonemOncesiGelirlerHaricTutulur()
    {
        var period = new CashFlowPeriod(new DateOnly(2027, 3, 10), new DateOnly(2027, 4, 10));
        var adHocIncomes = new[]
        {
            new AdHocIncome { Amount = 12_000m, ExactDate = new DateOnly(2027, 3, 6), Description = "Penceresiz Gelir" }
        };

        var result = _calculator.Calculate(period, [], [], adHocIncomes, prePeriodIncomeStart: null);

        Assert.Empty(result.Items);
        Assert.Equal(0m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_OncedenCozumlenmisGelirlerGecildiginde_DogrudanKullanirVeToplar()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));
        var resolved = new[]
        {
            new ActiveRecurringIncome
            {
                RecurringIncomeId = Guid.NewGuid(),
                Name = "Hazır Gelir",
                PaymentDay = 15,
                Amount = 40_000m,
                EffectiveDate = new DateOnly(2026, 1, 1)
            }
        };

        var adHoc = new[]
        {
            new AdHocIncome { Amount = 10_000m, ExactDate = new DateOnly(2026, 9, 20), Description = "Ek Gelir" }
        };

        var result = _calculator.Calculate(period, resolved, adHoc);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(40_000m, result.RecurringTotal);
        Assert.Equal(10_000m, result.AdHocTotal);
        Assert.Equal(50_000m, result.TotalIncome);
    }

    [Fact]
    public void Calculate_KurusluTutarlari_KurusHassasiyetiyleYuvarlar()
    {
        var period = new CashFlowPeriod(new DateOnly(2026, 9, 10), new DateOnly(2026, 10, 10));
        var resolved = new[]
        {
            new ActiveRecurringIncome
            {
                RecurringIncomeId = Guid.NewGuid(),
                Name = "Kuruşlu Gelir",
                PaymentDay = 12,
                Amount = 33_333.333m,
                EffectiveDate = new DateOnly(2026, 1, 1)
            }
        };

        var adHoc = new[]
        {
            new AdHocIncome { Amount = 66_666.666m, ExactDate = new DateOnly(2026, 9, 18), Description = "Kuruşlu Arızi" }
        };

        var result = _calculator.Calculate(period, resolved, adHoc);

        Assert.Equal(33_333.33m, result.RecurringTotal);
        Assert.Equal(66_666.67m, result.AdHocTotal);
        Assert.Equal(100_000.00m, result.TotalIncome);
    }
}
