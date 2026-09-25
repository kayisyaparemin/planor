using Mizan.Infrastructure.Persistence;
using SQLite;

namespace Mizan.Infrastructure.Tests.Persistence;

public sealed class DatabaseSchemaTests : IDisposable
{
    private readonly string _databasePath;
    private readonly DatabaseSchema _schema;

    public DatabaseSchemaTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"mizan_test_schema_{Guid.NewGuid():N}.db3");
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

    [Fact]
    public async Task EnsureInitializedAsync_TemizVeritabaninda_30TablonunTamaminiOlusturur()
    {
        var connection = new SQLiteAsyncConnection(_databasePath);
        await _schema.EnsureInitializedAsync(connection);

        var tables = await _schema.GetTableNamesAsync(connection);
        await connection.CloseAsync();

        Assert.Equal(30, tables.Count);
        Assert.Contains(DatabaseConstants.TableRecurringIncomes, tables);
        Assert.Contains(DatabaseConstants.TableIncomeAmountHistories, tables);
        Assert.Contains(DatabaseConstants.TableAdHocIncomes, tables);
        Assert.Contains(DatabaseConstants.TableLoans, tables);
        Assert.Contains(DatabaseConstants.TableLoanPrepayments, tables);
        Assert.Contains(DatabaseConstants.TablePaymentPlans, tables);
        Assert.Contains(DatabaseConstants.TablePaymentInstallments, tables);
        Assert.Contains(DatabaseConstants.TableCreditCards, tables);
        Assert.Contains(DatabaseConstants.TableCardInstallments, tables);
        Assert.Contains(DatabaseConstants.TableCreditCardStatements, tables);
        Assert.Contains(DatabaseConstants.TableCreditCardPaymentPlans, tables);
        Assert.Contains(DatabaseConstants.TableCreditCardPaymentPreferences, tables);
        Assert.Contains(DatabaseConstants.TablePlannedLargeExpenses, tables);
        Assert.Contains(DatabaseConstants.TableSettings, tables);
        Assert.Contains(DatabaseConstants.TableFinancialSnapshots, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanSnapshots, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanPaymentLines, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanIncomeLines, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanRevisions, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanRevisionPaymentLines, tables);
        Assert.Contains(DatabaseConstants.TablePeriodPlanRevisionIncomeLines, tables);
        Assert.Contains(DatabaseConstants.TablePeriodActuals, tables);
        Assert.Contains(DatabaseConstants.TableActualPayments, tables);
        Assert.Contains(DatabaseConstants.TableActualFlows, tables);
        Assert.Contains(DatabaseConstants.TableActualLivingBreakdowns, tables);
        Assert.Contains(DatabaseConstants.TablePeriodObservations, tables);
        Assert.Contains(DatabaseConstants.TablePeriodObservationPayments, tables);
        Assert.Contains(DatabaseConstants.TableSimulationDrafts, tables);
        Assert.Contains(DatabaseConstants.TableSimulationDraftConditions, tables);
        Assert.Contains(DatabaseConstants.TablePaymentReminderResponses, tables);
    }

    [Fact]
    public async Task EnsureInitializedAsync_TemizVeritabaninda_UserVersion1OlarakBelirler()
    {
        var connection = new SQLiteAsyncConnection(_databasePath);
        await _schema.EnsureInitializedAsync(connection);

        var version = await _schema.GetUserVersionAsync(connection);
        await connection.CloseAsync();

        Assert.Equal(DatabaseConstants.CurrentSchemaVersion, version);
    }

    [Fact]
    public async Task EnsureInitializedAsync_ZatenSurum1Ise_HataVermedenGecer()
    {
        var connection = new SQLiteAsyncConnection(_databasePath);
        await _schema.EnsureInitializedAsync(connection);

        // İkinci çağrı: şema sürümü zaten 1, DDL tetiklenmeden sessizce başarıyla dönmeli
        await _schema.EnsureInitializedAsync(connection);
        var version = await _schema.GetUserVersionAsync(connection);
        await connection.CloseAsync();

        Assert.Equal(1, version);
    }

    [Fact]
    public async Task EnsureInitializedAsync_DesteklenmeyenIleriSurum_InvalidOperationExceptionFirlatir()
    {
        var connection = new SQLiteAsyncConnection(_databasePath);
        await _schema.SetUserVersionAsync(connection, 99);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _schema.EnsureInitializedAsync(connection));

        await connection.CloseAsync();
        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task SqliteConnectionFactory_BaglantiUrettiginde_SemayiIlklendirirVeYabanciAnahtarlariAcar()
    {
        var factory = new SqliteConnectionFactory(_schema);
        var connection = await factory.CreateConnectionAsync(_databasePath);

        var version = await _schema.GetUserVersionAsync(connection);
        var fkStatus = await connection.ExecuteScalarAsync<int>("PRAGMA foreign_keys;");
        await connection.CloseAsync();

        Assert.Equal(1, version);
        Assert.Equal(1, fkStatus);
    }
}
