using Mizan.Application.Abstractions;

namespace Mizan.Infrastructure.Telemetry;

/// <summary>
/// Telemetri portunun tek somut adaptörü: hiçbir olayı hiçbir yere göndermez.
/// Mizan çevrimdışıdır; kullanıcının finansal verisi cihazdan çıkmaz (I40, S60).
/// Eski uygulamanın Sentry adaptörü sahte bir adrese gönderiyor ve uygulamanın internet
/// izni istemesinin tek sebebi oluyordu. Port yine de duruyor, çünkü ekranların hata
/// yakalayan blokları (K5) bildirimi bir yere vermek zorunda. Bu sınıf bildirimi sessizce
/// yutar ve asla istisna fırlatmaz: bir catch bloğundaki telemetri çağrısı uygulamayı düşüremez.
/// </summary>
public sealed class NullTelemetryService : ITelemetryService
{
    /// <inheritdoc />
    public void TrackEvent(string eventName, IReadOnlyDictionary<string, string>? properties = null)
    {
    }

    /// <inheritdoc />
    public void TrackInvariantViolation(
        string invariantCode,
        string description,
        IReadOnlyDictionary<string, string>? details = null)
    {
    }

    /// <inheritdoc />
    public void CaptureException(Exception exception, IReadOnlyDictionary<string, string>? context = null)
    {
    }

    /// <inheritdoc />
    public void AddBreadcrumb(string message, string category = "navigation")
    {
    }
}
