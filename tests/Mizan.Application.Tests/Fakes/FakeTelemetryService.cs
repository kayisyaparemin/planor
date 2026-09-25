using Mizan.Application.Abstractions;

namespace Mizan.Application.Tests.Fakes;

/// <summary>
/// Birim testlerde telemetri çağrılarını doğrulamak ve gözlemlemek için kullanılan test sahtesi.
/// </summary>
public sealed class FakeTelemetryService : ITelemetryService
{
    private readonly List<TelemetryEventRecord> _events = [];
    private readonly List<TelemetryInvariantViolationRecord> _invariantViolations = [];
    private readonly List<TelemetryExceptionRecord> _exceptions = [];
    private readonly List<TelemetryBreadcrumbRecord> _breadcrumbs = [];

    public IReadOnlyList<TelemetryEventRecord> Events => _events;
    public IReadOnlyList<TelemetryInvariantViolationRecord> InvariantViolations => _invariantViolations;
    public IReadOnlyList<TelemetryExceptionRecord> Exceptions => _exceptions;
    public IReadOnlyList<TelemetryBreadcrumbRecord> Breadcrumbs => _breadcrumbs;

    public void TrackEvent(string eventName, IReadOnlyDictionary<string, string>? properties = null)
    {
        _events.Add(new TelemetryEventRecord(eventName, properties));
    }

    public void TrackInvariantViolation(
        string invariantCode,
        string description,
        IReadOnlyDictionary<string, string>? details = null)
    {
        _invariantViolations.Add(new TelemetryInvariantViolationRecord(invariantCode, description, details));
    }

    public void CaptureException(Exception exception, IReadOnlyDictionary<string, string>? context = null)
    {
        _exceptions.Add(new TelemetryExceptionRecord(exception, context));
    }

    public void AddBreadcrumb(string message, string category = "navigation")
    {
        _breadcrumbs.Add(new TelemetryBreadcrumbRecord(message, category));
    }

    public void Clear()
    {
        _events.Clear();
        _invariantViolations.Clear();
        _exceptions.Clear();
        _breadcrumbs.Clear();
    }
}

public sealed record TelemetryEventRecord(string Name, IReadOnlyDictionary<string, string>? Properties);

public sealed record TelemetryInvariantViolationRecord(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string>? Details);

public sealed record TelemetryExceptionRecord(
    Exception Exception,
    IReadOnlyDictionary<string, string>? Context);

public sealed record TelemetryBreadcrumbRecord(string Message, string Category);
