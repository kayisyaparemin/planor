using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Dönem başında planlanmamış, dönem içinde beklenmedik şekilde ortaya çıkan arızi gelir
/// veya plansız gider akışı taslak girdisidir.
/// </summary>
public sealed record ActualFlowDraft
{
    /// <summary>Plansız hareketin yönü (Plansız Gelir veya Plansız Harcama).</summary>
    public required ActualFlowType Type { get; init; }

    /// <summary>Plansız nakit akışının anlaşılır adı.</summary>
    public required string Name { get; init; }

    /// <summary>Plansız nakit hareketinin ait olduğu harcama veya gelir kategorisi.</summary>
    public required string Category { get; init; }

    /// <summary>Plansız hareketin gerçekleştiği tarih.</summary>
    public required DateOnly Date { get; init; }

    /// <summary>Plansız nakit hareketinin pozitif para tutarı.</summary>
    public required decimal Amount { get; init; }
}
