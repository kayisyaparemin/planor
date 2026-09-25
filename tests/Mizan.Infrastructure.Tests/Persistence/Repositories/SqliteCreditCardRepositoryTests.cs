using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Entities;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteCreditCardRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteCreditCardRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_cards_{Guid.NewGuid():N}.db3");
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        new DatabaseSchema().EnsureInitializedAsync(_connection).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection.CloseAsync().GetAwaiter().GetResult();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetCreditCardsAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqliteCreditCardRepository(_connection);

        var list = await repository.GetCreditCardsAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task UpsertCreditCardAsync_TumBilesenleriyleKartiKaydederVeGeriOkur()
    {
        var repository = new SqliteCreditCardRepository(_connection);
        var cardId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var statementDate = new DateOnly(2026, 9, 20);
        var dueDate = new DateOnly(2026, 10, 1);

        var card = new CreditCard
        {
            Id = cardId,
            Name = "Axess Platinum",
            Bank = "Akbank",
            Limit = 100000m,
            CarriedBalance = 5000m,
            UnbilledSpending = 12000m,
            BalanceAsOfDate = new DateOnly(2026, 9, 25),
            StatementClosingDay = 20,
            PaymentDueDay = 1,
            MinimumPaymentRate = 0.40m,
            PaymentStrategy = CreditCardPaymentStrategy.Minimum,
            FixedPaymentAmount = null,
            ProjectionFallbackStrategy = ProjectionFallbackStrategy.None,
            ProjectionFallbackFixedAmount = null,
            KnownNextStatementDate = new DateOnly(2026, 10, 20),
            KnownNextDueDate = new DateOnly(2026, 11, 1),
            IsActive = true,
            CurrentStatement = new CreditCardStatement
            {
                Id = statementId,
                CreditCardId = cardId,
                StatementDate = statementDate,
                DueDate = dueDate,
                StatementAmount = 24500m,
                MinimumPaymentAmount = 9800m,
                NextStatementDate = new DateOnly(2026, 10, 20),
                NextDueDate = new DateOnly(2026, 11, 1),
                CreatedAt = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero)
            },
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan
            {
                Mode = CurrentStatementPaymentMode.Minimum,
                CustomAmount = null
            },
            Charges =
            [
                new CardCharge
                {
                    Id = Guid.NewGuid(),
                    CreditCardId = cardId,
                    Description = "Elektronik 2/3",
                    PostingDate = new DateOnly(2026, 10, 20),
                    Amount = 4000m
                }
            ],
            PaymentPlans =
            [
                new CreditCardPaymentPlan
                {
                    Id = Guid.NewGuid(),
                    CreditCardId = cardId,
                    DueDate = new DateOnly(2026, 10, 1),
                    PaymentType = CreditCardPaymentType.Minimum,
                    Amount = null
                }
            ],
            PaymentPreferences =
            [
                new CreditCardPaymentPreference
                {
                    Id = Guid.NewGuid(),
                    CreditCardId = cardId,
                    Mode = CurrentStatementPaymentMode.Minimum,
                    CustomAmount = null,
                    EffectiveFromStatementDate = statementDate,
                    CreatedAt = new DateTimeOffset(2026, 9, 20, 12, 0, 0, TimeSpan.Zero),
                    Note = "Asgari ödenecek"
                }
            ]
        };

        await repository.UpsertCreditCardAsync(card);

        var cards = await repository.GetCreditCardsAsync();
        Assert.Single(cards);
        var read = cards[0];
        Assert.Equal(cardId, read.Id);
        Assert.Equal("Axess Platinum", read.Name);
        Assert.Equal("Akbank", read.Bank);
        Assert.Equal(100000m, read.Limit);
        Assert.Equal(5000m, read.CarriedBalance);
        Assert.Equal(12000m, read.UnbilledSpending);
        Assert.Equal(new DateOnly(2026, 9, 25), read.BalanceAsOfDate);
        Assert.Equal(20, read.StatementClosingDay);
        Assert.Equal(1, read.PaymentDueDay);
        Assert.Equal(0.40m, read.MinimumPaymentRate);
        Assert.Equal(new DateOnly(2026, 10, 20), read.KnownNextStatementDate);
        Assert.Equal(new DateOnly(2026, 11, 1), read.KnownNextDueDate);
        Assert.True(read.IsActive);

        // Alt ilişkileri doğrula
        Assert.NotNull(read.CurrentStatement);
        Assert.Equal(24500m, read.CurrentStatement.StatementAmount);
        Assert.Equal(9800m, read.CurrentStatement.MinimumPaymentAmount);
        Assert.NotNull(read.CurrentStatementPaymentPlan);
        Assert.Equal(CurrentStatementPaymentMode.Minimum, read.CurrentStatementPaymentPlan.Mode);

        Assert.Single(read.Charges);
        Assert.Equal("Elektronik 2/3", read.Charges[0].Description);
        Assert.Equal(4000m, read.Charges[0].Amount);

        Assert.Single(read.PaymentPlans);
        Assert.Equal(new DateOnly(2026, 10, 1), read.PaymentPlans[0].DueDate);

        Assert.Single(read.PaymentPreferences);
        Assert.Equal(CurrentStatementPaymentMode.Minimum, read.PaymentPreferences[0].Mode);
    }

    [Fact]
    public async Task DeleteCreditCardAsync_KartiSiler_Ve_CascadeIleTumAltTablolariTemizler()
    {
        var repository = new SqliteCreditCardRepository(_connection);
        var cardId = Guid.NewGuid();
        var card = new CreditCard
        {
            Id = cardId,
            Name = "Bonus",
            Bank = "Garanti",
            Limit = 50000m,
            Charges =
            [
                new CardCharge { Id = Guid.NewGuid(), CreditCardId = cardId, Description = "Market", PostingDate = new DateOnly(2026, 10, 1), Amount = 1500m }
            ],
            PaymentPlans =
            [
                new CreditCardPaymentPlan { Id = Guid.NewGuid(), CreditCardId = cardId, DueDate = new DateOnly(2026, 10, 5), PaymentType = CreditCardPaymentType.FullStatement }
            ],
            PaymentPreferences =
            [
                new CreditCardPaymentPreference { Id = Guid.NewGuid(), CreditCardId = cardId, Mode = CurrentStatementPaymentMode.Full, EffectiveFromStatementDate = new DateOnly(2026, 9, 25), CreatedAt = DateTimeOffset.UtcNow }
            ]
        };
        await repository.UpsertCreditCardAsync(card);

        await repository.DeleteCreditCardAsync(cardId);

        var cards = await repository.GetCreditCardsAsync();
        Assert.Empty(cards);

        // ON DELETE CASCADE ile temizlenen alt tabloları denetle
        var chargesInDb = await _connection.Table<CardInstallmentEntity>().ToListAsync();
        var plansInDb = await _connection.Table<CreditCardPaymentPlanEntity>().ToListAsync();
        var prefsInDb = await _connection.Table<CreditCardPaymentPreferenceEntity>().ToListAsync();

        Assert.Empty(chargesInDb);
        Assert.Empty(plansInDb);
        Assert.Empty(prefsInDb);
    }
}
