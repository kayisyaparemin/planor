using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Dondurulan plan ödeme satırına ait kullanıcının dönem mutabakatında girdiği fiilî gerçekleşme girdisidir.
/// Planlanan borç veya harcamanın ödenip ödenmediğini, fiilî ödeme tutarını ve tarihini taşır.
/// </summary>
public sealed record ActualPaymentDraft
{
    /// <summary>İlişkili dondurulmuş plan ödeme satırının kimliği.</summary>
    public required Guid PeriodPlanPaymentLineId { get; init; }

    /// <summary>Ödemenin fiilî gerçekleşme durumu (Ödendi, Ödenmedi, Farklı Tutar vb.).</summary>
    public required ActualPaymentStatus Status { get; init; }

    /// <summary>Fiilen ödenen net para tutarı.</summary>
    public required decimal ActualAmount { get; init; }

    /// <summary>Ödemenin fiilen gerçekleştiği tarih; ödenmediyse null olabilir.</summary>
    public DateOnly? ActualPaymentDate { get; init; }

    /// <summary>Kullanıcının ödeme satırına dair girdiği isteğe bağlı açıklama notu.</summary>
    public string Note { get; init; } = string.Empty;
}
