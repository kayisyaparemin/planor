using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;
using SQLite;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Bir kredi kartını harcamaları, ödeme planları, ekstresi ve ödeme tercihleriyle birlikte yazan
/// dahili yardımcı sınıf. Kart hem kendi deposundan hem dönem kapanışından (fiilî ödemenin
/// uygulandığı bakiye) yazılıyor; kapanış kartı hiç yazmadığı için ikisi aynı yazıcıyı paylaşır.
/// Çağıran işlemi açar.
/// </summary>
internal static class CreditCardEntityWriter
{
    public static void Save(SQLiteConnection conn, CreditCard card)
    {
        var cardId = card.Id.ToString();
        SaveCardMain(conn, cardId, card);
        SaveCharges(conn, cardId, card.Charges);
        SavePaymentPlans(conn, cardId, card.PaymentPlans);
        SaveStatement(conn, cardId, card.CurrentStatement, card.CurrentStatementPaymentPlan);
        SavePreferences(conn, cardId, card.PaymentPreferences);
    }

    private static void SaveCardMain(SQLiteConnection conn, string cardId, CreditCard card) =>
        conn.Upsert(new CreditCardEntity
        {
            Id = cardId,
            Name = card.Name,
            Bank = card.Bank,
            Limit = card.Limit,
            CarriedBalance = card.CarriedBalance,
            UnbilledSpending = card.UnbilledSpending,
            BalanceAsOfDate = card.BalanceAsOfDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            StatementClosingDay = card.StatementClosingDay,
            PaymentDueDay = card.PaymentDueDay,
            MinimumPaymentRate = card.MinimumPaymentRate,
            PaymentStrategy = (int)card.PaymentStrategy,
            FixedPaymentAmount = card.FixedPaymentAmount,
            ProjectionFallbackStrategy = (int)card.ProjectionFallbackStrategy,
            ProjectionFallbackFixedAmount = card.ProjectionFallbackFixedAmount,
            KnownNextStatementDate = card.KnownNextStatementDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            KnownNextDueDate = card.KnownNextDueDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            IsActive = card.IsActive
        });

    private static void SaveCharges(SQLiteConnection conn, string cardId, IReadOnlyList<CardCharge> charges)
    {
        conn.Execute("DELETE FROM card_installments WHERE CreditCardId = ?", cardId);
        foreach (var c in charges)
        {
            conn.Insert(new CardInstallmentEntity
            {
                Id = c.Id.ToString(),
                CreditCardId = cardId,
                Description = c.Description,
                PostingDate = c.PostingDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Amount = c.Amount
            });
        }
    }

    private static void SavePaymentPlans(SQLiteConnection conn, string cardId, IReadOnlyList<CreditCardPaymentPlan> plans)
    {
        conn.Execute("DELETE FROM credit_card_payment_plans WHERE CreditCardId = ?", cardId);
        foreach (var p in plans)
        {
            conn.Insert(new CreditCardPaymentPlanEntity
            {
                Id = p.Id.ToString(),
                CreditCardId = cardId,
                DueDate = p.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PaymentType = (int)p.PaymentType,
                Amount = p.Amount
            });
        }
    }

    private static void SaveStatement(SQLiteConnection conn, string cardId, CreditCardStatement? stmt, CurrentStatementPaymentPlan? plan)
    {
        conn.Execute("DELETE FROM credit_card_statements WHERE CreditCardId = ?", cardId);
        if (stmt is null)
        {
            return;
        }

        conn.Insert(new CreditCardStatementEntity
        {
            Id = stmt.Id.ToString(),
            CreditCardId = cardId,
            StatementDate = stmt.StatementDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            DueDate = stmt.DueDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            StatementAmount = stmt.StatementAmount,
            MinimumPaymentAmount = stmt.MinimumPaymentAmount,
            NextStatementDate = stmt.NextStatementDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            NextDueDate = stmt.NextDueDate?.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            CreatedAt = stmt.CreatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            UpdatedAt = stmt.UpdatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
            CurrentPaymentMode = plan is not null ? (int)plan.Mode : 0,
            CurrentPaymentCustomAmount = plan?.CustomAmount
        });
    }

    private static void SavePreferences(SQLiteConnection conn, string cardId, IReadOnlyList<CreditCardPaymentPreference> prefs)
    {
        conn.Execute("DELETE FROM credit_card_payment_preferences WHERE CreditCardId = ?", cardId);
        foreach (var p in prefs)
        {
            conn.Insert(new CreditCardPaymentPreferenceEntity
            {
                Id = p.Id.ToString(),
                CreditCardId = cardId,
                Mode = (int)p.Mode,
                CustomAmount = p.CustomAmount,
                EffectiveFromStatementDate = p.EffectiveFromStatementDate.ToString(DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                CreatedAt = p.CreatedAt.ToString(DatabaseConstants.DateTimeOffsetFormat, CultureInfo.InvariantCulture),
                Note = p.Note
            });
        }
    }
}
