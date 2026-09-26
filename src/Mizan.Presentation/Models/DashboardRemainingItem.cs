namespace Mizan.Presentation.Models;

/// <summary>
/// Ana sayfadaki "Kalan Yükümlülükler" listesinde tekil bir bekleyen ödemeyi,
/// vadesi geçmiş veya ertelenmiş olma durumunu sunan sunum modelidir.
/// </summary>
public sealed record DashboardRemainingItem
{
    /// <summary>Planlanan vade tarihi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Yükümlülük veya ödeme adı.</summary>
    public required string Name { get; init; }

    /// <summary>Planlanan ödeme tutarı.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Detay veya alt açıklama.</summary>
    public string? Detail { get; init; }

    /// <summary>Ödemenin bu dönem için ertelenip ertelenmediği.</summary>
    public bool IsSnoozed { get; init; }

    /// <summary>Vadesinin bugünden önce olup olmadığı.</summary>
    public bool IsOverdue { get; init; }
}
