using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

public sealed class ProjectionBoundaryResolverTests
{
    private readonly ProjectionBoundaryResolver _resolver = new(new CashFlowPeriodCalculator());

    [Fact]
    public void Resolve_NullParametreler_ArgumentNullExceptionFirlatir()
    {
        var history = FinancialHistoryData.Empty;
        var snapshot = new FinancialSnapshot();
        var settings = new UserSettings();
        var asOf = new DateOnly(2026, 8, 20);

        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(null!, snapshot, settings, asOf));
        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(history, null!, settings, asOf));
        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(history, snapshot, null!, asOf));
    }

    [Fact]
    public void Resolve_GecmisGerceklesmeYoksa_OnboardingCapaVeAcilisBakiyesiniKullanir()
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            ProjectionAnchorDate = new DateOnly(2026, 8, 15),
            ProjectionOpeningBalance = 35000m
        };
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15)
        };
        var history = FinancialHistoryData.Empty;
        var asOf = new DateOnly(2026, 8, 10);

        var boundary = _resolver.Resolve(history, snapshot, settings, asOf);

        Assert.Same(snapshot, boundary.Snapshot);
        Assert.Equal(new DateOnly(2026, 8, 15), boundary.ProjectionAnchorDate);
        Assert.Equal(new DateOnly(2026, 8, 15), boundary.FirstUnrealizedPeriodStartDate);
        Assert.Equal(35000m, boundary.StartingBalance);
        Assert.Null(boundary.ClosedCheckpointDate);
        Assert.Null(boundary.SourcePeriodActualId);
    }

    [Fact]
    public void Resolve_SnapshotCapaTarihiDefaultIse_AsOfTarihiniKullanir()
    {
        var snapshot = new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            ProjectionAnchorDate = default,
            ProjectionOpeningBalance = 20000m
        };
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(1)
        };
        var history = FinancialHistoryData.Empty;
        var asOf = new DateOnly(2026, 9, 10);

        var boundary = _resolver.Resolve(history, snapshot, settings, asOf);

        Assert.Equal(asOf, boundary.ProjectionAnchorDate);
        // asOf 10 Eylül, çapa 1 -> sonraki dönem başlangıcı 1 Ekim
        Assert.Equal(new DateOnly(2026, 10, 1), boundary.FirstUnrealizedPeriodStartDate);
        Assert.Equal(20000m, boundary.StartingBalance);
        Assert.Null(boundary.ClosedCheckpointDate);
    }

    [Fact]
    public void Resolve_SonGerceklesmeVarsa_ProjeksiyonKapanisinActigiDonemdenBaslar()
    {
        var snapshotId = Guid.NewGuid();
        var currentSnapshot = new FinancialSnapshot
        {
            Id = snapshotId,
            ProjectionOpeningBalance = 42500m
        };
        var actualId = Guid.NewGuid();
        var actual = new PeriodActual
        {
            Id = actualId,
            ResultFinancialSnapshotId = snapshotId,
            PeriodStart = new DateOnly(2026, 1, 15),
            PeriodEnd = new DateOnly(2026, 2, 15),
            FinalizedAtUtc = new DateTimeOffset(2026, 2, 15, 20, 0, 0, TimeSpan.Zero)
        };
        var history = new FinancialHistoryData([], [], [], [actual]);
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(15)
        };

        var boundary = _resolver.Resolve(history, currentSnapshot, settings, new DateOnly(2026, 2, 16));

        Assert.Same(currentSnapshot, boundary.Snapshot);
        Assert.Equal(new DateOnly(2026, 2, 15), boundary.ClosedCheckpointDate);
        Assert.Equal(actualId, boundary.SourcePeriodActualId);
        Assert.Equal(new DateOnly(2026, 2, 15), boundary.ProjectionAnchorDate);
        // Kapanan dönem [15 Ocak, 15 Şubat) yarı açık; bitiş günü açık dönemin ilk günüdür (S84)
        Assert.Equal(new DateOnly(2026, 2, 15), boundary.FirstUnrealizedPeriodStartDate);
        Assert.Equal(42500m, boundary.StartingBalance);
    }

    [Fact]
    public void Resolve_BirdenFazlaGerceklesmeVarsa_EnSonKapatilaniSecer()
    {
        var snapshotId = Guid.NewGuid();
        var currentSnapshot = new FinancialSnapshot
        {
            Id = snapshotId,
            ProjectionOpeningBalance = 60000m
        };
        var eskiActual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            ResultFinancialSnapshotId = snapshotId,
            PeriodStart = new DateOnly(2026, 1, 1),
            PeriodEnd = new DateOnly(2026, 2, 1),
            FinalizedAtUtc = new DateTimeOffset(2026, 2, 1, 10, 0, 0, TimeSpan.Zero)
        };
        var yeniActual = new PeriodActual
        {
            Id = Guid.NewGuid(),
            ResultFinancialSnapshotId = snapshotId,
            PeriodStart = new DateOnly(2026, 2, 1),
            PeriodEnd = new DateOnly(2026, 3, 1),
            FinalizedAtUtc = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero)
        };
        var history = new FinancialHistoryData([], [], [], [eskiActual, yeniActual]);
        var settings = new UserSettings
        {
            PeriodAnchor = new PeriodAnchor(1)
        };

        var boundary = _resolver.Resolve(history, currentSnapshot, settings, new DateOnly(2026, 3, 2));

        Assert.Equal(yeniActual.Id, boundary.SourcePeriodActualId);
        Assert.Equal(new DateOnly(2026, 3, 1), boundary.ClosedCheckpointDate);
        Assert.Equal(new DateOnly(2026, 3, 1), boundary.FirstUnrealizedPeriodStartDate);
        Assert.Equal(60000m, boundary.StartingBalance);
    }
}
