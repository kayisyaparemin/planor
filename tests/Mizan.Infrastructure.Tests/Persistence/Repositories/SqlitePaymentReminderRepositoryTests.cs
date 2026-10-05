using Mizan.Application.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Entities;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqlitePaymentReminderRepositoryTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_reminders_{Guid.NewGuid():N}.db3");
    private SQLiteAsyncConnection _connection = null!;

    public async Task InitializeAsync()
    {
        SQLitePCL.Batteries_V2.Init();
        _connection = new SQLiteAsyncConnection(_databasePath);
        await new DatabaseSchema().EnsureInitializedAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* cleanup */ }
        }
    }

    [Fact]
    public async Task GetModeAsync_VarsayilanOffDoner()
    {
        var repository = new SqlitePaymentReminderRepository(_connection);

        var mode = await repository.GetModeAsync();

        Assert.Equal(PaymentReminderMode.Off, mode);
    }

    [Fact]
    public async Task SaveModeAsync_ModuGunceller_FinansalAyarRowunaZararVermez()
    {
        var repository = new SqlitePaymentReminderRepository(_connection);

        await repository.SaveModeAsync(PaymentReminderMode.Relaxed);
        var updatedMode = await repository.GetModeAsync();
        Assert.Equal(PaymentReminderMode.Relaxed, updatedMode);

        var settingsRow = await _connection.Table<SettingsEntity>().FirstAsync();
        Assert.Equal(10, settingsRow.PeriodAnchorDay);
        Assert.Equal((int)PaymentReminderMode.Relaxed, settingsRow.PaymentReminderMode);
    }

    [Fact]
    public async Task UpsertResponsesAsync_YanitlariKaydeder_SiraliGetirir()
    {
        var repository = new SqlitePaymentReminderRepository(_connection);
        var now = new DateTime(2026, 9, 25, 14, 30, 0);

        var response1 = new PaymentReminderResponse
        {
            DueKey = "due-loan-1",
            Name = "Konut Kredisi",
            DueDate = new DateOnly(2026, 10, 5),
            Amount = 14500m,
            Kind = PaymentReminderAnswerKind.Paid,
            AnsweredAt = now
        };

        var response2 = new PaymentReminderResponse
        {
            DueKey = "due-card-1",
            Name = "Bonus Kart",
            DueDate = new DateOnly(2026, 10, 1),
            Amount = 8000m,
            Kind = PaymentReminderAnswerKind.Snoozed,
            AnsweredAt = now,
            SnoozedUntil = now.AddHours(3)
        };

        await repository.UpsertResponsesAsync([response1, response2]);
        var responses = await repository.GetResponsesAsync();

        Assert.Equal(2, responses.Count);
        Assert.Equal("due-card-1", responses[0].DueKey);
        Assert.Equal(PaymentReminderAnswerKind.Snoozed, responses[0].Kind);
        Assert.NotNull(responses[0].SnoozedUntil);
        Assert.Equal("due-loan-1", responses[1].DueKey);
        Assert.Equal(PaymentReminderAnswerKind.Paid, responses[1].Kind);
    }

    [Fact]
    public async Task DeleteResponseAsync_YanitiSiler()
    {
        var repository = new SqlitePaymentReminderRepository(_connection);
        var now = new DateTime(2026, 9, 25, 10, 0, 0);

        await repository.UpsertResponsesAsync([
            new PaymentReminderResponse
            {
                DueKey = "due-to-delete",
                Name = "Fatura",
                DueDate = new DateOnly(2026, 9, 30),
                Amount = 500m,
                Kind = PaymentReminderAnswerKind.Paid,
                AnsweredAt = now
            }
        ]);

        await repository.DeleteResponseAsync("due-to-delete");
        var responses = await repository.GetResponsesAsync();

        Assert.Empty(responses);
    }
}
