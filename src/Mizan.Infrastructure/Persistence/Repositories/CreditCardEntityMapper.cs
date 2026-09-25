using System.Globalization;
using Mizan.Domain.Models;
using Mizan.Infrastructure.Persistence.Entities;

namespace Mizan.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kredi kartı varlık modelleri ile SQLite entity sınıfları arasındaki dönüşümleri sağlayan dahili haritalayıcı.
/// </summary>
internal static class CreditCardEntityMapper
{
    public static CreditCard MapCreditCard(
        CreditCardEntity row,
        List<CardInstallmentEntity> charges,
        List<CreditCardStatementEntity> statements,
        List<CreditCardPaymentPlanEntity> paymentPlans,
        List<CreditCardPaymentPreferenceEntity> preferences)
    {
        var stmtEntity = statements
            .Where(s => s.CreditCardId == row.Id)
            .OrderByDescending(s => s.StatementDate)
            .ThenByDescending(s => s.UpdatedAt)
            .FirstOrDefault();

        return new CreditCard
        {
            Id = Guid.Parse(row.Id),
            Name = row.Name,
            Bank = row.Bank,
            Limit = row.Limit,
            CarriedBalance = row.CarriedBalance,
            UnbilledSpending = row.UnbilledSpending,
            BalanceAsOfDate = DateOnly.ParseExact(row.BalanceAsOfDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            StatementClosingDay = row.StatementClosingDay,
            PaymentDueDay = row.PaymentDueDay,
            MinimumPaymentRate = row.MinimumPaymentRate,
            PaymentStrategy = (CreditCardPaymentStrategy)row.PaymentStrategy,
            FixedPaymentAmount = row.FixedPaymentAmount,
            ProjectionFallbackStrategy = (ProjectionFallbackStrategy)row.ProjectionFallbackStrategy,
            ProjectionFallbackFixedAmount = row.ProjectionFallbackFixedAmount,
            KnownNextStatementDate = string.IsNullOrWhiteSpace(row.KnownNextStatementDate) ? null : DateOnly.ParseExact(row.KnownNextStatementDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            KnownNextDueDate = string.IsNullOrWhiteSpace(row.KnownNextDueDate) ? null : DateOnly.ParseExact(row.KnownNextDueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
            IsActive = row.IsActive,
            CurrentStatement = stmtEntity is null ? null : MapStatement(stmtEntity),
            CurrentStatementPaymentPlan = stmtEntity is null ? null : MapCurrentPaymentPlan(stmtEntity),
            Charges = MapCharges(charges, row.Id),
            PaymentPlans = MapPaymentPlans(paymentPlans, row.Id),
            PaymentPreferences = MapPreferences(preferences, row.Id)
        };
    }

    public static CreditCardStatement MapStatement(CreditCardStatementEntity s) => new()
    {
        Id = Guid.Parse(s.Id),
        CreditCardId = Guid.Parse(s.CreditCardId),
        StatementDate = DateOnly.ParseExact(s.StatementDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        DueDate = DateOnly.ParseExact(s.DueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        StatementAmount = s.StatementAmount,
        MinimumPaymentAmount = s.MinimumPaymentAmount,
        NextStatementDate = string.IsNullOrWhiteSpace(s.NextStatementDate) ? null : DateOnly.ParseExact(s.NextStatementDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        NextDueDate = string.IsNullOrWhiteSpace(s.NextDueDate) ? null : DateOnly.ParseExact(s.NextDueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
        CreatedAt = DateTimeOffset.Parse(s.CreatedAt, CultureInfo.InvariantCulture),
        UpdatedAt = DateTimeOffset.Parse(s.UpdatedAt, CultureInfo.InvariantCulture)
    };

    private static CurrentStatementPaymentPlan MapCurrentPaymentPlan(CreditCardStatementEntity stmt) => new()
    {
        Mode = (CurrentStatementPaymentMode)stmt.CurrentPaymentMode,
        CustomAmount = stmt.CurrentPaymentCustomAmount
    };

    private static CardCharge[] MapCharges(List<CardInstallmentEntity> charges, string cardId) =>
        charges.Where(c => c.CreditCardId == cardId)
            .Select(c => new CardCharge
            {
                Id = Guid.Parse(c.Id),
                CreditCardId = Guid.Parse(c.CreditCardId),
                Description = c.Description,
                PostingDate = DateOnly.ParseExact(c.PostingDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                Amount = c.Amount
            })
            .OrderBy(c => c.PostingDate)
            .ToArray();

    private static CreditCardPaymentPlan[] MapPaymentPlans(List<CreditCardPaymentPlanEntity> plans, string cardId) =>
        plans.Where(p => p.CreditCardId == cardId)
            .Select(p => new CreditCardPaymentPlan
            {
                Id = Guid.Parse(p.Id),
                CreditCardId = Guid.Parse(p.CreditCardId),
                DueDate = DateOnly.ParseExact(p.DueDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                PaymentType = (CreditCardPaymentType)p.PaymentType,
                Amount = p.Amount
            })
            .OrderBy(p => p.DueDate)
            .ToArray();

    private static CreditCardPaymentPreference[] MapPreferences(List<CreditCardPaymentPreferenceEntity> preferences, string cardId) =>
        preferences.Where(p => p.CreditCardId == cardId)
            .Select(p => new CreditCardPaymentPreference
            {
                Id = Guid.Parse(p.Id),
                CreditCardId = Guid.Parse(p.CreditCardId),
                Mode = (CurrentStatementPaymentMode)p.Mode,
                CustomAmount = p.CustomAmount,
                EffectiveFromStatementDate = DateOnly.ParseExact(p.EffectiveFromStatementDate, DatabaseConstants.DateFormat, CultureInfo.InvariantCulture),
                CreatedAt = DateTimeOffset.Parse(p.CreatedAt, CultureInfo.InvariantCulture),
                Note = p.Note
            })
            .OrderBy(p => p.EffectiveFromStatementDate)
            .ToArray();
}
