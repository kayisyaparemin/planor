namespace Mizan.Application.Models;

/// <summary>
/// İşletim sistemine kurulacak olan tek bir zamanlanmış bildirimin verilerini temsil eden model.
/// Aynı güne düşen ödemeler tek bir bildirimde birleştirilir.
/// </summary>
public sealed record PaymentReminder
{
    /// <summary>Bildirimin tekil anahtarı (vade ve saat diliminden türer).</summary>
    public required string Key { get; init; }

    /// <summary>Bildirimin çalacağı tarih ve yerel saat.</summary>
    public required DateTime NotifyAt { get; init; }

    /// <summary>Bildirim başlığı ("Bugün ödeme günü", "3 gün sonra ödeme var" vb.).</summary>
    public required string Title { get; init; }

    /// <summary>Bildirim gövde metni (ödeme adları ve toplam tutar).</summary>
    public required string Message { get; init; }

    /// <summary>Ödemenin vadesi.</summary>
    public required DateOnly DueDate { get; init; }

    /// <summary>Bildirimin kapsadığı ödemelerin listesi.</summary>
    public IReadOnlyList<PaymentDue> Payments { get; init; } = [];
}
