using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Plan revizyon imzasında tekil bir ödeme taahhüdü satırının kimliğini, türünü
/// ve tutarını temsil eden değer nesnesidir. İki planın ödeme satırları arasında
/// fark olup olmadığını karşılaştırmak için kullanılır.
/// </summary>
public sealed record PlanRevisionLineSignature
{
    /// <summary>Ödeme kaynağının tekil kimliği.</summary>
    public Guid SourceEntityId { get; init; }

    /// <summary>Ödeme planı kaynak türü.</summary>
    public PlanPaymentSourceType SourceType { get; init; }

    /// <summary>Ödeme kalemi adı veya açıklaması.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Planlanan ödeme tarihi.</summary>
    public DateOnly PlannedDate { get; init; }

    /// <summary>Planlanan ödeme tutarı.</summary>
    public decimal? PlannedAmount { get; init; }

    /// <summary>Tutarın tahminî olup olmadığı.</summary>
    public bool IsEstimate { get; init; }

    /// <summary>Ödeme detayı veya notu.</summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>
    /// Verilen dönem planı ödeme satırından bir satır imzası türetir.
    /// </summary>
    public static PlanRevisionLineSignature From(PeriodPlanPaymentLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new PlanRevisionLineSignature
        {
            SourceEntityId = line.SourceEntityId,
            SourceType = line.SourceType,
            Name = line.Name,
            PlannedDate = line.PlannedDate,
            PlannedAmount = line.PlannedAmount,
            IsEstimate = line.IsEstimate,
            Detail = line.Detail
        };
    }
}
