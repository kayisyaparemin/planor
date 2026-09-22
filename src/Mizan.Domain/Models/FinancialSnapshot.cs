namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının belirli bir tarihteki finansal durumunun kalıcı üst başlık kaydı.
/// Açılış nakit bakiyesi, referans çapa kuralı ve bir sonraki mutabakat tarihini
/// dondurarak dönem planı zincirinin temel taşını oluşturmak için vardır.
/// </summary>
public sealed record FinancialSnapshot
{
    /// <summary>Finansal durum kaydının tekil kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Durumun kaydedildiği veya yürürlüğe girdiği referans takvim tarihi.</summary>
    public DateOnly SnapshotDate { get; init; }

    /// <summary>Projeksiyon hesaplamalarında başlangıç olarak alınan referans çapa tarihi.</summary>
    public DateOnly ProjectionAnchorDate { get; init; }

    /// <summary>Bu duruma bağlı açık dönemin tamamlanıp mutabakata açılacağı sonraki kapanış tarihi.</summary>
    public DateOnly NextSettlementDate { get; init; }

    /// <summary>Dönem başlangıcındaki mevcut açılış nakit bakiyesi.</summary>
    public decimal ProjectionOpeningBalance { get; init; }

    /// <summary>Dönemlerin ne zaman döndüğünü belirleyen dönem çapası kuralı.</summary>
    public PeriodAnchor Anchor { get; init; } = new(1);

    /// <summary>Zincirdeki bir önceki finansal durumun kimliği; ilk kurulumda boştur.</summary>
    public Guid? PreviousSnapshotId { get; init; }

    /// <summary>Bu durum kaydının oluşturulma kaynağı (kurulum, aylık kapanış, toparlama).</summary>
    public FinancialSnapshotSource Source { get; init; }

    /// <summary>Bu durumun şu anda yürürlükte olan aktif finansal durum olup olmadığı.</summary>
    public bool IsCurrent { get; init; }

    /// <summary>Kaydın UTC oluşturulma zaman damgası.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Kullanıcı veya sistem tarafından eklenen açıklama notu.</summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>Bu durumun ilk kurulum (onboarding) snapshot'ı olup olmadığını belirtir.</summary>
    public bool IsInitial => Source == FinancialSnapshotSource.Initial;
}
