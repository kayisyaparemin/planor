using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kaydedilmiş bir simülasyon taslağı içerisindeki tekil senaryo koşulunun SQLite tablo varlığı.
/// </summary>
[Table("simulation_draft_conditions")]
internal sealed class SimulationDraftConditionEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string DraftId { get; set; } = string.Empty;

    public int Position { get; set; }

    public bool IsEnabled { get; set; }

    public int Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string StartDate { get; set; } = string.Empty;

    public int PaymentCount { get; set; }

    public string? FirstPaymentDate { get; set; }

    public string? CreditCardId { get; set; }

    public decimal? TotalRepaymentAmount { get; set; }

    public string? RecurringIncomeId { get; set; }

    public string ScenarioId { get; set; } = string.Empty;

    public int? CardPaymentType { get; set; }

    public bool AppliesToAllStatements { get; set; }

    public string? LoanId { get; set; }

    public int? PrepaymentMode { get; set; }
}
