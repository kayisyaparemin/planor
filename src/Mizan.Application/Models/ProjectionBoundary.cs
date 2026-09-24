using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Projeksiyon motorunun başlangıç çapa tarihini, ilk projeksiyon dönemi başlangıcını,
/// açılış nakit bakiyesini ve varsa dayandığı son kapanış (checkpoint) gerçekleşme
/// bilgilerini taşıyan sınır sözleşmesi.
/// Projeksiyon zincirinin hangi zamandan ve hangi mali durumdan başlayacağını belirlemek için vardır.
/// </summary>
public sealed record ProjectionBoundary
{
    /// <summary>Projeksiyonun dayandığı temel finansal durum anlık görüntüsü.</summary>
    public required FinancialSnapshot Snapshot { get; init; }

    /// <summary>Projeksiyon hesaplamasında başlangıç kabul edilen referans çapa tarihi.</summary>
    public required DateOnly ProjectionAnchorDate { get; init; }

    /// <summary>Geçmişte kapanmamış, henüz gerçekleşmemiş ilk nakit akış döneminin başlangıç tarihi.</summary>
    public required DateOnly FirstUnrealizedPeriodStartDate { get; init; }

    /// <summary>Projeksiyonun başlayacağı devreden açılış nakit bakiyesi.</summary>
    public required decimal StartingBalance { get; init; }

    /// <summary>Projeksiyon kapanmış bir döneme dayanıyorsa, o dönemin mutabakat kapanış tarihi.</summary>
    public DateOnly? ClosedCheckpointDate { get; init; }

    /// <summary>Projeksiyonun dayandığı kapanmış dönem gerçekleşmesinin tekil kimliği.</summary>
    public Guid? SourcePeriodActualId { get; init; }
}
