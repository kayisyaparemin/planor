namespace Mizan.Presentation.Onboarding.Models;

/// <summary>
/// Kurulum sihirbazının sekiz aşamalı adımlarını temsil eden sıra numarası belirteci.
/// </summary>
public enum OnboardingStep
{
    /// <summary>Adım 1: Dönem çapa günü seçimi.</summary>
    Period = 1,

    /// <summary>Adım 2: Düzenli gelirlerin tanımlanması.</summary>
    Income = 2,

    /// <summary>Adım 3: Kredi kartlarının tanımlanması.</summary>
    Card = 3,

    /// <summary>Adım 4: Kredilerin tanımlanması.</summary>
    Loan = 4,

    /// <summary>Adım 5: Yaklaşan tekil veya taksitli ödemeler.</summary>
    Expense = 5,

    /// <summary>Adım 6: Dönemlik serbest yaşam harcaması havuzu.</summary>
    Living = 6,

    /// <summary>Adım 7: İlk günün açılış / mevcut nakit bakiyesi.</summary>
    Balance = 7,

    /// <summary>Adım 8: Kontrol ve kurulumu başlatma özeti.</summary>
    Summary = 8
}
