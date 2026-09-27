namespace Mizan.Presentation.Onboarding.Models;

/// <summary>
/// Kurulum sihirbazında eklenen taslak gelir, kart, kredi veya ödeme kayıtlarını
/// arayüz listesinde göstermek için kullanılan saf sunum modelidir.
/// </summary>
public sealed record OnboardingDraftItem(
    Guid Id,
    string Title,
    string Subtitle,
    decimal Amount,
    OnboardingRecordKind Kind);
