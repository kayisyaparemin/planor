using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Finansal geçmişi ve mevcut durum kaydını inceleyerek projeksiyon motorunun başlayacağı
/// çapa tarihini, ilk açık dönem başlangıcını ve devreden açılış bakiyesini çözümleyen servistir.
/// Kapanmış dönemlerin mükerrer simüle edilmesini önlemek ve zincir sürekliliğini sağlamak için vardır.
/// </summary>
public sealed class ProjectionBoundaryResolver(
    CashFlowPeriodCalculator periodCalculator)
{
    private readonly CashFlowPeriodCalculator _periodCalculator =
        periodCalculator ?? throw new ArgumentNullException(nameof(periodCalculator));

    /// <summary>
    /// Verilen tarihçe, aktif durum ve kullanıcı ayarları üzerinden projeksiyon başlangıç sınırını çözümler.
    /// </summary>
    public ProjectionBoundary Resolve(
        FinancialHistoryData history,
        FinancialSnapshot currentSnapshot,
        UserSettings settings,
        DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(currentSnapshot);
        ArgumentNullException.ThrowIfNull(settings);

        var actual = history.Actuals
            .Where(x => x.ResultFinancialSnapshotId == currentSnapshot.Id)
            .OrderByDescending(x => x.FinalizedAtUtc)
            .FirstOrDefault();

        return actual is not null
            ? CreateFromActual(currentSnapshot, settings.PeriodAnchor, actual)
            : CreateFromSnapshot(currentSnapshot, settings.PeriodAnchor, asOf);
    }

    private ProjectionBoundary CreateFromActual(
        FinancialSnapshot snapshot,
        PeriodAnchor anchor,
        PeriodActual actual) => new()
    {
        Snapshot = snapshot,
        ClosedCheckpointDate = actual.PeriodEnd,
        SourcePeriodActualId = actual.Id,
        ProjectionAnchorDate = actual.PeriodEnd,
        // Dönem yarı açık olduğu için kapanan dönemin bitişi açık dönemin ilk günüdür; atlanırsa
        // açık dönemin gelir ve giderleri zincirden düşer (S84).
        FirstUnrealizedPeriodStartDate = _periodCalculator.GetFirstPeriodStartOnOrAfter(actual.PeriodEnd, anchor),
        StartingBalance = snapshot.ProjectionOpeningBalance
    };

    private ProjectionBoundary CreateFromSnapshot(
        FinancialSnapshot snapshot,
        PeriodAnchor anchor,
        DateOnly asOf)
    {
        var anchorDate = snapshot.ProjectionAnchorDate == default ? asOf : snapshot.ProjectionAnchorDate;
        return new ProjectionBoundary
        {
            Snapshot = snapshot,
            ProjectionAnchorDate = anchorDate,
            FirstUnrealizedPeriodStartDate = _periodCalculator.GetFirstPeriodStartOnOrAfter(anchorDate, anchor),
            StartingBalance = snapshot.ProjectionOpeningBalance
        };
    }
}
