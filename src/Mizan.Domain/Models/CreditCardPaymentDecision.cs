namespace Mizan.Domain.Models;

/// <summary>
/// Kredi kartı ekstre ödeme kararı çözümlemesi sonucunda belirlenen ödeme tutarı,
/// karar kaynağı ve uygulanan ödeme tipini taşıyan sözleşme.
/// </summary>
public sealed record CreditCardPaymentDecision(
    decimal? Payment,
    CreditCardPaymentResolution Resolution,
    CreditCardPaymentType? PaymentType);
