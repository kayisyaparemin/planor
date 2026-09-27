namespace Mizan.Presentation.Onboarding.Models;

/// <summary>
/// Kurulum sihirbazı son adımındaki özet gösterge verilerini taşır.
/// </summary>
public sealed record OnboardingSummary
{
    /// <summary>Kullanıcının belirlediği dönem başlangıç günü.</summary>
    public int PeriodDay { get; init; } = 15;

    /// <summary>Tanımlanan gelir sayısı.</summary>
    public int IncomeCount { get; init; }

    /// <summary>Tanımlanan toplam gelir tutarı.</summary>
    public decimal IncomeTotal { get; init; }

    /// <summary>Tanımlanan kredi kartı sayısı.</summary>
    public int CardCount { get; init; }

    /// <summary>Tanımlanan kartların toplam limiti.</summary>
    public decimal CardLimitTotal { get; init; }

    /// <summary>Tanımlanan kartların toplam güncel borcu.</summary>
    public decimal CardDebtTotal { get; init; }

    /// <summary>Tanımlanan kredi sayısı.</summary>
    public int LoanCount { get; init; }

    /// <summary>Kredilerin toplam aylık taksit tutarı.</summary>
    public decimal LoanPaymentTotal { get; init; }

    /// <summary>Tanımlanan yaklaşan harcama ve ödeme sayısı.</summary>
    public int ExpenseCount { get; init; }

    /// <summary>Yaklaşan harcamaların toplam tutarı.</summary>
    public decimal ExpenseTotal { get; init; }

    /// <summary>Dönemlik serbest harçlık / değişken gider payı.</summary>
    public decimal VariableExpenseAllowance { get; init; }

    /// <summary>Mevcut nakit bakiye.</summary>
    public decimal CurrentBalance { get; init; }
}
