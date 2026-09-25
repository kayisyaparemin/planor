using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Entities;
using Mizan.Infrastructure.Persistence.Repositories;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence.Repositories;

public sealed class SqliteTemporaryPaymentPlanRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly SQLiteAsyncConnection _connection;

    public SqliteTemporaryPaymentPlanRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_plans_{Guid.NewGuid():N}.db3");
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
    public async Task GetPaymentPlansAsync_Bosken_BosDiziDondurur()
    {
        var repository = new SqliteTemporaryPaymentPlanRepository(_connection);

        var list = await repository.GetPaymentPlansAsync();

        Assert.Empty(list);
    }

    [Fact]
    public async Task UpsertPaymentPlanAsync_PlanVeTaksitleriEklerVeGeriOkur()
    {
        var repository = new SqliteTemporaryPaymentPlanRepository(_connection);
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Mobilya Taksiti",
            Kind = PaymentPlanKind.Installment,
            OriginalAmount = 18000m,
            TotalRepaymentAmount = 18000m,
            Installments =
            [
                new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 10, 10), Amount = 6000m, IsPaid = false },
                new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 11, 10), Amount = 6000m, IsPaid = false },
                new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 12, 10), Amount = 6000m, IsPaid = false }
            ]
        };

        await repository.UpsertPaymentPlanAsync(plan);

        var plans = await repository.GetPaymentPlansAsync();
        Assert.Single(plans);
        var read = plans[0];
        Assert.Equal(planId, read.Id);
        Assert.Equal("Mobilya Taksiti", read.Name);
        Assert.Equal(PaymentPlanKind.Installment, read.Kind);
        Assert.Equal(18000m, read.OriginalAmount);
        Assert.Equal(3, read.Installments.Count);
        Assert.Equal(18000m, read.TotalInstallmentAmount);
    }

    [Fact]
    public async Task DeletePaymentPlanAsync_PlaniVeCascadeIleTaksitleriniSiler()
    {
        var repository = new SqliteTemporaryPaymentPlanRepository(_connection);
        var planId = Guid.NewGuid();
        var plan = new TemporaryPaymentPlan
        {
            Id = planId,
            Name = "Elden Borç",
            Kind = PaymentPlanKind.Temporary,
            Installments =
            [
                new TemporaryPaymentInstallment { Id = Guid.NewGuid(), PlanId = planId, DueDate = new DateOnly(2026, 10, 1), Amount = 5000m, IsPaid = true }
            ]
        };
        await repository.UpsertPaymentPlanAsync(plan);

        await repository.DeletePaymentPlanAsync(planId);

        var plans = await repository.GetPaymentPlansAsync();
        Assert.Empty(plans);

        var installmentsInDb = await _connection.Table<PaymentInstallmentEntity>().ToListAsync();
        Assert.Empty(installmentsInDb); // ON DELETE CASCADE doğrulaması
    }
}
