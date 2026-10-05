using Mizan.Infrastructure.Persistence;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

public sealed class DatabaseConstraintTests : IDisposable
{
    private readonly string _databasePath;
    private readonly DatabaseSchema _schema;

    public DatabaseConstraintTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_constraints_{Guid.NewGuid():N}.db3");
        SQLitePCL.Batteries_V2.Init();
        _schema = new DatabaseSchema();
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); } catch { /* test cleanup */ }
        }
    }

    private async Task<SQLiteAsyncConnection> CreateInitializedConnectionAsync()
    {
        var factory = new SqliteConnectionFactory(_schema);
        return await factory.CreateConnectionAsync(_databasePath);
    }

    [Fact]
    public async Task YabanciAnahtar_GecersizUstKayit_HataFirlatir()
    {
        var connection = await CreateInitializedConnectionAsync();
        var invalidInsertSql = @"
            INSERT INTO income_amount_histories (Id, RecurringIncomeId, Amount, EffectiveDate, Description)
            VALUES ('history-1', 'non-existent-income', 50000.0, '2026-01-01', 'Test');";

        await Assert.ThrowsAsync<SQLiteException>(() => connection.ExecuteAsync(invalidInsertSql));
        await connection.CloseAsync();
    }

    [Fact]
    public async Task YabanciAnahtar_KaskadSilme_GelirVeGecmisiniSiler()
    {
        var connection = await CreateInitializedConnectionAsync();
        await connection.ExecuteAsync(@"
            INSERT INTO recurring_incomes (Id, Name, PaymentDay, IsActive)
            VALUES ('income-1', 'Gelir', 15, 1);");
        await connection.ExecuteAsync(@"
            INSERT INTO income_amount_histories (Id, RecurringIncomeId, Amount, EffectiveDate, Description)
            VALUES ('history-1', 'income-1', 90000.0, '2025-06-01', 'Eski zam');");

        var countBefore = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM income_amount_histories WHERE RecurringIncomeId = 'income-1';");
        Assert.Equal(1, countBefore);

        await connection.ExecuteAsync("DELETE FROM recurring_incomes WHERE Id = 'income-1';");

        var countAfter = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM income_amount_histories WHERE RecurringIncomeId = 'income-1';");
        await connection.CloseAsync();

        Assert.Equal(0, countAfter);
    }

    [Fact]
    public async Task YabanciAnahtar_KaskadSilme_KrediVeErkenOdemeyiSiler()
    {
        var connection = await CreateInitializedConnectionAsync();
        await connection.ExecuteAsync(@"
            INSERT INTO loans (Id, Name, Bank, MonthlyPayment, PaymentDay, NextPaymentDate, RemainingInstallmentCount, Kind, IsActive)
            VALUES ('loan-1', 'Konut', 'Garanti', 50000.0, 15, '2026-02-15', 10, 0, 1);");
        await connection.ExecuteAsync(@"
            INSERT INTO loan_prepayments (Id, LoanId, Date, Mode, PrincipalAmount)
            VALUES ('prep-1', 'loan-1', '2026-02-01', 0, 100000.0);");

        await connection.ExecuteAsync("DELETE FROM loans WHERE Id = 'loan-1';");

        var countAfter = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM loan_prepayments WHERE LoanId = 'loan-1';");
        await connection.CloseAsync();

        Assert.Equal(0, countAfter);
    }

    [Fact]
    public async Task YabanciAnahtar_KaskadSilme_PlanSilindigindeSatirlariSiler()
    {
        var connection = await CreateInitializedConnectionAsync();
        await InsertSampleFinancialSnapshotAsync(connection, "fs-1");
        await InsertSamplePeriodPlanAsync(connection, "plan-1", "fs-1");

        await connection.ExecuteAsync(@"
            INSERT INTO period_plan_income_lines (Id, PeriodPlanSnapshotId, SourceType, Name, PlannedDate, PlannedAmount)
            VALUES ('inc-line-1', 'plan-1', 0, 'Gelir', '2026-01-15', 80000.0);");
        await connection.ExecuteAsync(@"
            INSERT INTO period_plan_payment_lines (Id, PeriodPlanSnapshotId, SourceEntityId, SourceType, Name, PlannedDate, IsEstimate, Detail)
            VALUES ('pay-line-1', 'plan-1', 'loan-1', 0, 'Kredi', '2026-01-15', 0, '');");

        await connection.ExecuteAsync("DELETE FROM period_plan_snapshots WHERE Id = 'plan-1';");

        var incomeLines = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM period_plan_income_lines WHERE PeriodPlanSnapshotId = 'plan-1';");
        var paymentLines = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM period_plan_payment_lines WHERE PeriodPlanSnapshotId = 'plan-1';");
        await connection.CloseAsync();

        Assert.Equal(0, incomeLines);
        Assert.Equal(0, paymentLines);
    }

    [Fact]
    public async Task TekilKisit_AyniSnapshotIcinIkinciActualKaydi_Engellenir()
    {
        var connection = await CreateInitializedConnectionAsync();
        await InsertSampleFinancialSnapshotAsync(connection, "fs-1");
        await InsertSamplePeriodPlanAsync(connection, "plan-uniq-1", "fs-1");

        var actualInsertSql1 = @"
            INSERT INTO period_actuals (Id, PeriodPlanSnapshotId, SourceFinancialSnapshotId, ResultFinancialSnapshotId, PeriodStart, PeriodEnd, FinalizedAtUtc, ActualIncome, ActualLoanPayments, ActualCardPayments, ActualTemporaryPayments, ActualInstallmentPayments, ActualOtherScheduledPayments, ActualLargeExpenses, ActualMandatoryPayments, ActualLivingSpend, ActualInterest, UnplannedIncome, UnplannedPayments, DerivedEndingBalance, ConfirmedEndingBalance, ReconciliationAdjustment, ComparisonSummary, Note)
            VALUES ('act-1', 'plan-uniq-1', 'fs-1', 'fs-1', '2026-01-15', '2026-02-14', '2026-02-15T00:00:00Z', 100.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100.0, 100.0, 0.0, '', '');";
        var actualInsertSql2 = @"
            INSERT INTO period_actuals (Id, PeriodPlanSnapshotId, SourceFinancialSnapshotId, ResultFinancialSnapshotId, PeriodStart, PeriodEnd, FinalizedAtUtc, ActualIncome, ActualLoanPayments, ActualCardPayments, ActualTemporaryPayments, ActualInstallmentPayments, ActualOtherScheduledPayments, ActualLargeExpenses, ActualMandatoryPayments, ActualLivingSpend, ActualInterest, UnplannedIncome, UnplannedPayments, DerivedEndingBalance, ConfirmedEndingBalance, ReconciliationAdjustment, ComparisonSummary, Note)
            VALUES ('act-2', 'plan-uniq-1', 'fs-1', 'fs-1', '2026-01-15', '2026-02-14', '2026-02-15T00:00:00Z', 100.0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100.0, 100.0, 0.0, '', '');";

        await connection.ExecuteAsync(actualInsertSql1);
        await Assert.ThrowsAsync<SQLiteException>(() => connection.ExecuteAsync(actualInsertSql2));
        await connection.CloseAsync();
    }

    [Fact]
    public async Task Sema_OluKolonlari_Icermez()
    {
        var connection = await CreateInitializedConnectionAsync();

        var snapshotColumns = await GetColumnNamesAsync(connection, DatabaseConstants.TablePeriodPlanSnapshots);
        var cardColumns = await GetColumnNamesAsync(connection, DatabaseConstants.TableCreditCards);
        var incomeColumns = await GetColumnNamesAsync(connection, DatabaseConstants.TableRecurringIncomes);
        var actualColumns = await GetColumnNamesAsync(connection, DatabaseConstants.TablePeriodActuals);
        var financialSnapshotColumns = await GetColumnNamesAsync(connection, DatabaseConstants.TableFinancialSnapshots);

        await connection.CloseAsync();

        Assert.DoesNotContain("StartDate", snapshotColumns);
        Assert.DoesNotContain("EndDate", snapshotColumns);
        Assert.DoesNotContain("CurrentTotalDebt", cardColumns);
        Assert.DoesNotContain("SalaryDay", incomeColumns);
        Assert.DoesNotContain("ActualSnapshotDate", actualColumns);
        Assert.DoesNotContain("SchemaVersion", financialSnapshotColumns);
    }

    private static async Task InsertSampleFinancialSnapshotAsync(SQLiteAsyncConnection connection, string id)
    {
        await connection.ExecuteAsync(@"
            INSERT INTO financial_snapshots (Id, SnapshotDate, ProjectionAnchorDate, NextSettlementDate, ProjectionOpeningBalance, IncomeDay, Source, IsCurrent, CreatedAtUtc, Note)
            VALUES (?, '2026-01-15', '2026-01-15', '2026-02-15', 50000.0, 15, 0, 1, '2026-01-15T00:00:00Z', '');", id);
    }

    private static async Task InsertSamplePeriodPlanAsync(SQLiteAsyncConnection connection, string planId, string fsId)
    {
        await connection.ExecuteAsync(@"
            INSERT INTO period_plan_snapshots (Id, FinancialSnapshotId, PeriodStart, PeriodEnd, SettlementAvailableFrom, CreatedAtUtc, OpeningBalance, PlannedIncome, PlannedLoanPayments, PlannedCardPayments, PlannedTemporaryPayments, PlannedInstallmentPayments, PlannedOtherScheduledPayments, PlannedMandatoryPayments, PlannedVariableExpenseAllowance, PlannedLargeExpenses, PlannedCardInterest, PlannedDeficitInterest, PlannedEndingBalance)
            VALUES (?, ?, '2026-01-15', '2026-02-14', '2026-01-15', '2026-01-15T00:00:00Z', 50000.0, 100000.0, 20000.0, 10000.0, 0, 0, 0, 30000.0, 15000.0, 0, 0, 0, 85000.0);", planId, fsId);
    }

    private static async Task<List<string>> GetColumnNamesAsync(SQLiteAsyncConnection connection, string tableName)
    {
        var rows = await connection.QueryAsync<TableInfoRow>($"PRAGMA table_info({tableName});");
        return rows.Select(r => r.Name).ToList();
    }

    private sealed class TableInfoRow
    {
        public string Name { get; set; } = string.Empty;
    }
}
