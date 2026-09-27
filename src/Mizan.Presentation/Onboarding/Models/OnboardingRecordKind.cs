namespace Mizan.Presentation.Onboarding.Models;

/// <summary>
/// Kurulum sihirbazında listelenen taslak kayıtların türünü (gelir, kart, kredi, harcama) belirten enum.
/// </summary>
public enum OnboardingRecordKind
{
    /// <summary>Düzenli gelir akışı.</summary>
    Income,

    /// <summary>Kredi kartı sözleşmesi.</summary>
    Card,

    /// <summary>Banka kredisi sözleşmesi.</summary>
    Loan,

    /// <summary>Planlanan büyük harcama veya vadeli borç planı.</summary>
    Expense
}
