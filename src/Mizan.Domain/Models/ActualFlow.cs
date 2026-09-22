namespace Mizan.Domain.Models;

/// <summary>
/// Dönem planında yer almayan, dönem içinde fiilen gerçekleşen plansız nakit akışı satırı.
/// Beklenmeyen gelir veya harcamaların nakit dengesini nasıl etkilediğini belgelemek için vardır.
/// </summary>
public sealed record ActualFlow
{
    /// <summary>Plansız nakit akışı kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Bağlı olduğu dönem gerçekleşmesi kaydının kimliği.</summary>
    public Guid PeriodActualId { get; init; }

    /// <summary>Akışın türü (plansız gelir veya plansız ödeme).</summary>
    public ActualFlowType Type { get; init; }

    /// <summary>Plansız akışın adı veya kısa açıklaması.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Akışın isteğe bağlı sınıflandırma kategorisi.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Akışın gerçekleştiği takvim tarihi.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Gerçekleşen tutar.</summary>
    public decimal Amount { get; init; }

    /// <summary>Akışın bir gelir olup olmadığını belirtir.</summary>
    public bool IsIncome => Type == ActualFlowType.UnplannedIncome;

    /// <summary>Akışın bir nakit çıkışı olup olmadığını belirtir.</summary>
    public bool IsPayment => Type == ActualFlowType.UnplannedPayment;
}
