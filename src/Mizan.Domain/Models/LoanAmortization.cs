namespace Mizan.Domain.Models;

/// <summary>
/// Bir kredinin Fransız annüite modeline göre çözümlenmiş itfa durumunu temsil eder.
/// Aylık efektif faiz, güncel anapara ve taksit parametrelerini taşıyarak
/// erken kapama teklifleri ve simülasyon hesaplamaları için standart zemin sağlar.
/// </summary>
public sealed record LoanAmortization
{
    /// <summary>Çözümlenen aylık efektif faiz oranı (vergiler dahil).</summary>
    public decimal MonthlyRate { get; init; }

    /// <summary>Kredinin güncel kalan anapara borcu.</summary>
    public decimal Principal { get; init; }

    /// <summary>Kalan toplam taksit adedi.</summary>
    public int RemainingInstallments { get; init; }

    /// <summary>Aylık standart taksit ödeme tutarı.</summary>
    public decimal MonthlyPayment { get; init; }

    /// <summary>Son taksit tutarı.</summary>
    public decimal FinalPayment { get; init; }

    /// <summary>Bir önceki taksit ödeme tarihi.</summary>
    public DateOnly PreviousDueDate { get; init; }

    /// <summary>Faiz ve anaparanın türetildiği kaynak.</summary>
    public LoanRateSource Source { get; init; }

    /// <summary>Kalan tüm taksitlerin nominal toplam tutarı.</summary>
    public decimal RemainingInstallmentTotal => RemainingInstallments < 1
        ? 0m
        : MonthlyPayment * (RemainingInstallments - 1) + FinalPayment;

    /// <summary>Kalan toplam taksitler içindeki toplam faiz yükü.</summary>
    public decimal RemainingInterest => Math.Max(0m, RemainingInstallmentTotal - Principal);
}
