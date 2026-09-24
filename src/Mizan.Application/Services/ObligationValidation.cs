using Mizan.Application.Abstractions;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kredi kartı ve yükümlülük modellerinin saat ve sistem bağımlılıklarıyla
/// normalizasyonunu ve iş kuralları doğrulamasını sağlayan saf Application yardımcısı.
/// Veri tabanına kaydedilmeden önce borç enstrümanlarının tutarlı duruma getirilmesi için vardır.
/// </summary>
public static class ObligationValidation
{
    /// <summary>
    /// Kredi kartının eksik bakiye tarihini, ekstre zaman damgasını, harcama ve ödeme planı
    /// kimliklerini ve sıralamasını saat sağlayıcısı üzerinden normalize eder.
    /// </summary>
    /// <param name="card">Normalize edilecek kredi kartı.</param>
    /// <param name="clock">Zaman sağlayıcısı portu.</param>
    /// <returns>Tarihleri ve ilişkili alt koleksiyon kimlikleri tamamlanmış kredi kartı.</returns>
    public static CreditCard NormalizeCreditCard(CreditCard card, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(clock);

        return card with
        {
            BalanceAsOfDate = card.BalanceAsOfDate == default
                ? clock.Today
                : card.BalanceAsOfDate,
            CurrentStatement = card.CurrentStatement is null
                ? null
                : card.CurrentStatement with
                {
                    CreditCardId = card.Id,
                    UpdatedAt = card.CurrentStatement.UpdatedAt == default
                        ? clock.UtcNow
                        : card.CurrentStatement.UpdatedAt
                },
            CurrentStatementPaymentPlan =
                card.CurrentStatementPaymentPlan is null ||
                card.CurrentStatementPaymentPlan.Mode == CurrentStatementPaymentMode.Custom
                    ? card.CurrentStatementPaymentPlan
                    : card.CurrentStatementPaymentPlan with
                    {
                        CustomAmount = null
                    },
            Charges = card.Charges
                .OrderBy(x => x.PostingDate)
                .Select(x => x with { CreditCardId = card.Id })
                .ToArray(),
            PaymentPlans = card.PaymentPlans
                .OrderBy(x => x.DueDate)
                .Select(x => x with { CreditCardId = card.Id })
                .ToArray()
        };
    }

    /// <summary>
    /// Geçici/senetli ödeme planını vadeye göre sıralayarak taksitlerin PlanId değerlerini eşitler.
    /// </summary>
    /// <param name="plan">Normalize edilecek ödeme planı.</param>
    /// <returns>Taksitleri sıralanmış ve kimlikleri kenetlenmiş ödeme planı.</returns>
    public static TemporaryPaymentPlan NormalizePaymentPlan(TemporaryPaymentPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.Normalize();
    }

    /// <summary>
    /// Kredi kartının ödeme ayarlarını ve ekstre tutarlılığını doğrular.
    /// </summary>
    /// <param name="card">Doğrulanacak kredi kartı.</param>
    public static void ValidateCreditCard(CreditCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        CreditCardValidator.Validate(card);
    }
}
