namespace Mizan.Presentation.Models;

/// <summary>
/// Hatırlatıcı kartında kullanıcıya sunulan aktif ödeme kalemini temsil eden
/// salt okunur sunum modelidir.
/// </summary>
public sealed record ReminderItem
{
    /// <summary>Ödemenin dayanıklı tekil anahtarıdır.</summary>
    public required string DueKey { get; init; }

    /// <summary>Ödeme kaleminin kullanıcıya görünen adıdır.</summary>
    public required string Name { get; init; }

    /// <summary>Ödemenin vade tarihidir.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Ödemenin tutarıdır; belirsizse boştur.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Ödemenin daha önce kullanıcı tarafından ertelenip ertelenmediğidir.</summary>
    public bool IsSnoozed { get; init; }

    /// <summary>Ödeme ertelenmişse bir sonraki hatırlatma zamanıdır.</summary>
    public DateTime? SnoozedUntil { get; init; }
}
