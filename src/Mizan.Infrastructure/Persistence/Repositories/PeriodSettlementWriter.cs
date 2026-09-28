using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dönem mutabakatı (checkpoint) sırasında güncellenen finansal araçları ve ayarları
/// SQLite veritabanına yazan dahili yardımcı sınıf.
/// </summary>
internal static class PeriodSettlementWriter
{
    public static void SaveInstruments(SQLiteConnection conn, PeriodSettlementCommit commit)
    {
        SaveLoans(conn, commit.UpdatedLoans);
        foreach (var plan in commit.UpdatedPaymentPlans)
        {
            PaymentPlanEntityWriter.Save(conn, plan);
        }

        foreach (var card in commit.UpdatedCreditCards)
        {
            CreditCardEntityWriter.Save(conn, card);
        }

        SaveLargeExpenses(conn, commit.UpdatedLargeExpenses);
        RemoveLoanPrepayments(conn, commit.RemovedLoanPrepaymentIds);
    }

    public static void SaveSettings(SQLiteConnection conn, UserSettings s)
    {
        var existing = conn.Table<SettingsEntity>().FirstOrDefault() ?? new SettingsEntity();
        existing.PeriodAnchorDay = s.PeriodAnchor.DayOfMonth;
        existing.PeriodVariableExpenseAllowance = s.PeriodVariableExpenseAllowance;
        existing.ProjectionOpeningBalance = s.ProjectionOpeningBalance;
        existing.ProjectionAnchorDate = s.ProjectionAnchorDate == default
            ? string.Empty
            : s.ProjectionAnchorDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture);
        existing.CreditCardCarryInterestRate = s.CreditCardCarryInterestRate;
        existing.DeficitFinancingInterestRate = s.DeficitFinancingInterestRate;
        conn.Upsert(existing);
    }

    private static void SaveLoans(SQLiteConnection conn, IReadOnlyList<Loan> loans)
    {
        foreach (var loan in loans)
        {
            conn.Upsert(new LoanEntity
            {
                Id = loan.Id.ToString(),
                Name = loan.Name,
                Bank = loan.Bank,
                MonthlyPayment = loan.MonthlyPayment,
                PaymentDay = loan.PaymentDay,
                NextPaymentDate = loan.NextPaymentDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                RemainingInstallmentCount = loan.RemainingInstallmentCount,
                FinalPaymentAmount = loan.FinalPaymentAmount,
                RemainingDebt = loan.RemainingDebt,
                EarlyClosureAmount = loan.EarlyClosureAmount,
                EarlyClosureAmountAsOf = loan.EarlyClosureAmountAsOf?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Kind = (int)loan.Kind,
                IsActive = loan.IsActive
            });
        }
    }

    private static void SaveLargeExpenses(SQLiteConnection conn, IReadOnlyList<PlannedLargeExpense> expenses)
    {
        foreach (var exp in expenses)
        {
            conn.Upsert(new PlannedLargeExpenseEntity
            {
                Id = exp.Id.ToString(),
                Name = exp.Name,
                Amount = exp.Amount,
                ExactDate = exp.ExactDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Note = exp.Note,
                Status = (int)exp.Status
            });
        }
    }

    private static void RemoveLoanPrepayments(SQLiteConnection conn, IReadOnlyList<Guid> prepayIds)
    {
        foreach (var id in prepayIds)
        {
            conn.Execute("DELETE FROM loan_prepayments WHERE Id = ?", id.ToString());
        }
    }
}
