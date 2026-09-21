using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class IncomeResolverTests
{
    private readonly IncomeResolver _sut = new();

    [Fact]
    public void Resolve_BirdenFazlaAktifAkisVarsa_HerIkiGeliriDeCozumler_BirincisiEzilmez()
    {
        // Arrange (S2 kuralı: birden fazla düzenli gelir birbirini ezmemeli)
        var stream1 = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 10 };
        var stream2 = new RecurringIncome { Id = Guid.NewGuid(), Name = "Kira", PaymentDay = 15 };

        var history = new[]
        {
            new IncomeAmountHistory
            {
                RecurringIncomeId = stream1.Id,
                Amount = 50_000m,
                EffectiveDate = new DateOnly(2026, 1, 1)
            },
            new IncomeAmountHistory
            {
                RecurringIncomeId = stream2.Id,
                Amount = 15_000m,
                EffectiveDate = new DateOnly(2026, 2, 1)
            }
        };

        // Act
        var result = _sut.Resolve(new DateOnly(2026, 2, 10), [stream1, stream2], history);

        // Assert
        Assert.Equal(2, result.Count);
        var res1 = Assert.Single(result, x => x.RecurringIncomeId == stream1.Id);
        var res2 = Assert.Single(result, x => x.RecurringIncomeId == stream2.Id);
        Assert.Equal(50_000m, res1.Amount);
        Assert.Equal(15_000m, res2.Amount);
    }

    [Fact]
    public void Resolve_PasifAkislari_SonucaDahilEtmez()
    {
        // Arrange
        var activeStream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Aktif Gelir", PaymentDay = 1, IsActive = true };
        var inactiveStream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Eski Gelir", PaymentDay = 1, IsActive = false };

        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = activeStream.Id, Amount = 40_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = inactiveStream.Id, Amount = 20_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
        };

        // Act
        var result = _sut.Resolve(new DateOnly(2026, 1, 1), [activeStream, inactiveStream], history);

        // Assert
        var single = Assert.Single(result);
        Assert.Equal(activeStream.Id, single.RecurringIncomeId);
        Assert.Equal(40_000m, single.Amount);
    }

    [Fact]
    public void Resolve_DonemOncesiVeDonemBasindakiZamlari_EnGuncelOlarakUygular()
    {
        // Arrange
        var stream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 10 };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = stream.Id, Amount = 100_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = stream.Id, Amount = 130_000m, EffectiveDate = new DateOnly(2026, 7, 1) }
        };

        // Act & Assert (Haziran'da eski tutar, Temmuz'da zamlı tutar)
        var resultHaziran = _sut.Resolve(new DateOnly(2026, 6, 10), [stream], history);
        var resultTemmuz = _sut.Resolve(new DateOnly(2026, 7, 1), [stream], history);

        Assert.Equal(100_000m, Assert.Single(resultHaziran).Amount);
        Assert.Equal(130_000m, Assert.Single(resultTemmuz).Amount);
    }

    [Fact]
    public void Resolve_DonemIciZamlari_Uygulamaz_OncekiGecerliTutariKorur()
    {
        // Arrange (BR-INCOME-01: 10 Ocak döneminde 15 Ocak zammı devreye girmez)
        var stream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 10 };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = stream.Id, Amount = 80_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { RecurringIncomeId = stream.Id, Amount = 110_000m, EffectiveDate = new DateOnly(2026, 1, 15) }
        };

        // Act
        var result = _sut.Resolve(new DateOnly(2026, 1, 10), [stream], history);

        // Assert
        var resolved = Assert.Single(result);
        Assert.Equal(80_000m, resolved.Amount);
        Assert.Equal(new DateOnly(2026, 1, 1), resolved.EffectiveDate);
    }

    [Fact]
    public void Resolve_AkisaAitGecerliTarihceYoksa_AkisiSonucaDahilEtmez()
    {
        // Arrange (Henüz tarihi gelmemiş gelecek tarihli gelir akışı)
        var stream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Yeni İş", PaymentDay = 1 };
        var history = new[]
        {
            new IncomeAmountHistory { RecurringIncomeId = stream.Id, Amount = 90_000m, EffectiveDate = new DateOnly(2026, 6, 1) }
        };

        // Act
        var result = _sut.Resolve(new DateOnly(2026, 1, 1), [stream], history);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Resolve_AyniEtkinTarihteIkiRevizyonVarsa_IdyeGoreDeterministikCozumler()
    {
        // Arrange
        var stream = new RecurringIncome { Id = Guid.NewGuid(), Name = "Maaş", PaymentDay = 1 };
        var id1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var history = new[]
        {
            new IncomeAmountHistory { Id = id1, RecurringIncomeId = stream.Id, Amount = 50_000m, EffectiveDate = new DateOnly(2026, 1, 1) },
            new IncomeAmountHistory { Id = id2, RecurringIncomeId = stream.Id, Amount = 55_000m, EffectiveDate = new DateOnly(2026, 1, 1) }
        };

        // Act
        var result = _sut.Resolve(new DateOnly(2026, 1, 1), [stream], history);

        // Assert (büyük ID kazanır)
        var resolved = Assert.Single(result);
        Assert.Equal(55_000m, resolved.Amount);
    }

    [Fact]
    public void Resolve_BosGirdiVerildiginde_BosListeDöner()
    {
        var result = _sut.Resolve(new DateOnly(2026, 1, 1), [], []);
        Assert.Empty(result);
    }

    [Fact]
    public void Resolve_NullParametrelerVerildiginde_HataFirlatir()
    {
        var asOf = new DateOnly(2026, 1, 1);
        Assert.Throws<ArgumentNullException>(() => _sut.Resolve(asOf, null!, []));
        Assert.Throws<ArgumentNullException>(() => _sut.Resolve(asOf, [], null!));
    }
}
