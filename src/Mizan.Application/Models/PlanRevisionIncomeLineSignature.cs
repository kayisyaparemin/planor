using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Plan revizyon imzasında tekil bir gelir satırının kaynağını, tarihini ve tutarını
/// temsil eden değer nesnesidir. Satır ve plan kimliklerini dışarıda bırakır; iki planın
/// gelir satırları arasında anlamlı bir fark (yeni gelir, tutar ya da yatış günü değişikliği)
/// olup olmadığını karşılaştırmak için kullanılır.
/// </summary>
public sealed record PlanRevisionIncomeLineSignature
{
    /// <summary>Gelirin türü (düzenli veya tek seferlik).</summary>
    public IncomeSourceType SourceType { get; init; }

    /// <summary>Kaynak düzenli gelir akışının kimliği (varsa).</summary>
    public Guid? RecurringIncomeId { get; init; }

    /// <summary>Kaynak tek seferlik gelirin kimliği (varsa).</summary>
    public Guid? AdHocIncomeId { get; init; }

    /// <summary>Gelirin adı veya açıklaması.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gelirin hesaba geçmesi beklenen tarih.</summary>
    public DateOnly PlannedDate { get; init; }

    /// <summary>Beklenen gelir tutarı.</summary>
    public decimal PlannedAmount { get; init; }

    /// <summary>
    /// Verilen dönem planı gelir satırından bir satır imzası türetir.
    /// </summary>
    public static PlanRevisionIncomeLineSignature From(PeriodPlanIncomeLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return new PlanRevisionIncomeLineSignature
        {
            SourceType = line.SourceType,
            RecurringIncomeId = line.RecurringIncomeId,
            AdHocIncomeId = line.AdHocIncomeId,
            Name = line.Name,
            PlannedDate = line.PlannedDate,
            PlannedAmount = line.PlannedAmount
        };
    }
}
