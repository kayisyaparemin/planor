namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının verdiği cevabın kalıcı defterdeki satırını temsil eden model.
/// Anahtarı ödemenin kaynağından ve vadesinden türer; plan revizyonlarından veya dönem kapanışlarından etkilenmez.
/// </summary>
public sealed record PaymentReminderResponse
{
    /// <summary>Ödemenin dayanıklı tekil anahtarı ({SourceId:N}-{DueDate:yyyyMMdd}).</summary>
    public required string DueKey { get; init; }

    /// <summary>Ödeme kalemi adı.</summary>
    public required string Name { get; init; }

    /// <summary>Ödeme vadesi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Ödeme tutarı.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Kullanıcının verdiği yanıtın türü (Ödedim / Ertele).</summary>
    public required PaymentReminderAnswerKind Kind { get; init; }

    /// <summary>Yanıtın verildiği an.</summary>
    public required DateTime AnsweredAt { get; init; }

    /// <summary>Erteleme seçildiyse yeniden hatırlatma zamanı.</summary>
    public DateTime? SnoozedUntil { get; init; }
}
